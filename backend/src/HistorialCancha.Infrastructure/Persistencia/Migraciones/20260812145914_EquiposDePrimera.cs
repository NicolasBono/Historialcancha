using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HistorialCancha.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EquiposDePrimera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Equipos",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Aldosivi" },
                    { 2, true, "Argentinos Juniors" },
                    { 3, true, "Atlético Tucumán" },
                    { 4, true, "Banfield" },
                    { 5, true, "Barracas Central" },
                    { 6, true, "Belgrano" },
                    { 7, true, "Boca Juniors" },
                    { 8, true, "Central Córdoba (SdE)" },
                    { 9, true, "Defensa y Justicia" },
                    { 10, true, "Deportivo Riestra" },
                    { 11, true, "Estudiantes (LP)" },
                    { 12, true, "Gimnasia y Esgrima (LP)" },
                    { 13, true, "Godoy Cruz" },
                    { 14, true, "Huracán" },
                    { 15, true, "Independiente" },
                    { 16, true, "Independiente Rivadavia" },
                    { 17, true, "Instituto" },
                    { 18, true, "Lanús" },
                    { 19, true, "Newell's Old Boys" },
                    { 20, true, "Platense" },
                    { 21, true, "Racing Club" },
                    { 22, true, "River Plate" },
                    { 23, true, "Rosario Central" },
                    { 24, true, "San Lorenzo" },
                    { 25, true, "San Martín (SJ)" },
                    { 26, true, "Sarmiento" },
                    { 27, true, "Talleres" },
                    { 28, true, "Tigre" },
                    { 29, true, "Unión" },
                    { 30, true, "Vélez Sarsfield" }
                });

            migrationBuilder.CreateIndex(
                name: "UX_Equipos_Nombre",
                table: "Equipos",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Equipos");
        }
    }
}
