using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure.Context
{
    public class GenericRepository : IGenericRepository
    {
        private readonly DataContext _context;

        public GenericRepository(DataContext context)
        {
            _context = context;
        }

        public async Task Add<T>(T entity) where T : class, IEntity
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Update<T>(T entity) where T : class, IEntity
        {
            _context.Set<T>().Update(entity);
        }
    }
}
