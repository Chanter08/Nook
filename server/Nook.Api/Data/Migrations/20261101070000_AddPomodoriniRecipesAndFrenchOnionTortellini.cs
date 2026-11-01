using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Seeds two canned pomodorini dinners and a French-onion beef-broth tortellini.
/// Name guards avoid duplicates and preserve user recipe edits and meal plans.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261101070000_AddPomodoriniRecipesAndFrenchOnionTortellini")]
public partial class AddPomodoriniRecipesAndFrenchOnionTortellini : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Spicy Sausage and Pomodorini Ragù'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Spicy Sausage and Pomodorini Ragù',
                    N'A hearty Italian-inspired pasta with browned Italian or Maltese sausage, sweet canned pomodorini, garlic, toasted fennel, chilli, Parmesan and butter. The sauce reduces until glossy and clings to paccheri, rigatoni or another short pasta.',
                    10, 25, 2, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Paccheri or rigatoni', 220, N'g', N'Dry pasta; other short pasta works', 1),
                    (N'Italian or Maltese sausage', 200, N'g', N'Remove casing, crumble and brown well', 2),
                    (N'Canned pomodorini', 400, N'g', N'One can of whole cherry tomatoes, including juices', 3),
                    (N'Garlic', 3, N'clove', N'Thinly sliced or minced', 4),
                    (N'Tomato paste', 1, N'tbsp', N'Cook until a shade darker', 5),
                    (N'Fennel seeds', 0.5, N'tsp', N'Lightly crushed; optional if sausage already contains fennel', 6),
                    (N'Chilli flakes', 0.5, N'tsp', N'Adjust to taste', 7),
                    (N'Olive oil', 1, N'tbsp', N'Use only as needed if sausage is lean', 8),
                    (N'Butter', 15, N'g', N'For the final glossy sauce', 9),
                    (N'Parmesan', 30, N'g', N'Finely grated, plus extra to serve', 10),
                    (N'Fresh basil', NULL, NULL, N'Optional, torn at the end', 11),
                    (N'Salt', NULL, NULL, N'To taste; sausage and Parmesan may be salty', 12),
                    (N'Black pepper', NULL, NULL, N'Freshly ground, to taste', 13);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                           ROW_NUMBER() OVER (
                               PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]
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
                    (@RecipeId, 1, N'Bring a pot of salted water to the boil. Remove the casings from 200 g sausage and break up the meat into small pieces. Slice or mince 3 garlic cloves.'),
                    (@RecipeId, 2, N'Set a large frying pan over medium-high heat. Brown the crumbled sausage for 6–8 minutes, allowing crisp golden edges to form. Add up to 1 tbsp olive oil only if the pan is too dry.'),
                    (@RecipeId, 3, N'Reduce heat to medium. Add 3 garlic cloves and ½ tsp crushed fennel seeds; stir for 45 seconds, taking care not to burn the garlic. Add ½ tsp chilli flakes and 1 tbsp tomato paste, and cook for another 1–2 minutes until the paste darkens.'),
                    (@RecipeId, 4, N'Add the entire 400 g can of pomodorini with its liquid. Crush roughly half the tomatoes, leaving the others whole for texture. Simmer uncovered for 12–15 minutes until thick and concentrated, stirring occasionally.'),
                    (@RecipeId, 5, N'Meanwhile, cook 220 g paccheri or rigatoni until 1–2 minutes shy of package directions. Reserve a mug of starchy cooking water before draining.'),
                    (@RecipeId, 6, N'Transfer the pasta to the sausage and tomato pan with 60–80 ml pasta water. Toss over medium heat for 1–2 minutes until the pasta finishes cooking and the sauce coats every piece; add more pasta water as needed.'),
                    (@RecipeId, 7, N'Reduce heat to low or turn it off. Toss in 15 g butter and 30 g finely grated Parmesan until glossy. Season with black pepper and taste before adding salt.'),
                    (@RecipeId, 8, N'Serve immediately with optional torn basil, extra Parmesan and a little chilli if desired.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Italian'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Italian');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) IN (LOWER(N'Italian'));

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Lunch'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Lunch');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) IN (LOWER(N'Dinner'), LOWER(N'Lunch'));
            END;
            """);

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Garlic and Balsamic Braised Chicken Thighs with Pomodorini'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Garlic and Balsamic Braised Chicken Thighs with Pomodorini',
                    N'Golden-seared boneless chicken thighs gently braised in a rich garlic, canned cherry tomato, oregano and balsamic sauce. Reduced pan juices, a little stock and butter make a deeply savoury, slightly tangy glaze. Delicious with buttery couscous, mashed potatoes or crusty bread.',
                    10, 35, 2, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Boneless chicken thighs', 500, N'g', N'About 4 small boneless, skinless thighs; use a thermometer to check doneness', 1),
                    (N'Canned pomodorini', 400, N'g', N'One can with all its juice', 2),
                    (N'Garlic', 4, N'clove', N'Thinly sliced', 3),
                    (N'Chicken stock', 150, N'ml', N'Preferably low-salt', 4),
                    (N'Balsamic vinegar', 1.5, N'tbsp', N'Start with 1 tbsp and adjust at the end', 5),
                    (N'Dried oregano', 1, N'tsp', N'Or 2 tsp fresh oregano', 6),
                    (N'Olive oil', 1, N'tbsp', N'For searing', 7),
                    (N'Butter', 15, N'g', N'Stir in at the end for a glossy sauce', 8),
                    (N'Salt', NULL, NULL, N'To season the chicken and sauce', 9),
                    (N'Black pepper', NULL, NULL, N'Freshly ground', 10),
                    (N'Chilli flakes', 0.25, N'tsp', N'Optional', 11),
                    (N'Fresh parsley or basil', NULL, NULL, N'Optional garnish', 12),
                    (N'Brown sugar', 0.25, N'tsp', N'Optional pinch only if the tomatoes are very acidic', 13);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                           ROW_NUMBER() OVER (
                               PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]
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
                    (@RecipeId, 1, N'Pat 500 g boneless chicken thighs dry and season both sides with salt and black pepper. Slice 4 garlic cloves.'),
                    (@RecipeId, 2, N'Heat 1 tbsp olive oil in a deep frying pan over medium-high heat. Sear the chicken for 3–4 minutes on each side until deeply golden. It does not need to be cooked through yet. Transfer to a plate.'),
                    (@RecipeId, 3, N'Lower heat to medium-low. Add sliced garlic to the same pan, plus optional chilli flakes. Stir for 30–45 seconds until fragrant without browning.'),
                    (@RecipeId, 4, N'Add 400 g canned pomodorini and juices, 150 ml chicken stock, 1 tsp oregano and 1 tbsp balsamic vinegar. Scrape up the browned chicken bits. Gently crush several tomatoes and bring to a simmer.'),
                    (@RecipeId, 5, N'Return the chicken and any accumulated juices to the pan. Cover partially and simmer gently for 12–18 minutes, turning once, until the chicken reaches at least 74°C in the centre.'),
                    (@RecipeId, 6, N'Remove the lid and simmer for 5–8 minutes until the tomato sauce thickens and becomes glossy. If the chicken is already cooked, set it aside temporarily while reducing the sauce so it stays juicy.'),
                    (@RecipeId, 7, N'Turn heat to low and stir in 15 g butter. Taste, then add the remaining ½ tbsp balsamic if you want more tang; use an optional pinch of sugar only if the sauce tastes too acidic. Adjust salt and pepper.'),
                    (@RecipeId, 8, N'Return the chicken to coat it in the sauce. Finish with fresh parsley or basil if available, and serve with couscous, mashed potato or crusty bread.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Italian'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Italian');
                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Mediterranean'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Mediterranean');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) IN (LOWER(N'Italian'), LOWER(N'Mediterranean'));

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Lunch'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Lunch');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) IN (LOWER(N'Dinner'), LOWER(N'Lunch'));
            END;
            """);

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'French Onion and Beef Broth Tortellini'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'French Onion and Beef Broth Tortellini',
                    N'French-onion-soup-inspired beef broth with slowly caramelised onions, thyme, Worcestershire sauce and tender pork-and-beef-filled tortellini. Finished with Gruyère or Parmesan for a rich, savoury, comforting bowl.',
                    10, 45, 2, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Onions', 2, N'piece', N'Large yellow or brown onions, around 350–400 g total, thinly sliced', 1),
                    (N'Butter', 25, N'g', N'For slowly caramelising the onions', 2),
                    (N'Olive oil', 1, N'tbsp', N'Helps prevent butter burning', 3),
                    (N'Beef stock', 700, N'ml', N'Preferably good-quality, low-sodium', 4),
                    (N'Pork and beef tortellini', 300, N'g', N'Filled fresh pasta or similar meat dumplings; check packet cooking instructions', 5),
                    (N'Fresh thyme', 2, N'sprig', N'Or ½ tsp dried thyme', 6),
                    (N'Worcestershire sauce', 1, N'tsp', N'Adds savoury depth', 7),
                    (N'Dry white wine', 50, N'ml', N'Optional, to deglaze; replace with stock if not using', 8),
                    (N'Gruyère or Parmesan', 35, N'g', N'Finely grated, to finish', 9),
                    (N'Salt', NULL, NULL, N'Only to taste after stock and cheese', 10),
                    (N'Black pepper', NULL, NULL, N'Freshly ground', 11),
                    (N'Water', 50, N'ml', N'Only as needed while onions caramelise', 12);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                           ROW_NUMBER() OVER (
                               PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]
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
                    (@RecipeId, 1, N'Thinly slice 2 large onions. Heat 25 g butter with 1 tbsp olive oil in a wide, heavy saucepan over medium-low heat.'),
                    (@RecipeId, 2, N'Add onions with a small pinch of salt. Cook slowly for 30–35 minutes, stirring every few minutes, until deep golden brown, soft and sweet. Reduce heat if scorching; add a tablespoon of water now and then to loosen the browned fond instead of rushing the process.'),
                    (@RecipeId, 3, N'Optionally pour in 50 ml dry white wine and scrape the bottom of the pan, letting the wine reduce almost completely. Otherwise use a little beef stock for this step.'),
                    (@RecipeId, 4, N'Pour in 700 ml beef stock and add 2 thyme sprigs (or ½ tsp dried thyme), 1 tsp Worcestershire sauce and several grinds of black pepper. Simmer gently for 8–10 minutes to infuse the onions into the broth.'),
                    (@RecipeId, 5, N'Remove thyme stems. Add 300 g pork-and-beef-filled tortellini and simmer gently according to the packet directions, typically 3–5 minutes for fresh tortellini; frozen dumplings or uncooked meat fillings may need longer. Ensure the meat filling is fully cooked before serving.'),
                    (@RecipeId, 6, N'Taste the broth and add salt only if needed. Divide the onions, tortellini and rich beef broth between two warm bowls.'),
                    (@RecipeId, 7, N'Top each bowl with grated Gruyère or Parmesan and freshly ground black pepper. Serve at once. For make-ahead meals, cook and store the tortellini separately to prevent sogginess.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'French'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'French');
                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Italian'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Italian');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) IN (LOWER(N'French'), LOWER(N'Italian'));

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Soup'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Soup');
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) IN (LOWER(N'Soup'), LOWER(N'Dinner'));
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive: preserve recipe edits and linked meal plans.
    }
}
