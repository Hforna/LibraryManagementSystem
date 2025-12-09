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
    // Token de acesso para autenticação na API do Dropbox
    public class DropBoxStorageService : IStorageService
    {
        private readonly string _accessToken;
        private readonly ILogger _logger;
        private const string BaseFilePath = "/uploads/files/";
        
        // Construtor que recebe o token de acesso do Dropbox
        public DropBoxStorageService(string accessToken, ILogger<DropBoxStorageService> logger)
        {
            _accessToken = accessToken;
            _logger = logger;
        }
        
        // Obtém uma URL temporária para acessar um arquivo armazenado no Dropbox
        public async Task<string> GetFileUrl(string fileName, string bookName)
        {            
            using var client = new DropboxClient(_accessToken); // Cria um cliente Dropbox usando o token de acesso
            try
            {
                var link = await client.Files.GetTemporaryLinkAsync($"{BaseFilePath}{fileName}");
                
                _logger.LogInformation($"Getting file link: {link}");

                return link.Link.Replace("?dl=0", "?raw=1");
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // Exclui um arquivo do Dropbo
        public async Task DeleteFile(string fileName, string bookName)
        {
            using var client = new DropboxClient(_accessToken);  // Cria um cliente Dropbox para realizar a operação
            await client.Files.DeleteV2Async(fileName); // Exclui o arquivo usando a versão 2 da API de deleção
        }

        // Faz upload de um arquivo para o Dropbox
        public async Task UploadFile(Stream file, string fileName)
        {
            using var client = new DropboxClient(_accessToken); // Cria um cliente Dropbox para realizar o upload
            var result = await client.Files.UploadAsync( // Faz o upload do arquivo
                path: $"{BaseFilePath}{fileName}", // Caminho completo no Dropbox
                mode: WriteMode.Overwrite.Instance,  // Sobrescreve o arquivo se já existir
                body: file);  // Stream com os dados do arquivo
        }
    }
}
