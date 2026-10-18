using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Adds the sticky jerk chicken wings recipe through Nook's existing deployment
/// migration step. The insert is guarded so a manually-added copy is preserved.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261018193700_AddStickyJerkChickenWings")]
public partial class AddStickyJerkChickenWings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1 FROM [Recipe]
                WHERE LOWER([Name]) = LOWER(N'Sticky Jamaican Jerk Chicken Wings')
            )
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Sticky Jamaican Jerk Chicken Wings',
                    N'Crispy oven-baked chicken wings tossed in a glossy sweet, smoky Jamaican jerk and honey glaze. Oven and air fryer methods included.',
                    10, 50, 4, NULL, SYSUTCDATETIME(), 0,
                    NULL, NULL, NULL, NULL
                );

                DECLARE @RecipeId int = CONVERT(int, SCOPE_IDENTITY());

                DECLARE @Ingredients TABLE (
                    [Name] nvarchar(200) NOT NULL,
                    [Quantity] decimal(18,2) NULL,
                    [Unit] nvarchar(50) NULL,
                    [Notes] nvarchar(max) NULL,
                    [SortOrder] int NOT NULL
                );

                INSERT INTO @Ingredients ([Name], [Quantity], [Unit], [Notes], [SortOrder])
                VALUES
                    (N'Chicken wings', 1000, N'g', N'Separated into flats and drumettes', 1),
                    (N'Jerk sauce', 110, N'ml', N'Ready-to-use sauce; if using concentrated jerk paste, start with 30–50 ml diluted to taste', 2),
                    (N'Honey', 20, N'g', N'For the glaze', 3),
                    (N'Neutral oil', 15, N'ml', N'About 1 tablespoon', 4),
                    (N'Baking powder', 4, N'g', N'About 1 teaspoon; NOT baking soda', 5),
                    (N'Garlic powder', 1.5, N'g', N'About 1/2 teaspoon', 6),
                    (N'Black pepper', NULL, NULL, N'To taste', 7),
                    (N'Water', 20, N'ml', N'Optional; add 1–2 tablespoons only if the glaze is too thick', 8),
                    (N'Salt', NULL, NULL, N'Optional; add only if the jerk sauce is not already salty', 9);

                INSERT INTO [Ingredient] ([Name], [DefaultUnit])
                SELECT i.[Name], i.[Unit]
                FROM @Ingredients AS i
                WHERE NOT EXISTS (
                    SELECT 1 FROM [Ingredient] AS e
                    WHERE LOWER(e.[Name]) = LOWER(i.[Name])
                );

                INSERT INTO [RecipeIngredient] (
                    [RecipeId], [IngredientId], [Quantity], [Unit], [Notes], [SortOrder]
                )
                SELECT @RecipeId, ingredient.[IngredientId],
                       i.[Quantity], i.[Unit], i.[Notes], i.[SortOrder]
                FROM @Ingredients AS i
                CROSS APPLY (
                    SELECT TOP (1) e.[IngredientId]
                    FROM [Ingredient] AS e
                    WHERE LOWER(e.[Name]) = LOWER(i.[Name])
                    ORDER BY e.[IngredientId]
                ) AS ingredient;

                INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
                VALUES
                    (@RecipeId, 1, N'Preheat the oven to 220°C fan. Pat the chicken wings very dry with kitchen paper. Toss with neutral oil, baking powder (not baking soda), garlic powder and black pepper. Add salt only if your jerk sauce needs it.'),
                    (@RecipeId, 2, N'Arrange the wings in a single layer on a wire rack set above a baking tray. Bake for 35–45 minutes, flipping halfway through, until the skin is golden and crispy.'),
                    (@RecipeId, 3, N'While the wings bake, simmer the jerk sauce with the honey in a small saucepan for 3–5 minutes until glossy. Add 1–2 tablespoons of water if too thick. Use sauce that has not been in contact with raw chicken; if using a raw jerk marinade, cook it thoroughly according to its label.'),
                    (@RecipeId, 4, N'Toss the cooked wings with the warm glaze. Return to the oven for another 3–5 minutes to lightly caramelise. Keep an eye on them because the honey can burn.'),
                    (@RecipeId, 5, N'Check that the thickest part of the wings has reached at least 74°C internally. Serve hot, optionally with coconut rice, coleslaw or a cooling yogurt dip.'),
                    (@RecipeId, 6, N'Air fryer alternative: cook the unglazed wings at 200°C for about 20–25 minutes, turning halfway; then glaze and air fry for another 2–3 minutes. Check that the internal temperature reaches 74°C.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE [Name] = N'Caribbean')
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Caribbean');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId]
                FROM [Cuisine]
                WHERE [Name] = N'Caribbean';

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE [Name] = N'Dinner')
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId]
                FROM [RecipeType]
                WHERE [Name] = N'Dinner';
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Preserve the recipe and any user meal plans created from it on rollback.
        // The Up migration is idempotent if it is applied again.
    }
}
