using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TollCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TollPassages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PassedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollPassages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TollRateIntervals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Start = table.Column<TimeOnly>(type: "time", nullable: false),
                    End = table.Column<TimeOnly>(type: "time", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollRateIntervals", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TollRateIntervals",
                columns: new[] { "Id", "Amount", "End", "Start" },
                values: new object[,]
                {
                    { 1, 8, new TimeOnly(6, 30, 0), new TimeOnly(6, 0, 0) },
                    { 2, 13, new TimeOnly(7, 0, 0), new TimeOnly(6, 30, 0) },
                    { 3, 18, new TimeOnly(8, 0, 0), new TimeOnly(7, 0, 0) },
                    { 4, 13, new TimeOnly(8, 30, 0), new TimeOnly(8, 0, 0) },
                    { 5, 8, new TimeOnly(15, 0, 0), new TimeOnly(8, 30, 0) },
                    { 6, 13, new TimeOnly(15, 30, 0), new TimeOnly(15, 0, 0) },
                    { 7, 18, new TimeOnly(17, 0, 0), new TimeOnly(15, 30, 0) },
                    { 8, 13, new TimeOnly(18, 0, 0), new TimeOnly(17, 0, 0) },
                    { 9, 8, new TimeOnly(18, 30, 0), new TimeOnly(18, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TollPassages_RegistrationNumber_PassedAt",
                table: "TollPassages",
                columns: new[] { "RegistrationNumber", "PassedAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TollRateIntervals_Start",
                table: "TollRateIntervals",
                column: "Start",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TollPassages");

            migrationBuilder.DropTable(
                name: "TollRateIntervals");
        }
    }
}
