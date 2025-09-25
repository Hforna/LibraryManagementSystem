using LibraryApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Repositories
{
    public interface IGenericRepository
    {
        public Task Add<T>(T entity) where T : class, IEntity;
    }
}
