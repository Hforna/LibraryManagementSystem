using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Application.Requests;

public class CommentRequest
{
    [MaxLength(500, ErrorMessage = "Tamanho maximo de texto é de 500")]
    public string Text { get; set; }
}