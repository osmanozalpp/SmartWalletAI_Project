using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWalletAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialGoalIdToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SenderWalletId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "FinancialGoalId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FinancialGoalId",
                table: "Transactions",
                column: "FinancialGoalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_FinancialGoals_FinancialGoalId",
                table: "Transactions",
                column: "FinancialGoalId",
                principalTable: "FinancialGoals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_FinancialGoals_FinancialGoalId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_FinancialGoalId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "FinancialGoalId",
                table: "Transactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "SenderWalletId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
