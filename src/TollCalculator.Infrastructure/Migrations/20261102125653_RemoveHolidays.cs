using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TollCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHolidays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holidays");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Date);
                });

            migrationBuilder.InsertData(
                table: "Holidays",
                columns: new[] { "Date", "Name" },
                values: new object[,]
                {
                    { new DateOnly(2026, 1, 1), "Nyårsdagen" },
                    { new DateOnly(2026, 1, 6), "Trettondedag jul" },
                    { new DateOnly(2026, 4, 3), "Långfredagen" },
                    { new DateOnly(2026, 4, 5), "Påskdagen" },
                    { new DateOnly(2026, 4, 6), "Annandag påsk" },
                    { new DateOnly(2026, 5, 1), "Första maj" },
                    { new DateOnly(2026, 5, 14), "Kristi himmelsfärdsdag" },
                    { new DateOnly(2026, 5, 24), "Pingstdagen" },
                    { new DateOnly(2026, 6, 6), "Sveriges nationaldag" },
                    { new DateOnly(2026, 6, 20), "Midsommardagen" },
                    { new DateOnly(2026, 10, 31), "Alla helgons dag" },
                    { new DateOnly(2026, 12, 25), "Juldagen" },
                    { new DateOnly(2026, 12, 26), "Annandag jul" },
                    { new DateOnly(2027, 1, 1), "Nyårsdagen" },
                    { new DateOnly(2027, 1, 6), "Trettondedag jul" },
                    { new DateOnly(2027, 3, 26), "Långfredagen" },
                    { new DateOnly(2027, 3, 28), "Påskdagen" },
                    { new DateOnly(2027, 3, 29), "Annandag påsk" },
                    { new DateOnly(2027, 5, 1), "Första maj" },
                    { new DateOnly(2027, 5, 6), "Kristi himmelsfärdsdag" },
                    { new DateOnly(2027, 5, 16), "Pingstdagen" },
                    { new DateOnly(2027, 6, 6), "Sveriges nationaldag" },
                    { new DateOnly(2027, 6, 26), "Midsommardagen" },
                    { new DateOnly(2027, 11, 6), "Alla helgons dag" },
                    { new DateOnly(2027, 12, 25), "Juldagen" },
                    { new DateOnly(2027, 12, 26), "Annandag jul" }
                });
        }
    }
}
