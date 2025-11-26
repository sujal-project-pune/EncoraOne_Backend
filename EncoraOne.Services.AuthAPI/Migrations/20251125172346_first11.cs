using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EncoraOne.Grievance.API.Migrations
{
    /// <inheritdoc />
    public partial class first11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 25, 17, 23, 46, 380, DateTimeKind.Utc).AddTicks(6382));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 25, 17, 23, 32, 969, DateTimeKind.Utc).AddTicks(7187));
        }
    }
}
