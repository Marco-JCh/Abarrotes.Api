using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixStockRealColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "stock_real",
                table: "productos",
                type: "numeric(12,3)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "stock_real",
                table: "productos");
        }
    }
}
