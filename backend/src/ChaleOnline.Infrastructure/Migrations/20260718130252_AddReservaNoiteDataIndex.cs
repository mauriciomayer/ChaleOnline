using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChaleOnline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservaNoiteDataIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ReservaNoite_Data",
                table: "ReservaNoite",
                column: "Data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReservaNoite_Data",
                table: "ReservaNoite");
        }
    }
}
