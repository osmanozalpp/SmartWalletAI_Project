using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWalletAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceNumberForReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF Core yapamadı, biz elle ekliyoruz!
            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Olur da geri almak istersek diye silme kuralı
            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "Transactions");
        }
    }
}
