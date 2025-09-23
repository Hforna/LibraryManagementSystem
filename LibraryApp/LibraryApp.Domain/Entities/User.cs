using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;


namespace LibraryApp.Domain.Entities
{
    public class User : IdentityUser<long>
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("nome")]
        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Column("email")]
        [Required, MaxLength(150)]
        public string Email { get; set; }
        [Column("senha_hash")]
        [Required]
        public override string? PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }

        [Column("criado_em")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Book> Books { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<View> Views { get; set; }
        public ICollection<Download> Downloads { get; set; }
    }

    public class Role : IdentityRole<long>
    {
        public Role(string name) : base(name) { }
    }
}
