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
    public class DataContext : IdentityDbContext<User, Role, long>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Download> Downloads { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<View> Views { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("usuarios");
            builder.Entity<IdentityRole<long>>().ToTable("papeis");
            builder.Entity<IdentityUserRole<long>>().ToTable("usuarios_papeis");
            builder.Entity<IdentityUserClaim<long>>().ToTable("usuarios_reivindicacoes");
            builder.Entity<IdentityUserLogin<long>>().ToTable("usuarios_logins");
            builder.Entity<IdentityRoleClaim<long>>().ToTable("papeis_reivindicacoes");
            builder.Entity<IdentityUserToken<long>>().ToTable("usuarios_tokens");

            builder.Entity<Book>(entity =>
            {
                entity.ToTable("livros");
                entity.HasKey(b => b.Id);

                entity.HasOne(b => b.User)
                      .WithMany(u => u.Books)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Category>(entity =>
            {
                entity.ToTable("categorias");
                entity.HasKey(c => c.Id);
            });

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

    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), @"../LibraryApp.Api/appsettings.Development.json"), optional: false, reloadOnChange: true)
                .Build();

            optionsBuilder.UseNpgsql(configuration.GetConnectionString("postgres"));

            return new DataContext(optionsBuilder.Options);
        }
    }
}
