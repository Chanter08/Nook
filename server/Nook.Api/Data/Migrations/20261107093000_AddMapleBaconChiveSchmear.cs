using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>Seeds Maple Bacon & Chive Schmear once, preserving any existing recipe edits.</summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261107093000_AddMapleBaconChiveSchmear")]
public partial class AddMapleBaconChiveSchmear : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Maple Bacon & Chive Schmear'))
            BEGIN
                INSERT INTO [Recipe] ([Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams])
                VALUES (
                    N'Maple Bacon & Chive Schmear',
                    N'Fluffy cream cheese with crispy streaky bacon, real maple syrup and fresh chives. Sweet-and-savoury, bacon-forward New York-style bagel schmear. Chill for 30 minutes before serving. Keep refrigerated in an airtight container and use within 4 days.',
                    10, 5, 8, NULL, SYSUTCDATETIME(), 0,
                    NULL, NULL, NULL, NULL
                );

                DECLARE @RecipeId int = CONVERT(int, SCOPE_IDENTITY());
                DECLARE @Ingredients TABLE (
                    [Name] nvarchar(200) NOT NULL, [Quantity] decimal(18,2) NULL,
                    [Unit] nvarchar(50) NULL, [Notes] nvarchar(max) NULL,
                    [SortOrder] int NOT NULL
                );
                INSERT INTO @Ingredients ([Name], [Quantity], [Unit], [Notes], [SortOrder])
                VALUES
                    (N'Cream cheese', 250, N'g', N'Full-fat, softened', 1),
                    (N'Streaky bacon', 100, N'g', N'Cook until crisp, cool and finely chop', 2),
                    (N'Maple syrup', 20, N'g', N'Pure maple syrup; increase to 30 g for a sweeter version', 3),
                    (N'Fresh chives', 10, N'g', N'Finely chopped', 4),
                    (N'Black pepper', 0.5, N'tsp', N'Freshly ground', 5),
                    (N'Garlic powder', 0.25, N'tsp', NULL, 6),
                    (N'Dijon mustard', 1, N'tsp', N'Optional, recommended for a savoury balance', 7),
                    (N'Smoked paprika', NULL, NULL, N'Optional pinch', 8);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                        ROW_NUMBER() OVER (PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]) AS rn
                    FROM @Ingredients AS i
                    WHERE NOT EXISTS (
                        SELECT 1 FROM [Ingredient] e WHERE LOWER(e.[Name]) = LOWER(i.[Name])
                    )
                )
                INSERT INTO [Ingredient] ([Name], [DefaultUnit])
                SELECT [Name], [Unit] FROM Missing WHERE rn = 1;

                INSERT INTO [RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [Unit], [Notes], [SortOrder])
                SELECT @RecipeId, e.[IngredientId], i.[Quantity], i.[Unit], i.[Notes], i.[SortOrder]
                FROM @Ingredients i
                CROSS APPLY (
                    SELECT TOP(1) x.[IngredientId] FROM [Ingredient] x
                    WHERE LOWER(x.[Name]) = LOWER(i.[Name])
                    ORDER BY x.[IngredientId]
                ) e;

                INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
                VALUES
                    (@RecipeId, 1, N'Fry 100 g streaky bacon over medium heat until deeply golden and crisp, about 5–8 minutes depending on thickness. Drain on kitchen paper and cool completely. Chop finely and reserve a tablespoon for garnish.'),
                    (@RecipeId, 2, N'Beat 250 g softened full-fat cream cheese with a whisk or mixer for 1–2 minutes until fluffy.'),
                    (@RecipeId, 3, N'Mix in 20 g pure maple syrup, ½ tsp black pepper and ¼ tsp garlic powder. For a more savoury version, add 1 tsp Dijon mustard and an optional pinch of smoked paprika.'),
                    (@RecipeId, 4, N'Fold in the cooled chopped bacon and 10 g finely chopped fresh chives, reserving a little chive for garnish.'),
                    (@RecipeId, 5, N'Cover and chill for at least 30 minutes to develop the flavours. Top with reserved bacon and chives, then spread on toasted bagels. Refrigerate leftovers promptly in an airtight container and use within 4 days.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'American'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'American');
                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'American');

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Breakfast'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Breakfast');
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Spread'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Spread');
                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) IN (LOWER(N'Breakfast'), LOWER(N'Spread'));
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep user recipe edits and any meal-plan links intact.
    }
}
