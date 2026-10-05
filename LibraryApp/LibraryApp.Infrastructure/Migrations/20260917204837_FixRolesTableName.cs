using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRolesTableName : Migration
    {
        // A tabela "papeis" era um mapeamento morto (configurado por engano em cima de IdentityRole<long> em vez
        // de Role), sempre vazio. Os papéis (Student/Admin) sempre viveram na tabela real "AspNetRoles". Esta
        // migração renomeia "AspNetRoles" para "papeis" (preservando os dados) e descarta a tabela órfã vazia.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_papeis_reivindicacoes_AspNetRoles_RoleId",
                table: "papeis_reivindicacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_papeis_AspNetRoles_RoleId",
                table: "usuarios_papeis");

            migrationBuilder.DropTable(
                name: "papeis");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "papeis");

            migrationBuilder.Sql(
                "ALTER TABLE papeis RENAME CONSTRAINT \"PK_AspNetRoles\" TO \"PK_papeis\";");

            migrationBuilder.AddForeignKey(
                name: "FK_papeis_reivindicacoes_papeis_RoleId",
                table: "papeis_reivindicacoes",
                column: "RoleId",
                principalTable: "papeis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_papeis_papeis_RoleId",
                table: "usuarios_papeis",
                column: "RoleId",
                principalTable: "papeis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_papeis_reivindicacoes_papeis_RoleId",
                table: "papeis_reivindicacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_papeis_papeis_RoleId",
                table: "usuarios_papeis");

            migrationBuilder.Sql(
                "ALTER TABLE papeis RENAME CONSTRAINT \"PK_papeis\" TO \"PK_AspNetRoles\";");

            migrationBuilder.RenameTable(
                name: "papeis",
                newName: "AspNetRoles");

            migrationBuilder.CreateTable(
                name: "papeis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    NormalizedName = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_papeis", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_papeis_reivindicacoes_AspNetRoles_RoleId",
                table: "papeis_reivindicacoes",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_papeis_AspNetRoles_RoleId",
                table: "usuarios_papeis",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
