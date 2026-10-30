using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Seeds ultimate Maltese baked rice (ross fil-forn) with a browned beef-pork
/// ragù, kunserva, eggs, cheese and a deeply golden oven crust.
/// The recipe-name guard avoids duplicates and preserves existing user edits.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261030120000_AddMalteseBakedRice")]
public partial class AddMalteseBakedRice : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1 FROM [Recipe]
                WHERE LOWER([Name]) = LOWER(N'Ultimate Maltese Baked Rice (Ross fil-Forn)')
            )
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Ultimate Maltese Baked Rice (Ross fil-Forn)',
                    N'A deep, savoury Maltese ross fil-forn made with browned beef and pork mince, pancetta, concentrated tomato kunserva, red wine and a gently simmered ragù. Uncooked long-grain rice bakes in the sauce and beef stock, bound with eggs and Parmesan/Pecorino, then finished with a dark golden cheese crust. Makes six generous servings in a roughly 30 × 22 cm dish. Optional breadcrumbs for crispness, mild curry powder for a family-style Maltese touch, or ricotta for creaminess. Rest 15 minutes before serving.',
                    20, 100, 6, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Long-grain white rice', 375, N'g', N'Uncooked; ordinary white long-grain rice, not instant or wholegrain', 1),
                    (N'Beef mince', 300, N'g', N'For deeply browned meat sauce', 2),
                    (N'Pork mince', 200, N'g', N'Adds richness and juiciness', 3),
                    (N'Pancetta', 75, N'g', N'Finely diced; streaky bacon is an acceptable substitute', 4),
                    (N'Beef stock', 550, N'ml', N'Hot; keep an extra 50–100 ml boiling water available if the rice needs it', 5),
                    (N'Onion', 1, N'piece', N'Large, finely chopped', 6),
                    (N'Garlic', 3, N'clove', N'Minced', 7),
                    (N'Carrot', 1, N'piece', N'Medium, finely grated', 8),
                    (N'Passata', 700, N'g', N'Or crushed tomatoes', 9),
                    (N'Kunserva (concentrated tomato paste)', 70, N'g', N'Cook in the meat fat for 2 minutes to deepen flavour', 10),
                    (N'Dry red wine', 100, N'ml', N'Reduce almost completely before adding tomato', 11),
                    (N'Olive oil', 2, N'tbsp', N'For browning and sweating the vegetables', 12),
                    (N'Dried oregano', 1, N'tsp', NULL, 13),
                    (N'Ground nutmeg', 0.25, N'tsp', NULL, 14),
                    (N'Mild curry powder', 0.25, N'tsp', N'Optional; used in some Maltese families', 15),
                    (N'Salt', NULL, NULL, N'To taste; take into account the salt in the stock and cheese', 16),
                    (N'Black pepper', NULL, NULL, N'Freshly ground, to taste', 17),
                    (N'Eggs', 3, N'piece', N'Large; beat with 90 g grated cheese and fold in once the sauce cools slightly', 18),
                    (N'Grated Parmesan or Parmesan-Pecorino blend', 120, N'g', N'90 g mixed with eggs and 30 g reserved for the top', 19),
                    (N'Butter', 15, N'g', N'To grease the 30 × 22 cm oven dish', 20),
                    (N'Dry breadcrumbs', 15, N'g', N'Optional; for an extra-crisp surface', 21),
                    (N'Ricotta', 100, N'g', N'Optional richer variation; fold in before adding the eggs', 22);

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
                    (@RecipeId, 1, N'Heat 2 tbsp olive oil in a large heavy pot over medium-high heat. Add 75 g diced pancetta and cook for 3–4 minutes until golden and its fat has rendered.'),
                    (@RecipeId, 2, N'Add 1 finely chopped large onion and 1 grated medium carrot. Reduce the heat slightly and cook for 8–10 minutes until softened and sweet. Stir in 3 minced garlic cloves for the final minute.'),
                    (@RecipeId, 3, N'Increase the heat and add 300 g beef mince and 200 g pork mince. Brown thoroughly, breaking up clumps but leaving the meat undisturbed at intervals to develop deeply savoury browned edges.'),
                    (@RecipeId, 4, N'Stir in 70 g kunserva (concentrated tomato paste) and cook for 2 minutes, letting it darken slightly. Add 100 ml dry red wine, scrape up the browned bits and simmer until almost evaporated.'),
                    (@RecipeId, 5, N'Add 700 g passata, 1 tsp dried oregano, ¼ tsp ground nutmeg, optional ¼ tsp mild curry powder, salt and black pepper. Simmer uncovered for 15–20 minutes, stirring occasionally, until the meat sauce is rich, thick and glossy but not dry.'),
                    (@RecipeId, 6, N'Preheat the oven to 180°C conventional (170°C fan). Generously butter a roughly 30 × 22 cm oven dish with 15 g butter.'),
                    (@RecipeId, 7, N'Stir 550 ml hot beef stock into the finished ragù. Take off the heat and leave for 5–10 minutes to cool slightly; the mixture should remain quite wet because the uncooked rice will absorb liquid as it bakes.'),
                    (@RecipeId, 8, N'Mix in 375 g uncooked long-grain white rice, distributing the grains evenly. In a separate bowl beat 3 large eggs with 90 g finely grated Parmesan or a Parmesan-Pecorino blend. If making the richer variation, gently fold in 100 g ricotta now.'),
                    (@RecipeId, 9, N'When the rice and sauce are warm rather than piping hot, stir in the egg-and-cheese mixture thoroughly so the eggs do not scramble. Transfer to the buttered dish, spreading rice and meat evenly.'),
                    (@RecipeId, 10, N'Cover the baking dish tightly with foil and bake on the middle shelf for 35–40 minutes. The steam helps the uncooked rice cook evenly.'),
                    (@RecipeId, 11, N'Remove the foil. Check the rice and moisture: if the grains are still firm and the dish is drying out, add 50–100 ml boiling water around the edges and cover again for about 10 minutes before continuing.'),
                    (@RecipeId, 12, N'Scatter the reserved 30 g grated cheese over the top, plus 15 g dry breadcrumbs if using. Bake uncovered for another 25–35 minutes, until the rice is tender and the top is deeply golden brown.'),
                    (@RecipeId, 13, N'For an extra-crisp crust, raise the oven to 200°C for the final 5 minutes, watching closely so the cheese does not burn. The exact baking time depends on the rice and depth of the dish; always confirm that the centre is fully cooked.'),
                    (@RecipeId, 14, N'Let the ross fil-forn stand for 15 minutes before slicing and serving. For leftovers, cool promptly, refrigerate, and reheat until piping hot.');

                IF NOT EXISTS (
                    SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Maltese')
                )
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Maltese');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'Maltese');

                IF NOT EXISTS (
                    SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner')
                )
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');

                IF NOT EXISTS (
                    SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Lunch')
                )
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Lunch');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) IN (LOWER(N'Dinner'), LOWER(N'Lunch'));
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive: preserve any recipe edits and linked meal plans.
    }
}
