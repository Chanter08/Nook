using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Seeds roasted butternut squash pasta with Italian frying peppers, thyme,
/// brown butter, Parmesan and optional toasted pecans. No sage required.
/// A recipe-name guard avoids duplicate entries and preserves existing edits.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261028101500_AddButternutSquashPepperPasta")]
public partial class AddButternutSquashPepperPasta : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1 FROM [Recipe]
                WHERE LOWER([Name]) = LOWER(N'Creamy Butternut Squash and Italian Pepper Pasta')
            )
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Creamy Butternut Squash and Italian Pepper Pasta',
                    N'Silky roasted butternut squash, garlic and Parmesan pasta sauce with roasted strips of Italian frying pepper, nutty brown butter and thyme instead of sage. Optional toasted pecans add crunch. A squeeze of lemon balances the sweetness of the squash and peppers. Works with rigatoni, penne, paccheri or tagliatelle.',
                    15, 35, 3, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Pasta', 275, N'g', N'250–300 g dry rigatoni, penne, paccheri or tagliatelle', 1),
                    (N'Butternut squash', 500, N'g', N'450–500 g peeled, deseeded and cut into 2 cm cubes', 2),
                    (N'Italian frying peppers', 2, N'piece', N'Seeded and sliced into strips around 1 cm wide; roast and keep separate from the blended sauce', 3),
                    (N'Garlic', 4, N'clove', N'Peeled; roast with the squash', 4),
                    (N'Onion', 0.5, N'piece', N'Half a medium onion, or use one shallot; roughly chopped', 5),
                    (N'Olive oil', 1, N'tbsp', N'For roasting squash and peppers; add a little more if needed', 6),
                    (N'Chicken or vegetable stock', 125, N'ml', N'100–150 ml; add gradually when blending', 7),
                    (N'Cooking cream', 60, N'ml', N'Optional; 50–75 ml. The pureed squash is creamy without it', 8),
                    (N'Parmigiano Reggiano', 45, N'g', N'40–50 g finely grated; stir in with the heat turned off', 9),
                    (N'Butter', 20, N'g', N'Brown gently until nutty, not burnt', 10),
                    (N'Dried thyme', 0.5, N'tsp', N'Or use 2–3 fresh thyme sprigs; roast dried thyme with the squash, add fresh thyme to butter just before removing from heat', 11),
                    (N'Ground nutmeg', NULL, NULL, N'A small pinch', 12),
                    (N'Smoked paprika', 0.25, N'tsp', N'Optional, and no more than ¼ tsp so it does not overpower the squash', 13),
                    (N'Salt', NULL, NULL, N'To taste; Parmesan and stock contribute salt', 14),
                    (N'Black pepper', NULL, NULL, N'Freshly ground to taste', 15),
                    (N'Lemon juice', 1, N'tsp', N'Add a little at the end to balance sweetness; a splash of white wine vinegar also works', 16),
                    (N'Pecans', 25, N'g', N'Optional; chopped and dry-toasted for a crunchy topping', 17),
                    (N'Chilli flakes', NULL, NULL, N'Optional pinch to finish', 18);

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
                    (@RecipeId, 1, N'Preheat the oven to 210°C conventional (190°C fan). Peel and cube 450–500 g butternut squash. Slice 2 Italian frying peppers into approximately 1 cm strips, and roughly chop half an onion or a shallot.'),
                    (@RecipeId, 2, N'Spread squash, 4 peeled garlic cloves and the onion on a baking tray. Toss with most of the olive oil, salt, pepper, ½ tsp dried thyme (if using dried), a small pinch of nutmeg and up to ¼ tsp smoked paprika if desired. Place pepper strips on a separate area of the tray or on a second tray, tossed with the remaining oil, salt and pepper.'),
                    (@RecipeId, 3, N'Roast for 25–35 minutes, turning once. Remove the pepper strips after about 20–25 minutes once softened and lightly charred; roast squash until completely tender and slightly caramelised. Keep peppers aside: do not puree them.'),
                    (@RecipeId, 4, N'Meanwhile, boil 250–300 g pasta in salted water until just al dente. Reserve at least 200 ml of starchy pasta water before draining.'),
                    (@RecipeId, 5, N'Blend the roasted squash, garlic and onion with 100 ml stock and the optional 50–75 ml cream until smooth. Add up to another 50 ml stock as needed for a thick but pourable sauce. If blending hot ingredients, use a heat-safe blender and vent steam safely.'),
                    (@RecipeId, 6, N'In a large frying pan, melt 20 g butter over medium heat, swirling until the milk solids turn golden and the butter smells nutty. Remove from the heat promptly to prevent burning. If using fresh thyme instead of dried, add 2–3 sprigs near the end to perfume the butter, then remove the woody stems.'),
                    (@RecipeId, 7, N'Add the blended squash sauce to the pan and warm it gently. Toss in the drained pasta and roasted Italian pepper strips, loosening with reserved pasta water until the sauce clings silkily to the pasta.'),
                    (@RecipeId, 8, N'Turn off the heat and stir in 40–50 g finely grated Parmigiano Reggiano. Season with black pepper and salt if needed. Add about 1 tsp lemon juice, taste, and adjust to counter the natural sweetness of the squash.'),
                    (@RecipeId, 9, N'Optional: toast 25 g chopped pecans in a dry pan over medium-low heat for 3–4 minutes until fragrant. Serve the pasta immediately with the toasted pecans, extra Parmesan and a pinch of chilli flakes if desired.'),
                    (@RecipeId, 10, N'For make-ahead cooking, refrigerate the sauce and roasted peppers promptly in covered containers; cook pasta fresh when serving. Reheat the sauce gently with a splash of stock or pasta water.');

                IF NOT EXISTS (
                    SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Italian')
                )
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Italian');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'Italian');

                IF NOT EXISTS (
                    SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner')
                )
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) = LOWER(N'Dinner');
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Preserve any user edits or meal plans associated with this recipe.
    }
}
