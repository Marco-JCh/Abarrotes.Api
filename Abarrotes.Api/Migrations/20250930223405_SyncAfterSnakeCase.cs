using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abarrotes.Api.Migrations
{
    public partial class SyncAfterSnakeCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // no-op: solo sincroniza el snapshot
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // no-op
        }
    }
}