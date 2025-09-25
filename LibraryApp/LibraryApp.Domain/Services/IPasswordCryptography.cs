using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Services
{
    public interface IPasswordCryptography
    {
        public string Encrypt(string password);
        public bool CompareHash(string password, string hash);
    }
}
