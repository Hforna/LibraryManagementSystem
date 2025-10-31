using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Domain.Entities
{
    [Table("comentarios")]
    public class Comment : Entity
    {
        public Comment(long userId, long bookId, string content)
        {
            UserId = userId;
            BookId = bookId;
            Content = content;
        }

        public Comment() { }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("usuario_id")]
        public long UserId { get; set; }
        public User User { get; set; }

        [Column("livro_id")]
        public long BookId { get; set; }
        public Book Book { get; set; }

        [Column("conteudo")]
        [Required]
        public string Content { get; set; }

        [Column("criado_em")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
