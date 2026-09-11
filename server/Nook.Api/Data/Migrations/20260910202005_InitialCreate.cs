using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nook.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cuisine",
                columns: table => new
                {
                    CuisineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuisine", x => x.CuisineId);
                });

            migrationBuilder.CreateTable(
                name: "Ingredient",
                columns: table => new
                {
                    IngredientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultUnit = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredient", x => x.IngredientId);
                });

            migrationBuilder.CreateTable(
                name: "Recipe",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrepTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    CookTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    Servings = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    Calories = table.Column<int>(type: "int", nullable: true),
                    ProteinGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CarbohydrateGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FatGrams = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipe", x => x.RecipeId);
                });

            migrationBuilder.CreateTable(
                name: "RecipeType",
                columns: table => new
                {
                    RecipeTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeType", x => x.RecipeTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCatalogItem",
                columns: table => new
                {
                    ShoppingCatalogItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DefaultUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultNotes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UseCount = table.Column<int>(type: "int", nullable: false),
                    LastUsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCatalogItem", x => x.ShoppingCatalogItemId);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingList",
                columns: table => new
                {
                    ShoppingListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingList", x => x.ShoppingListId);
                });

            migrationBuilder.CreateTable(
                name: "MealPlanEntry",
                columns: table => new
                {
                    MealPlanEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MealType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlanEntry", x => x.MealPlanEntryId);
                    table.ForeignKey(
                        name: "FK_MealPlanEntry_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "RecipeId");
                });

            migrationBuilder.CreateTable(
                name: "RecipeCuisine",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    CuisineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeCuisine", x => new { x.RecipeId, x.CuisineId });
                    table.ForeignKey(
                        name: "FK_RecipeCuisine_Cuisine_CuisineId",
                        column: x => x.CuisineId,
                        principalTable: "Cuisine",
                        principalColumn: "CuisineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeCuisine_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredient",
                columns: table => new
                {
                    RecipeIngredientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredient", x => x.RecipeIngredientId);
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Ingredient_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredient",
                        principalColumn: "IngredientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeStep",
                columns: table => new
                {
                    RecipeStepId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeStep", x => x.RecipeStepId);
                    table.ForeignKey(
                        name: "FK_RecipeStep_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeRecipeType",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    RecipeTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeRecipeType", x => new { x.RecipeId, x.RecipeTypeId });
                    table.ForeignKey(
                        name: "FK_RecipeRecipeType_RecipeType_RecipeTypeId",
                        column: x => x.RecipeTypeId,
                        principalTable: "RecipeType",
                        principalColumn: "RecipeTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeRecipeType_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingListItem",
                columns: table => new
                {
                    ShoppingListItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShoppingListId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShoppingCatalogItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingListItem", x => x.ShoppingListItemId);
                    table.ForeignKey(
                        name: "FK_ShoppingListItem_Ingredient_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredient",
                        principalColumn: "IngredientId");
                    table.ForeignKey(
                        name: "FK_ShoppingListItem_ShoppingCatalogItem_ShoppingCatalogItemId",
                        column: x => x.ShoppingCatalogItemId,
                        principalTable: "ShoppingCatalogItem",
                        principalColumn: "ShoppingCatalogItemId");
                    table.ForeignKey(
                        name: "FK_ShoppingListItem_ShoppingList_ShoppingListId",
                        column: x => x.ShoppingListId,
                        principalTable: "ShoppingList",
                        principalColumn: "ShoppingListId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanEntry_PlanDate_MealType",
                table: "MealPlanEntry",
                columns: new[] { "PlanDate", "MealType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanEntry_RecipeId",
                table: "MealPlanEntry",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeCuisine_CuisineId",
                table: "RecipeCuisine",
                column: "CuisineId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_IngredientId",
                table: "RecipeIngredient",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_RecipeId",
                table: "RecipeIngredient",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeRecipeType_RecipeTypeId",
                table: "RecipeRecipeType",
                column: "RecipeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeStep_RecipeId",
                table: "RecipeStep",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCatalogItem_NormalizedName",
                table: "ShoppingCatalogItem",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItem_IngredientId",
                table: "ShoppingListItem",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItem_ShoppingCatalogItemId",
                table: "ShoppingListItem",
                column: "ShoppingCatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItem_ShoppingListId",
                table: "ShoppingListItem",
                column: "ShoppingListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealPlanEntry");

            migrationBuilder.DropTable(
                name: "RecipeCuisine");

            migrationBuilder.DropTable(
                name: "RecipeIngredient");

            migrationBuilder.DropTable(
                name: "RecipeRecipeType");

            migrationBuilder.DropTable(
                name: "RecipeStep");

            migrationBuilder.DropTable(
                name: "ShoppingListItem");

            migrationBuilder.DropTable(
                name: "Cuisine");

            migrationBuilder.DropTable(
                name: "RecipeType");

            migrationBuilder.DropTable(
                name: "Recipe");

            migrationBuilder.DropTable(
                name: "Ingredient");

            migrationBuilder.DropTable(
                name: "ShoppingCatalogItem");

            migrationBuilder.DropTable(
                name: "ShoppingList");
        }
    }
}
