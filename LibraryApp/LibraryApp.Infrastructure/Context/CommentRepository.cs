using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Pagination.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp.Infrastructure.Context
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DataContext _context;

        public CommentRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Pagination<Comment>> GetComments(long bookId, int page, int perPage)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(d => d.BookId == bookId)
                .OrderByDescending(d => d.CreatedAt)
                .AsPaginationAsync(page, perPage);
        }
    }
}
