using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace LibraryApp.Application.Requests
{
    // Classe de requisição para cadastro/criação de um novo usuário no sistema
    public class CreateUserRequest
    {
        [MinLength(4, ErrorMessage = "Nome de usuario não pode ser muito curto")] // Nome de usuário escolhido pelo novo usuário
        public string UserName { get; set; }
        [MinLength(8, ErrorMessage = "Senha deve conter 8 ou mais digitos")] // Senha escolhida pelo usuário para proteger sua conta
        public string Password { get; set; }
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Formato invalido de e-mail")] // Endereço de e-mail do usuário
        public string Email { get; set; }
    }
}