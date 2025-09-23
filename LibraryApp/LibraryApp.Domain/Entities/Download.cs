using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Domain.Entities
{
    [Table("downloads")]
    public class Download
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("usuario_id")]
        public long UserId { get; set; }
        public User User { get; set; }

        [Column("livro_id")]
        public long BookId { get; set; }
        public Book Book { get; set; }

        [Column("baixado_em")]
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }

}
