using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Seeds a make-ahead Italian ciabatta sandwich with roasted red pepper,
/// fresh mozzarella and sun-dried tomato–pistachio pesto.
/// The name guard avoids duplicates and preserves any existing user edits.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261028133000_AddPistachioPestoCiabatta")]
public partial class AddPistachioPestoCiabatta : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1 FROM [Recipe]
                WHERE LOWER([Name]) = LOWER(N'Roasted Pepper, Mozzarella and Pistachio Pesto Ciabatta')
            )
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Roasted Pepper, Mozzarella and Pistachio Pesto Ciabatta',
                    N'A cold, portable Italian-style lunch sandwich with roasted red bell peppers, fresh mozzarella, rocket and sun-dried tomato–pistachio pesto (such as Conad Pesto Pomodori Secchi e Pistacchi). Optional prosciutto adds a salty, savoury note. Roast and dry the peppers, drain the mozzarella well, then assemble and refrigerate for an easy next-day lunch. No balsamic needed.',
                    15, 25, 2, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Ciabatta rolls', 2, N'piece', N'Large crusty rolls, halved lengthwise; use one per person', 1),
                    (N'Sun-dried tomato and pistachio pesto', 3, N'tbsp', N'3–4 tbsp in total. Conad Pesto Pomodori Secchi e Pistacchi works well; check label for pistachios and other allergens', 2),
                    (N'Fresh mozzarella', 175, N'g', N'125–200 g; drain thoroughly and pat slices dry', 3),
                    (N'Red bell pepper', 1, N'piece', N'Large; roast, deseed, peel if desired, cool and pat dry', 4),
                    (N'Rocket', 25, N'g', N'Or baby spinach; completely dry before assembling', 5),
                    (N'Prosciutto crudo', 4, N'slice', N'Optional: 2 slices per sandwich for extra protein and saltiness', 6),
                    (N'Olive oil', 1, N'tsp', N'Light coating for roasting the pepper', 7),
                    (N'Black pepper', NULL, NULL, N'Freshly ground to taste', 8);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                           ROW_NUMBER() OVER (
                               PARTITION BY LOWER(i.[Name])
                               ORDER BY i.[SortOrder]
                           ) AS rn
                    FROM @Ingredients AS i
                    WHERE NOT EXISTS (
                        SELECT 1 FROM [Ingredient] AS existing
                        WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
                    )
                )
                INSERT INTO [Ingredient] ([Name], [DefaultUnit])
                SELECT [Name], [Unit] FROM Missing WHERE rn = 1;

                INSERT INTO [RecipeIngredient] (
                    [RecipeId], [IngredientId], [Quantity], [Unit], [Notes], [SortOrder]
                )
                SELECT @RecipeId, ingredient.[IngredientId],
                       i.[Quantity], i.[Unit], i.[Notes], i.[SortOrder]
                FROM @Ingredients AS i
                CROSS APPLY (
                    SELECT TOP (1) existing.[IngredientId]
                    FROM [Ingredient] AS existing
                    WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
                    ORDER BY existing.[IngredientId]
                ) AS ingredient;

                INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
                VALUES
                    (@RecipeId, 1, N'Preheat the oven to 220°C conventional (200°C fan). Rub 1 large red bell pepper lightly with about 1 tsp olive oil and roast on a tray for 20–25 minutes, turning once, until soft and blistered.'),
                    (@RecipeId, 2, N'Let the pepper cool until safe to handle, remove the stalk and seeds, and peel away the skin if desired. Slice into strips and pat thoroughly dry with kitchen paper. Cool completely before filling the sandwiches.'),
                    (@RecipeId, 3, N'Drain 125–200 g fresh mozzarella and cut into slices. Pat each slice dry to prevent excess moisture soaking the bread. Wash and thoroughly dry a small handful of rocket or baby spinach.'),
                    (@RecipeId, 4, N'Split 2 ciabatta rolls. Lightly toast the cut sides, then let the bread cool before assembly. This helps the bread resist overnight moisture without becoming too hard.'),
                    (@RecipeId, 5, N'Spread approximately 1½–2 tbsp sun-dried tomato and pistachio pesto across the cut surfaces of each roll. Add a layer of dry rocket or spinach against the bread, then divide the mozzarella and roasted pepper strips evenly between the sandwiches. Add optional prosciutto crudo (2 slices per roll) and freshly ground black pepper.'),
                    (@RecipeId, 6, N'Close and gently press the sandwiches. For a lunchbox, wrap each tightly in baking paper and put in an airtight container. Do not add balsamic glaze: the sun-dried tomato pesto already adds tanginess and sweetness.'),
                    (@RecipeId, 7, N'For next-day packed lunches, refrigerate immediately at 4°C or colder. Check that opened pesto is still within its label storage period. Keep chilled during transport with an ice pack or workplace fridge and eat the following day. Do not leave the sandwiches unrefrigerated for more than 2 hours in total.');

                IF NOT EXISTS (
                    SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Italian')
                )
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Italian');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'Italian');

                IF NOT EXISTS (
                    SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Lunch')
                )
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Lunch');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) = LOWER(N'Lunch');
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Preserve any edits and planned meals associated with the recipe.
    }
}
