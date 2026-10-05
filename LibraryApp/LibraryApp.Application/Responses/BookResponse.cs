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
        public string Status { get; set; } // Situação do livro: PendingAnalysis, Published ou Rejected
        public string? RejectionReason { get; set; } // Motivo informado pelo administrador quando o livro é reprovado
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

    // Classe de resposta resumida de um livro enviado pelo próprio estudante, incluindo a situação da análise
    public class MyBookResponse
    {
        public long Id { get; set; } // Identificador único do livro
        public string Title { get; set; } // Título do livro
        public string CoverUrl { get; set; } // URL da imagem de capa
        public string Status { get; set; } // Situação do livro: PendingAnalysis, Published ou Rejected
        public string? RejectionReason { get; set; } // Motivo informado pelo administrador quando o livro é reprovado
        public int ViewsCount { get; set; } // Número de visualizações do livro (relevante apenas quando publicado)
        public int LikesCount { get; set; } // Número de curtidas do livro (relevante apenas quando publicado)
        public DateTime UploadedAt { get; set; } // Data e hora em que o livro foi enviado
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

    // Classe de resposta para listagem paginada dos livros enviados pelo estudante autenticado
    public class MyBooksPaginatedResponse
    {
        public List<MyBookResponse> Books { get; set; } // Lista de livros da página atual
        public int CurrentPage { get; set; } // Número da página atual sendo retornada
        public int PageSize { get; set; } // Número de itens por página
        public int TotalItems { get; set; } // Número total de livros enviados pelo estudante
        public int TotalPages { get; set; } // Número total de páginas disponíveis
        public bool HasPreviousPage { get; set; } // Indica se existe uma página anterior à atual
        public bool HasNextPage { get; set; } // Indica se existe uma próxima página após a atual
    }

    // Classe de resposta para listagem paginada de livros no painel do administrador (com detalhes completos)
    public class AdminBooksPaginatedResponse
    {
        public List<BookResponse> Books { get; set; } // Lista de livros da página atual
        public int CurrentPage { get; set; } // Número da página atual sendo retornada
        public int PageSize { get; set; } // Número de itens por página
        public int TotalItems { get; set; } // Número total de livros na consulta
        public int TotalPages { get; set; } // Número total de páginas disponíveis
        public bool HasPreviousPage { get; set; } // Indica se existe uma página anterior à atual
        public bool HasNextPage { get; set; } // Indica se existe uma próxima página após a atual
    }

    // Classe de resposta com os dados exibidos no dashboard do administrador
    public class AdminDashboardResponse
    {
        public int TotalBooks { get; set; } // Total de livros cadastrados no sistema
        public int PendingAnalysisCount { get; set; } // Livros aguardando análise
        public int PublishedCount { get; set; } // Livros publicados
        public int RejectedCount { get; set; } // Livros reprovados
        public int TotalStudents { get; set; } // Total de usuários com papel de estudante
        public int TotalAdmins { get; set; } // Total de usuários com papel de administrador
        public int TotalDownloads { get; set; } // Total de downloads realizados na plataforma
    }
}