using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChaleOnline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarMidiaComFotosReais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 1,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 2,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 3,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 4,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 5,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 6,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 7,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 8,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 9,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 10,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 11,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 12,
                column: "FotoUrl",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 1,
                column: "Url",
                value: "/media/pousada/deck-vista.jpg");

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 2,
                column: "Url",
                value: "/media/pousada/fachada-entrada.jpg");

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 1, 2, "/media/pousada/sala-lareira.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 1, 3, "/media/pousada/quarto-casal.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 1, 4, "/media/pousada/quarto-duplo.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ChaleId", "Ordem", "Tipo", "Url" },
                values: new object[] { 1, 5, "Video", "/media/pousada/tour-virtual.mp4" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 2, "/media/pousada/deck-vista.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 2, "/media/pousada/fachada-entrada.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 2, 2, "/media/pousada/sala-lareira.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 2, 3, "/media/pousada/quarto-casal.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 2, 4, "/media/pousada/quarto-duplo.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "ChaleId", "Ordem", "Tipo", "Url" },
                values: new object[] { 2, 5, "Video", "/media/pousada/tour-virtual.mp4" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 3, "/media/pousada/deck-vista.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 3, "/media/pousada/fachada-entrada.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 3, 2, "/media/pousada/sala-lareira.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 3, 3, "/media/pousada/quarto-casal.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 3, 4, "/media/pousada/quarto-duplo.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "ChaleId", "Ordem", "Tipo", "Url" },
                values: new object[] { 3, 5, "Video", "/media/pousada/tour-virtual.mp4" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 4, "/media/pousada/deck-vista.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 4, "/media/pousada/fachada-entrada.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 4, 2, "/media/pousada/sala-lareira.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 4, 3, "/media/pousada/quarto-casal.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 4, 4, "/media/pousada/quarto-duplo.jpg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "ChaleId", "Ordem", "Tipo", "Url" },
                values: new object[] { 4, 5, "Video", "/media/pousada/tour-virtual.mp4" });

            migrationBuilder.InsertData(
                table: "ChaleMidia",
                columns: new[] { "Id", "ChaleId", "Ordem", "Tipo", "Url" },
                values: new object[,]
                {
                    { 25, 5, 0, "Foto", "/media/pousada/deck-vista.jpg" },
                    { 26, 5, 1, "Foto", "/media/pousada/fachada-entrada.jpg" },
                    { 27, 5, 2, "Foto", "/media/pousada/sala-lareira.jpg" },
                    { 28, 5, 3, "Foto", "/media/pousada/quarto-casal.jpg" },
                    { 29, 5, 4, "Foto", "/media/pousada/quarto-duplo.jpg" },
                    { 30, 5, 5, "Video", "/media/pousada/tour-virtual.mp4" },
                    { 31, 6, 0, "Foto", "/media/pousada/deck-vista.jpg" },
                    { 32, 6, 1, "Foto", "/media/pousada/fachada-entrada.jpg" },
                    { 33, 6, 2, "Foto", "/media/pousada/sala-lareira.jpg" },
                    { 34, 6, 3, "Foto", "/media/pousada/quarto-casal.jpg" },
                    { 35, 6, 4, "Foto", "/media/pousada/quarto-duplo.jpg" },
                    { 36, 6, 5, "Video", "/media/pousada/tour-virtual.mp4" },
                    { 37, 7, 0, "Foto", "/media/pousada/deck-vista.jpg" },
                    { 38, 7, 1, "Foto", "/media/pousada/fachada-entrada.jpg" },
                    { 39, 7, 2, "Foto", "/media/pousada/sala-lareira.jpg" },
                    { 40, 7, 3, "Foto", "/media/pousada/quarto-casal.jpg" },
                    { 41, 7, 4, "Foto", "/media/pousada/quarto-duplo.jpg" },
                    { 42, 7, 5, "Video", "/media/pousada/tour-virtual.mp4" },
                    { 43, 8, 0, "Foto", "/media/pousada/deck-vista.jpg" },
                    { 44, 8, 1, "Foto", "/media/pousada/fachada-entrada.jpg" },
                    { 45, 8, 2, "Foto", "/media/pousada/sala-lareira.jpg" },
                    { 46, 8, 3, "Foto", "/media/pousada/quarto-casal.jpg" },
                    { 47, 8, 4, "Foto", "/media/pousada/quarto-duplo.jpg" },
                    { 48, 8, 5, "Video", "/media/pousada/tour-virtual.mp4" },
                    { 49, 9, 0, "Foto", "/media/pousada/deck-vista.jpg" },
                    { 50, 9, 1, "Foto", "/media/pousada/fachada-entrada.jpg" },
                    { 51, 9, 2, "Foto", "/media/pousada/sala-lareira.jpg" },
                    { 52, 9, 3, "Foto", "/media/pousada/quarto-casal.jpg" },
                    { 53, 9, 4, "Foto", "/media/pousada/quarto-duplo.jpg" },
                    { 54, 9, 5, "Video", "/media/pousada/tour-virtual.mp4" },
                    { 55, 10, 0, "Foto", "/media/pousada/deck-vista.jpg" },
                    { 56, 10, 1, "Foto", "/media/pousada/fachada-entrada.jpg" },
                    { 57, 10, 2, "Foto", "/media/pousada/sala-lareira.jpg" },
                    { 58, 10, 3, "Foto", "/media/pousada/quarto-casal.jpg" },
                    { 59, 10, 4, "Foto", "/media/pousada/quarto-duplo.jpg" },
                    { 60, 10, 5, "Video", "/media/pousada/tour-virtual.mp4" },
                    { 61, 11, 0, "Foto", "/media/pousada/deck-vista.jpg" },
                    { 62, 11, 1, "Foto", "/media/pousada/fachada-entrada.jpg" },
                    { 63, 11, 2, "Foto", "/media/pousada/sala-lareira.jpg" },
                    { 64, 11, 3, "Foto", "/media/pousada/quarto-casal.jpg" },
                    { 65, 11, 4, "Foto", "/media/pousada/quarto-duplo.jpg" },
                    { 66, 11, 5, "Video", "/media/pousada/tour-virtual.mp4" },
                    { 67, 12, 0, "Foto", "/media/pousada/deck-vista.jpg" },
                    { 68, 12, 1, "Foto", "/media/pousada/fachada-entrada.jpg" },
                    { 69, 12, 2, "Foto", "/media/pousada/sala-lareira.jpg" },
                    { 70, 12, 3, "Foto", "/media/pousada/quarto-casal.jpg" },
                    { 71, 12, 4, "Foto", "/media/pousada/quarto-duplo.jpg" },
                    { 72, 12, 5, "Video", "/media/pousada/tour-virtual.mp4" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 1,
                column: "FotoUrl",
                value: "/media/chales/tipo-a.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 2,
                column: "FotoUrl",
                value: "/media/chales/tipo-a.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 3,
                column: "FotoUrl",
                value: "/media/chales/tipo-a.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 4,
                column: "FotoUrl",
                value: "/media/chales/tipo-a.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 5,
                column: "FotoUrl",
                value: "/media/chales/tipo-a.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 6,
                column: "FotoUrl",
                value: "/media/chales/tipo-a.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 7,
                column: "FotoUrl",
                value: "/media/chales/tipo-b.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 8,
                column: "FotoUrl",
                value: "/media/chales/tipo-b.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 9,
                column: "FotoUrl",
                value: "/media/chales/tipo-b.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 10,
                column: "FotoUrl",
                value: "/media/chales/tipo-b.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 11,
                column: "FotoUrl",
                value: "/media/chales/tipo-c.svg");

            migrationBuilder.UpdateData(
                table: "Chale",
                keyColumn: "Id",
                keyValue: 12,
                column: "FotoUrl",
                value: "/media/chales/tipo-c.svg");

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 1,
                column: "Url",
                value: "/media/chales/tipo-a.svg");

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 2,
                column: "Url",
                value: "/media/chales/tipo-a-interior.svg");

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 2, 0, "/media/chales/tipo-a.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 2, 1, "/media/chales/tipo-a-interior.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 3, 0, "/media/chales/tipo-a.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ChaleId", "Ordem", "Tipo", "Url" },
                values: new object[] { 3, 1, "Foto", "/media/chales/tipo-a-interior.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 4, "/media/chales/tipo-a.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 4, "/media/chales/tipo-a-interior.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 5, 0, "/media/chales/tipo-a.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 5, 1, "/media/chales/tipo-a-interior.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 6, 0, "/media/chales/tipo-a.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "ChaleId", "Ordem", "Tipo", "Url" },
                values: new object[] { 6, 1, "Foto", "/media/chales/tipo-a-interior.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 7, "/media/chales/tipo-b.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 7, "/media/chales/tipo-b-interior.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 8, 0, "/media/chales/tipo-b.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 8, 1, "/media/chales/tipo-b-interior.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 9, 0, "/media/chales/tipo-b.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "ChaleId", "Ordem", "Tipo", "Url" },
                values: new object[] { 9, 1, "Foto", "/media/chales/tipo-b-interior.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 10, "/media/chales/tipo-b.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "ChaleId", "Url" },
                values: new object[] { 10, "/media/chales/tipo-b-interior.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 11, 0, "/media/chales/tipo-c.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 11, 1, "/media/chales/tipo-c-interior.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "ChaleId", "Ordem", "Url" },
                values: new object[] { 12, 0, "/media/chales/tipo-c.svg" });

            migrationBuilder.UpdateData(
                table: "ChaleMidia",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "ChaleId", "Ordem", "Tipo", "Url" },
                values: new object[] { 12, 1, "Foto", "/media/chales/tipo-c-interior.svg" });
        }
    }
}
