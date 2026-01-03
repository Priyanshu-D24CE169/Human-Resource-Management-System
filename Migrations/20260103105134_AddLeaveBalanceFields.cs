using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hrms.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveBalanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentYearPTOUsed",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentYearSickUsed",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeaveBalanceLastReset",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PaidTimeOffBalance",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SickTimeOffBalance",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentYearPTOUsed",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CurrentYearSickUsed",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LeaveBalanceLastReset",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PaidTimeOffBalance",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SickTimeOffBalance",
                table: "Employees");
        }
    }
}
