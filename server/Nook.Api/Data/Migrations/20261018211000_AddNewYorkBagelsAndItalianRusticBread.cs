using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Seeds the New York-style bagels and Italian rustic bread recipes.
/// Each insert is guarded by recipe name so preexisting user-entered copies survive.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261018211000_AddNewYorkBagelsAndItalianRusticBread")]
public partial class AddNewYorkBagelsAndItalianRusticBread : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'New York-Style Bagels'))
BEGIN
    INSERT INTO [Recipe] (
        [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
        [Servings], [ImageUrl], [CreatedDate], [IsArchived],
        [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
    ) VALUES (
        N'New York-Style Bagels', N'Classic New York bakery-style bagels: dense, pleasantly chewy crumb and a deep golden, glossy crust. Boiled before baking; excellent with honey-bacon cream cheese schmear.', 115, 25,
        6, NULL, SYSUTCDATETIME(), 0, NULL, NULL, NULL, NULL
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
        (N'Strong bread flour', 500, N'g', N'High-protein bread flour works best', 1),
        (N'Water', 285, N'ml', N'Lukewarm, for the dough', 2),
        (N'Instant dry yeast', 7, N'g', N'Use 3 g instead for the optional overnight method', 3),
        (N'Honey', 20, N'g', N'For the dough; barley malt syrup can be substituted', 4),
        (N'Salt', 10, N'g', NULL, 5),
        (N'Water', 2000, N'ml', N'For boiling the bagels; not added to the dough', 6),
        (N'Honey', 1, N'tbsp', N'For the boiling water; barley malt syrup can be substituted', 7),
        (N'Sesame seeds', NULL, NULL, N'Optional topping', 8),
        (N'Poppy seeds', NULL, NULL, N'Optional topping', 9);

    INSERT INTO [Ingredient] ([Name], [DefaultUnit])
    SELECT DISTINCT i.[Name], i.[Unit] FROM @Ingredients AS i
    WHERE NOT EXISTS (
        SELECT 1 FROM [Ingredient] AS existing
        WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
    )
    AND i.[SortOrder] = (
        SELECT MIN(i2.[SortOrder]) FROM @Ingredients AS i2
        WHERE LOWER(i2.[Name]) = LOWER(i.[Name])
    );

    INSERT INTO [RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [Unit], [Notes], [SortOrder])
    SELECT @RecipeId, ingredient.[IngredientId], i.[Quantity], i.[Unit], i.[Notes], i.[SortOrder]
    FROM @Ingredients AS i
    CROSS APPLY (
        SELECT TOP (1) existing.[IngredientId] FROM [Ingredient] AS existing
        WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
        ORDER BY existing.[IngredientId]
    ) AS ingredient;

    INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
    VALUES
        (@RecipeId, 1, N'Mix 285 ml lukewarm water, 7 g instant yeast and 20 g honey. Add 500 g strong bread flour and 10 g salt. Knead 10–12 minutes by hand or 7–9 minutes in a sturdy stand mixer until very smooth and elastic. The dough should be firm, not sticky.'),
        (@RecipeId, 2, N'Cover and let rise for 60–75 minutes, until noticeably puffy; it does not need to become extremely airy.'),
        (@RecipeId, 3, N'Divide into six equal balls. Poke a thumb through each ball and stretch the centre hole to approximately 4 cm; the hole will shrink as the bagels proof.'),
        (@RecipeId, 4, N'Rest the shaped bagels for 15–25 minutes while preheating the oven to 230°C conventional or 210°C fan. Line a baking tray with parchment.'),
        (@RecipeId, 5, N'Bring approximately 2 litres of water and 1 tablespoon of honey or barley malt syrup to a gentle boil. Boil bagels for 30–45 seconds per side, then drain briefly.'),
        (@RecipeId, 6, N'While the surfaces are wet, sprinkle with sesame seeds, poppy seeds or your preferred bagel topping. Arrange on the baking tray.'),
        (@RecipeId, 7, N'Bake for 20–25 minutes until richly golden brown and glossy. Cool on a rack for at least 20 minutes before slicing.'),
        (@RecipeId, 8, N'Optional overnight version: use only 3 g instant yeast in the dough. Shape as directed, let the bagels become slightly puffy, then refrigerate covered for 8–16 hours. Boil directly from the fridge and bake as above.');

    IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'American'))
        INSERT INTO [Cuisine] ([Name]) VALUES (N'American');
    INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
    SELECT @RecipeId, [CuisineId] FROM [Cuisine]
    WHERE LOWER([Name]) = LOWER(N'American');
    
    IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Breakfast'))
        INSERT INTO [RecipeType] ([Name]) VALUES (N'Breakfast');
    INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
    SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
    WHERE LOWER([Name]) = LOWER(N'Breakfast');

    IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Lunch'))
        INSERT INTO [RecipeType] ([Name]) VALUES (N'Lunch');
    INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
    SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
    WHERE LOWER([Name]) = LOWER(N'Lunch');
END;");

        migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM [Recipe] WHERE LOWER([Name]) = LOWER(N'Italian Rustic Crusty Bread (Pane Casereccio)'))
BEGIN
    INSERT INTO [Recipe] (
        [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
        [Servings], [ImageUrl], [CreatedDate], [IsArchived],
        [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
    ) VALUES (
        N'Italian Rustic Crusty Bread (Pane Casereccio)', N'Rustic Italian-style round loaf with a deeply browned, crackling crust and an airy, chewy middle. A wet dough and steam create bakery-style results; Dutch oven and baking-tray methods included.', 185, 50,
        1, NULL, SYSUTCDATETIME(), 0, NULL, NULL, NULL, NULL
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
        (N'Strong white bread flour', 500, N'g', N'Caputo Nuvola also works; keep extra for shaping', 1),
        (N'Water', 360, N'ml', N'Room temperature; optionally add up to 15 ml more if the flour absorbs it well', 2),
        (N'Instant dry yeast', 6, N'g', N'Use 2 g instead for the optional overnight method', 3),
        (N'Salt', 10, N'g', NULL, 4);

    INSERT INTO [Ingredient] ([Name], [DefaultUnit])
    SELECT DISTINCT i.[Name], i.[Unit] FROM @Ingredients AS i
    WHERE NOT EXISTS (
        SELECT 1 FROM [Ingredient] AS existing
        WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
    )
    AND i.[SortOrder] = (
        SELECT MIN(i2.[SortOrder]) FROM @Ingredients AS i2
        WHERE LOWER(i2.[Name]) = LOWER(i.[Name])
    );

    INSERT INTO [RecipeIngredient] ([RecipeId], [IngredientId], [Quantity], [Unit], [Notes], [SortOrder])
    SELECT @RecipeId, ingredient.[IngredientId], i.[Quantity], i.[Unit], i.[Notes], i.[SortOrder]
    FROM @Ingredients AS i
    CROSS APPLY (
        SELECT TOP (1) existing.[IngredientId] FROM [Ingredient] AS existing
        WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
        ORDER BY existing.[IngredientId]
    ) AS ingredient;

    INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
    VALUES
        (@RecipeId, 1, N'Combine 500 g flour, 6 g instant yeast, 360 ml room-temperature water and 10 g salt until no dry patches remain. If the dough can absorb it, add up to 15 ml extra water. Cover and rest for 20 minutes.'),
        (@RecipeId, 2, N'With wet hands, lift and stretch one edge of the sticky dough and fold it over the middle. Rotate the bowl and repeat on all four sides. Cover, and repeat the four-sided folding process every 30 minutes for three sets total.'),
        (@RecipeId, 3, N'Allow the dough to complete its first rise until airy and roughly 70–100% larger. This usually takes about 1½–2½ hours from mixing in total, depending on temperature. Judge by the dough rather than the clock.'),
        (@RecipeId, 4, N'Gently transfer to a lightly floured surface, shape into a tight round loaf without knocking out all the air, then place seam-side up in a floured banneton or a floured towel-lined bowl. Cover and proof for 35–60 minutes until puffy.'),
        (@RecipeId, 5, N'Dutch oven method: preheat an oven-safe covered Dutch oven for 30 minutes at 240°C conventional or 220°C fan. Turn the loaf onto parchment, score the top, carefully lower into the hot pot and bake covered for 25 minutes. Remove the lid, reduce to 220°C conventional or 200°C fan, and bake another 20–25 minutes until the crust is deeply browned.'),
        (@RecipeId, 6, N'Baking tray alternative: preheat the oven for 30 minutes at 240°C conventional or 220°C fan with a sturdy metal tray on the lowest rack. Transfer and score the loaf on a separate baking tray. Place it in the oven, then carefully pour 200 ml boiling water into the lower metal tray while keeping clear of the steam. Bake for 15 minutes. Carefully remove the steam tray, lower the temperature by 20°C and bake another 25–30 minutes until deeply browned.'),
        (@RecipeId, 7, N'Cool uncovered on a wire rack for at least one hour to preserve the crackling crust. For additional crispness, leave the baked loaf in the switched-off oven with the door slightly ajar for about 5 minutes before cooling.'),
        (@RecipeId, 8, N'Optional overnight method: use 2 g instant yeast instead of 6 g. After the initial folds, refrigerate the covered dough overnight. The next day, allow it to warm and rise as needed, then shape, proof and bake as directed.');

    IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Italian'))
        INSERT INTO [Cuisine] ([Name]) VALUES (N'Italian');
    INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
    SELECT @RecipeId, [CuisineId] FROM [Cuisine]
    WHERE LOWER([Name]) = LOWER(N'Italian');
    
    IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Lunch'))
        INSERT INTO [RecipeType] ([Name]) VALUES (N'Lunch');
    INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
    SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
    WHERE LOWER([Name]) = LOWER(N'Lunch');

    IF NOT EXISTS (SELECT 1 FROM [RecipeType] WHERE LOWER([Name]) = LOWER(N'Dinner'))
        INSERT INTO [RecipeType] ([Name]) VALUES (N'Dinner');
    INSERT INTO [RecipeRecipeType] ([RecipeId], [RecipeTypeId])
    SELECT @RecipeId, [RecipeTypeId] FROM [RecipeType]
    WHERE LOWER([Name]) = LOWER(N'Dinner');
END;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep user meal plans and any edits to these recipes on rollback.
    }
}
