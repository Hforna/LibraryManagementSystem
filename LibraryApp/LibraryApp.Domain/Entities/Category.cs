using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Domain.Entities
{
    // Entidade que representa uma Categoria de livros
    [Table("categorias")]
    public class Category
    {
        // Identificador único da categoria
        [Key] // Chave primária
        [Column("id")] // Mapeia para coluna id no banco
        public long Id { get; set; }

        [Column("nome")] // Nome da categoria
        [Required, MaxLength(100)] // VARCHAR(100) de no máximo 100 caracteres
        public string Name { get; set; }
        public ICollection<BookCategory> BookCategories { get; set; } // Acessa através da tabela intermediária BookCategory
    }

    // Conecta livros com categorias
    [Table("livros_categorias")]
    public class BookCategory : Entity
    {
        [Column("livro_id")] // ID do livro (chave estrangeira)
        public long BookId { get; set; }
        public Book Book { get; set; } // Permite acessar: bookCategory.Book.Title, bookCategory.Book.User, etc.

        [Column("categoria_id")] // ID da categoria (chave estrangeira)
        public long CategoryId { get; set; }
        public Category Category { get; set; } // Permite acessar: bookCategory.Category.Name
    }


}