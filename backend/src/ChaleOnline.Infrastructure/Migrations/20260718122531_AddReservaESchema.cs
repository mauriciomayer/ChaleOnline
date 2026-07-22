using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChaleOnline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservaESchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reserva",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CodigoConsulta = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChaleId = table.Column<int>(type: "int", nullable: false),
                    NomeHospede = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailHospede = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCheckin = table.Column<DateOnly>(type: "date", nullable: false),
                    DataCheckout = table.Column<DateOnly>(type: "date", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reserva", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reserva_Chale_ChaleId",
                        column: x => x.ChaleId,
                        principalTable: "Chale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReservaNoite",
                columns: table => new
                {
                    ChaleId = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    ReservaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservaNoite", x => new { x.ChaleId, x.Data });
                    table.ForeignKey(
                        name: "FK_ReservaNoite_Chale_ChaleId",
                        column: x => x.ChaleId,
                        principalTable: "Chale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservaNoite_Reserva_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reserva",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_ChaleId",
                table: "Reserva",
                column: "ChaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_CodigoConsulta",
                table: "Reserva",
                column: "CodigoConsulta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservaNoite_ReservaId",
                table: "ReservaNoite",
                column: "ReservaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservaNoite");

            migrationBuilder.DropTable(
                name: "Reserva");
        }
    }
}
