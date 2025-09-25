using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Repositories;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure.Context
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>  x.Email == email);
        }

        public async Task<bool> HasUserByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }
    }
}
