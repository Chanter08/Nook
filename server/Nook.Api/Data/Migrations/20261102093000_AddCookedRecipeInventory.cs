using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

[DbContext(typeof(NookDbContext))]
[Migration("20261102093000_AddCookedRecipeInventory")]
public partial class AddCookedRecipeInventory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CookedRecipe",
            columns: table => new
            {
                CookedRecipeId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RecipeId = table.Column<int>(type: "int", nullable: false),
                MealPlanEntryId = table.Column<int>(type: "int", nullable: true),
                Servings = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                CookedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UndoneAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CookedRecipe", x => x.CookedRecipeId);
                table.ForeignKey("FK_CookedRecipe_Recipe_RecipeId", x => x.RecipeId,
                    "Recipe", "RecipeId", onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateTable(
            name: "CookedIngredientUsage",
            columns: table => new
            {
                CookedIngredientUsageId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CookedRecipeId = table.Column<int>(type: "int", nullable: false),
                PantryItemId = table.Column<int>(type: "int", nullable: false),
                RecipeIngredientId = table.Column<int>(type: "int", nullable: false),
                IngredientName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CookedIngredientUsage", x => x.CookedIngredientUsageId);
                table.ForeignKey("FK_CookedIngredientUsage_CookedRecipe_CookedRecipeId",
                    x => x.CookedRecipeId, "CookedRecipe", "CookedRecipeId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_CookedRecipe_RecipeId", "CookedRecipe", "RecipeId");
        migrationBuilder.CreateIndex("IX_CookedRecipe_MealPlanEntryId", "CookedRecipe", "MealPlanEntryId");
        migrationBuilder.CreateIndex("IX_CookedIngredientUsage_CookedRecipeId", "CookedIngredientUsage", "CookedRecipeId");
        migrationBuilder.CreateIndex("IX_CookedIngredientUsage_PantryItemId", "CookedIngredientUsage", "PantryItemId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CookedIngredientUsage");
        migrationBuilder.DropTable(name: "CookedRecipe");
    }
}