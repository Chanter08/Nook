using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

[DbContext(typeof(NookDbContext))]
[Migration("20261101201500_AddPantryInventory")]
public partial class AddPantryInventory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PantryItem",
            columns: table => new
            {
                PantryItemId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Location = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PantryItem", x => x.PantryItemId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PantryItem_Location_ExpiryDate",
            table: "PantryItem",
            columns: new[] { "Location", "ExpiryDate" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PantryItem");
    }
}
