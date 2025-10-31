using LibraryApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Repositories
{
    public interface IGenericRepository
    {
        public Task<T?> GetById<T>(long id) where T : class, IEntity;    
        public Task Add<T>(T entity) where T : class, IEntity;
        public Task AddRange<T>(List<T> entities) where T : class, IEntity;
        public void DeleteRange<T>(List<T> entities) where T : class, IEntity;
        public void Delete<T>(T entity) where T : class, IEntity;
        public void Update<T>(T entity) where T : class, IEntity;
    }
}
