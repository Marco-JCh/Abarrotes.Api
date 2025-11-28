using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    /// <inheritdoc />
    public partial class Usuarios_V5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "usuario",
                table: "usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "usuarios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "nombres",
                table: "usuarios",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "usuarios",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "usuario_login",
                table: "usuarios",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_usuario_login",
                table: "usuarios",
                column: "usuario_login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_usuarios_email",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "ix_usuarios_usuario_login",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "usuario_login",
                table: "usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "nombres",
                table: "usuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "usuarios",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "usuario",
                table: "usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
