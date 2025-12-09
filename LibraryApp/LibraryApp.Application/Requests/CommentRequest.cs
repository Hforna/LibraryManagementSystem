using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Application.Requests;

// Classe de requisição para criação de um comentário
public class CommentRequest
{
    [MaxLength(500, ErrorMessage = "Tamanho maximo de texto é de 500")] // Texto do comentário que o usuário está fazendo sobre o livro
    public string Text { get; set; }
}