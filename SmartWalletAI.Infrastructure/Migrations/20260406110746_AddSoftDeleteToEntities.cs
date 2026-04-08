using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWalletAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedData",
                table: "Wallets",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedData",
                table: "Users",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedData",
                table: "Transactions",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedData",
                table: "SavedContacts",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Wallets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SavedContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SavedContacts");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Wallets",
                newName: "CreatedData");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Users",
                newName: "CreatedData");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Transactions",
                newName: "CreatedData");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "SavedContacts",
                newName: "CreatedData");
        }
    }
}
