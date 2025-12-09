using LibraryApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Application.Responses
{
    // Classe de resposta completa com todos os detalhes de um livro
    public class BookResponse
    {
        public long Id { get; set; } // Identificador único do livro no banco de dados
        public long UserId { get; set; } // ID do usuário que fez upload/publicou o livro
        public string Title { get; set; } // Título do livro
        public string Description { get; set; } // Descrição/sinopse do livro
        public string FileUrl { get; set; } // URL completa para download/acesso ao arquivo do livro (PDF, EPUB, etc.)
        public string CoverUrl { get; set; } // URL completa para a imagem de capa do livro
        public string AuthorName { get; set; } // Nome do autor do livro (usuário que fez upload)
        public List<string> Categories { get; set; } = []; // Lista de nomes das categorias às quais o livro pertence
        public int LikesCount { get; set; } = 0; // Número total de curtidas (likes) que o livro recebeu
        public int TotalViews { get; set; } = 0; // Número total de visualizações do livro
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow; // Data e hora em que o livro foi enviado/publicado no sistema
    }

    // Classe de resposta resumida com informações básicas do livro
    public class BookShortResponse
    {
        public long Id { get; set; }  // Identificador único do livro
        public string AuthorName { get; set; } // Nome do autor/uploader do livro
        public string CoverUrl { get; set; }  // URL da imagem de capa
        public string Title { get; set; } // Título do livro
        public int ViewsCount { get; set; } // Número de visualizações do livro
    }

    // Classe de resposta para listagem paginada de livros
    public class BooksPaginatedResponse
    {
        public List<BookShortResponse> Books { get; set; } // Lista de livros da página atual (em formato resumido)
        public int CurrentPage { get; set; } // Número da página atual sendo retornada
        public int PageSize { get; set; } // Número de itens por página
        public int TotalItems { get; set; } // Número total de livros no sistema (ou na consulta filtrada)
        public int TotalPages { get; set; } // Número total de páginas disponíveis
        public bool HasPreviousPage { get; set; } // Indica se existe uma página anterior à atual
        public bool HasNextPage { get; set; } // Indica se existe uma próxima página após a atual
    }
}