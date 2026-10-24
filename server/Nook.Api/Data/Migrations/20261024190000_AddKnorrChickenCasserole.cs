using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Adds the Knorr Mealmaker Chicken Casserole packet recipe to Nook.
/// The name guard keeps any previously added version and its meal plans intact.
/// Based on the Knorr 48 g sachet instructions (4 portions).
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261024190000_AddKnorrChickenCasserole")]
public partial class AddKnorrChickenCasserole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1 FROM [Recipe]
                WHERE LOWER([Name]) = LOWER(N'Knorr Mealmaker Chicken Casserole')
            )
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Knorr Mealmaker Chicken Casserole',
                    N'A warming, one-dish chicken casserole using a 48 g Knorr Mealmaker Chicken Casserole sachet. Browned chicken legs bake in a herby gravy with bacon, onion, carrots and potato. Serve with steamed broccoli. Based on the original Knorr packet recipe; nutrition is the manufacturer estimate per serving including broccoli.',
                    15, 60, 4, NULL, SYSUTCDATETIME(), 0,
                    282, 35, 23, 5
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
                    (N'Knorr Mealmaker Chicken Casserole', 1, N'sachet', N'48 g dry seasoning sachet; not regular chicken stock or bouillon', 1),
                    (N'Chicken legs', 4, N'piece', N'Skinless; 1 per person', 2),
                    (N'Vegetable oil', 15, N'ml', N'1 tablespoon, for browning', 3),
                    (N'Onion', 1, N'piece', N'Medium; chopped', 4),
                    (N'Bacon rashers', 4, N'piece', N'3 to 4 rashers, chopped', 5),
                    (N'Carrots', 4, N'piece', N'Medium; thinly sliced (the shopping list on some packs specifies 3)', 6),
                    (N'Potato', 1, N'piece', N'Medium; peeled and diced into bite-size pieces', 7),
                    (N'Water', 400, N'ml', N'Cold or room temperature; whisk into the sachet mix', 8),
                    (N'Broccoli', 320, N'g', N'Optional but part of the original serving suggestion; steam separately', 9);

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
                    (@RecipeId, 1, N'Preheat the oven to 180°C conventional or 170°C fan (Gas Mark 4). Chop the onion and bacon, slice the carrots thinly, and dice the potato so it cooks evenly.'),
                    (@RecipeId, 2, N'Heat 1 tablespoon (15 ml) vegetable oil in a large frying pan over medium-high heat. Brown the 4 skinless chicken legs on both sides for about 3–4 minutes per side, then transfer to a lidded casserole dish. The chicken is not fully cooked yet.'),
                    (@RecipeId, 3, N'In the same frying pan, fry the chopped onion and bacon for 4–5 minutes until golden. Scatter the onion and bacon over the chicken, followed by the carrots and diced potato.'),
                    (@RecipeId, 4, N'Whisk the entire 48 g Knorr Mealmaker Chicken Casserole sachet into 400 ml water until smooth. Pour over the chicken and vegetables. The sachet is already seasoned, so do not add extra salt or stock at this stage.'),
                    (@RecipeId, 5, N'Cover the casserole dish with a fitted lid or tightly with foil. Bake for approximately 60 minutes until the chicken is cooked through and the vegetables are tender. The thickest part of the chicken should reach at least 75°C, measured away from the bone.'),
                    (@RecipeId, 6, N'During the final 5–7 minutes, steam 320 g broccoli until tender-crisp. If the casserole gravy is too thick, stir in a small splash of hot water; if too thin, simmer briefly uncovered.'),
                    (@RecipeId, 7, N'Serve one chicken leg per person with the potatoes, carrots and bacon gravy, plus steamed broccoli. Refrigerate leftovers promptly and reheat until piping hot.');

                IF NOT EXISTS (
                    SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'British')
                )
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'British');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'British');

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
        // Preserve any edits or meal plans associated with the seeded recipe.
    }
}
