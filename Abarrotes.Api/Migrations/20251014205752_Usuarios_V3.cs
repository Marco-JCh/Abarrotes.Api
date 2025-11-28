using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    /// <inheritdoc />
    public partial class Usuarios_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_usuarios_username",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "nombre",
                table: "usuarios");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "usuarios",
                newName: "nombres");

            migrationBuilder.RenameColumn(
                name: "rol",
                table: "usuarios",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "creado_utc",
                table: "usuarios",
                newName: "created_utc");

            migrationBuilder.RenameColumn(
                name: "activo",
                table: "usuarios",
                newName: "esta_activo");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login_utc",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "usuario",
                table: "usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_login_utc",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "usuario",
                table: "usuarios");

            migrationBuilder.RenameColumn(
                name: "nombres",
                table: "usuarios",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "esta_activo",
                table: "usuarios",
                newName: "activo");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "usuarios",
                newName: "rol");

            migrationBuilder.RenameColumn(
                name: "created_utc",
                table: "usuarios",
                newName: "creado_utc");

            migrationBuilder.AddColumn<string>(
                name: "nombre",
                table: "usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_username",
                table: "usuarios",
                column: "username",
                unique: true);
        }
    }
}
