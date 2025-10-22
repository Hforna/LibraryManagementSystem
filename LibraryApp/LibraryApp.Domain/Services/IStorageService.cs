using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Services
{
    public interface IStorageService
    {
        public Task UploadFile(Stream file,  string fileName);
        public Task<string> GetFileUrl(string fileName, string bookName);
    }
}
