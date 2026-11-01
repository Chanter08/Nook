using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

[DbContext(typeof(NookDbContext))]
[Migration("20261101210000_AddPantryInventory")]
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
                ShoppingCatalogItemId = table.Column<int>(type: "int", nullable: true),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                NormalizedName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                StorageLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                LowStockThreshold = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                Source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                LastPurchasedDate = table.Column<DateOnly>(type: "date", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PantryItem", x => x.PantryItemId);
                table.ForeignKey(
                    name: "FK_PantryItem_ShoppingCatalogItem_ShoppingCatalogItemId",
                    column: x => x.ShoppingCatalogItemId,
                    principalTable: "ShoppingCatalogItem",
                    principalColumn: "ShoppingCatalogItemId",
                    onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PantryItem_ExpiryDate",
            table: "PantryItem",
            column: "ExpiryDate");

        migrationBuilder.CreateIndex(
            name: "IX_PantryItem_NormalizedName",
            table: "PantryItem",
            column: "NormalizedName");

        migrationBuilder.CreateIndex(
            name: "IX_PantryItem_ShoppingCatalogItemId",
            table: "PantryItem",
            column: "ShoppingCatalogItemId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PantryItem");
    }
}
