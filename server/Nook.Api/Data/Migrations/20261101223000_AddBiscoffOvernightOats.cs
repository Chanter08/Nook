using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Seeds a filling, higher-protein Biscoff overnight oats breakfast.
/// The name guard avoids duplicates and preserves user edits.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261101223000_AddBiscoffOvernightOats")]
public partial class AddBiscoffOvernightOats : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Biscoff Overnight Oats'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Biscoff Overnight Oats',
                    N'Creamy make-ahead oats with Greek yogurt, chia and Lotus Biscoff. A modest amount of cookie butter keeps the caramelised biscuit flavour while Greek yogurt makes the jar more filling and protein-rich.',
                    5, 0, 1,
                    N'https://namelymarly.com/wp-content/uploads/2023/05/Biscoff-Overnight-Oats-1930-sq-web.jpg',
                    SYSUTCDATETIME(), 0,
                    500, 29, 61, 15
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
                    (N'Rolled oats', 50, N'g', N'Old-fashioned rolled oats give the best overnight texture', 1),
                    (N'Greek yogurt', 150, N'g', N'Plain 0% or low-fat Greek yogurt', 2),
                    (N'Milk', 100, N'ml', N'Dairy or unsweetened milk of choice; add a splash more before serving if needed', 3),
                    (N'Biscoff spread', 15, N'g', N'Use about 10 g in the oats and reserve 5 g for the top', 4),
                    (N'Chia seeds', 10, N'g', N'Helps thicken the oats and adds fibre', 5),
                    (N'Vanilla extract', 0.5, N'tsp', NULL, 6),
                    (N'Ground cinnamon', 0.25, N'tsp', N'Optional but works well with the spiced biscuit flavour', 7),
                    (N'Salt', NULL, NULL, N'Small pinch', 8),
                    (N'Biscoff biscuit', 1, N'piece', N'Crush over the top just before eating so it stays crunchy', 9);

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
                    (@RecipeId, 1, N'Add 150 g Greek yogurt, 100 ml milk, about 10 g of the Biscoff spread and ½ tsp vanilla to a jar or lidded container. Stir until mostly smooth.'),
                    (@RecipeId, 2, N'Add 50 g rolled oats, 10 g chia seeds, ¼ tsp cinnamon and a small pinch of salt. Stir thoroughly, making sure no dry pockets remain.'),
                    (@RecipeId, 3, N'Cover and refrigerate for at least 6 hours, preferably overnight. The oats will thicken as they absorb the liquid.'),
                    (@RecipeId, 4, N'Before serving, stir the oats and loosen with a splash of milk if you prefer a softer texture. Warm the reserved 5 g Biscoff spread for a few seconds until pourable and drizzle it over the top.'),
                    (@RecipeId, 5, N'Crush 1 Biscoff biscuit over the jar just before eating so it stays crisp. Serve cold, or let the oats sit at room temperature for 10 minutes if you prefer them less chilled.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Belgian'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Belgian');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'Belgian');

                IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Breakfast'))
                    INSERT INTO [RecipeType] ([Name]) VALUES (N'Breakfast');

                INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
                SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
                WHERE LOWER([Name]) = LOWER(N'Breakfast');
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive: preserve user edits and any linked meal-plan entries.
    }
}
