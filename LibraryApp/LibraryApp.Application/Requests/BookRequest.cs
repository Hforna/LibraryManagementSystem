using LibraryApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Application.Requests
{
    public class BookRequest
    {
        [MaxLength(100, ErrorMessage = "Tamanho de texto de titulo é 100")]
        public string Title { get; set; }
        [MaxLength(500, ErrorMessage = "Tamanho maximo de descrição é de 500")]
        public string Description { get; set; }
        public IFormFile File { get; set; }
        public IFormFile? Cover { get; set; }
        public List<long> CategoriesIds { get; set; }
    }
    public class UpdateBookRequest
    {
        [MaxLength(100, ErrorMessage = "Tamanho de texto de titulo é 100")]
        public string? Title { get; set; }
        [MaxLength(500, ErrorMessage = "Tamanho maximo de descrição é de 500")]
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
        public IFormFile? Cover { get; set; }
        public List<long>? CategoriesIds { get; set; }
    }
}
