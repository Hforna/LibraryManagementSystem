using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Domain.Entities
{
    [Table("categorias")]
    public class Category
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("nome")]
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public ICollection<BookCategory> BookCategories { get; set; }
    }

    [Table("livros_categorias")]
    public class BookCategory
    {
        [Column("livro_id")]
        public long BookId { get; set; }
        public Book Book { get; set; }

        [Column("categoria_id")]
        public long CategoryId { get; set; }
        public Category Category { get; set; }
    }


}
