using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoCompraToCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado_compra",
                schema: "public",
                table: "compras",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "REGISTRADA");

            migrationBuilder.Sql(@"
                UPDATE ""compras""
                SET ""estado_compra"" = CASE
                    WHEN COALESCE(""afecta_inventario"", TRUE) = FALSE THEN 'PENDIENTE'
                    ELSE 'REGISTRADA'
                END;
            ");
            migrationBuilder.CreateIndex(
                name: "ix_compras_estado_compra",
                table: "compras",
                column: "estado_compra");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_compra_estado_compra",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "estado_compra",
                table: "Compra");
        }
    }
}
