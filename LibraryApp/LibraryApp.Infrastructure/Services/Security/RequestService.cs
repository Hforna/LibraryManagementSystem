using LibraryApp.Domain.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure.Services.Security
{
    /// <summary>
    /// Serviço responsável por extrair gerenciar dados da requisição HTTP atual recebida.
    /// </summary>
    public class RequestService : IRequestService
    {
        private readonly IHttpContextAccessor _httpContext;
        
        public RequestService(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        /// <summary>
        /// Obtém o token Bearer presente no cabeçalho da requisição HTTP.
        /// </summary>
        /// <returns>
        /// O token JWT extraído do cabeçalho <c>Authorization</c>, 
        /// ou <c>null</c> caso não exista.
        /// </returns>
        public string? GetBearerToken()
        {
            var token = _httpContext.HttpContext.Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(token))
                return token;

            return token["Bearer".Length..].Trim();
        }
    }
}
