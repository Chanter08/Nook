using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Seeds five breakfast bagel cream-cheese schmears as separate Nook recipes.
/// Name guards avoid duplicates and preserve existing user edits and meal plans.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261026153000_AddBreakfastBagelSchmears")]
public partial class AddBreakfastBagelSchmears : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Honey and Toasted Walnut Schmear'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Honey and Toasted Walnut Schmear', N'Fluffy full-fat cream cheese with lightly toasted walnuts, honey and cinnamon. A sweet, crunchy New York-style breakfast bagel filling. Makes approximately four generous bagel fillings; chill for 30 minutes before serving.',
                    10, 5, 4, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Cream cheese', 200, N'g', N'Full-fat; softened slightly for whipping', 1),
                    (N'Walnuts', 25, N'g', N'Finely chopped after toasting', 2),
                    (N'Honey', 20, N'g', N'Mild runny honey', 3),
                    (N'Ground cinnamon', 0.25, N'tsp', N'Optional, to taste', 4);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                           ROW_NUMBER() OVER (PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]) AS rn
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
                    (@RecipeId, 1, N'Spread 25 g walnuts in a dry frying pan over medium-low heat. Toast for 3–5 minutes, stirring often, until fragrant. Do not let them burn. Remove from the pan, cool completely, then chop finely.'),
                    (@RecipeId, 2, N'Place 200 g full-fat cream cheese in a bowl. Let stand at room temperature for about 15–20 minutes if very firm, without leaving it out for an extended period. Beat with a hand mixer or whisk for 1–2 minutes until lighter and fluffy.'),
                    (@RecipeId, 3, N'Add 20 g honey and ¼ tsp cinnamon. Beat briefly until evenly combined, then fold in the cooled toasted walnuts.'),
                    (@RecipeId, 4, N'Cover and refrigerate for at least 30 minutes to let the flavours settle. Spread generously (around 50–65 g per bagel) on freshly toasted bagels; keep leftovers refrigerated in a covered container for 3–4 days.');

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

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Roasted Garlic and Parmesan Schmear'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Roasted Garlic and Parmesan Schmear', N'A thick, savoury whipped cream cheese schmear with mellow roasted garlic, finely grated Parmesan, parsley and black pepper. Delicious as the sole filling in a warm breakfast bagel. Makes about four generous portions.',
                    10, 30, 4, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Cream cheese', 200, N'g', N'Full-fat; softened slightly', 1),
                    (N'Garlic', 4, N'clove', N'3–4 medium cloves; roast until soft and sweet', 2),
                    (N'Parmesan', 20, N'g', N'Finely grated', 3),
                    (N'Fresh parsley', 1, N'tsp', N'Finely chopped', 4),
                    (N'Olive oil', 5, N'ml', N'About 1 tsp for roasting garlic', 5),
                    (N'Black pepper', 0.25, N'tsp', N'Freshly ground; add to taste', 6);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                           ROW_NUMBER() OVER (PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]) AS rn
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
                    (@RecipeId, 1, N'Heat the oven to 190°C (170°C fan). Put 3–4 unpeeled garlic cloves on a small piece of foil, drizzle with approximately 1 tsp olive oil, seal into a loose parcel and roast for about 25–35 minutes until the cloves are very soft. Check sooner if small.'),
                    (@RecipeId, 2, N'Allow garlic to cool completely, then squeeze the softened flesh from its skins and mash to a smooth paste with a fork.'),
                    (@RecipeId, 3, N'Beat 200 g full-fat cream cheese for 1–2 minutes until airy and spreadable. Stir in 20 g finely grated Parmesan, the mashed garlic, 1 tsp finely chopped parsley and black pepper.'),
                    (@RecipeId, 4, N'Taste before adding salt: Parmesan is naturally salty. Cover and chill for at least 30 minutes, then use around 50–60 g per breakfast bagel. Refrigerate leftovers promptly and use within 3–4 days.');

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

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Brown Sugar and Cinnamon Schmear'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Brown Sugar and Cinnamon Schmear', N'Fluffy cream cheese with dark, caramel-like brown sugar, fragrant cinnamon and vanilla. A mildly sweet cinnamon-roll-style New York bagel schmear for breakfast. Makes roughly four generous servings.',
                    5, 0, 4, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Cream cheese', 200, N'g', N'Full-fat; slightly softened', 1),
                    (N'Soft brown sugar', 20, N'g', N'Light or dark soft brown sugar', 2),
                    (N'Ground cinnamon', 0.5, N'tsp', N'Adjust to taste', 3),
                    (N'Vanilla extract', 0.5, N'tsp', N'Pure vanilla extract if available', 4),
                    (N'Salt', NULL, NULL, N'A tiny pinch, optional', 5);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                           ROW_NUMBER() OVER (PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]) AS rn
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
                    (@RecipeId, 1, N'Leave 200 g full-fat cream cheese at room temperature for 15–20 minutes if very firm, so it can be whipped easily.'),
                    (@RecipeId, 2, N'Beat the cream cheese with 20 g soft brown sugar for approximately 2 minutes until creamy, light and evenly sweetened.'),
                    (@RecipeId, 3, N'Add ½ tsp cinnamon, ½ tsp vanilla extract and an optional tiny pinch of salt. Beat for another 20–30 seconds, scraping down the bowl so no sugar collects at the bottom.'),
                    (@RecipeId, 4, N'Cover and chill for 30 minutes to allow the sugar to dissolve and flavours to mingle. Spread a thick layer (about 50–60 g) over a plain or cinnamon bagel. Keep refrigerated and use within 3–4 days.');

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

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Roasted Red Pepper and Smoked Paprika Schmear'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Roasted Red Pepper and Smoked Paprika Schmear', N'A breakfast-bagel-ready savoury schmear combining sweet roasted red bell pepper, smoked paprika, garlic and whipped cream cheese. Dry the pepper thoroughly for a thick deli-style texture. Makes about four generous servings.',
                    10, 25, 4, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Cream cheese', 200, N'g', N'Full-fat; softened slightly', 1),
                    (N'Red bell pepper', 1, N'piece', N'Medium; roast, peel, deseed and pat dry before adding', 2),
                    (N'Smoked paprika', 0.5, N'tsp', N'Use sweet smoked paprika rather than hot', 3),
                    (N'Garlic', 1, N'clove', N'Small; finely grated', 4),
                    (N'Olive oil', 5, N'ml', N'About 1 tsp', 5),
                    (N'Salt', NULL, NULL, N'Small pinch, to taste', 6),
                    (N'Black pepper', NULL, NULL, N'Freshly ground, to taste', 7);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                           ROW_NUMBER() OVER (PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]) AS rn
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
                    (@RecipeId, 1, N'Heat the oven to 220°C (200°C fan). Place the whole red bell pepper on a baking tray, rub with about 1 tsp olive oil and roast for 20–25 minutes, turning once, until the skin is well blistered and the flesh softened.'),
                    (@RecipeId, 2, N'Place the hot pepper in a covered bowl for 10 minutes. Once cool enough to handle, peel off the skin, remove stem and seeds, and pat the flesh very dry with kitchen paper. Cool completely, then finely chop it. Excess liquid will make the schmear runny.'),
                    (@RecipeId, 3, N'Beat 200 g full-fat cream cheese for 1–2 minutes until light and fluffy. Mix in ½ tsp sweet smoked paprika and 1 small finely grated garlic clove, then gently fold in the chopped roasted pepper.'),
                    (@RecipeId, 4, N'Season lightly with salt and black pepper, cover and refrigerate for 30–60 minutes so the filling firms up. Spoon generously onto toasted bagels. Keep refrigerated and use within 3–4 days.');

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

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Garlic and Chive Schmear'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Garlic and Chive Schmear', N'A lighter but still thick and creamy breakfast bagel schmear, made with whipped cream cheese, Greek yogurt, fresh chives, garlic powder and black pepper. Intended as the main bagel filling; makes 3–4 portions.',
                    5, 0, 4, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Cream cheese', 150, N'g', N'Full-fat; slightly softened', 1),
                    (N'Greek yogurt', 50, N'g', N'Thick plain Greek yogurt; avoid watery yogurt', 2),
                    (N'Fresh chives', 1, N'tsp', N'Finely chopped; add more to taste', 3),
                    (N'Garlic powder', 0.25, N'tsp', N'Adjust to taste', 4),
                    (N'Black pepper', NULL, NULL, N'Freshly ground, to taste', 5);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                           ROW_NUMBER() OVER (PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]) AS rn
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
                    (@RecipeId, 1, N'Let 150 g cream cheese soften slightly at room temperature for about 15 minutes. Chop the fresh chives as finely as possible.'),
                    (@RecipeId, 2, N'Beat the cream cheese for 1–2 minutes until fluffy. Add 50 g thick Greek yogurt, a spoonful at a time, mixing only until smooth so the spread stays thick.'),
                    (@RecipeId, 3, N'Fold in 1 tsp chopped chives, ¼ tsp garlic powder and a little black pepper. Taste and increase the chives if you prefer a stronger fresh herb flavour.'),
                    (@RecipeId, 4, N'Cover and refrigerate for at least 30 minutes before serving. Spread approximately 50 g onto each toasted bagel. Because it contains yogurt and fresh herbs, keep refrigerated and use within 3 days.');

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
        // Intentionally non-destructive: keep user-edited recipes and linked meal plans.
    }
}
