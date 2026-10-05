using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Constants;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using LibraryApp.Domain.Services;
using LibraryApp.Application.Requests;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LibraryApp.Application.Services
{    
    // Interface que define os contratos (métodos) do serviço de livros
    public interface IBookService
    {        
        public Task<BookResponse> GetBook(long id); // Busca livro completo por ID

        public Task<BookResponse> CreateBook(BookRequest request); // Cria novo livro no sistema

        public Task<BookResponse> UpdateBook(UpdateBookRequest request, long id); // Atualiza livro existente

        public Task<BooksPaginatedResponse> GetBooksPaginated(int page, int perPage); // Lista livros com paginação

        public Task<MyBooksPaginatedResponse> GetMyBooks(int page, int perPage); // Lista, paginado, os livros enviados pelo estudante autenticado (com a situação de cada um)

        public Task DeleteBook(long bookId); // Remove livro do sistema

        public Task LikeBook(long bookId); // Usuário curte um livro

        public Task UnlikeBook(long bookId); // Usuário remove curtida de um livro

        public Task<bool> UserLikedBook(long bookId); // Verifica se usuário já curtiu o livro

        public Task<AdminBooksPaginatedResponse> GetBooksPendingAnalysis(int page, int perPage); // Lista, paginado, os livros aguardando análise (uso do administrador)

        public Task<BookResponse> ApproveBook(long bookId); // Administrador aprova e publica um livro em análise

        public Task<BookResponse> RejectBook(long bookId, string reason); // Administrador reprova um livro em análise

        public Task<(Stream Stream, string FileName)> DownloadBookForReview(long bookId); // Administrador baixa o arquivo original do livro para validar seu conteúdo
    }

    // Implementação do serviço de livros
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _uow; // Unit of Work - coordena transações e acesso aos repositórios
        private readonly IMapper _mapper; // AutoMapper - converte automaticamente entre objetos
        private readonly ILogger<IBookService> _logger; // Logger - registra logs de erro, informação, debug
        private readonly ITokenService _tokenService; // Serviço de tokens JWT - obtém usuário do token de autenticação     
        private readonly IFileService _fileService; // Serviço de arquivos - valida PDFs e imagens      
        private readonly IStorageService _storageService; // Serviço de armazenamento (Dropbox) - upload/download de arquivos

        // Construtor com injeção de dependências
        public BookService(IUnitOfWork uow, IMapper mapper,
            ILogger<IBookService> logger, ITokenService tokenService,
            IFileService fileService, IStorageService storageService)
        {
            _uow = uow;
            _fileService = fileService;
            _storageService = storageService;
            _mapper = mapper;
            _logger = logger;
            _tokenService = tokenService;
        }

        // Cria um novo livro no sistema
        public async Task<BookResponse> CreateBook(BookRequest request)
        {
            var titleExists = await _uow.BookRepository.BookByTitleExists(request.Title); // VALIDAÇÃO 1: Verificar se livro com mesmo título já existe

            if (titleExists)
                throw new RequestException("Livro com mesmo nome ja existe");
            
            var categories = await _uow.BookRepository.GetCategoriesByIds(request.CategoriesIds); // VALIDAÇÃO 2: Verificar se todas as categorias existem

            // Se quantidade de categorias encontradas != quantidade solicitada
            if (categories.Count != request.CategoriesIds.Count)
                throw new RequestException("Categorias fornecidas invalidas");

            var file = request.File.OpenReadStream(); // VALIDAÇÃO 3: Validar arquivo PDF do livro
            var validateFile = _fileService.ValidatePdf(file);

            if (!validateFile.isValid)
                throw new RequestException("Formato de arquivo deve ser pdf");

            var book = _mapper.Map<Book>(request); // PREPARAÇÃO: Converter Request para Entity e gerar nomes

            book.FileName = $"{request.Title}_file.{validateFile.ext}"; // Nome do arquivo: "1984_file.pdf"

            // VALIDAÇÃO 4: Processar capa (opcional)
            if (request.Cover is not null)
            {
                var image = request.Cover.OpenReadStream();
                var validateImage = _fileService.ValidateImage(image);

                if (!validateImage.isValid)
                {
                    _logger.LogError("It was not possible to process cover file received by request");

                    throw new RequestException("Formato de imagem invalida, deve ser apenas png ou jpg");
                }
                
                book.CoverName = $"{request.Title}_cover.{validateImage.ext}"; // Nome da capa: "1984_cover.jpg"

                await _storageService.UploadFile(image, book.CoverName); // Upload da capa para Dropbox
            }

            await _storageService.UploadFile(file, book.FileName); // UPLOAD: Enviar arquivo PDF para Dropbox

            var user = await _tokenService.GetUserByToken(); // AUTENTICAÇÃO: Obter usuário logado do token JWT
            book.UserId = user.Id;
            
            await _uow.GenericRepository.Add<Book>(book); // PERSISTÊNCIA 1: Salvar livro no banco
            await _uow.Commit(); // Commit para obter book.Id gerado
            
            var bookCategories = new List<BookCategory>(); // RELACIONAMENTO: Associar categorias ao livro
            foreach (var category in categories)
            {
                bookCategories.Add(new BookCategory()
                {
                    BookId = book.Id,
                    CategoryId = category.Id
                });
            }
            await _uow.GenericRepository.AddRange<BookCategory>(bookCategories);
            await _uow.Commit();
            
            var response = _mapper.Map<BookResponse>(book); // RESPOSTA: Montar BookResponse para retornar

            response.Categories = categories // Converte lista de Category para lista de nomes (strings)
                .Select(d => d.Name)
                .ToList();

            return response;
        }
        
        // Busca um livro completo por ID
        public async Task<BookResponse> GetBook(long id)
        {            
            var book = await _uow.BookRepository.GetFullBook(id) ?? // BUSCA: Obter livro com todos os relacionamentos
                throw new NotFoundException("Livro não foi encontrado");

            var user = await _tokenService.GetUserByToken(); // VISUALIZAÇÃO: Registrar view se usuário autenticado

            // VISIBILIDADE: livros que ainda não foram publicados só podem ser vistos pelo autor ou por um administrador
            if (book.Status != BookStatus.Published)
            {
                var isOwner = user is not null && book.UserId == user.Id;
                var isAdmin = user is not null && _tokenService.CurrentUserIsInRole(Roles.Admin);

                if (!isOwner && !isAdmin)
                    throw new NotFoundException("Livro não foi encontrado");
            }

            // Se usuário está logado E ainda não visualizou este livro
            if (user is not null && book.Views.Any(d => d.UserId == user.Id) == false)
            {
                var view = new View() { BookId = book.Id, UserId = user.Id };

                await _uow.GenericRepository.Add<View>(view);
                await _uow.Commit();
            }
            
            var response = _mapper.Map<BookResponse>(book); // RESPOSTA: Montar BookResponse com todas as informações

            response.LikesCount = book.Likes.Count; // Contador de like
            response.TotalViews = book.Views.Count; // Contador de views

            response.Categories = book.BookCategories // Categorias: extrair apenas os nomes
                .Select(d => d.Category.Name)
                .ToList();
            
            response.CoverUrl = string.IsNullOrEmpty(book.CoverName) // URLs temporárias do Dropbox
                ? ""
                : await _storageService.GetFileUrl(book.CoverName, book.Title);
            response.FileUrl = await _storageService.GetFileUrl(book.FileName, book.Title);
            
            response.AuthorName = book.User.UserName; // Nome do autor

            return response;
        }

        // Atualiza um livro existente       
        public async Task<BookResponse> UpdateBook(UpdateBookRequest request, long id)
        {            
            var book = await _uow.BookRepository.GetFullBook(id) // BUSCA: Verificar se livro existe
                ?? throw new NotFoundException("Livro não foi encontrado");
            
            var user = await _tokenService.GetUserByToken(); // AUTORIZAÇÃO: Verificar se usuário é o dono do livro
            if (book.UserId != user.Id)
                throw new UnauthorizedException("Usúario não tem permissão para atualizar esse livro");

            // VALIDAÇÃO 1: Verificar novo título (se fornecido)
            if (!string.IsNullOrEmpty(book.Title))
            {
                var titleExists = await _uow.BookRepository.BookByTitleExists(request.Title);

                if (titleExists)
                    throw new RequestException("Livro com mesmo nome ja existe");
            }

            // ATUALIZAÇÃO 1: Atualizar categorias (se fornecidas)
            if (request.CategoriesIds is not null && request.CategoriesIds.Any())
            {                
                var categories = await _uow.BookRepository.GetCategoriesByIds(request.CategoriesIds); // Validar que todas as categorias existem

                if (categories.Count != request.CategoriesIds.Count)
                    throw new RequestException("Categorias fornecidas invalidas");
                
                var bookCategories = new List<BookCategory>(); // Criar novas associações
                foreach (var category in categories)
                {
                    bookCategories.Add(new BookCategory()
                    {
                        BookId = book.Id,
                        CategoryId = category.Id
                    });
                }
                await _uow.GenericRepository.AddRange<BookCategory>(bookCategories);
                
                _uow.GenericRepository.DeleteRange<BookCategory>(book.BookCategories.ToList()); // Remover associações antigas
            }

            // ATUALIZAÇÃO 2: Atualizar capa (se fornecida)           
            if (request.Cover is not null)
            {
                var image = request.Cover.OpenReadStream();
                var validateImage = _fileService.ValidateImage(image);

                if (!validateImage.isValid)
                {
                    _logger.LogError("It was not possible to process cover file received by request");

                    throw new RequestException("Formato de imagem invalida, deve ser apenas png ou jpg");
                }
                
                await _storageService.UploadFile(image, book.CoverName); // Sobrescreve arquivo existente no Dropbox
            }

            // ATUALIZAÇÃO 3: Atualizar arquivo PDF (se fornecido)           
            if (request.File is not null)
            {
                var file = request.File.OpenReadStream();
                var validateFile = _fileService.ValidatePdf(file);

                if (!validateFile.isValid)
                    throw new RequestException("Formato de arquivo deve ser pdf");
                
                await _storageService.UploadFile(file, book.FileName); // Sobrescreve arquivo existente no Dropbox

                // Trocar o arquivo invalida a análise anterior: o conteúdo precisa ser revisado novamente antes de ficar público
                book.Status = BookStatus.PendingAnalysis;
                book.RejectionReason = null;
                book.ReviewedByUserId = null;
                book.ReviewedAt = null;
            }

            // PERSISTÊNCIA: Atualizar campos do livro
            _mapper.Map(request, book);
            _uow.GenericRepository.Update<Book>(book);
            await _uow.Commit();
            
            var response = _mapper.Map<BookResponse>(book); // RESPOSTA: Montar BookResponse atualizado
            response.LikesCount = book.Likes.Count;
            response.Categories = book.BookCategories
                .Select(d => d.Category.Name)
                .ToList();
            response.TotalViews = book.Views.Count;
            response.CoverUrl = string.IsNullOrEmpty(book.CoverName)
                ? ""
                : await _storageService.GetFileUrl(book.CoverName, book.Title);
            response.FileUrl = await _storageService.GetFileUrl(book.FileName, book.Title);
            response.AuthorName = book.User.UserName;

            return response;
        }

        // Remove um livro do sistema
        public async Task DeleteBook(long bookId)
        {
            var user = await _tokenService.GetUserByToken(); // AUTORIZAÇÃO: Verificar permissões

            var book = await _uow.GenericRepository.GetById<Book>(bookId)
                       ?? throw new NotFoundException("Livro não foi encontrado");

            if (book.UserId != user.Id)
                throw new UnauthorizedException("Usuario não tem permissão para deletar livro");

            await _storageService.DeleteFile(book.FileName, book.Title); // LIMPEZA: Remover arquivos do Dropbox

            // Remover capa se existir
            if (!string.IsNullOrEmpty(book.CoverName)) 
                await _storageService.DeleteFile(book.CoverName, book.Title);

            _uow.BookRepository.DeleteBook(book); // PERSISTÊNCIA: Remover do banco
            await _uow.Commit();
        }

        // Usuário curte um livro
        public async Task LikeBook(long bookId)
        {            
            var user = await _tokenService.GetUserByToken(); // AUTENTICAÇÃO: Obter usuário logado              
            var book = await _uow.GenericRepository.GetById<Book>(bookId) // VALIDAÇÃO 1: Verificar se livro existe
                       ?? throw new NotFoundException("Livro não foi encontrado");            
            var userLiked = await _uow.BookRepository.UserLikedBook(user.Id, bookId); // VALIDAÇÃO 2: Verificar se usuário já curtiu

            if (userLiked)
                throw new RequestException("Usuário ja curtiu esse livro");
            
            var like = new Like() // PERSISTÊNCIA: Criar registro de curtida
            {
                UserId = user.Id,
                BookId = bookId
            };

            await _uow.GenericRepository.Add<Like>(like);
            await _uow.Commit();
        }
        
        // Usuário remove curtida de um livro
        public async Task UnlikeBook(long bookId)
        {
            var user = await _tokenService.GetUserByToken(); // AUTENTICAÇÃO: Obter usuário logado             
            var book = await _uow.GenericRepository.GetById<Book>(bookId) // VALIDAÇÃO 1: Verificar se livro existe        
                       ?? throw new NotFoundException("Livro não foi encontrado");            
            var userLiked = await _uow.BookRepository.UserLikedBook(user.Id, bookId); // VALIDAÇÃO 2: Verificar se usuário curtiu antes           

            if (!userLiked)
                throw new RequestException("Usuário ainda não curtiu esse livro");
             
            var like = await _uow.BookRepository.GetLikeByUserAndBook(user.Id, book.Id); // PERSISTÊNCIA: Remover registro de curtida           

            _uow.GenericRepository.Delete<Like>(like);
            await _uow.Commit();
        }

        // Lista livros com paginação       
        public async Task<BooksPaginatedResponse> GetBooksPaginated(int page, int perPage)
        {
            var books = await _uow.BookRepository.GetBooksPaginated(page, perPage, BookStatus.Published); // BUSCA: Obter página de livros já publicados
            var response = _mapper.Map<BooksPaginatedResponse>(books);// MAPEAMENTO: Converter para resposta paginada
            var booksResponse = books.Results.Select(async book =>  // Processar cada livro para obter URLs e informações adicionais
            {
                var response = _mapper.Map<BookShortResponse>(book);
                
                response.CoverUrl = string.IsNullOrEmpty(book.CoverName) // Obter URL da capa do Dropbox
                    ? ""
                    : await _storageService.GetFileUrl(book.FileName, book.Title);

                response.AuthorName = book.User.UserName;
                response.ViewsCount = book.Views.Count;

                return response;
            });
                         
            var booksList = await Task.WhenAll(booksResponse); // AGUARDAR: Esperar todas as operações assíncronas     

            response.Books = booksList.ToList();

            return response;
        }

        // Lista, paginado, os livros enviados pelo estudante autenticado, com a situação de cada um (pendente, publicado ou reprovado)
        public async Task<MyBooksPaginatedResponse> GetMyBooks(int page, int perPage)
        {
            var user = await _tokenService.GetUserByToken()
                ?? throw new UnauthorizedException("Usuario deve estar autenticado para acessar esse recurso");

            var books = await _uow.BookRepository.GetBooksByUserPaginated(user.Id, page, perPage);

            var response = _mapper.Map<MyBooksPaginatedResponse>(books);

            var booksResponse = books.Results.Select(async book =>
            {
                var bookResponse = _mapper.Map<MyBookResponse>(book);

                bookResponse.CoverUrl = string.IsNullOrEmpty(book.CoverName)
                    ? ""
                    : await _storageService.GetFileUrl(book.CoverName, book.Title);

                bookResponse.ViewsCount = book.Views.Count;
                bookResponse.LikesCount = book.Likes.Count;

                return bookResponse;
            });

            response.Books = (await Task.WhenAll(booksResponse)).ToList();

            return response;
        }

        // Verifica se o usuário logado curtiu um livro específico
        public async Task<bool> UserLikedBook(long bookId)
        {
            var user = await _tokenService.GetUserByToken();
            
            if (user is null) return false; // Se usuário não está logado, retorna false

            return await _uow.BookRepository.UserLikedBook(user.Id, bookId);
        }

        // Lista, paginada, os livros aguardando análise (fila de moderação do administrador)
        public async Task<AdminBooksPaginatedResponse> GetBooksPendingAnalysis(int page, int perPage)
        {
            var books = await _uow.BookRepository.GetBooksPaginated(page, perPage, BookStatus.PendingAnalysis);

            var response = _mapper.Map<AdminBooksPaginatedResponse>(books);

            var booksResponse = books.Results.Select(async book =>
            {
                var bookResponse = _mapper.Map<BookResponse>(book);

                bookResponse.CoverUrl = string.IsNullOrEmpty(book.CoverName)
                    ? ""
                    : await _storageService.GetFileUrl(book.CoverName, book.Title);
                bookResponse.FileUrl = await _storageService.GetFileUrl(book.FileName, book.Title);
                bookResponse.AuthorName = book.User.UserName;

                return bookResponse;
            });

            response.Books = (await Task.WhenAll(booksResponse)).ToList();

            return response;
        }

        // Administrador aprova um livro em análise, tornando-o público
        public async Task<BookResponse> ApproveBook(long bookId)
        {
            var book = await _uow.GenericRepository.GetById<Book>(bookId)
                ?? throw new NotFoundException("Livro não foi encontrado");

            if (book.Status != BookStatus.PendingAnalysis)
                throw new RequestException("Somente livros aguardando análise podem ser aprovados");

            var admin = await _tokenService.GetUserByToken();

            book.Status = BookStatus.Published;
            book.RejectionReason = null;
            book.ReviewedByUserId = admin!.Id;
            book.ReviewedAt = DateTime.UtcNow;

            _uow.GenericRepository.Update<Book>(book);
            await _uow.Commit();

            return _mapper.Map<BookResponse>(book);
        }

        // Administrador reprova um livro em análise, informando o motivo
        public async Task<BookResponse> RejectBook(long bookId, string reason)
        {
            var book = await _uow.GenericRepository.GetById<Book>(bookId)
                ?? throw new NotFoundException("Livro não foi encontrado");

            if (book.Status != BookStatus.PendingAnalysis)
                throw new RequestException("Somente livros aguardando análise podem ser reprovados");

            var admin = await _tokenService.GetUserByToken();

            book.Status = BookStatus.Rejected;
            book.RejectionReason = reason;
            book.ReviewedByUserId = admin!.Id;
            book.ReviewedAt = DateTime.UtcNow;

            _uow.GenericRepository.Update<Book>(book);
            await _uow.Commit();

            return _mapper.Map<BookResponse>(book);
        }

        // Administrador baixa o arquivo original enviado pelo estudante para validar seu conteúdo antes de publicar
        public async Task<(Stream Stream, string FileName)> DownloadBookForReview(long bookId)
        {
            var book = await _uow.GenericRepository.GetById<Book>(bookId)
                ?? throw new NotFoundException("Livro não foi encontrado");

            var stream = await _storageService.DownloadFile(book.FileName);

            return (stream, book.FileName);
        }
    }
}