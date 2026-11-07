using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Adds sticky honey-soy beef made with tenderised minute steak.
/// Name guard prevents duplicates; rollback deliberately preserves user edits.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261107080000_AddStickyHoneySoyMinuteSteak")]
public partial class AddStickyHoneySoyMinuteSteak : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1 FROM [Recipe]
                WHERE LOWER([Name]) = LOWER(N'Sticky Honey-Soy Minute Steak Beef')
            )
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Sticky Honey-Soy Minute Steak Beef',
                    N'Quick Chinese takeaway-style sticky beef using mechanically tenderised minute steak, lightly coated in cornflour and tossed in a glossy honey-soy, oyster, ginger and garlic glaze. Serve with jasmine rice and broccoli or peppers. Four portions: dinner for two and lunch for two. For lunch, pack the sauce-coated beef separately from the rice or noodles, refrigerate promptly and reheat thoroughly.',
                    15, 10, 4, NULL, SYSUTCDATETIME(), 0,
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
                    (N'Tenderised minute steak', 600, N'g', N'Mechanically tenderised; slice into 1–1.5 cm strips, against the grain where possible', 1),
                    (N'Cornflour', 24, N'g', N'Approximately 3 level tbsp, for lightly coating the beef', 2),
                    (N'Light soy sauce', 75, N'ml', N'15 ml for the beef plus 60 ml (4 tbsp) for the glaze', 3),
                    (N'Neutral cooking oil', 30, N'ml', N'15 ml to coat the beef; remaining 15 ml for batch frying, adding a little more if required', 4),
                    (N'Black pepper', NULL, NULL, N'To taste', 5),
                    (N'Honey', 42, N'g', N'Approximately 2 tbsp', 6),
                    (N'Brown sugar', 12, N'g', N'Approximately 1 tbsp', 7),
                    (N'Oyster sauce', 15, N'ml', N'Approximately 1 tbsp', 8),
                    (N'Rice vinegar', 15, N'ml', N'Approximately 1 tbsp', 9),
                    (N'Garlic', 3, N'clove', N'Minced', 10),
                    (N'Fresh ginger', 5, N'g', N'Approximately 1 tsp, grated', 11),
                    (N'Water', 80, N'ml', N'For loosening and simmering the glaze', 12),
                    (N'Sesame oil', 5, N'ml', N'Approximately 1 tsp; add to the sauce', 13),
                    (N'Gochujang', 5, N'g', N'Optional for heat; alternatively use chilli flakes', 14),
                    (N'Jasmine rice', 280, N'g', N'Optional accompaniment for four portions; dry weight', 15),
                    (N'Broccoli', 400, N'g', N'Optional; steam or stir-fry, or substitute carrots or bell peppers', 16),
                    (N'Spring onions', 2, N'piece', N'Optional garnish, sliced', 17),
                    (N'Sesame seeds', 10, N'g', N'Optional garnish', 18);

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
                    (@RecipeId, 1, N'Cut 600 g tenderised minute steak into strips roughly 1–1.5 cm wide, against the grain if possible. Pat dry. Toss with 15 ml light soy sauce, 15 ml neutral oil, black pepper and 24 g cornflour until lightly coated.'),
                    (@RecipeId, 2, N'In a bowl, stir together the remaining 60 ml light soy sauce, 42 g honey, 12 g brown sugar, 15 ml oyster sauce, 15 ml rice vinegar, 3 minced garlic cloves, 5 g grated ginger, 80 ml water and 5 ml sesame oil. Add optional 5 g gochujang or chilli flakes if desired.'),
                    (@RecipeId, 3, N'If serving with rice, cook 280 g dry jasmine rice according to packet directions. Steam or stir-fry 400 g broccoli (or other vegetables).'),
                    (@RecipeId, 4, N'Heat a wide frying pan over medium-high heat with some of the remaining neutral oil. Cook steak strips in small batches without crowding, about 1–2 minutes per side depending on thickness. Because minute steak may be mechanically tenderised, ensure the beef reaches at least 63°C internally and rests for 3 minutes before serving; do not serve it rare. Transfer the cooked beef to a plate.'),
                    (@RecipeId, 5, N'Pour the mixed sauce into the pan. Simmer for 2–3 minutes, stirring frequently, until glossy and slightly syrupy. If too thick, add a splash of water.'),
                    (@RecipeId, 6, N'Return beef and any collected juices to the sauce. Toss for 30–60 seconds to coat and warm through without prolonged cooking. Top with sliced spring onions and sesame seeds if using.'),
                    (@RecipeId, 7, N'Serve with jasmine rice and broccoli for dinner. For next-day lunch, refrigerate leftovers promptly in shallow airtight containers. Cool cooked rice rapidly, refrigerate within one hour and consume within 24 hours. Reheat beef and rice thoroughly until steaming hot, only once. Add a splash of water to loosen the glaze; alternatively serve the beef with freshly cooked noodles.');
                
                IF NOT EXISTS (
                    SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Chinese-inspired')
                )
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Chinese-inspired');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'Chinese-inspired');

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
        // Preserve recipe customisations, meals and shopping-list links.
    }
}
