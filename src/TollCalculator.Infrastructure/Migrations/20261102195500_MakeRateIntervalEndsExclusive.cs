using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TollCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeRateIntervalEndsExclusive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 1,
                column: "End",
                value: new TimeOnly(6, 30, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 2,
                column: "End",
                value: new TimeOnly(7, 0, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 3,
                column: "End",
                value: new TimeOnly(8, 0, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 4,
                column: "End",
                value: new TimeOnly(8, 30, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 5,
                column: "End",
                value: new TimeOnly(15, 0, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 6,
                column: "End",
                value: new TimeOnly(15, 30, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 7,
                column: "End",
                value: new TimeOnly(17, 0, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 8,
                column: "End",
                value: new TimeOnly(18, 0, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 9,
                column: "End",
                value: new TimeOnly(18, 30, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 1,
                column: "End",
                value: new TimeOnly(6, 29, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 2,
                column: "End",
                value: new TimeOnly(6, 59, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 3,
                column: "End",
                value: new TimeOnly(7, 59, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 4,
                column: "End",
                value: new TimeOnly(8, 29, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 5,
                column: "End",
                value: new TimeOnly(14, 59, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 6,
                column: "End",
                value: new TimeOnly(15, 29, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 7,
                column: "End",
                value: new TimeOnly(16, 59, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 8,
                column: "End",
                value: new TimeOnly(17, 59, 0));

            migrationBuilder.UpdateData(
                table: "TollRateIntervals",
                keyColumn: "Id",
                keyValue: 9,
                column: "End",
                value: new TimeOnly(18, 29, 0));
        }
    }
}
