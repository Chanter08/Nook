using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Adds loaded New York-style hot dogs to Nook's recipes during deployment.
/// Protects any existing recipe with the same name and its linked meal plans.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261019050000_AddLoadedNewYorkStyleHotDogs")]
public partial class AddLoadedNewYorkStyleHotDogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1 FROM [Recipe]
                WHERE LOWER([Name]) = LOWER(N'Loaded New York-Style Hot Dogs')
            )
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Loaded New York-Style Hot Dogs',
                    N'Store-bought hot dog wieners in buttery toasted buns, topped with caramelised onions, gherkins and a tangy Greek-yogurt burger sauce. Optional cheddar adds a cheesy finish.',
                    10, 20, 4, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Hot dog wieners', 4, N'piece', N'Store-bought, ready-to-heat frankfurter-style hot dogs', 1),
                    (N'Hot dog buns', 4, N'piece', N'Split lengthwise, keeping the bottom intact', 2),
                    (N'Butter', 10, N'g', N'For toasting the buns', 3),
                    (N'Onions', 2, N'piece', N'Medium; thinly sliced, for caramelising', 4),
                    (N'Olive oil', 15, N'ml', N'About 1 tablespoon, for the onions', 5),
                    (N'Brown sugar', 4, N'g', N'About 1 teaspoon, for the onions', 6),
                    (N'Worcestershire sauce', 5, N'ml', N'About 1 teaspoon, for the onions', 7),
                    (N'Water', 30, N'ml', N'About 2 tablespoons, to loosen the onions', 8),
                    (N'Salt', NULL, NULL, N'A pinch for the onions', 9),
                    (N'Gherkins', 100, N'g', N'About 4–6 small pickles, thinly sliced; add at the end', 10),
                    (N'Greek yogurt', 50, N'g', N'For the burger-style sauce', 11),
                    (N'Mayonnaise', 20, N'g', N'For the sauce', 12),
                    (N'Ketchup', 15, N'g', N'For the sauce', 13),
                    (N'Mustard', 10, N'g', N'For the sauce; add more to finish if desired', 14),
                    (N'Pickle brine', 5, N'ml', N'About 1 teaspoon, for the sauce', 15),
                    (N'Smoked paprika', 0.50, N'g', N'About 1/4 teaspoon, for the sauce', 16),
                    (N'Garlic powder', 0.50, N'g', N'A pinch, for the sauce', 17),
                    (N'Cheddar cheese', 40, N'g', N'Optional; grated, about 10 g per hot dog', 18);

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
                    (@RecipeId, 1, N'Thinly slice the onions. Heat the olive oil in a frying pan over medium-low heat, add onions and a pinch of salt, and cook for 15–20 minutes, stirring now and then, until softened and golden. Add brown sugar, Worcestershire sauce and water in the final few minutes and cook until glossy.'),
                    (@RecipeId, 2, N'Meanwhile, stir together Greek yogurt, mayonnaise, ketchup, mustard, pickle brine, smoked paprika and garlic powder until smooth. Taste and adjust mustard or pickle brine if desired.'),
                    (@RecipeId, 3, N'Heat the store-bought wieners according to their packet instructions. For precooked frankfurters, gently warm in hot (not vigorously boiling) water for about 3–4 minutes, then optionally brown briefly in a frying pan for extra flavour.'),
                    (@RecipeId, 4, N'Lightly butter the cut sides of the hot dog buns and toast them cut-side down in a frying pan for 1–2 minutes until golden.'),
                    (@RecipeId, 5, N'Spread the burger-style sauce inside each warm bun. Add a hot dog wiener, top generously with caramelised onions, then finish with sliced gherkins.'),
                    (@RecipeId, 6, N'If using the optional grated cheddar, sprinkle it over the hot wieners and onions so it softens. Serve immediately, with extra mustard if you like.');

                IF NOT EXISTS (
                    SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'American')
                )
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'American');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId]
                FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'American');

                IF NOT EXISTS (
                    SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Lunch')
                )
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Lunch');

                IF NOT EXISTS (
                    SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner')
                )
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId]
                FROM [RecipeType]
                WHERE LOWER([Name]) IN (LOWER(N'Lunch'), LOWER(N'Dinner'));
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Preserve user recipes and any linked meal plans on rollback.
        // The Up migration skips an existing copy when reapplied.
    }
}
