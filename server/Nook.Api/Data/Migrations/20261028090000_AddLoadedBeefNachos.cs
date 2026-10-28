using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Seeds fully loaded beef nachos for movie night, including smoky spicy mayo
/// and garlic-lime crema. The name guard prevents duplicate recipe records.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261028090000_AddLoadedBeefNachos")]
public partial class AddLoadedBeefNachos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1 FROM [Recipe]
                WHERE LOWER([Name]) = LOWER(N'Fully Loaded Beef Nachos')
            )
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Fully Loaded Beef Nachos',
                    N'Movie-night nachos for two: crunchy tortilla chips layered with smoky taco-spiced beef mince, sautéed bell and Italian frying peppers and melted cheese. Served with two easy dips: smoky spicy mayo and a fresh garlic-lime yoghurt crema. Keep the beef thick rather than watery and add dips at the table so the chips stay crisp.',
                    15, 15, 2, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Beef mince', 325, N'g', N'300–350 g; preferably lean or medium-fat', 1),
                    (N'Tortilla chips', 180, N'g', N'150–200 g plain or lightly salted nacho chips', 2),
                    (N'Bell pepper', 1, N'piece', N'Medium; diced', 3),
                    (N'Italian frying pepper', 0.5, N'piece', N'Finely chopped; optional if unavailable', 4),
                    (N'Onion', 1, N'piece', N'Small; finely diced, optional', 5),
                    (N'Garlic', 2.5, N'clove', N'2 cloves for beef; ½ small clove grated for crema', 6),
                    (N'Tomato paste', 1, N'tbsp', N'For rich, savoury beef', 7),
                    (N'Cheddar cheese', 125, N'g', N'100–150 g; grated, or mix with mozzarella', 8),
                    (N'Water or beef stock', 70, N'ml', N'60–80 ml, added as needed; simmer off excess liquid', 9),
                    (N'Olive oil', 1, N'tsp', N'For frying if required by the mince', 10),
                    (N'Smoked paprika', 1.5, N'tsp', N'1 tsp for beef and ½ tsp for spicy mayo', 11),
                    (N'Ground cumin', 1, N'tsp', N'Beef seasoning', 12),
                    (N'Dried oregano', 0.5, N'tsp', N'Beef seasoning', 13),
                    (N'Chilli powder', 0.5, N'tsp', N'Adjust to taste', 14),
                    (N'Garlic powder', 0.5, N'tsp', N'Beef seasoning', 15),
                    (N'Salt', NULL, NULL, N'To taste; chips and cheese are salty', 16),
                    (N'Black pepper', NULL, NULL, N'Freshly ground to taste', 17),
                    (N'Mayonnaise', 2, N'tbsp', N'For smoky spicy mayo', 18),
                    (N'Hot sauce', 1, N'tsp', N'For smoky spicy mayo; adjust to taste', 19),
                    (N'Greek yoghurt', 3, N'tbsp', N'Or sour cream; for garlic-lime crema', 20),
                    (N'Lime', 1, N'piece', N'Juice to taste for both dips; lemon may be substituted', 21),
                    (N'Cherry tomatoes', NULL, NULL, N'Optional fresh topping; dice and drain well', 22),
                    (N'Spring onions', NULL, NULL, N'Optional topping; thinly sliced', 23),
                    (N'Jalapeños', NULL, NULL, N'Optional topping, sliced', 24);

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
                SELECT [Name], [Unit]
                FROM Missing
                WHERE rn = 1;

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
                    (@RecipeId, 1, N'Preheat the oven to 200°C conventional (180°C fan). Finely dice 1 bell pepper, ½ Italian frying pepper and an optional small onion. Mince 2 garlic cloves and grate another ½ small clove for the crema.'),
                    (@RecipeId, 2, N'Brown 300–350 g beef mince in a frying pan over medium-high heat, using a teaspoon of oil only if needed. Break it up and let it develop some golden, crisp edges. Drain excessive fat if necessary.'),
                    (@RecipeId, 3, N'Add the diced peppers and optional onion. Sauté for 4–5 minutes until softened; stir in 2 minced garlic cloves and cook for 30 seconds.'),
                    (@RecipeId, 4, N'Stir in 1 tbsp tomato paste, 1 tsp smoked paprika, 1 tsp ground cumin, ½ tsp dried oregano, ½ tsp chilli powder and ½ tsp garlic powder. Cook for 1 minute, then add about 70 ml water or beef stock. Simmer until the beef is thick and glossy, not wet, and season with salt and pepper to taste.'),
                    (@RecipeId, 5, N'For the smoky spicy mayo, stir together 2 tbsp mayonnaise, 1 tsp hot sauce, ½ tsp smoked paprika and a small squeeze of lime or lemon. Taste and adjust the heat.'),
                    (@RecipeId, 6, N'For the garlic-lime crema, combine 3 tbsp Greek yoghurt or sour cream, ½ small finely grated garlic clove, a squeeze of lime or lemon and a pinch of salt. Set both sauces aside, chilled if preparing ahead.'),
                    (@RecipeId, 7, N'On a large ovenproof tray, spread half of the 150–200 g tortilla chips. Spoon over half the cooked beef, then sprinkle with half of the 100–150 g grated cheese. Repeat with the remaining chips, beef and cheese so every layer gets toppings.'),
                    (@RecipeId, 8, N'Bake for approximately 5–8 minutes until the cheese has fully melted and the chips are hot. Watch carefully so exposed chips do not burn.'),
                    (@RecipeId, 9, N'Add optional diced cherry tomatoes, spring onions or jalapeños immediately before serving. Serve the smoky mayo and garlic-lime crema in small bowls on the side so the nachos remain crunchy. Eat straight away.');

                IF NOT EXISTS (
                    SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Tex-Mex')
                )
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Tex-Mex');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'Tex-Mex');

                IF NOT EXISTS (
                    SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner')
                )
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');

                IF NOT EXISTS (
                    SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Snack')
                )
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Snack');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) IN (LOWER(N'Dinner'), LOWER(N'Snack'));
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Preserve recipe edits and any linked meal plans.
    }
}
