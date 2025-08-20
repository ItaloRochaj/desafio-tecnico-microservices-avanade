using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Stock.API.Migrations
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
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityInStock = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "Price", "QuantityInStock", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 20, 18, 40, 38, 542, DateTimeKind.Utc).AddTicks(4309), "Notebook Dell Inspiron 15", "Notebook Dell", 4500.00m, 100, new DateTime(2025, 8, 20, 18, 40, 38, 542, DateTimeKind.Utc).AddTicks(4314) },
                    { 2, new DateTime(2025, 8, 20, 18, 40, 38, 542, DateTimeKind.Utc).AddTicks(5239), "Mouse óptico sem fio", "Mouse Sem Fio", 85.90m, 200, new DateTime(2025, 8, 20, 18, 40, 38, 542, DateTimeKind.Utc).AddTicks(5240) },
                    { 3, new DateTime(2025, 8, 20, 18, 40, 38, 542, DateTimeKind.Utc).AddTicks(5242), "Teclado mecânico RGB", "Teclado Mecânico", 320.00m, 75, new DateTime(2025, 8, 20, 18, 40, 38, 542, DateTimeKind.Utc).AddTicks(5242) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
