using LibraryApp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure.Services.Security
{
    public class PasswordCryptography : IPasswordCryptography
    {
        public bool CompareHash(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public string Encrypt(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
