using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWalletAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMarketPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpeningPrice",
                table: "MarketPrices");

            migrationBuilder.AddColumn<double>(
                name: "DailyChangePercentage",
                table: "MarketPrices",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyChangePercentage",
                table: "MarketPrices");

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningPrice",
                table: "MarketPrices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
