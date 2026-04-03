using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWalletAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceNumberFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedData",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Transactions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedData",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Transactions");
        }
    }
}
