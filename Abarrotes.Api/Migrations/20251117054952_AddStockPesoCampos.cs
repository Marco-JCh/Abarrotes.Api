using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    public partial class AddStockPesoCampos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ELIMINAR tipo_medida si existe
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'productos' 
                    AND column_name = 'tipo_medida'
                ) THEN
                    ALTER TABLE productos DROP COLUMN tipo_medida;
                END IF;
            END $$;
        ");

                // Agregar es_por_peso SI NO EXISTE
                migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'productos'
                    AND column_name = 'es_por_peso'
                ) THEN
                    ALTER TABLE productos 
                    ADD COLUMN es_por_peso boolean NOT NULL DEFAULT FALSE;
                END IF;
            END $$;
        ");

                // Agregar factor_base SI NO EXISTE
                migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'productos'
                    AND column_name = 'factor_base'
                ) THEN
                    ALTER TABLE productos 
                    ADD COLUMN factor_base integer NOT NULL DEFAULT 1;
                END IF;
            END $$;
        ");

                // Agregar unidad_base SI NO EXISTE
                migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'productos'
                    AND column_name = 'unidad_base'
                ) THEN
                    ALTER TABLE productos 
                    ADD COLUMN unidad_base varchar(10) NOT NULL DEFAULT 'unidad';
                END IF;
            END $$;
        ");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "es_por_peso",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "factor_base",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "unidad_base",
                table: "productos");
        }
    }
}
