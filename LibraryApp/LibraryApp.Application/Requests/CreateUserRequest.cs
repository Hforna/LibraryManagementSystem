using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace LibraryApp.Application.Requests
{
    public class CreateUserRequest
    {
        [MinLength(4, ErrorMessage = "Nome de usuario não pode ser muito curto")]
        public string UserName { get; set; }
        [MinLength(8, ErrorMessage = "Senha deve conter 8 ou mais digitos")]
        public string Password { get; set; }
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Formato invalido de e-mail")]
        public string Email { get; set; }
    }
}
