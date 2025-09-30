using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application.Responses
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
