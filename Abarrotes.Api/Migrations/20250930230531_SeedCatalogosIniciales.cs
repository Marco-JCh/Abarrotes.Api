using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace Abarrotes.Api.Migrations
{
    public partial class SeedCatalogosIniciales : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO estadopago (id, nombre) VALUES
                  (1, 'pagado'), (2, 'pendiente'), (3, 'anulado')
                ON CONFLICT (id) DO NOTHING;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO metodopago (id, nombre) VALUES
                  (1, 'efectivo'), (2, 'yape'), (3, 'tarjeta')
                ON CONFLICT (id) DO NOTHING;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM estadopago WHERE id IN (1,2,3);");
            migrationBuilder.Sql("DELETE FROM metodopago WHERE id IN (1,2,3);");
        }
    }
}
