using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

[DbContext(typeof(NookDbContext))]
[Migration("20261107114000_AddGordonRamsayInspiredPorkBelly")]
public partial class AddGordonRamsayInspiredPorkBelly : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) LIKE N'%pork belly%' OR LOWER([Name]) LIKE N'%belly of pork%')
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams])
                VALUES (N'Gordon Ramsay''s Slow-Roasted Pork Belly with Fennel', N'Gordon Ramsay''s slow-roasted pork belly with fennel, aromatic spices and mustard-enriched pan sauce. Adapted cooking instructions; original: https://www.gordonramsay.com/gr/recipes/slow-roasted-pork-belly/ . Oven: 180°C conventional. Serves four.', 20, 160,
                    4, NULL, SYSUTCDATETIME(), 0, NULL, NULL, NULL, NULL);

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
                    (N'Skin-on pork belly', 1000, N'g', N'One whole slab; score skin without cutting deeply into the meat', 1),
                    (N'Sea salt', 2, N'tsp', N'Adjust to taste, particularly for crackling', 2),
                    (N'Black pepper', 0.5, N'tsp', N'Freshly ground', 3),
                    (N'Fennel bulb', 1, N'piece', N'Trim and slice roughly', 4),
                    (N'Fresh bay leaves', 4, N'piece', N'Or 2 dried leaves if needed', 5),
                    (N'Garlic', 3, N'clove', N'Peeled and lightly crushed', 6),
                    (N'Cardamom pods', 1, N'tsp', N'Lightly crushed; discard before serving', 7),
                    (N'Star anise', 4, N'piece', N'Discard before serving', 8),
                    (N'Fennel seeds', 1, N'tbsp', N'Divide into two portions', 9),
                    (N'Olive oil', 15, N'ml', N'For the roasting tray', 10),
                    (N'Dry white wine', 325, N'ml', N'For deglazing', 11),
                    (N'Chicken stock', 650, N'ml', N'Use 500–750 ml as needed to reach the fat layer below the skin', 12),
                    (N'Wholegrain mustard', 1, N'tbsp', N'For the pan sauce', 13);

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

                INSERT INTO [RecipeIngredient] (
                    [RecipeId], [IngredientId], [Quantity], [Unit], [Notes], [SortOrder])
                SELECT @RecipeId, e.[IngredientId], i.[Quantity], i.[Unit], i.[Notes], i.[SortOrder]
                FROM @Ingredients i
                CROSS APPLY (
                    SELECT TOP (1) x.[IngredientId] FROM [Ingredient] x
                    WHERE LOWER(x.[Name]) = LOWER(i.[Name])
                    ORDER BY x.[IngredientId]
                ) e;

                INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
                VALUES
                    (@RecipeId, 1, N'Preheat the oven to 180°C conventional (160°C fan). With a sharp knife, make shallow crisscross cuts about 1.5 cm apart in the pork skin, avoiding deep cuts into the meat. Dry the skin well and massage in salt and black pepper.'),
                    (@RecipeId, 2, N'Set an ovenproof roasting pan over medium-high heat. Add olive oil, sliced fennel, bay leaves, crushed garlic, cardamom, star anise and half the fennel seeds. Stir for roughly 2 minutes until fragrant.'),
                    (@RecipeId, 3, N'Move aromatics aside and place the pork skin-down against the hot pan for about 5 minutes to lightly brown it. Turn skin-side up, sprinkle over remaining fennel seeds and add a little more salt to the skin.'),
                    (@RecipeId, 4, N'Pour in 325 ml dry white wine around the meat and loosen any browned bits from the bottom. Bring to a boil, then add enough chicken stock, usually 500–750 ml, to come up to the fat just beneath the skin; keep the skin out of the liquid.'),
                    (@RecipeId, 5, N'Bring the liquid back to a gentle boil, then carefully transfer the uncovered pan to the oven. Roast for approximately 2½ hours until the pork is tender and the top is crisp. Check liquid periodically and add a little hot stock if it threatens to dry out.'),
                    (@RecipeId, 6, N'Rest the pork on a board. Skim excess fat from the pan juices, return the tray to medium heat and whisk in the wholegrain mustard. Simmer briefly to combine and adjust seasoning; remove whole cardamom pods and star anise.'),
                    (@RecipeId, 7, N'Cut the pork into four portions and spoon over a little of the pan sauce, avoiding the crackling. Serve with potatoes and broccoli or other vegetables. Refrigerate leftovers promptly.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'British'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'British');
                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'British');

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');
                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Lunch'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Lunch');
                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) IN (LOWER(N'Dinner'), LOWER(N'Lunch'));
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep user edits and linked meal plans intact.
    }
}
