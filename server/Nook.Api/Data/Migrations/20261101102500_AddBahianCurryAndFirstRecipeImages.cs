using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Adds a Bahian coconut chicken curry adapted from Schwartz Las Iguanas seasoning,
/// including bell pepper and sweet potato, and assigns representative recipe images
/// to nine earlier recipes only when their image URL has not been customized.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261101102500_AddBahianCurryAndFirstRecipeImages")]
public partial class AddBahianCurryAndFirstRecipeImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE [Recipe]
            SET [ImageUrl] = N'https://cdn.prod.website-files.com/6578962814fdfe86333b16e3/65e20f54a4f05a01da509260_b4-p-1080.jpg'
            WHERE LOWER([Name]) = LOWER(N'Spicy Sausage and Pomodorini Ragù')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://res.cloudinary.com/hz3gmuqw6/image/upload/c_fill%2Ch_600%2Cq_auto%2Cw_800/f_auto/v1/recipes/DSCF0698_644ee8644a668'
            WHERE LOWER([Name]) = LOWER(N'Creamy Butternut Squash and Italian Pepper Pasta')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://momycooks.com/wp-content/uploads/2025/07/One-Pan-Balsamic-Chicken-Quick-Healthy-and-Delicious.webp'
            WHERE LOWER([Name]) = LOWER(N'Garlic and Balsamic Braised Chicken Thighs with Pomodorini')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://itsonly.recipes/images/recipeimages/hearty-chicken-casserole.webp'
            WHERE LOWER([Name]) = LOWER(N'Knorr Mealmaker Chicken Casserole')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://www.savoryonline.com/app/uploads/recipes/99908/onion-soup-with-tortellini.jpg'
            WHERE LOWER([Name]) = LOWER(N'French Onion and Beef Broth Tortellini')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://snapcalorie-webflow-website.s3.us-east-2.amazonaws.com/media/food_pics_v2/medium/chicken_soup_with_pastina.jpg'
            WHERE LOWER([Name]) = LOWER(N'Creamy Chicken Pastina Soup')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://apronandwhisk.com/wp-content/uploads/2022/03/Ross-il-Forn-Baked-Rice-Apron-Whislk-16-819x1024.jpg'
            WHERE LOWER([Name]) = LOWER(N'Ultimate Maltese Baked Rice (Ross fil-Forn)')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://jasminerecipes.com/assets/images/1767481845727-wpza8xhb.webp'
            WHERE LOWER([Name]) = LOWER(N'Fully Loaded Beef Nachos')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://pageneralstore.com/cdn/shop/collections/HIPPEYS.jpg?v=1729714594'
            WHERE LOWER([Name]) = LOWER(N'Loaded New York-Style Hot Dogs')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            IF NOT EXISTS (
                SELECT 1 FROM [Recipe]
                WHERE LOWER([Name]) = LOWER(N'Bahian Coconut Chicken Curry with Sweet Potato and Bell Peppers')
            )
            BEGIN
                INSERT INTO [Recipe] (
                    [Name], [Description], [PrepTimeMinutes], [CookTimeMinutes],
                    [Servings], [ImageUrl], [CreatedDate], [IsArchived],
                    [Calories], [ProteinGrams], [CarbohydrateGrams], [FatGrams]
                )
                VALUES (
                    N'Bahian Coconut Chicken Curry with Sweet Potato and Bell Peppers',
                    N'A Brazilian-inspired Schwartz Las Iguanas Bahian coconut curry made with boneless chicken thighs, sweet bell peppers, soft sweet potato, spinach and a full tin of coconut milk. A 25 g recipe-mix sachet supplies ginger, cumin, coriander, garlic and a little chilli. Serve with basmati rice and fresh lime. The seasoning mix contains milk ingredients.',
                    15, 30, 4,
                    N'https://snapcalorie-webflow-website.s3.us-east-2.amazonaws.com/media/recipe_pics_v2/medium/caribbean_vegetable_chicken_curry.jpg',
                    SYSUTCDATETIME(), 0,
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
                    (N'Schwartz Las Iguanas Bahian Coconut Curry Recipe Mix', 1, N'sachet', N'One 25g packet; medium heat; seasoning contains milk ingredients', 1),
                    (N'Boneless skinless chicken thighs', 450, N'g', N'Dice into bite-size pieces; chicken breast can substitute', 2),
                    (N'Full-fat coconut milk', 400, N'g', N'One standard tin; shake before opening', 3),
                    (N'Onion', 1, N'piece', N'Medium, diced', 4),
                    (N'Red or mixed bell pepper', 1, N'piece', N'Large, deseeded and sliced', 5),
                    (N'Sweet potato', 300, N'g', N'Peel and cut into small 1–1.5cm cubes so it cooks through', 6),
                    (N'Baby spinach', 60, N'g', N'One generous handful; stir in right at the end', 7),
                    (N'Olive oil', 1, N'tbsp', N'For the onions, peppers and chicken', 8),
                    (N'Water', 60, N'ml', N'Add as needed when softening sweet potato and loosening the sauce', 9),
                    (N'Lime', 1, N'piece', N'Juice to finish, to taste', 10),
                    (N'Fresh coriander', NULL, NULL, N'Optional, chopped to serve', 11),
                    (N'Basmati rice', 240, N'g', N'Optional dry rice for four portions, cooked separately', 12),
                    (N'Salt', NULL, NULL, N'Only if needed, after tasting the seasoning mix', 13);

                ;WITH Missing AS (
                    SELECT i.[Name], i.[Unit],
                        ROW_NUMBER() OVER (
                            PARTITION BY LOWER(i.[Name]) ORDER BY i.[SortOrder]
                        ) AS rn
                    FROM @Ingredients i
                    WHERE NOT EXISTS (
                        SELECT 1 FROM [Ingredient] existing
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
                FROM @Ingredients i
                CROSS APPLY (
                    SELECT TOP (1) existing.[IngredientId]
                    FROM [Ingredient] existing
                    WHERE LOWER(existing.[Name]) = LOWER(i.[Name])
                    ORDER BY existing.[IngredientId]
                ) ingredient;

                INSERT INTO [RecipeStep] ([RecipeId], [StepNumber], [Instruction])
                VALUES
                    (@RecipeId, 1, N'Dice 450 g boneless chicken thighs into bite-size pieces. Dice 1 onion, slice 1 bell pepper, and peel and cut 300 g sweet potato into small 1–1.5 cm cubes. Wash 60 g baby spinach.'),
                    (@RecipeId, 2, N'Heat 1 tbsp oil in a large deep frying pan over medium heat. Fry the onion and bell pepper for 4–5 minutes until the onion softens and the pepper begins to turn sweet.'),
                    (@RecipeId, 3, N'Add the diced sweet potato and about 60 ml water. Cover and cook on medium-low for 6–8 minutes, stirring once, until the sweet potato begins to soften but is not yet completely cooked. Add a splash more water if needed to prevent catching.'),
                    (@RecipeId, 4, N'Increase heat to medium-high. Add the diced chicken thighs and cook for 4–5 minutes, turning so the outside is lightly browned. The chicken will finish cooking in the sauce.'),
                    (@RecipeId, 5, N'Sprinkle in the entire 25 g Schwartz Las Iguanas Bahian Coconut Curry sachet and stir for 20–30 seconds so the seasoning coats the chicken and vegetables.'),
                    (@RecipeId, 6, N'Pour in the 400 g tin of full-fat coconut milk, stirring to dissolve the seasoning and scrape the bottom of the pan. Bring to a gentle simmer, then reduce heat to medium-low.'),
                    (@RecipeId, 7, N'Simmer partially covered for 12–15 minutes, stirring occasionally, until the sweet potato is completely fork-tender, the sauce is lightly thickened and chicken is cooked to at least 74°C in the centre. If the curry gets too thick before the potato is tender, add a splash of water and continue simmering.'),
                    (@RecipeId, 8, N'Stir in 60 g baby spinach during the final 1–2 minutes, until just wilted. Turn off the heat and add fresh lime juice a little at a time to balance the sweet coconut and sweet potato.'),
                    (@RecipeId, 9, N'Taste before adding any extra salt, as the seasoning packet already contains it. Finish with fresh coriander if using, and serve with cooked basmati rice and an extra lime wedge.'),
                    (@RecipeId, 10, N'For make-ahead meals, cool promptly and refrigerate in a covered container for up to 2 days; reheat thoroughly and cook rice fresh or cool and refrigerate it promptly.');

                IF NOT EXISTS (SELECT 1 FROM [Cuisine] WHERE LOWER([Name]) = LOWER(N'Brazilian'))
                    INSERT INTO [Cuisine] ([Name]) VALUES (N'Brazilian');

                INSERT INTO [RecipeCuisine] ([RecipeId], [CuisineId])
                SELECT @RecipeId, [CuisineId] FROM [Cuisine]
                WHERE LOWER([Name]) = LOWER(N'Brazilian');

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
        // Non-destructive: keep users' image edits and linked meal plans.
    }
}
