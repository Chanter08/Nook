using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

[DbContext(typeof(NookDbContext))]
[Migration("20261107113000_AddSundayRoastBeefAndGravy")]
public partial class AddSundayRoastBeefAndGravy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Classic British Roast Beef & Onion Gravy'))
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams])
                VALUES (N'Classic British Roast Beef & Onion Gravy', N'Traditional British Sunday roast: juicy topside of beef, crispy roast potatoes, Yorkshire puddings and a rich onion-and-beef-dripping gravy. Serve with carrots and greens. Beef timing depends on joint shape and oven; use a meat thermometer.', 25, 100,
                    6, NULL, SYSUTCDATETIME(), 0, NULL, NULL, NULL, NULL);

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
                    (N'Beef topside roasting joint', 1200, N'g', N'At room temperature briefly before roasting; preferably tied', 1),
                    (N'Beef dripping or neutral oil', 25, N'g', N'For searing and roasting beef', 2),
                    (N'Sea salt', 2, N'tsp', N'Season beef generously', 3),
                    (N'Black pepper', 1, N'tsp', N'Freshly ground', 4),
                    (N'English mustard', 15, N'g', N'Optional coating for the beef', 5),
                    (N'Fresh rosemary', 2, N'sprig', N'Optional', 6),
                    (N'Onions', 2, N'piece', N'Medium, thinly sliced for gravy', 7),
                    (N'Plain flour', 25, N'g', N'For gravy thickening', 8),
                    (N'Beef stock', 600, N'ml', N'Warm, preferably low salt', 9),
                    (N'Worcestershire sauce', 10, N'ml', N'For gravy', 10),
                    (N'Unsalted butter', 15, N'g', N'For the onions', 11),
                    (N'Potatoes', 1200, N'g', N'Floury potatoes, for roasties', 12),
                    (N'Oil or beef dripping', 60, N'g', N'For potatoes', 13),
                    (N'Carrots', 500, N'g', N'For roasting or steaming', 14),
                    (N'Yorkshire puddings', 6, N'piece', N'To serve; optional, homemade or ready-made', 15);

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
                    (@RecipeId, 1, N'Heat oven to 220°C (200°C fan). Pat the 1.2 kg beef joint dry, coat with 25 g dripping or oil and optional mustard, then season generously with salt and pepper. Place on a rack or bed of onion in a roasting tin.'),
                    (@RecipeId, 2, N'Roast beef at 220°C (200°C fan) for 20 minutes, then reduce to 180°C (160°C fan). Continue roasting until the centre reaches your preferred temperature; for medium, aim for around 60–63°C after resting. Use a thermometer, not time alone; cooking times depend on joint thickness.'),
                    (@RecipeId, 3, N'Meanwhile peel and cut 1.2 kg floury potatoes into large chunks. Parboil in salted water for 8–10 minutes until edges soften. Drain, steam-dry, shake gently to roughen, then roast in 60 g preheated fat at 200–220°C for 45–60 minutes, turning until deeply golden. Roast or steam the carrots.'),
                    (@RecipeId, 4, N'Lift beef out and rest loosely covered for 20–30 minutes. Save the meat drippings and any browned bits in the roasting tin for the gravy.'),
                    (@RecipeId, 5, N'For gravy, melt 15 g butter with a spoon of reserved drippings in a saucepan and slowly cook two thinly sliced onions until soft and golden, about 15–20 minutes. Stir in 25 g plain flour and cook for a minute.'),
                    (@RecipeId, 6, N'Gradually whisk in 600 ml warm beef stock, scraping the roast tin to dissolve the brown bits into the gravy. Add 10 ml Worcestershire sauce and simmer for 8–12 minutes until glossy. Strain if desired; season to taste.'),
                    (@RecipeId, 7, N'Slice the rested beef thinly against the grain and serve with roast potatoes, carrots, Yorkshire puddings and generous onion gravy. Refrigerate leftovers promptly and reheat thoroughly for next-day lunches.');

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
