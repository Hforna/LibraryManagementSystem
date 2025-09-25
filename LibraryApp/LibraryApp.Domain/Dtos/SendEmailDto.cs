using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Dtos
{
    public sealed record SendEmailDto
    {
        public SendEmailDto(string toEmail, string toUserName, string subject, string body)
        {
            ToEmail = toEmail;
            ToUserName = toUserName;
            Subject = subject;
            Body = body;
        }

        public string ToEmail { get; set; }
        public string ToUserName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
