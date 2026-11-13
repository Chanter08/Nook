using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>Safely seed two 500 g wing recipes on deployment without overwriting user edits.</summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261113203000_AddHoneyMustardAndBuffaloWings")]
public partial class AddHoneyMustardAndBuffaloWings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Sticky Honey Mustard Chicken Wings'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (N'Sticky Honey Mustard Chicken Wings', N'Crispy oven or air fryer chicken wings coated after baking in a glossy, buttery honey, Dijon and yellow mustard glaze. Mild and sweet-tangy. Ingredients are measured for 500 g wings, ideal as one flavour in a mixed platter.', 10, 50, 2, NULL,
                    SYSUTCDATETIME(), 0, NULL, NULL, NULL, NULL);

                DECLARE @RecipeId int = CONVERT(int, SCOPE_IDENTITY());
                DECLARE @Ingredients TABLE (
                    [Name] nvarchar(200) NOT NULL, [Quantity] decimal(18,2) NULL,
                    [Unit] nvarchar(50) NULL, [Notes] nvarchar(max) NULL, [SortOrder] int NOT NULL
                );
                INSERT INTO @Ingredients ([Name], [Quantity], [Unit], [Notes], [SortOrder])
                VALUES
                    (N'Chicken wings', 500, N'g', N'Flats and drumettes; pat completely dry', 1),
                    (N'Aluminium-free baking powder', 2, N'g', N'About 1/2 tsp; NOT baking soda; helps crisp the skin', 2),
                    (N'Neutral cooking oil', 8, N'ml', N'For the wings, about 1/2 tbsp', 3),
                    (N'Garlic powder', 2, N'g', N'About 1/2 tsp for wings and a pinch for glaze', 4),
                    (N'Paprika', 1, N'g', N'For dry seasoning', 5),
                    (N'Salt', 1, N'g', N'Adjust to taste', 6),
                    (N'Black pepper', 0.5, N'g', N'Freshly ground', 7),
                    (N'Honey', 32, N'g', N'About 1 1/2 tbsp; up to 42 g for sweeter glaze', 8),
                    (N'Dijon mustard', 15, N'g', N'About 1 tbsp', 9),
                    (N'Yellow mustard', 15, N'g', N'About 1 tbsp', 10),
                    (N'Unsalted butter', 15, N'g', N'For the warm glaze', 11),
                    (N'Apple cider vinegar', 5, N'ml', N'About 1 tsp', 12);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                        ROW_NUMBER() OVER (PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]) AS rn
                    FROM @Ingredients i
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
                    (@RecipeId, 1, N'Heat the oven to 200°C fan (220°C conventional). Pat 500 g chicken wings very dry. Toss with 8 ml neutral oil, 2 g aluminium-free baking powder (not baking soda), garlic powder, paprika, salt and black pepper.'),
                    (@RecipeId, 2, N'Spread the wings apart on a wire rack over a lined roasting tray. Roast for 40–50 minutes, turning halfway, until golden and crispy. Cook until the thickest part reaches at least 74°C.'),
                    (@RecipeId, 3, N'Towards the end of cooking, melt 15 g butter over low heat. Stir in 32 g honey, 15 g Dijon mustard, 15 g yellow mustard and 5 ml apple cider vinegar; whisk with a pinch of garlic powder and pepper. Warm gently for 1–2 minutes, without boiling, until glossy.'),
                    (@RecipeId, 4, N'Toss the freshly cooked, still-hot wings through the honey mustard glaze and serve immediately. For extra sweetness increase the honey to 42 g. Do not glaze before roasting, or the skin will soften.'),
                    (@RecipeId, 5, N'Air fryer alternative: cook the unglazed seasoned wings in a single, uncrowded layer at 200°C for about 20–25 minutes, turning halfway and checking for 74°C; toss with warm glaze after cooking.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'American'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'American');
                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'American');

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');
                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) = LOWER(N'Dinner');
            END;
            """);

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Classic Buffalo Chicken Wings'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (N'Classic Buffalo Chicken Wings', N'Crispy oven or air fryer chicken wings tossed in a Frank’s RedHot-style buttery, tangy Buffalo sauce. Adjustable mild-to-medium heat. Ingredients are for 500 g wings, one flavour in a mixed wing platter.', 10, 50, 2, NULL,
                    SYSUTCDATETIME(), 0, NULL, NULL, NULL, NULL);

                DECLARE @RecipeId int = CONVERT(int, SCOPE_IDENTITY());
                DECLARE @Ingredients TABLE (
                    [Name] nvarchar(200) NOT NULL, [Quantity] decimal(18,2) NULL,
                    [Unit] nvarchar(50) NULL, [Notes] nvarchar(max) NULL, [SortOrder] int NOT NULL
                );
                INSERT INTO @Ingredients ([Name], [Quantity], [Unit], [Notes], [SortOrder])
                VALUES
                    (N'Chicken wings', 500, N'g', N'Flats and drumettes; pat completely dry', 1),
                    (N'Aluminium-free baking powder', 2, N'g', N'About 1/2 tsp; NOT baking soda', 2),
                    (N'Neutral cooking oil', 8, N'ml', N'For coating the raw wings', 3),
                    (N'Garlic powder', 2, N'g', N'Split between wing seasoning and sauce', 4),
                    (N'Paprika', 1, N'g', N'For dry seasoning', 5),
                    (N'Salt', 1, N'g', N'Adjust for the saltiness of the hot sauce', 6),
                    (N'Black pepper', 0.5, N'g', N'Freshly ground', 7),
                    (N'Cayenne pepper hot sauce', 45, N'ml', N'3 tbsp Frank''s RedHot Original or similar; use 30 ml for milder wings', 8),
                    (N'Unsalted butter', 25, N'g', N'Use 30 g with less hot sauce for a milder version', 9),
                    (N'Honey', 7, N'g', N'About 1 tsp; up to 14 g for milder, sweeter wings', 10),
                    (N'Worcestershire sauce', 2.5, N'ml', N'About 1/2 tsp', 11),
                    (N'Smoked paprika', 0.3, N'g', N'Pinch for sauce', 12);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                        ROW_NUMBER() OVER (PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]) AS rn
                    FROM @Ingredients i
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
                    (@RecipeId, 1, N'Heat oven to 200°C fan (220°C conventional). Pat 500 g wings very dry, then toss with 8 ml neutral oil, 2 g aluminium-free baking powder (not baking soda), garlic powder, paprika, salt and black pepper.'),
                    (@RecipeId, 2, N'Spread the wings on a wire rack above a tray, leaving space between pieces. Roast for 40–50 minutes, turning once, until deeply golden and crisp. Check that the centre reaches at least 74°C.'),
                    (@RecipeId, 3, N'Just before serving, melt 25 g unsalted butter over low heat; whisk in 45 ml cayenne hot sauce (such as Frank''s RedHot Original), 7 g honey, 2.5 ml Worcestershire sauce, a pinch of garlic powder and smoked paprika. Warm gently until smooth; do not let it boil.'),
                    (@RecipeId, 4, N'For milder Buffalo sauce use 30 ml hot sauce, 30 g butter and up to 14 g honey. For classic heat, keep the standard proportions.'),
                    (@RecipeId, 5, N'Toss the piping-hot cooked wings in the warm Buffalo sauce immediately before serving. Serve with optional ranch or blue cheese dip and celery or carrot sticks.'),
                    (@RecipeId, 6, N'Air fryer alternative: cook the unglazed wings at 200°C for roughly 20–25 minutes, turning halfway in uncrowded batches and verifying 74°C; sauce only after cooking.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'American'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'American');
                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'American');

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');
                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) = LOWER(N'Dinner');
            END;

            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Preserve user recipes, edits and meal-plan links when rolling back.
    }
}
