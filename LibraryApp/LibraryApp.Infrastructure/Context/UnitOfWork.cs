using LibraryApp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure.Context
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        public IUserRepository UserRepository { get; }
        public IBookRepository BookRepository { get; }
        public IGenericRepository GenericRepository { get; }

        public UnitOfWork(DataContext context, IUserRepository userRepository, IBookRepository bookRepository, IGenericRepository genericRepository)
        {
            _context = context;
            UserRepository = userRepository;
            BookRepository = bookRepository;
            GenericRepository = genericRepository;
        }

        public async Task Commit()
        {
            await _context.SaveChangesAsync();
        }
    }
}
