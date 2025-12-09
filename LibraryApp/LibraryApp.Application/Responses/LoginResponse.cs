using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application.Responses
{
    // Classe de resposta retornada após autenticação bem-sucedida do usuário
    public class LoginResponse
    {
        public string AccessToken { get; set; } // Token de acesso (Access Token) usado para autenticar requisições à API
        public string RefreshToken { get; set; } // Token de renovação (Refresh Token) usado para obter novos Access Tokens
        public DateTime RefreshTokenExpiration { get; set; } // Data e hora de expiração do Refresh Token em UTC
    }
}