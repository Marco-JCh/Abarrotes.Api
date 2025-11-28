using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "anulada",
                schema: "public",
                table: "compras",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "comprobante_tipo",
                schema: "public",
                table: "compras",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_pago",
                schema: "public",
                table: "compras",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "anulada",
                schema: "public",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "comprobante_tipo",
                schema: "public",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "tipo_pago",
                schema: "public",
                table: "compras");
        }
    }
}
