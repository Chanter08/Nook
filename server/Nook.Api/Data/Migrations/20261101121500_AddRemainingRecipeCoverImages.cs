using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Completes the previously identified missing recipe covers (two breads,
/// pesto ciabatta and five breakfast schmears). Existing user-assigned covers
/// are not overwritten.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261101121500_AddRemainingRecipeCoverImages")]
public partial class AddRemainingRecipeCoverImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE [Recipe]
            SET [ImageUrl] = N'https://www.webstaurantstore.com/images/products/extra_large/686545/2359908.jpg'
            WHERE LOWER([Name]) = LOWER(N'New York-Style Bagels')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://snapcalorie-webflow-website.s3.us-east-2.amazonaws.com/media/food_pics_v2/medium/tuscan_bread.jpg'
            WHERE LOWER([Name]) = LOWER(N'Italian Rustic Crusty Bread (Pane Casereccio)')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://s3.us-east-2.amazonaws.com/pfimg1/013/05/9f/059f36e03631d14f54abd9ea3cd6bf68_500x500.jpg'
            WHERE LOWER([Name]) = LOWER(N'Roasted Pepper, Mozzarella and Pistachio Pesto Ciabatta')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://snapcalorie-webflow-website.s3.us-east-2.amazonaws.com/media/food_pics_v2/medium/cinnamon_crunch_bagel_panera_with_honey_walnut_cream_cheese.jpg'
            WHERE LOWER([Name]) = LOWER(N'Honey and Toasted Walnut Schmear')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://snapcalorie-webflow-website.s3.us-east-2.amazonaws.com/media/recipe_pics_v2/medium/garlic_herb_bagel_spread.jpg'
            WHERE LOWER([Name]) = LOWER(N'Roasted Garlic and Parmesan Schmear')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://www.dominosugar.com/sites/default/files/2024-08/Web_Meal_Planner_Category_Image-Brown%20Sugar%20Cinnamon%20Cream%20Cheese%20Spread%2011.jpg'
            WHERE LOWER([Name]) = LOWER(N'Brown Sugar and Cinnamon Schmear')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://img.handmadev.com/photos/2025/11/smoky-roasted-red-pepper-cream-cheese-delight.jpg'
            WHERE LOWER([Name]) = LOWER(N'Roasted Red Pepper and Smoked Paprika Schmear')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://res.cloudinary.com/perkchops/image/upload/v1673645982/product/2023013/kltk1xzdrvaayzes22xo.jpg'
            WHERE LOWER([Name]) = LOWER(N'Garlic and Chive Schmear')
              AND ([ImageUrl] IS NULL OR LTRIM(RTRIM([ImageUrl])) = N'');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive: do not remove user-edited image links.
    }
}
