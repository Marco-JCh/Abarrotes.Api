using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCompraIdToLotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "lotes",
                newName: "lotes",
                newSchema: "public");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                schema: "public",
                table: "lotes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVO",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "compra_id",
                schema: "public",
                table: "lotes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_lotes_compra_id",
                schema: "public",
                table: "lotes",
                column: "compra_id");

            migrationBuilder.AddForeignKey(
                name: "fk_lotes_compras_compra_id",
                schema: "public",
                table: "lotes",
                column: "compra_id",
                principalSchema: "public",
                principalTable: "compras",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_lotes_compras_compra_id",
                schema: "public",
                table: "lotes");

            migrationBuilder.DropIndex(
                name: "ix_lotes_compra_id",
                schema: "public",
                table: "lotes");

            migrationBuilder.DropColumn(
                name: "compra_id",
                schema: "public",
                table: "lotes");

            migrationBuilder.RenameTable(
                name: "lotes",
                schema: "public",
                newName: "lotes");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                table: "lotes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "ACTIVO");
        }
    }
}
