using LibraryApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Repositories
{
    public interface IUnitOfWork
    {
        public IUserRepository UserRepository { get; }
        public IBookRepository BookRepository { get; }
        public IGenericRepository GenericRepository { get; }
        public ICommentRepository CommentRepository { get; }
        public Task Commit();
    }
}
