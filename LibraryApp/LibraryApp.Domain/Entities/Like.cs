using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Domain.Entities
{
    [Table("curtidas")]
    public class Like : Entity
    {
        [Column("usuario_id")]
        public long UserId { get; set; }
        public User User { get; set; }

        [Column("livro_id")]
        public long BookId { get; set; }
        public Book Book { get; set; }

        [Column("criado_em")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
