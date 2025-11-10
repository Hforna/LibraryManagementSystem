using AutoMapper;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using LibraryApp.Domain.Services;
using LibraryApp.Application.Requests;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LibraryApp.Application.Services
{
    public interface IBookService
    {
        public Task<BookResponse> GetBook(long id);
        public Task<BookResponse> CreateBook(BookRequest request);
        public Task<BookResponse> UpdateBook(UpdateBookRequest request, long id);
        public Task<BooksPaginatedResponse> GetBooksPaginated(int page, int perPage);
        public Task DeleteBook(long bookId);
        public Task LikeBook(long bookId);
        public Task UnlikeBook(long bookId);
    }

    public class BookService : IBookService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<IBookService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IFileService _fileService;
        private readonly IStorageService _storageService;

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

        public async Task<BookResponse> CreateBook(BookRequest request)
        {
            var titleExists = await _uow.BookRepository.BookByTitleExists(request.Title);

            if (titleExists)
                throw new RequestException("Livro com mesmo nome ja existe");

            var categories = await _uow.BookRepository.GetCategories(request.CategoriesIds);

            if (categories.Count != request.CategoriesIds.Count)
                throw new RequestException("Categorias fornecidas invalidas");

            var file = request.File.OpenReadStream();
            var validateFile = _fileService.ValidatePdf(file);

            if (!validateFile.isValid)
                throw new RequestException("Formato de arquivo deve ser pdf");

            var book = _mapper.Map<Book>(request);
            book.FileName = $"{request.Title}_file.{validateFile.ext}";

            if (request.Cover is not null)
            {
                var image = request.Cover.OpenReadStream();
                var validateImage = _fileService.ValidateImage(image);

                if (!validateImage.isValid)
                {
                    _logger.LogError("It was not possible to process cover file received by request");

                    throw new RequestException("Formato de imagem invalida, deve ser apenas png ou jpg");
                }
                book.CoverName = $"{request.Title}_cover.{validateImage.ext}";
                await _storageService.UploadFile(image, book.CoverName);
            }
            await _storageService.UploadFile(file, book.FileName);

            var user = await _tokenService.GetUserByToken();
            book.UserId = user.Id;

            await _uow.GenericRepository.Add<Book>(book);
            await _uow.Commit();

            var bookCategories = new List<BookCategory>();
            foreach(var category in categories)
            {
                bookCategories.Add(new BookCategory() { BookId = book.Id, CategoryId = category.Id });
            }
            await _uow.GenericRepository.AddRange<BookCategory>(bookCategories);
            await _uow.Commit();

            var response = _mapper.Map<BookResponse>(book);
            response.Categories = categories
                .Select(d => d.Name)
                .ToList();

            return response;
        }

        public async Task<BookResponse> GetBook(long id)
        {
            var book = await _uow.BookRepository.GetFullBook(id) ?? 
                throw new NotFoundException("Livro não foi encontrado");

            var user = await _tokenService.GetUserByToken();

            if (user is not null && book.Views.Any(d => d.UserId == user.Id) == false)
            {
                var view = new View() { BookId = book.Id, UserId = user.Id};
                
                await _uow.GenericRepository.Add<View>(view);
                await _uow.Commit();
            }

            var response = _mapper.Map<BookResponse>(book);
            response.LikesCount = book.Likes.Count;
            response.Categories = book.BookCategories.Select(d => d.Category.Name).ToList();
            response.TotalViews = book.Views.Count;
            response.CoverUrl = string.IsNullOrEmpty(book.CoverName) ? "" : await _storageService.GetFileUrl(book.CoverName, book.Title);
            response.FileUrl = await _storageService.GetFileUrl(book.FileName, book.Title);

            return response;
        }

        public async Task<BookResponse> UpdateBook(UpdateBookRequest request, long id)
        {
            var book = await _uow.BookRepository.GetFullBook(id)
                ?? throw new NotFoundException("Livro não foi encontrado");

            var user = await _tokenService.GetUserByToken();
            if (book.UserId != user.Id)
                throw new UnauthorizedException("Usúario não tem permissão para atualizar esse livro");

            if (!string.IsNullOrEmpty(book.Title))
            {
                var titleExists = await _uow.BookRepository.BookByTitleExists(request.Title);

                if (titleExists)
                    throw new RequestException("Livro com mesmo nome ja existe");
            }

            if(request.CategoriesIds is not null && request.CategoriesIds.Any())
            {
                var categories = await _uow.BookRepository.GetCategories(request.CategoriesIds);

                if (categories.Count != request.CategoriesIds.Count)
                    throw new RequestException("Categorias fornecidas invalidas");

                var bookCategories = new List<BookCategory>();
                foreach (var category in categories)
                {
                    bookCategories.Add(new BookCategory() { BookId = book.Id, CategoryId = category.Id });
                }
                await _uow.GenericRepository.AddRange<BookCategory>(bookCategories);
                _uow.GenericRepository.DeleteRange<BookCategory>(book.BookCategories.ToList());
            }

            if(request.Cover is not null)
            {
                var image = request.Cover.OpenReadStream();
                var validateImage = _fileService.ValidateImage(image);

                if (!validateImage.isValid)
                {
                    _logger.LogError("It was not possible to process cover file received by request");

                    throw new RequestException("Formato de imagem invalida, deve ser apenas png ou jpg");
                }
                await _storageService.UploadFile(image, book.CoverName);
            }
            if(request.File is not null)
            {
                var file = request.File.OpenReadStream();
                var validateFile = _fileService.ValidatePdf(file);

                if (!validateFile.isValid)
                    throw new RequestException("Formato de arquivo deve ser pdf");

                await _storageService.UploadFile(file, book.FileName);
            }
            _mapper.Map(request, book);
            _uow.GenericRepository.Update<Book>(book);
            await _uow.Commit();

            var response = _mapper.Map<BookResponse>(book);
            response.LikesCount = book.Likes.Count;
            response.Categories = book.BookCategories.Select(d => d.Category.Name).ToList();
            response.TotalViews = book.Views.Count;
            response.CoverUrl = string.IsNullOrEmpty(book.CoverName) ? "" : await _storageService.GetFileUrl(book.CoverName, book.Title);
            response.FileUrl = await _storageService.GetFileUrl(book.FileName, book.Title);

            return response;
        }

        public async Task DeleteBook(long bookId)
        {
            var user = await _tokenService.GetUserByToken();
       
            var book = await _uow.GenericRepository.GetById<Book>(bookId) 
                       ?? throw new NotFoundException("Livro não foi encontrado");
            
            if (book.UserId != user.Id)
                throw new UnauthorizedException("Usuario não tem permissão para deletar livro");

            await _storageService.DeleteFile(book.FileName, book.Title);
            if(!string.IsNullOrEmpty(book.CoverName)) 
                await _storageService.DeleteFile(book.CoverName, book.Title);
            
            _uow.BookRepository.DeleteBook(book);
            await _uow.Commit();
        }

        public async Task LikeBook(long bookId)
        {
            var user = await _tokenService.GetUserByToken();

            var book = await _uow.GenericRepository.GetById<Book>(bookId)
                       ?? throw new NotFoundException("Livro não foi encontrado");

            var userLiked = await _uow.BookRepository.UserLikedBook(user.Id, bookId);

            if (userLiked)
                throw new RequestException("Usuário ja curtiu esse livro");

            var like = new Like()
            {
                UserId = user.Id,
                BookId = bookId
            };

            await _uow.GenericRepository.Add<Like>(like);
            await _uow.Commit();
        }

        public async Task UnlikeBook(long bookId)
        {
            var user = await _tokenService.GetUserByToken();

            var book = await _uow.GenericRepository.GetById<Book>(bookId)
                       ?? throw new NotFoundException("Livro não foi encontrado");

            var userLiked = await _uow.BookRepository.UserLikedBook(user.Id, bookId);

            if (!userLiked)
                throw new RequestException("Usuário ainda não curtiu esse livro");

            var like = await _uow.BookRepository.GetLikeByUserAndBook(user.Id, book.Id);

            _uow.GenericRepository.Delete<Like>(like);
            await _uow.Commit();
        }

        public async Task<BooksPaginatedResponse> GetBooksPaginated(int page, int perPage)
        {
            var books = await _uow.BookRepository.GetBooksPaginated(page, perPage);

            var response = _mapper.Map<BooksPaginatedResponse>(books);
            var booksResponse = books.Results.Select(async book =>
            {
                var response = _mapper.Map<BookShortResponse>(book);
                response.CoverUrl = string.IsNullOrEmpty(book.CoverName) ? "" : await _storageService.GetFileUrl(book.FileName, book.Title);

                return response;
            });

            var booksList = await Task.WhenAll(booksResponse);

            response.Books = booksList.ToList();

            return response;
        }
    }
}
