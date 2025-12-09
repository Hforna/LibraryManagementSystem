using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Dtos
{
    // DTO (Data Transfer Object) para transferência de dados de email
    public sealed record SendEmailDto
    {
        // Construtor que força a inicialização de todas as propriedades
        public SendEmailDto(string toEmail, string toUserName, string subject, string body)
        {
            ToEmail = toEmail;
            ToUserName = toUserName;
            Subject = subject;
            Body = body;
        }
        
        public string ToEmail { get; set; } // Email do destinatário    
        public string ToUserName { get; set; } // Nome do destinatário
        public string Subject { get; set; } // Assunto do email     
        public string Body { get; set; } // Corpo do email (conteúdo)
    }
}