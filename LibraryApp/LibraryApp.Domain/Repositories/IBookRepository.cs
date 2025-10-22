using LibraryApp.Domain.Entities;

namespace LibraryApp.Domain.Repositories;

public interface IBookRepository
{
    public Task<Book?> GetFullBook(long id);
}