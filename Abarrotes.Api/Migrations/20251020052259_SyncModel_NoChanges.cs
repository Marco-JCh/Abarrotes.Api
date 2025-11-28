using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel_NoChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ventas",
                schema: "public",
                newName: "ventas");

            migrationBuilder.RenameTable(
                name: "venta_requests",
                schema: "public",
                newName: "venta_requests");

            migrationBuilder.RenameTable(
                name: "usuarios",
                schema: "public",
                newName: "usuarios");

            migrationBuilder.RenameTable(
                name: "proveedores",
                schema: "public",
                newName: "proveedores");

            migrationBuilder.RenameTable(
                name: "productos",
                schema: "public",
                newName: "productos");

            migrationBuilder.RenameTable(
                name: "notas_pedido",
                schema: "public",
                newName: "notas_pedido");

            migrationBuilder.RenameTable(
                name: "metodopago",
                schema: "public",
                newName: "metodopago");

            migrationBuilder.RenameTable(
                name: "lotes",
                schema: "public",
                newName: "lotes");

            migrationBuilder.RenameTable(
                name: "facturas",
                schema: "public",
                newName: "facturas");

            migrationBuilder.RenameTable(
                name: "estadoproducto",
                schema: "public",
                newName: "estadoproducto");

            migrationBuilder.RenameTable(
                name: "detalleventas",
                schema: "public",
                newName: "detalleventas");

            migrationBuilder.RenameTable(
                name: "consumos_lote",
                schema: "public",
                newName: "consumos_lote");

            migrationBuilder.RenameTable(
                name: "clientes",
                schema: "public",
                newName: "clientes");

            migrationBuilder.RenameTable(
                name: "categorias",
                schema: "public",
                newName: "categorias");

            migrationBuilder.RenameTable(
                name: "boletas",
                schema: "public",
                newName: "boletas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ventas",
                newName: "ventas",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "venta_requests",
                newName: "venta_requests",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "usuarios",
                newName: "usuarios",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "proveedores",
                newName: "proveedores",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "productos",
                newName: "productos",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "notas_pedido",
                newName: "notas_pedido",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "metodopago",
                newName: "metodopago",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "lotes",
                newName: "lotes",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "facturas",
                newName: "facturas",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "estadoproducto",
                newName: "estadoproducto",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "detalleventas",
                newName: "detalleventas",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "consumos_lote",
                newName: "consumos_lote",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "clientes",
                newName: "clientes",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "categorias",
                newName: "categorias",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "boletas",
                newName: "boletas",
                newSchema: "public");
        }
    }
}
