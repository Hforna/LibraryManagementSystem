using LibraryApp.Domain.Entities;

namespace LibraryApp.Domain.Repositories;

public interface IBookRepository
{
    public Task<Book?> GetFullBook(long id);
    public Task<List<Category>> GetCategories(List<long> categoriesIds);
    public Task<bool> BookByTitleExists(string title);
    public void DeleteBook(Book book);
    public Task<bool> UserDownloadedBook(long userId, long bookId);
}