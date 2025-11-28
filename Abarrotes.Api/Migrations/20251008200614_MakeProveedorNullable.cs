using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakeProveedorNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hacer NULLABLE la columna proveedor_id en la tabla lotes
            migrationBuilder.AlterColumn<int>(
                name: "proveedor_id",
                table: "lotes",
                type: "integer",
                nullable: true,                 // <-- ahora acepta NULL
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir a NOT NULL (si haces rollback)
            migrationBuilder.AlterColumn<int>(
                name: "proveedor_id",
                table: "lotes",
                type: "integer",
                nullable: false,                // <-- vuelve a NOT NULL
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
