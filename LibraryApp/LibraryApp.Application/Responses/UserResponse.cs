using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application.Responses
{
    // Classe de resposta representando informações básicas de um usuário
    public class UserResponse
    {
        public long Id { get; set; } // Identificador único do usuário no banco de dados
        public string UserName { get; set; } // Nome de usuário único escolhido pelo usuário
        public string Email { get; set; } // Endereço de e-mail do usuário
        public DateTime CreatedAt { get; set; } // Data e hora em que a conta do usuário foi criada
    }
}