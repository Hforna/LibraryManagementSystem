using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Domain.Entities
{
    // Registra quando um usuário baixa um livro
    [Table("downloads")]
    public class Download : Entity
    {
        [Key] // Chave primária
        [Column("id")] // Mapeia para coluna id no banco
        public long Id { get; set; }

        [Column("usuario_id")] // ID do usuário que baixou
        public long UserId { get; set; }
        public User User { get; set; } // Acesso aos dados do usuário

        [Column("livro_id")] // ID do livro que foi baixado
        public long BookId { get; set; }
        public Book Book { get; set; } // Acesso aos dados do livro

        [Column("baixado_em")] // Data e hora do download
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }

}