namespace LibraryApp.Application.Responses;

// Classe de resposta representando um comentário individual
public class CommentResponse
{
    public long Id { get; set; } // Identificador único do comentário no banco de dados
    public long UserId { get; set; } // ID do usuário que escreveu o comentário
    public long BookId { get; set; } // ID do livro ao qual o comentário pertence
    public string Content { get; set; } // Conteúdo textual do comentário escrito pelo usuário
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow; // Data e hora em que o comentário foi publicado
}

// Classe de resposta para listagem paginada de comentários
public class CommentsPaginatedResponse
{
    public List<CommentResponse> Comments { get; set; } // Lista de comentários da página atual
    public int CurrentPage { get; set; } // Número da página atual sendo retornada
    public int PageSize { get; set; } // Quantidade de comentários por página
    public int TotalItems { get; set; } // Número total de comentários do livro (ou na consulta filtrada)
    public int TotalPages { get; set; } // Número total de páginas disponíveis
    public bool HasPreviousPage { get; set; } // Indica se existe uma página anterior à atual
    public bool HasNextPage { get; set; } // Indica se existe uma próxima página após a atual
}