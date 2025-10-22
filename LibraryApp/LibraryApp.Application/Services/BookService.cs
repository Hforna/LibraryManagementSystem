using AutoMapper;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using LibraryApp.Domain.Services;

namespace LibraryApp.Application.Services
{
    public interface IBookService
    {
        public Task<BookResponse> GetBook(long id);
    }

    public class BookService : IBookService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<IBookService> _logger;
        private readonly ITokenService _tokenService;

        public BookService(IUnitOfWork uow, IMapper mapper, 
            ILogger<IBookService> logger, ITokenService tokenService)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
            _tokenService = tokenService;
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

            return response;
        }
    }
}
