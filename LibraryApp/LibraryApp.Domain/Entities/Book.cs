using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Xml.Linq;

namespace LibraryApp.Domain.Entities
{
    // Entidade que representa um Livro no sistema
    [Table("livros")]
    public class Book : Entity
    {
        // Identificador único do livro
        [Key] // Define como chave primária
        [Column("id")] // Mapeia para coluna id no banco
        public long Id { get; set; }

        [Column("usuario_id")] // ID do usuário que publicou/enviou o livro
        public long UserId { get; set; }
        public User User { get; set; } // Propriedade de navegação para o usuário
        
        [Column("titulo")] // Título do livro
        [Required, MaxLength(255)] // VARCHAR(255) com limite de caracteres de 255
        public string Title { get; set; }

        [Column("descricao")] // Descrição/sinopse do livro
        public string Description { get; set; }

        [Column("nome_arquivo")] // Nome do arquivo PDF do livro armazenado
        [Required] // Obrigatório que todo livro precisa ter um arquivo
        public string FileName { get; set; }

        [Column("nome_capa")] // Nome do arquivo da capa do livro
        public string CoverName { get; set; }

        [Column("criado_em")] // Data e hora de criação do registro
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 

        public ICollection<BookCategory> BookCategories { get; set; } // Usa tabela intermediária BookCategory
        public ICollection<Like> Likes { get; set; } // Curtidas que este livro recebeu
        public ICollection<Comment> Comments { get; set; } // Comentários feitos neste livro
        public ICollection<View> Views { get; set; } // Visualizações deste livro
    }

    // Entidade para armazenar informações dos livros
    [Table("livros_infos")]
    public class BooksInfos
    {
        [Column("livro_id")] // ID do livro relacionado
        public long BookId { get; set; }
    }
}