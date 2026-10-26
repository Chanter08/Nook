using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Seeds a creamy chicken pastina soup using two cans of Heinz Cream of Chicken.
/// Keeps an existing recipe with the same name and related user meal plans intact.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261026110000_AddCreamyChickenPastinaSoup")]
public partial class AddCreamyChickenPastinaSoup : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1 FROM [Recipe]
                WHERE LOWER([Name]) = LOWER(N'Creamy Chicken Pastina Soup')
            )
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Creamy Chicken Pastina Soup',
                    N'A restaurant-style creamy chicken and tiny pasta soup made richer and fresher with gently poached shredded chicken, sautéed aromatic vegetables, thyme, Parmesan, and an optional lemon and crisp garlic breadcrumb finish. Uses 2 cans of Heinz Cream of Chicken soup as its base. Cook and store pasta separately to prevent sogginess.',
                    15, 25, 4, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Heinz Cream of Chicken soup', 2, N'can', N'Use 2 standard cans, undiluted; this is the creamy soup base', 1),
                    (N'Chicken breast', 325, N'g', N'300–350 g; boneless and skinless', 2),
                    (N'Small pasta', 90, N'g', N'80–100 g pastina, stelline, ditalini, orzo or small shells; cook separately', 3),
                    (N'Water', 500, N'ml', N'For gently poaching chicken; add enough to cover, optionally replace with unsalted chicken stock', 4),
                    (N'Onion', 0.5, N'piece', N'Small onion or half a medium onion; finely diced', 5),
                    (N'Carrot', 1, N'piece', N'Small; finely diced', 6),
                    (N'Celery', 1, N'stalk', N'Finely diced', 7),
                    (N'Garlic', 2, N'clove', N'Finely minced', 8),
                    (N'Butter', 15, N'g', N'For sweating the vegetables', 9),
                    (N'Olive oil', 5, N'ml', N'1 tsp, alongside the butter', 10),
                    (N'Thyme', 2, N'sprig', N'Or 1/2 tsp dried thyme; some for poaching, some for the soup', 11),
                    (N'Black pepper', 0.5, N'tsp', N'Freshly ground; to taste', 12),
                    (N'Salt', NULL, NULL, N'Lightly season poaching water only, then taste before adding more; canned soup contains salt', 13),
                    (N'Ground nutmeg', NULL, NULL, N'Very small pinch; optional', 14),
                    (N'Double cream', 75, N'ml', N'Optional, 50–100 ml to enrich the soup', 15),
                    (N'Parmesan', 25, N'g', N'Finely grated, plus extra to serve', 16),
                    (N'Lemon', 0.5, N'piece', N'Optional; a small squeeze of juice at the end', 17),
                    (N'Fresh parsley', 1, N'tbsp', N'Or fresh chives, finely chopped', 18),
                    (N'Plain breadcrumbs', 25, N'g', N'Optional crispy topping', 19),
                    (N'Butter', 10, N'g', N'Optional, for toasting the breadcrumbs', 20),
                    (N'Garlic', 1, N'clove', N'Optional; finely minced, for the breadcrumb topping', 21);

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
                    (@RecipeId, 1, N'Place 300–350 g chicken breast in a small saucepan, add roughly 500 ml water or unsalted chicken stock (enough to just cover), a thyme sprig, a little pepper and a small pinch of salt. Bring just to a gentle simmer, never a hard boil. Poach for roughly 10–14 minutes depending on thickness until the centre reaches at least 74°C. Transfer to a board and rest briefly, then shred with two forks. Reserve the poaching liquid; strain it if necessary.'),
                    (@RecipeId, 2, N'While the chicken poaches, finely dice the onion, carrot and celery and mince the garlic. In a separate heavy saucepan, melt 15 g butter with 1 tsp olive oil over medium-low heat. Add the vegetables and cook gently for 6–8 minutes until soft and sweet but not browned.'),
                    (@RecipeId, 3, N'Add 2 minced garlic cloves and the remaining thyme (or dried thyme). Stir for 30 seconds, then add a tiny pinch of nutmeg and some black pepper.'),
                    (@RecipeId, 4, N'Pour in both cans of Heinz Cream of Chicken soup, scraping out the cans well. Stir in 300 ml reserved chicken-poaching liquid until smooth. Simmer gently for 5 minutes, stirring now and then, then add another 50–150 ml liquid until the consistency is rich and spoonable rather than thick like a sauce.'),
                    (@RecipeId, 5, N'Meanwhile, boil the 80–100 g small pasta in a separate pan of lightly salted water until 1 minute shy of the package time. Drain. Cooking pasta separately prevents it from absorbing too much soup and becoming soggy, especially if saving leftovers.'),
                    (@RecipeId, 6, N'Stir the shredded chicken into the soup and heat gently for 2–3 minutes. If using double cream, add 50–100 ml now; avoid a hard boil. Stir in grated Parmesan until melted. Taste first, then adjust with black pepper and only enough salt if necessary.'),
                    (@RecipeId, 7, N'Just before serving, divide the drained pasta among 3–4 warm bowls, ladle the creamy chicken soup over it, and finish with chopped parsley or chives, a little extra Parmesan, and an optional small squeeze of lemon for brightness.'),
                    (@RecipeId, 8, N'Optional topping: melt 10 g butter in a small frying pan over medium heat, lightly sizzle 1 finely minced garlic clove for 15–20 seconds, then add 25 g breadcrumbs. Toss for 2–3 minutes until golden and crisp. Sprinkle over bowls immediately before eating. Store any leftover pasta separately from the soup.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Italian'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Italian');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'Italian');

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Soup'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Soup');
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Lunch'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Lunch');
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) IN (LOWER(N'Soup'), LOWER(N'Lunch'), LOWER(N'Dinner'));
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive: preserve a recipe that may have been edited or meal-planned.
    }
}
