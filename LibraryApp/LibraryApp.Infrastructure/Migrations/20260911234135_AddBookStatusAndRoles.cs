using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookStatusAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "motivo_reprovacao",
                table: "livros",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "revisado_em",
                table: "livros",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "revisado_por",
                table: "livros",
                type: "bigint",
                nullable: true);

            // Livros criados antes desta migração já eram públicos, então continuam publicados (nao se pode reprovar o passado)
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "livros",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: nameof(LibraryApp.Domain.Entities.BookStatus.Published));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "motivo_reprovacao",
                table: "livros");

            migrationBuilder.DropColumn(
                name: "revisado_em",
                table: "livros");

            migrationBuilder.DropColumn(
                name: "revisado_por",
                table: "livros");

            migrationBuilder.DropColumn(
                name: "status",
                table: "livros");
        }
    }
}
