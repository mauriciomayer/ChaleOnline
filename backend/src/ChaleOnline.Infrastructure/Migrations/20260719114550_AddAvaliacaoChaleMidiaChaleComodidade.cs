using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChaleOnline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAvaliacaoChaleMidiaChaleComodidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Avaliacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChaleId = table.Column<int>(type: "int", nullable: false),
                    Nota = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avaliacao_Chale_ChaleId",
                        column: x => x.ChaleId,
                        principalTable: "Chale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChaleComodidade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChaleId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChaleComodidade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChaleComodidade_Chale_ChaleId",
                        column: x => x.ChaleId,
                        principalTable: "Chale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChaleMidia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChaleId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ordem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChaleMidia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChaleMidia_Chale_ChaleId",
                        column: x => x.ChaleId,
                        principalTable: "Chale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Avaliacao",
                columns: new[] { "Id", "ChaleId", "Comentario", "Nota" },
                values: new object[,]
                {
                    { 1, 1, "Chalé impecável, exatamente como nas fotos.", 5 },
                    { 2, 1, "Ótima localização e muito aconchegante.", 5 },
                    { 3, 2, "Bom custo-benefício, voltaríamos com certeza.", 4 },
                    { 4, 2, "Estrutura boa, mas o Wi-Fi falhou algumas vezes.", 3 },
                    { 5, 3, "Vista incrível, recomendo para casais.", 5 },
                    { 6, 3, "Limpeza impecável e anfitrião muito atencioso.", 4 },
                    { 7, 4, "Chalé impecável, exatamente como nas fotos.", 5 },
                    { 8, 4, "Ótima localização e muito aconchegante.", 5 },
                    { 9, 5, "Bom custo-benefício, voltaríamos com certeza.", 4 },
                    { 10, 5, "Estrutura boa, mas o Wi-Fi falhou algumas vezes.", 3 },
                    { 11, 6, "Vista incrível, recomendo para casais.", 5 },
                    { 12, 6, "Limpeza impecável e anfitrião muito atencioso.", 4 },
                    { 13, 7, "Chalé impecável, exatamente como nas fotos.", 5 },
                    { 14, 7, "Ótima localização e muito aconchegante.", 5 },
                    { 15, 8, "Bom custo-benefício, voltaríamos com certeza.", 4 },
                    { 16, 8, "Estrutura boa, mas o Wi-Fi falhou algumas vezes.", 3 },
                    { 17, 9, "Vista incrível, recomendo para casais.", 5 },
                    { 18, 9, "Limpeza impecável e anfitrião muito atencioso.", 4 },
                    { 19, 10, "Chalé impecável, exatamente como nas fotos.", 5 },
                    { 20, 10, "Ótima localização e muito aconchegante.", 5 },
                    { 21, 11, "Bom custo-benefício, voltaríamos com certeza.", 4 },
                    { 22, 11, "Estrutura boa, mas o Wi-Fi falhou algumas vezes.", 3 },
                    { 23, 12, "Vista incrível, recomendo para casais.", 5 },
                    { 24, 12, "Limpeza impecável e anfitrião muito atencioso.", 4 }
                });

            migrationBuilder.InsertData(
                table: "ChaleComodidade",
                columns: new[] { "Id", "ChaleId", "Nome" },
                values: new object[,]
                {
                    { 1, 1, "Lareira" },
                    { 2, 1, "Deck com hidromassagem" },
                    { 3, 1, "Vista para o bosque" },
                    { 4, 2, "Deck com hidromassagem" },
                    { 5, 2, "Vista para o bosque" },
                    { 6, 2, "Wi-Fi" },
                    { 7, 3, "Vista para o bosque" },
                    { 8, 3, "Wi-Fi" },
                    { 9, 3, "Estacionamento privativo" },
                    { 10, 4, "Wi-Fi" },
                    { 11, 4, "Estacionamento privativo" },
                    { 12, 4, "Churrasqueira" },
                    { 13, 5, "Estacionamento privativo" },
                    { 14, 5, "Churrasqueira" },
                    { 15, 5, "Lareira" },
                    { 16, 6, "Churrasqueira" },
                    { 17, 6, "Lareira" },
                    { 18, 6, "Deck com hidromassagem" },
                    { 19, 7, "Lareira" },
                    { 20, 7, "Deck com hidromassagem" },
                    { 21, 7, "Vista para o bosque" },
                    { 22, 8, "Deck com hidromassagem" },
                    { 23, 8, "Vista para o bosque" },
                    { 24, 8, "Wi-Fi" },
                    { 25, 9, "Vista para o bosque" },
                    { 26, 9, "Wi-Fi" },
                    { 27, 9, "Estacionamento privativo" },
                    { 28, 10, "Wi-Fi" },
                    { 29, 10, "Estacionamento privativo" },
                    { 30, 10, "Churrasqueira" },
                    { 31, 11, "Estacionamento privativo" },
                    { 32, 11, "Churrasqueira" },
                    { 33, 11, "Lareira" },
                    { 34, 12, "Churrasqueira" },
                    { 35, 12, "Lareira" },
                    { 36, 12, "Deck com hidromassagem" }
                });

            migrationBuilder.InsertData(
                table: "ChaleMidia",
                columns: new[] { "Id", "ChaleId", "Ordem", "Tipo", "Url" },
                values: new object[,]
                {
                    { 1, 1, 0, "Foto", "/media/chales/tipo-a.svg" },
                    { 2, 1, 1, "Foto", "/media/chales/tipo-a-interior.svg" },
                    { 3, 2, 0, "Foto", "/media/chales/tipo-a.svg" },
                    { 4, 2, 1, "Foto", "/media/chales/tipo-a-interior.svg" },
                    { 5, 3, 0, "Foto", "/media/chales/tipo-a.svg" },
                    { 6, 3, 1, "Foto", "/media/chales/tipo-a-interior.svg" },
                    { 7, 4, 0, "Foto", "/media/chales/tipo-a.svg" },
                    { 8, 4, 1, "Foto", "/media/chales/tipo-a-interior.svg" },
                    { 9, 5, 0, "Foto", "/media/chales/tipo-a.svg" },
                    { 10, 5, 1, "Foto", "/media/chales/tipo-a-interior.svg" },
                    { 11, 6, 0, "Foto", "/media/chales/tipo-a.svg" },
                    { 12, 6, 1, "Foto", "/media/chales/tipo-a-interior.svg" },
                    { 13, 7, 0, "Foto", "/media/chales/tipo-b.svg" },
                    { 14, 7, 1, "Foto", "/media/chales/tipo-b-interior.svg" },
                    { 15, 8, 0, "Foto", "/media/chales/tipo-b.svg" },
                    { 16, 8, 1, "Foto", "/media/chales/tipo-b-interior.svg" },
                    { 17, 9, 0, "Foto", "/media/chales/tipo-b.svg" },
                    { 18, 9, 1, "Foto", "/media/chales/tipo-b-interior.svg" },
                    { 19, 10, 0, "Foto", "/media/chales/tipo-b.svg" },
                    { 20, 10, 1, "Foto", "/media/chales/tipo-b-interior.svg" },
                    { 21, 11, 0, "Foto", "/media/chales/tipo-c.svg" },
                    { 22, 11, 1, "Foto", "/media/chales/tipo-c-interior.svg" },
                    { 23, 12, 0, "Foto", "/media/chales/tipo-c.svg" },
                    { 24, 12, 1, "Foto", "/media/chales/tipo-c-interior.svg" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacao_ChaleId",
                table: "Avaliacao",
                column: "ChaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ChaleComodidade_ChaleId",
                table: "ChaleComodidade",
                column: "ChaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ChaleMidia_ChaleId",
                table: "ChaleMidia",
                column: "ChaleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avaliacao");

            migrationBuilder.DropTable(
                name: "ChaleComodidade");

            migrationBuilder.DropTable(
                name: "ChaleMidia");
        }
    }
}
