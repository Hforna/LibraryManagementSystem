using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Xml.Linq;

namespace LibraryApp.Domain.Entities
{
    [Table("livros")]
    public class Book
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("usuario_id")]
        public long UserId { get; set; }
        public User User { get; set; }

        [Column("titulo")]
        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Column("descricao")]
        public string Description { get; set; }

        [Column("nome_arquivo")]
        [Required]
        public string FileName { get; set; }

        [Column("nome_capa")]
        public string CoverName { get; set; }

        [Column("criado_em")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookCategory> BookCategories { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<View> Views { get; set; }
    }


    [Table("livros_infos")]
    public class BooksInfos
    {
        [Column("livro_id")]
        public long BookId { get; set; }
    }
}
