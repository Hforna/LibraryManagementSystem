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
        public IGenericRepository GenericRepository { get; }

        public UnitOfWork(DataContext context, IUserRepository userRepository, IGenericRepository genericRepository)
        {
            _context = context;
            UserRepository = userRepository;
            GenericRepository = genericRepository;
        }

        public async Task Commit()
        {
            await _context.SaveChangesAsync();
        }
    }
}
