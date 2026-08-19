using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Rival = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Torneo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Condicion = table.Column<byte>(type: "smallint", nullable: false),
                    Estadio = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    GolesAFavor = table.Column<int>(type: "integer", nullable: false),
                    GolesEnContra = table.Column<int>(type: "integer", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidos", x => x.Id);
                    table.CheckConstraint("CK_Partidos_Condicion", "\"Condicion\" IN (0, 1)");
                    table.CheckConstraint("CK_Partidos_GolesAFavor", "\"GolesAFavor\" >= 0");
                    table.CheckConstraint("CK_Partidos_GolesEnContra", "\"GolesEnContra\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Vivencias",
                columns: table => new
                {
                    PartidoId = table.Column<int>(type: "integer", nullable: false),
                    Modalidad = table.Column<byte>(type: "smallint", nullable: false),
                    Sector = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ConQuien = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Nota = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vivencias", x => x.PartidoId);
                    table.CheckConstraint("CK_Vivencias_Modalidad", "\"Modalidad\" BETWEEN 0 AND 4");
                    table.CheckConstraint("CK_Vivencias_Nota", "\"Nota\" IS NULL OR \"Nota\" BETWEEN 1 AND 10");
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
