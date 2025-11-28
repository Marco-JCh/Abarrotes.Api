using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClienteNuevoModelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Renombrar columnas para NO perder datos
            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "clientes",
                newName: "nombres");

            migrationBuilder.RenameColumn(
                name: "dni_ruc",
                table: "clientes",
                newName: "numero_documento");

            // Ajustar columnas existentes
            migrationBuilder.AlterColumn<string>(
                name: "telefono",
                table: "clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            // Agregar columnas nuevas
            migrationBuilder.AddColumn<string>(
                name: "apellidos",
                table: "clientes",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tipo_documento",
                table: "clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "DNI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "apellidos",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "tipo_documento",
                table: "clientes");

            migrationBuilder.AlterColumn<string>(
                name: "telefono",
                table: "clientes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "clientes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            // Restaurar nombres antiguos
            migrationBuilder.RenameColumn(
                name: "nombres",
                table: "clientes",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "numero_documento",
                table: "clientes",
                newName: "dni_ruc");
        }
    }
}
