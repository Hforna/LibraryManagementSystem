using LibraryApp.Domain.Entities;
using Pagination.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp.Domain.Repositories
{
    public interface ICommentRepository
    {
        public Task<Pagination<Comment>> GetComments(long bookId, int page, int perPage);
    }
}
