using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Infrastructure.Context;

public class BookRepository : IBookRepository
{
    private readonly DataContext _context;

    public BookRepository(DataContext dataContext) => _context = dataContext;


    public async Task<Book?> GetFullBook(long id)
    {
        return await _context.Books
            .AsNoTracking()
            .Include(d => d.BookCategories)
            .ThenInclude(d => d.Category)
            .Include(d => d.Likes)
            .Include(d => d.Views)
            .SingleOrDefaultAsync(d => d.Id == id);
    }
}