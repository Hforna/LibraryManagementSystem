using LibraryApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Application.Requests
{
    // Classe de requisição para criação de um novo livro
    public class BookRequest
    {
        [MaxLength(100, ErrorMessage = "Tamanho de texto de titulo é 100")] // Título do livro
        public string Title { get; set; } 
        [MaxLength(500, ErrorMessage = "Tamanho maximo de descrição é de 500")] // Descrição/sinopse do livro
        public string Description { get; set; } 
        public IFormFile File { get; set; } // Arquivo do livro (PDF, EPUB, etc.)
        public IFormFile? Cover { get; set; } // Imagem de capa do livro
        public List<long> CategoriesIds { get; set; } // Lista de IDs das categorias às quais o livro pertence
    }

    // Classe de requisição para atualização de um livro existente
    public class UpdateBookRequest
    {
        [MaxLength(100, ErrorMessage = "Tamanho de texto de titulo é 100")] // Novo título do livro (opcional)
        public string? Title { get; set; }
        [MaxLength(500, ErrorMessage = "Tamanho maximo de descrição é de 500")] // Nova descrição do livro (opcional)
        public string? Description { get; set; } 
        public IFormFile? File { get; set; } // Novo arquivo do livro (opcional)
        public IFormFile? Cover { get; set; } // Nova imagem de capa (opcional) 
        public List<long>? CategoriesIds { get; set; } // Nova lista de categorias (opcional)
    }
}