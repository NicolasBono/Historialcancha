using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistorialCancha.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EsquemaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Rival = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Torneo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Condicion = table.Column<byte>(type: "tinyint", nullable: false),
                    Estadio = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    GolesAFavor = table.Column<int>(type: "int", nullable: false),
                    GolesEnContra = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidos", x => x.Id);
                    table.CheckConstraint("CK_Partidos_Condicion", "[Condicion] IN (0, 1)");
                    table.CheckConstraint("CK_Partidos_GolesAFavor", "[GolesAFavor] >= 0");
                    table.CheckConstraint("CK_Partidos_GolesEnContra", "[GolesEnContra] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Vivencias",
                columns: table => new
                {
                    PartidoId = table.Column<int>(type: "int", nullable: false),
                    Modalidad = table.Column<byte>(type: "tinyint", nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ConQuien = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Nota = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vivencias", x => x.PartidoId);
                    table.CheckConstraint("CK_Vivencias_Modalidad", "[Modalidad] BETWEEN 0 AND 4");
                    table.CheckConstraint("CK_Vivencias_Nota", "[Nota] IS NULL OR [Nota] BETWEEN 1 AND 10");
                    table.ForeignKey(
                        name: "FK_Vivencias_Partidos_PartidoId",
                        column: x => x.PartidoId,
                        principalTable: "Partidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_Rival",
                table: "Partidos",
                column: "Rival");

            migrationBuilder.CreateIndex(
                name: "UX_Partidos_Fecha",
                table: "Partidos",
                column: "Fecha",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vivencias");

            migrationBuilder.DropTable(
                name: "Partidos");
        }
    }
}
