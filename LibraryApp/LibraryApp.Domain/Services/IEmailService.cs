using LibraryApp.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Services
{
    public interface IEmailService
    {
        public Task SendEmail(SendEmailDto dto);
    }
}
