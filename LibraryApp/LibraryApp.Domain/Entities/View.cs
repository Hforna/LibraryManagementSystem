using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Domain.Entities
{
    [Table("visualizacoes")]
    public class View : Entity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("usuario_id")]
        public long? UserId { get; set; } = null;
        public User User { get; set; }

        [Column("livro_id")]
        public long BookId { get; set; }
        public Book Book { get; set; }

        [Column("visualizado_em")]
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }

}
