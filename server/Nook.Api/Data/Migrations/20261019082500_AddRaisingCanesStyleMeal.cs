using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Seeds an unofficial Raising Cane's-inspired copycat meal as four recipes.
/// Photos are publicly hosted Wikimedia Commons thumbnails; licensing and
/// photographer credits are documented in docs/recipe-image-credits.md.
/// Name guards preserve existing user-created recipes.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261019082500_AddRaisingCanesStyleMeal")]
public partial class AddRaisingCanesStyleMeal : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Raising Cane''s-Style Chicken Tenders'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                ) VALUES (
                    N'Raising Cane''s-Style Chicken Tenders', N'Unofficial copycat: buttermilk-marinated tenderloins with a light craggy, crisp flour coating. Photo: Jami430 / Wikimedia Commons (CC BY-SA 4.0). Marinate 8–12 hours, or up to 24 hours refrigerated.',
                    20, 15, 2,
                    N'https://upload.wikimedia.org/wikipedia/commons/thumb/0/0c/Chicken_fingers_at_Raising_Cane%27s_in_Baton_Rouge%2C_Louisiana.jpg/800px-Chicken_fingers_at_Raising_Cane%27s_in_Baton_Rouge%2C_Louisiana.jpg', SYSUTCDATETIME(), 0,
                    NULL, NULL, NULL, NULL
                );

                DECLARE @RecipeId int = CONVERT(int, SCOPE_IDENTITY());

                DECLARE @Items TABLE (
                    [Name] nvarchar(200) NOT NULL,
                    [Quantity] decimal(18,2) NULL,
                    [Unit] nvarchar(50) NULL,
                    [Notes] nvarchar(max) NULL,
                    [SortOrder] int NOT NULL
                );

                INSERT INTO @Items ([Name], [Quantity], [Unit], [Notes], [SortOrder])
                VALUES
                    (N'Chicken tenderloins', 500, N'g', N'Chicken breast sliced into strips can be substituted', 1),
                    (N'Buttermilk', 250, N'ml', N'For marinating the chicken', 2),
                    (N'Salt', 5, N'g', N'For marinade', 3),
                    (N'Garlic powder', 0.5, N'tsp', N'For the marinade', 4),
                    (N'White pepper', 0.25, N'tsp', N'For the marinade', 5),
                    (N'Paprika', 0.25, N'tsp', N'For the marinade', 6),
                    (N'Plain flour', 180, N'g', N'For dredging', 7),
                    (N'Cornflour', 30, N'g', N'Cornstarch; for dredging', 8),
                    (N'Garlic powder', 1, N'tsp', N'For dredging', 9),
                    (N'Onion powder', 0.5, N'tsp', N'For dredging', 10),
                    (N'White pepper', 0.5, N'tsp', N'For dredging', 11),
                    (N'Paprika', 0.5, N'tsp', N'For dredging', 12),
                    (N'Baking powder', 0.5, N'tsp', N'Not baking soda; for dredging', 13),
                    (N'Salt', 0.5, N'tsp', N'For dredging', 14),
                    (N'Neutral frying oil', 1000, N'ml', N'Approximate quantity required for 5–6 cm depth in a suitable deep heavy pot; most oil is not absorbed', 15);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                        ROW_NUMBER() OVER (
                            PARTITION BY LOWER(i.[Name])
                            ORDER BY i.[SortOrder]
                        ) AS rn
                    FROM @Items AS i
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
                SELECT @RecipeId, ingredient.[IngredientId], i.[Quantity], i.[Unit], i.[Notes], i.[SortOrder]
                FROM @Items AS i
                CROSS APPLY (
                    SELECT TOP (1) existing.[IngredientId]
                    FROM [Ingredient] AS existing
                    WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
                    ORDER BY existing.[IngredientId]
                ) AS ingredient;

                INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
                VALUES
                    (@RecipeId, 1, N'At least 8 hours ahead, combine 250 ml buttermilk, 5 g salt, ½ tsp garlic powder, ¼ tsp white pepper and ¼ tsp paprika. Add chicken tenderloins, cover and refrigerate for 8–12 hours (up to 24 hours). Keep at or below 4°C.'),
                    (@RecipeId, 2, N'When ready to cook, combine 180 g plain flour, 30 g cornflour, 1 tsp garlic powder, ½ tsp onion powder, ½ tsp white pepper, ½ tsp paprika, ½ tsp baking powder and ½ tsp salt.'),
                    (@RecipeId, 3, N'Add 1–2 tablespoons of buttermilk marinade to the dry flour coating and rub between your fingers to create small irregular clumps. Lift the chicken from the marinade and let excess drip off; firmly press each piece into the seasoned flour until coated all over. Discard the leftover raw-chicken marinade.'),
                    (@RecipeId, 4, N'Let coated tenders rest on a wire rack for 10 minutes. Heat 5–6 cm neutral frying oil in a deep, heavy pot to 175°C; never fill the pot more than halfway.'),
                    (@RecipeId, 5, N'Fry 2–3 tenders at a time for roughly 4–6 minutes, turning as needed, until richly golden and the thickest centre reaches at least 74°C. Reheat oil to 175°C between batches.'),
                    (@RecipeId, 6, N'Drain on a wire rack rather than paper towels to keep the coating crisp. Serve immediately with Copycat Cane''s-Style Dipping Sauce, Garlic-Butter Texas Toast and Crinkle-Cut Fries.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'American'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'American');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'American');
                
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Lunch'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Lunch');
                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) = LOWER(N'Lunch');

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');
                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) = LOWER(N'Dinner');
            END;
            """);

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Copycat Cane''s-Style Dipping Sauce'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                ) VALUES (
                    N'Copycat Cane''s-Style Dipping Sauce', N'Peppery, creamy, tangy mayonnaise-ketchup dipping sauce. Unofficial copycat. Photo: Authalic / Wikimedia Commons (CC BY 3.0), illustrative fry sauce.',
                    5, 0, 4,
                    N'https://upload.wikimedia.org/wikipedia/commons/thumb/f/ff/Frysauce.JPG/800px-Frysauce.JPG', SYSUTCDATETIME(), 0,
                    NULL, NULL, NULL, NULL
                );

                DECLARE @RecipeId int = CONVERT(int, SCOPE_IDENTITY());

                DECLARE @Items TABLE (
                    [Name] nvarchar(200) NOT NULL,
                    [Quantity] decimal(18,2) NULL,
                    [Unit] nvarchar(50) NULL,
                    [Notes] nvarchar(max) NULL,
                    [SortOrder] int NOT NULL
                );

                INSERT INTO @Items ([Name], [Quantity], [Unit], [Notes], [SortOrder])
                VALUES
                    (N'Mayonnaise', 100, N'g', NULL, 1),
                    (N'Ketchup', 45, N'g', NULL, 2),
                    (N'Worcestershire sauce', 1, N'tsp', NULL, 3),
                    (N'Garlic powder', 0.5, N'tsp', NULL, 4),
                    (N'Black pepper', 0.75, N'tsp', N'Freshly ground; essential to the peppery flavour', 5),
                    (N'Paprika', 0.25, N'tsp', NULL, 6),
                    (N'Salt', NULL, NULL, N'Tiny pinch, to taste', 7);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                        ROW_NUMBER() OVER (
                            PARTITION BY LOWER(i.[Name])
                            ORDER BY i.[SortOrder]
                        ) AS rn
                    FROM @Items AS i
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
                SELECT @RecipeId, ingredient.[IngredientId], i.[Quantity], i.[Unit], i.[Notes], i.[SortOrder]
                FROM @Items AS i
                CROSS APPLY (
                    SELECT TOP (1) existing.[IngredientId]
                    FROM [Ingredient] AS existing
                    WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
                    ORDER BY existing.[IngredientId]
                ) AS ingredient;

                INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
                VALUES
                    (@RecipeId, 1, N'Measure the mayonnaise and ketchup into a small bowl and stir together until uniformly creamy.'),
                    (@RecipeId, 2, N'Stir in Worcestershire sauce, garlic powder, freshly ground black pepper, paprika and a tiny pinch of salt.'),
                    (@RecipeId, 3, N'Cover and refrigerate for at least 30 minutes (a few hours is even better) to let the flavours develop.'),
                    (@RecipeId, 4, N'Taste and adjust black pepper or Worcestershire sauce before serving. Keep refrigerated, and use a clean spoon for serving.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'American'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'American');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'American');
                
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Sauce'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Sauce');
                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) = LOWER(N'Sauce');
            END;
            """);

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Garlic-Butter Texas Toast (Cane''s-Style)'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                ) VALUES (
                    N'Garlic-Butter Texas Toast (Cane''s-Style)', N'Thick white bread griddled in garlic butter until crisp and golden outside, soft inside. Photo: FranHogan / Wikimedia Commons (CC BY-SA 4.0), illustrative garlic toast.',
                    5, 5, 2,
                    N'https://upload.wikimedia.org/wikipedia/commons/thumb/4/44/Toast_with_garlic_and_butter.jpg/800px-Toast_with_garlic_and_butter.jpg', SYSUTCDATETIME(), 0,
                    NULL, NULL, NULL, NULL
                );

                DECLARE @RecipeId int = CONVERT(int, SCOPE_IDENTITY());

                DECLARE @Items TABLE (
                    [Name] nvarchar(200) NOT NULL,
                    [Quantity] decimal(18,2) NULL,
                    [Unit] nvarchar(50) NULL,
                    [Notes] nvarchar(max) NULL,
                    [SortOrder] int NOT NULL
                );

                INSERT INTO @Items ([Name], [Quantity], [Unit], [Notes], [SortOrder])
                VALUES
                    (N'Thick-sliced white bread', 2, N'slice', N'Texas-toast thickness, roughly 2–2.5 cm', 1),
                    (N'Butter', 25, N'g', N'Softened', 2),
                    (N'Garlic', 1, N'clove', N'Finely grated, or use ¼ tsp garlic powder', 3),
                    (N'Salt', NULL, NULL, N'Small pinch, optional', 4),
                    (N'Parsley', NULL, NULL, N'Optional, finely chopped', 5);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                        ROW_NUMBER() OVER (
                            PARTITION BY LOWER(i.[Name])
                            ORDER BY i.[SortOrder]
                        ) AS rn
                    FROM @Items AS i
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
                SELECT @RecipeId, ingredient.[IngredientId], i.[Quantity], i.[Unit], i.[Notes], i.[SortOrder]
                FROM @Items AS i
                CROSS APPLY (
                    SELECT TOP (1) existing.[IngredientId]
                    FROM [Ingredient] AS existing
                    WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
                    ORDER BY existing.[IngredientId]
                ) AS ingredient;

                INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
                VALUES
                    (@RecipeId, 1, N'Mix softened butter with finely grated garlic and a pinch of salt; add parsley if you like.'),
                    (@RecipeId, 2, N'Spread the garlic butter thinly over the cut faces of the thick bread slices.'),
                    (@RecipeId, 3, N'Heat a large frying pan over medium heat. Toast bread buttered-side down for 2–3 minutes until deep golden; optionally butter and toast the second side for 1–2 minutes.'),
                    (@RecipeId, 4, N'Serve immediately with fried chicken tenders and dipping sauce.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'American'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'American');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'American');
                
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Side'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Side');
                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) = LOWER(N'Side');
            END;
            """);

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Crispy Crinkle-Cut Fries (Cane''s-Style)'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                ) VALUES (
                    N'Crispy Crinkle-Cut Fries (Cane''s-Style)', N'Simple golden crinkle-cut fries, crisp on the outside and fluffy inside; a classic side for copycat Cane''s chicken. Photo: Kurtkaiser / Wikimedia Commons (CC0).',
                    5, 25, 2,
                    N'https://upload.wikimedia.org/wikipedia/commons/thumb/1/18/French_fries_crinkle_cut.jpg/800px-French_fries_crinkle_cut.jpg', SYSUTCDATETIME(), 0,
                    NULL, NULL, NULL, NULL
                );

                DECLARE @RecipeId int = CONVERT(int, SCOPE_IDENTITY());

                DECLARE @Items TABLE (
                    [Name] nvarchar(200) NOT NULL,
                    [Quantity] decimal(18,2) NULL,
                    [Unit] nvarchar(50) NULL,
                    [Notes] nvarchar(max) NULL,
                    [SortOrder] int NOT NULL
                );

                INSERT INTO @Items ([Name], [Quantity], [Unit], [Notes], [SortOrder])
                VALUES
                    (N'Frozen crinkle-cut fries', 400, N'g', N'Use unseasoned fries; cook from frozen', 1),
                    (N'Neutral oil', 1, N'tsp', N'Optional for air frying; omit if the fries are already coated', 2),
                    (N'Fine salt', NULL, NULL, N'To season immediately after cooking', 3);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                        ROW_NUMBER() OVER (
                            PARTITION BY LOWER(i.[Name])
                            ORDER BY i.[SortOrder]
                        ) AS rn
                    FROM @Items AS i
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
                SELECT @RecipeId, ingredient.[IngredientId], i.[Quantity], i.[Unit], i.[Notes], i.[SortOrder]
                FROM @Items AS i
                CROSS APPLY (
                    SELECT TOP (1) existing.[IngredientId]
                    FROM [Ingredient] AS existing
                    WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
                    ORDER BY existing.[IngredientId]
                ) AS ingredient;

                INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
                VALUES
                    (@RecipeId, 1, N'Preheat an air fryer to 200°C, or a fan oven to 220°C. Keep the crinkle-cut fries frozen until cooking.'),
                    (@RecipeId, 2, N'For an air fryer, toss with the optional oil if needed, then cook in a single layer for about 15–20 minutes, shaking halfway. Cook in batches if necessary.'),
                    (@RecipeId, 3, N'For an oven, spread on a preheated baking tray and bake for around 20–25 minutes, turning halfway, until golden and crisp. Follow the packet directions if they differ.'),
                    (@RecipeId, 4, N'Season immediately with fine salt and serve hot beside Raising Cane''s-Style Chicken Tenders, Copycat Cane''s-Style Dipping Sauce and Texas toast.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'American'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'American');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'American');
                
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Side'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Side');
                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) = LOWER(N'Side');
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Deliberately non-destructive: recipes may have been edited or
        // referenced by user meal plans after being seeded.
    }
}
