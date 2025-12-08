using Dropbox.Api;
using Dropbox.Api.Files;
using LibraryApp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using static Dropbox.Api.Files.PathOrLink;

namespace LibraryApp.Infrastructure.Services
{
    public class DropBoxStorageService : IStorageService
    {
        private readonly string _accessToken;
        private readonly ILogger _logger;
        private const string BaseFilePath = "/uploads/files/";

        public DropBoxStorageService(string accessToken, ILogger<DropBoxStorageService> logger)
        {
            _accessToken = accessToken;
            _logger = logger;
        }

        public async Task<string> GetFileUrl(string fileName, string bookName)
        {
            using var client = new DropboxClient(_accessToken);
            try
            {
                var link = await client.Files.GetTemporaryLinkAsync($"{BaseFilePath}{fileName}");
                
                _logger.LogInformation($"Getting file link: {link}");

                return link.Link.Replace("?dl=0", "?raw=1");
            }catch (Exception ex)
            {
                return "";
            }
        }

        public async Task DeleteFile(string fileName, string bookName)
        {
            using var client = new DropboxClient(_accessToken);
            await client.Files.DeleteV2Async(fileName);
        }

        public async Task UploadFile(Stream file, string fileName)
        {
            using var client = new DropboxClient(_accessToken);
            var result = await client.Files.UploadAsync(
                path: $"{BaseFilePath}{fileName}",
                mode: WriteMode.Overwrite.Instance,
                body: file);
        }
    }
}
