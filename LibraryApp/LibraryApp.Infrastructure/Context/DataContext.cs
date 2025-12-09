using LibraryApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure.Context
{
    // Contexto principal do Entity Framework que gerencia as entidades e o Identity
    public class DataContext : IdentityDbContext<User, Role, long>
    {
        // Construtor que recebe as configurações do contexto via injeção de dependência
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        // Tabela de livros
        public DbSet<Book> Books { get; set; }
        // Tabela de categorias
        public DbSet<Category> Categories { get; set; }
        // Tabela de relacionamento entre livros e categorias (N:N)
        public DbSet<BookCategory> BookCategories { get; set; }
        // Tabela de comentários
        public DbSet<Comment> Comments { get; set; }
        // Tabela de downloads
        public DbSet<Download> Downloads { get; set; }
        // Tabela de curtidas
        public DbSet<Like> Likes { get; set; }
        // Tabela de visualizações
        public DbSet<View> Views { get; set; }

        // Configura o mapeamento das entidades para o banco de dados
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Configurações padrão do Identity
            base.OnModelCreating(builder);

            // Renomeação das tabelas do Identity
            builder.Entity<User>().ToTable("usuarios");
            builder.Entity<IdentityRole<long>>().ToTable("papeis");
            builder.Entity<IdentityUserRole<long>>().ToTable("usuarios_papeis");
            builder.Entity<IdentityUserClaim<long>>().ToTable("usuarios_reivindicacoes");
            builder.Entity<IdentityUserLogin<long>>().ToTable("usuarios_logins");
            builder.Entity<IdentityRoleClaim<long>>().ToTable("papeis_reivindicacoes");
            builder.Entity<IdentityUserToken<long>>().ToTable("usuarios_tokens");

            // Mapeamento da entidade Livro
            builder.Entity<Book>(entity =>
            {
                entity.ToTable("livros");
                entity.HasKey(b => b.Id);

                entity.HasOne(b => b.User)
                      .WithMany(u => u.Books)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Mapeamento da entidade Categoria
            builder.Entity<Category>(entity =>
            {
                entity.ToTable("categorias");
                entity.HasKey(c => c.Id);
            });

            // Mapeamento do relacionamento Livro x Categoria
            builder.Entity<BookCategory>(entity =>
            {
                entity.ToTable("livros_categorias");
                entity.HasKey(bc => new { bc.BookId, bc.CategoryId });

                entity.HasOne(bc => bc.Book)
                      .WithMany(b => b.BookCategories)
                      .HasForeignKey(bc => bc.BookId);

                entity.HasOne(bc => bc.Category)
                      .WithMany(c => c.BookCategories)
                      .HasForeignKey(bc => bc.CategoryId);
            });

            // Mapeamento da entidade Curtida
            builder.Entity<Like>(entity =>
            {
                entity.ToTable("curtidas");
                entity.HasKey(l => new { l.UserId, l.BookId });

                entity.HasOne(l => l.User)
                      .WithMany(u => u.Likes)
                      .HasForeignKey(l => l.UserId);

                entity.HasOne(l => l.Book)
                      .WithMany(b => b.Likes)
                      .HasForeignKey(l => l.BookId);
            });

            // Mapeamento da entidade Comentário
            builder.Entity<Comment>(entity =>
            {
                entity.ToTable("comentarios");
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.UserId);

                entity.HasOne(c => c.Book)
                      .WithMany(b => b.Comments)
                      .HasForeignKey(c => c.BookId);
            });

            // Mapeamento da entidade Visualização
            builder.Entity<View>(entity =>
            {
                entity.ToTable("visualizacoes");
                entity.HasKey(v => v.Id);

                entity.HasOne(v => v.User)
                      .WithMany(u => u.Views)
                      .HasForeignKey(v => v.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(v => v.Book)
                      .WithMany(b => b.Views)
                      .HasForeignKey(v => v.BookId);
            });

            // Mapeamento da entidade Download
            builder.Entity<Download>(entity =>
            {
                entity.ToTable("downloads");
                entity.HasKey(d => d.Id);

                entity.HasOne(d => d.User)
                      .WithMany(u => u.Downloads)
                      .HasForeignKey(d => d.UserId);

                entity.HasOne(d => d.Book)
                      .WithMany()
                      .HasForeignKey(d => d.BookId);
            });
        }
    }

    // Fábrica do contexto usada em tempo de design (ex: migrations)
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        // Cria o contexto para execução de migrations
        public DataContext CreateDbContext(string[] args)
        {
            // Builder das opções do DbContext
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            // Leitura das configurações do arquivo appsettings
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), @"../LibraryApp.Api/appsettings.Development.json"), optional: false, reloadOnChange: true)
                .Build();

            // Configuração da conexão com PostgreSQL
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("postgres"));

            // Retorna o contexto configurado
            return new DataContext(optionsBuilder.Options);
        }
    }
}
