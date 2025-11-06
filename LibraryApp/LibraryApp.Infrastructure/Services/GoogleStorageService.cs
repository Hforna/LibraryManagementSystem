using LibraryApp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure.Services
{
    public class GoogleStorageService : IStorageService
    {
        public async Task<string> GetFileUrl(string fileName, string bookName)
        {
            return "";
        }

        public Task DeleteFile(string fileName, string bookName)
        {
            throw new NotImplementedException();
        }

        public async Task UploadFile(Stream file, string fileName)
        {
            //throw new NotImplementedException();
        }
    }
}
