using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application.Responses
{
    public class UserResponse
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
