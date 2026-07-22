using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChaleOnline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Chale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroQuartos = table.Column<int>(type: "int", nullable: false),
                    NumeroBanheiros = table.Column<int>(type: "int", nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    FotoUrl = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chale", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Chale",
                columns: new[] { "Id", "FotoUrl", "Nome", "NumeroBanheiros", "NumeroQuartos", "Preco", "Tipo" },
                values: new object[,]
                {
                    { 1, "/media/chales/tipo-a.svg", "Pinheiro Bravo", 1, 2, 420m, "A" },
                    { 2, "/media/chales/tipo-a.svg", "Trilha da Neblina", 1, 2, 435m, "A" },
                    { 3, "/media/chales/tipo-a.svg", "Cabana do Vale", 1, 2, 410m, "A" },
                    { 4, "/media/chales/tipo-a.svg", "Recanto da Araucária", 1, 2, 445m, "A" },
                    { 5, "/media/chales/tipo-a.svg", "Clareira Dourada", 1, 2, 425m, "A" },
                    { 6, "/media/chales/tipo-a.svg", "Refúgio do Riacho", 1, 2, 430m, "A" },
                    { 7, "/media/chales/tipo-b.svg", "Vista da Serra", 1, 3, 620m, "B" },
                    { 8, "/media/chales/tipo-b.svg", "Morada Alpina", 1, 3, 635m, "B" },
                    { 9, "/media/chales/tipo-b.svg", "Chalé do Bosque", 1, 3, 610m, "B" },
                    { 10, "/media/chales/tipo-b.svg", "Encosta Verde", 1, 3, 645m, "B" },
                    { 11, "/media/chales/tipo-c.svg", "Grande Refúgio", 2, 4, 890m, "C" },
                    { 12, "/media/chales/tipo-c.svg", "Casa da Montanha", 2, 4, 910m, "C" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chale");
        }
    }
}
