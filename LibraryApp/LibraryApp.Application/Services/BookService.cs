using AutoMapper;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using LibraryApp.Domain.Services;
using LibraryApp.Application.Requests;

namespace LibraryApp.Application.Services
{
    public interface IBookService
    {
        public Task<BookResponse> GetBook(long id);
        public Task<BookResponse> CreateBook(CreateBookRequest request);
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

        public async Task<BookResponse> CreateBook(CreateBookRequest request)
        {
            var categories = await _uow.BookRepository.GetCategories(request.CategoriesIds);

            if (categories.Count != request.CategoriesIds.Count)
                throw new RequestException("Categorias fornecidas invalidas");

            using var file = request.File.OpenReadStream();
            var validateFile = _fileService.ValidatePdf(file);

            if (!validateFile.isValid)
                throw new RequestException("Formato de arquivo deve ser pdf");

            var book = _mapper.Map<Book>(request);
            book.FileName = $"{request.Title}_file.{validateFile.ext}";

            if (request.Cover is not null)
            {
                using var image = request.Cover.OpenReadStream();
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
            response.CoverUrl = await _storageService.GetFileUrl(book.CoverName, book.Title);
            response.FileUrl = await _storageService.GetFileUrl(book.FileName, book.Title);

            return response;
        }
    }
}
