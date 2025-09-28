using LibraryApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Repositories
{
    public interface IUserRepository
    {
        public Task<User?> GetUserByEmailAsync(string email);
        public Task<User?> UserByEmailNotConfirmed(string email);
        public Task<bool> HasUserByEmailAsync(string email);
    }
}
