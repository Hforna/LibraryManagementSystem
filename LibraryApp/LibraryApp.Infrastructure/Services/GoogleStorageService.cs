using LibraryApp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure.Services
{
    public class GoogleStorageService : IStorageService
    {
        public Task<string> GetFileUrl(string fileName, string bookName)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFile(string fileName, string bookName)
        {
            throw new NotImplementedException();
        }

        public Task UploadFile(Stream file, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
