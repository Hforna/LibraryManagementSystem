using Dropbox.Api;
using Dropbox.Api.Files;
using LibraryApp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using static Dropbox.Api.Files.PathOrLink;

namespace LibraryApp.Infrastructure.Services
{
    public class DropBoxStorageService : IStorageService
    {
        private readonly string _accessToken;
        private const string BaseFilePath = "/uploads/files/";

        public DropBoxStorageService(string accessToken)
        {
            _accessToken = accessToken;
        }

        public async Task<string> GetFileUrl(string fileName, string bookName)
        {
            using var client = new DropboxClient(_accessToken);
            try
            {
                var list = await client.Sharing.ListSharedLinksAsync($"{BaseFilePath}{fileName}", directOnly: true);
                if (list.Links.Any())
                {
                    return list.Links.FirstOrDefault()!.Url.Replace("?dl=0", "?raw=1");
                }else
                {
                    var link = await client.Sharing.CreateSharedLinkWithSettingsAsync($"{BaseFilePath}{fileName}");
                    return link.Url.Replace("?dl=0", "?raw=1");
                }
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
            await client.Files.UploadAsync(
                path: $"{BaseFilePath}{fileName}",
                mode: WriteMode.Overwrite.Instance,
                body: file);
        }
    }
}
