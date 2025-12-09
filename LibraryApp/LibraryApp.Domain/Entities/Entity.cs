using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Domain.Entities
{
    public class Entity : IEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("criado_em")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public interface IEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("criado_em")]
        public DateTime CreatedAt { get; set; }
    }
}