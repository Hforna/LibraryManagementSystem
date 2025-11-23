using LibraryApp.Domain.Entities;
using Pagination.EntityFrameworkCore;
using Pagination.EntityFrameworkCore.Extensions;

namespace LibraryApp.Domain.Repositories;

public interface IBookRepository
{
    public Task<Book?> GetFullBook(long id);
    public Task<List<Category>> GetCategoriesByIds(List<long> categoriesIds);
    public Task<List<Category>> GetCategories();
    public Task<bool> BookByTitleExists(string title);
    public void DeleteBook(Book book);
    public Task<bool> UserDownloadedBook(long userId, long bookId);
    public Task<bool> UserLikedBook(long userId, long bookId);
    public Task<Like?> GetLikeByUserAndBook(long userId, long bookId);
    public Task<Pagination<Book>> GetBooksPaginated(int page, int perPage);
    public Task<int> GetTotalViewsOfABook(long bookId);
}