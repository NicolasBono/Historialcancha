using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HistorialCancha.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AutenticacionUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Partidos_Fecha",
                table: "Partidos");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Partidos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Dni = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    HashContrasena = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Partidos_Usuario_Fecha",
                table: "Partidos",
                columns: new[] { "UsuarioId", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Usuarios_Dni",
                table: "Usuarios",
                column: "Dni",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Partidos_Usuarios_UsuarioId",
                table: "Partidos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partidos_Usuarios_UsuarioId",
                table: "Partidos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropIndex(
                name: "UX_Partidos_Usuario_Fecha",
                table: "Partidos");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Partidos");

            migrationBuilder.CreateIndex(
                name: "UX_Partidos_Fecha",
                table: "Partidos",
                column: "Fecha",
                unique: true);
        }
    }
}
