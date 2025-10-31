using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
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

        public async Task<T?> GetById<T>(long id) where T : class, IEntity
        {
            return await _context.Set<T>()
                .AsNoTracking()
                .SingleOrDefaultAsync(book => book.Id == id);
        }

        public void Delete<T>(T entity) where T : class, IEntity
        {
            _context.Set<T>().Remove(entity);
        }

        public void Update<T>(T entity) where T : class, IEntity
        {
            _context.Set<T>().Update(entity);
        }

        public async Task AddRange<T>(List<T> entities) where T : class, IEntity
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }

        public void DeleteRange<T>(List<T> entities) where T : class, IEntity
        {
            _context.Set<T>().RemoveRange(entities);
        }
    }
}
