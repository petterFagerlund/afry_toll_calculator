using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace TollCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "TollSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyCap = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypeRules",
                columns: table => new
                {
                    VehicleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsTollFree = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypeRules", x => x.VehicleType);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    RegistrationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.RegistrationNumber);
                });

            migrationBuilder.CreateTable(
                name: "TollPassages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PassedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollPassages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TollPassages_Vehicles_RegistrationNumber",
                        column: x => x.RegistrationNumber,
                        principalTable: "Vehicles",
                        principalColumn: "RegistrationNumber",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.InsertData(
                table: "TollRateIntervals",
                columns: new[] { "Id", "Amount", "End", "Start" },
                values: new object[,]
                {
                    { 1, 8, new TimeOnly(6, 29, 0), new TimeOnly(6, 0, 0) },
                    { 2, 13, new TimeOnly(6, 59, 0), new TimeOnly(6, 30, 0) },
                    { 3, 18, new TimeOnly(7, 59, 0), new TimeOnly(7, 0, 0) },
                    { 4, 13, new TimeOnly(8, 29, 0), new TimeOnly(8, 0, 0) },
                    { 5, 8, new TimeOnly(14, 59, 0), new TimeOnly(8, 30, 0) },
                    { 6, 13, new TimeOnly(15, 29, 0), new TimeOnly(15, 0, 0) },
                    { 7, 18, new TimeOnly(16, 59, 0), new TimeOnly(15, 30, 0) },
                    { 8, 13, new TimeOnly(17, 59, 0), new TimeOnly(17, 0, 0) },
                    { 9, 8, new TimeOnly(18, 29, 0), new TimeOnly(18, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "TollSettings",
                columns: new[] { "Id", "DailyCap" },
                values: new object[] { 1, 60 });

            migrationBuilder.InsertData(
                table: "VehicleTypeRules",
                columns: new[] { "VehicleType", "IsTollFree" },
                values: new object[,]
                {
                    { "Car", false },
                    { "Diplomat", true },
                    { "Emergency", true },
                    { "Foreign", true },
                    { "Military", true },
                    { "Motorbike", true },
                    { "Tractor", true }
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
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "TollPassages");

            migrationBuilder.DropTable(
                name: "TollRateIntervals");

            migrationBuilder.DropTable(
                name: "TollSettings");

            migrationBuilder.DropTable(
                name: "VehicleTypeRules");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
