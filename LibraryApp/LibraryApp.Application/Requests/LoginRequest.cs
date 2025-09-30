using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LibraryApp.Application.Requests
{
    public class LoginRequest
    {
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Formato invalido de e-mail")]
        public string Email { get; set; }
        [MinLength(8, ErrorMessage = "Senha deve conter 8 ou mais digitos")]
        public string Password { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }
}
