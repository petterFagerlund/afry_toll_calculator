using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TollCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecordVehicleTypeOnPassage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TollPassages_Vehicles_RegistrationNumber",
                table: "TollPassages");

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "TollPassages",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unknown");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "TollPassages");

            migrationBuilder.AddForeignKey(
                name: "FK_TollPassages_Vehicles_RegistrationNumber",
                table: "TollPassages",
                column: "RegistrationNumber",
                principalTable: "Vehicles",
                principalColumn: "RegistrationNumber",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
