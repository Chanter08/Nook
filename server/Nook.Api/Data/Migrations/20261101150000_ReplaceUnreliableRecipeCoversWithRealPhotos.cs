using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Replace placeholder, unavailable or questionable cover-image URLs with
/// identifiable photographs from recipe publishers and food retailers.
/// Preserve any custom images manually assigned after earlier migrations.
/// No third-party image is redistributed in this repository.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261101150000_ReplaceUnreliableRecipeCoversWithRealPhotos")]
public partial class ReplaceUnreliableRecipeCoversWithRealPhotos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE [Recipe]
            SET [ImageUrl] = N'https://images.slurrp.com/prod/recipe_images/an-italian-in-my-kitchen/pasta-with-italian-sausage-and-fresh-tomatoes-1648823379_AY2AH1VMDIH75MFMBFFR.webp?height=675&impolicy=slurrp-20210601&width=1200'
            WHERE LOWER([Name]) = LOWER(N'Spicy Sausage and Pomodorini Ragù')
              AND ([ImageUrl] = N'https://cdn.prod.website-files.com/6578962814fdfe86333b16e3/65e20f54a4f05a01da509260_b4-p-1080.jpg'
                   OR [ImageUrl] IS NULL
                   OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://s3.us-east-2.amazonaws.com/pfimg1/013/35/9b/359bcc17b9e2b904c887ffec78ac5ddc_500x500.jpg'
            WHERE LOWER([Name]) = LOWER(N'Garlic and Balsamic Braised Chicken Thighs with Pomodorini')
              AND ([ImageUrl] = N'https://momycooks.com/wp-content/uploads/2025/07/One-Pan-Balsamic-Chicken-Quick-Healthy-and-Delicious.webp'
                   OR [ImageUrl] IS NULL
                   OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://production-media.gousto.co.uk/cms/mood-image/5625_One-Pot-Creamy-Chicken-Breast--Vegatable-Fricassee_002_0-1723130666578-x700.jpg'
            WHERE LOWER([Name]) = LOWER(N'Knorr Mealmaker Chicken Casserole')
              AND ([ImageUrl] = N'https://itsonly.recipes/images/recipeimages/hearty-chicken-casserole.webp'
                   OR [ImageUrl] IS NULL
                   OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://cookingwithmammac.com/wp-content/uploads/2021/01/1200-Chicken-Pastina-Soup-Image.jpg'
            WHERE LOWER([Name]) = LOWER(N'Creamy Chicken Pastina Soup')
              AND ([ImageUrl] = N'https://snapcalorie-webflow-website.s3.us-east-2.amazonaws.com/media/food_pics_v2/medium/chicken_soup_with_pastina.jpg'
                   OR [ImageUrl] IS NULL
                   OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://d3s8tbcesxr4jm.cloudfront.net/recipe-images/ic-1/layered-ultimate-beef-nachos-supreme.jpg'
            WHERE LOWER([Name]) = LOWER(N'Fully Loaded Beef Nachos')
              AND ([ImageUrl] = N'https://jasminerecipes.com/assets/images/1767481845727-wpza8xhb.webp'
                   OR [ImageUrl] IS NULL
                   OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://img.chefkoch-cdn.de/rezepte/2091181337763462/bilder/1322814/crop-640x427/pane-casareccio.jpg'
            WHERE LOWER([Name]) = LOWER(N'Italian Rustic Crusty Bread (Pane Casereccio)')
              AND ([ImageUrl] = N'https://snapcalorie-webflow-website.s3.us-east-2.amazonaws.com/media/food_pics_v2/medium/tuscan_bread.jpg'
                   OR [ImageUrl] IS NULL
                   OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://s3.amazonaws.com/images.comet.aol.com/food/recipe/media/Well%2BPlated%2B3.jpg'
            WHERE LOWER([Name]) = LOWER(N'Honey and Toasted Walnut Schmear')
              AND ([ImageUrl] = N'https://snapcalorie-webflow-website.s3.us-east-2.amazonaws.com/media/food_pics_v2/medium/cinnamon_crunch_bagel_panera_with_honey_walnut_cream_cheese.jpg'
                   OR [ImageUrl] IS NULL
                   OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://imagesvc.meredithcorp.io/v3/mm/image?c=sc&h=512&orient=true&poi=auto&q=60&url=https%3A%2F%2Fpublic-assets.meredithcorp.io%2Fa54316702298110ce08bfafd1d81b8ac%2F1774613699196Chopped-Garlic-Bagel.jpeg'
            WHERE LOWER([Name]) = LOWER(N'Roasted Garlic and Parmesan Schmear')
              AND ([ImageUrl] = N'https://snapcalorie-webflow-website.s3.us-east-2.amazonaws.com/media/recipe_pics_v2/medium/garlic_herb_bagel_spread.jpg'
                   OR [ImageUrl] IS NULL
                   OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://images.getrecipekit.com/20240403121357-frischkaese-aufstrich.jpg?aspect_ratio=16%3A9&quality=90'
            WHERE LOWER([Name]) = LOWER(N'Roasted Red Pepper and Smoked Paprika Schmear')
              AND ([ImageUrl] = N'https://img.handmadev.com/photos/2025/11/smoky-roasted-red-pepper-cream-cheese-delight.jpg'
                   OR [ImageUrl] IS NULL
                   OR LTRIM(RTRIM([ImageUrl])) = N'');

            UPDATE [Recipe]
            SET [ImageUrl] = N'https://cdn.galleries.smcloud.net/thumbs/gf-681C-kj6d-GvFQ_egzotyczny-obiad-bez-wysilku-kurczak-kokosowy-z-batatami-cie-zachwyci-9-j.jpg'
            WHERE LOWER([Name]) = LOWER(N'Bahian Coconut Chicken Curry with Sweet Potato and Bell Peppers')
              AND ([ImageUrl] = N'https://snapcalorie-webflow-website.s3.us-east-2.amazonaws.com/media/recipe_pics_v2/medium/caribbean_vegetable_chicken_curry.jpg'
                   OR [ImageUrl] IS NULL
                   OR LTRIM(RTRIM([ImageUrl])) = N'');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive: preserve recipe cover edits and user data.
    }
}
