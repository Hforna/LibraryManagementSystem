using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Domain.Entities
{
    // Entidade que representa um comentário feito por um usuário em um livro
    [Table("comentarios")]
    public class Comment : Entity
    {
        // Construtor com parâmetros para criar um comentário válido
        public Comment(long userId, long bookId, string content)
        {
            UserId = userId; // ID do usuário que está comentando
            BookId = bookId; // ID do livro sendo comentado
            Content = content; // Texto do comentário
        }

        // Construtor dos comentários
        public Comment() { }

        [Key] // Chave primária
        [Column("id")] // Mapeia para coluna id no banco
        public long Id { get; set; }

        [Column("usuario_id")] // ID do usuário que escreveu o comentário
        public long UserId { get; set; }
        public User User { get; set; } // Propriedade de navegação para o usuário autor do comentário

        [Column("livro_id")] // ID do livro que está sendo comentado
        public long BookId { get; set; }
        public Book Book { get; set; } // Propriedade de navegação para o livro comentado

        [Column("conteudo")]  // Conteúdo textual do comentário
        [Required] // Obrigatório no banco de dados
        public string Content { get; set; }

        [Column("criado_em")] // Data e hora em que o comentário foi criado/publicado
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}