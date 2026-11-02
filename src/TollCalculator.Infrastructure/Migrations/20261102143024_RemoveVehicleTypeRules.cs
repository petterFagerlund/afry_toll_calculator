using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TollCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVehicleTypeRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleTypeRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
