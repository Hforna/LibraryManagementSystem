using LibraryApp.Domain.Entities;

namespace LibraryApp.Domain.Repositories;

public interface IBookRepository
{
    public Task<Book?> GetFullBook(long id);
    public Task<List<Category>> GetCategories(List<long> categoriesIds);
}