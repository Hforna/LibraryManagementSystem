using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LibraryApp.Application.Requests
{
    // Classe de requisição para autenticação (login) de usuário no sistema
    public class LoginRequest
    {
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Formato invalido de e-mail")] // E-mail do usuário registrado no sistema
        public string Email { get; set; }
        [MinLength(8, ErrorMessage = "Senha deve conter 8 ou mais digitos")] // Senha do usuário
        public string Password { get; set; }
    }

    // Classe de requisição para renovar o token de acesso sem fazer login novamente
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } // Token de renovação (Refresh Token) fornecido durante o login inicial
    }
}