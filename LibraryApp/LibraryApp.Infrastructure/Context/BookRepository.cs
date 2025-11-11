using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Pagination.EntityFrameworkCore.Extensions;

namespace LibraryApp.Infrastructure.Context;

public class BookRepository : IBookRepository
{
    private readonly DataContext _context;

    public BookRepository(DataContext dataContext) => _context = dataContext;

    public async Task<bool> BookByTitleExists(string title)
    {
        return await _context.Books.AnyAsync(d => d.Title == title);
    }

    public void DeleteBook(Book book)
    {
        _context.Books.Remove(book);
    }

    public async Task<bool> UserDownloadedBook(long userId, long bookId)
    {
        return await _context.Downloads.AnyAsync(d => d.UserId == userId && d.BookId == bookId);
    }

    public async Task<List<Category>> GetCategories(List<long> categoriesIds)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(d => categoriesIds.Contains(d.Id))
            .ToListAsync();
    }

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

    public async Task<bool> UserLikedBook(long userId, long bookId)
    {
        return await _context
            .Likes
            .AnyAsync(d => d.UserId == userId && d.BookId == bookId);
    }

    public async Task<Like?> GetLikeByUserAndBook(long userId, long bookId)
    {
        return await _context.Likes
            .SingleOrDefaultAsync(d => d.UserId == userId && d.BookId == bookId);
    }

    public async Task<Pagination<Book>> GetBooksPaginated(int page, int perPage)
    {
        return await _context.Books
            .AsNoTracking()
            .OrderBy(d => d.Id)
            .AsPaginationAsync(page, perPage);
    }
}