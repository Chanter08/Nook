using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

[DbContext(typeof(NookDbContext))]
[Migration("20261103113000_AddTiramisuOvernightOats")]
public partial class AddTiramisuOvernightOats : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Tiramisu Overnight Oats'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Tiramisu Overnight Oats',
                    N'Creamy chocolate-espresso overnight oats with a vanilla protein yoghurt topping. Make ahead and chill overnight. Ingredient weights are approximate metric conversions; nutrition figures are supplied estimates per serving and have not been recalculated.',
                    10, 0, 2, NULL, SYSUTCDATETIME(), 0,
                    458, 32.5, 67, 9
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
                    (N'Rolled oats', 80, N'g', NULL, 1),
                    (N'Raw cacao powder', 15, N'g', N'Plus a little extra for dusting', 2),
                    (N'Chia seeds', 18, N'g', NULL, 3),
                    (N'Pure vanilla extract', 5, N'ml', N'For oat base', 4),
                    (N'Pink Himalayan salt', NULL, NULL, N'Small pinch', 5),
                    (N'Espresso', 45, N'ml', N'Brewed and cooled', 6),
                    (N'Full-fat almond milk', 300, N'ml', NULL, 7),
                    (N'Raw honey', 42, N'g', N'For oat base', 8),
                    (N'Raw honey', 10, N'g', N'For yoghurt topping', 9),
                    (N'Pure vanilla extract', 1, N'ml', N'For yoghurt topping', 10),
                    (N'Vanilla protein powder', 30, N'g', NULL, 11),
                    (N'Oikos Triple Zero vanilla yoghurt', 360, N'g', NULL, 12);

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
                    (@RecipeId, 1, N'Combine 80 g rolled oats, 15 g cacao powder, 18 g chia seeds and a pinch of salt in a bowl.'),
                    (@RecipeId, 2, N'Add 45 ml cooled espresso, 300 ml almond milk, 42 g honey and 5 ml vanilla extract. Stir thoroughly and transfer to two sealable jars or one large airtight container.'),
                    (@RecipeId, 3, N'Whisk 360 g vanilla yoghurt, 10 g honey, 1 ml vanilla extract and 30 g vanilla protein powder until smooth.'),
                    (@RecipeId, 4, N'Spread the yoghurt topping evenly over the oats and dust lightly with extra cacao powder.'),
                    (@RecipeId, 5, N'Cover and refrigerate for at least 8 hours or overnight. Serve chilled, divided into two portions.');

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
        // Non-destructive: existing user meal plans and recipe edits must be preserved.
    }
}
