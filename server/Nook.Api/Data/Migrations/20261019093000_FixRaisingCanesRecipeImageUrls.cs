using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Replaces invalid hotlinked Wikimedia thumbnail paths with canonical links
/// to verified original file objects. Only updates our earlier URL values;
/// user-selected replacement photos are preserved.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261019093000_FixRaisingCanesRecipeImageUrls")]
public partial class FixRaisingCanesRecipeImageUrls : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE [Recipe]
SET [ImageUrl] = N'https://upload.wikimedia.org/wikipedia/commons/0/0c/Chicken_fingers_at_Raising_Cane%27s_in_Baton_Rouge%2C_Louisiana.jpg'
WHERE [Name] = N'Raising Cane''s-Style Chicken Tenders'
  AND ([ImageUrl] IS NULL OR [ImageUrl] = N'https://upload.wikimedia.org/wikipedia/commons/thumb/0/0c/Chicken_fingers_at_Raising_Cane%27s_in_Baton_Rouge%2C_Louisiana.jpg/800px-Chicken_fingers_at_Raising_Cane%27s_in_Baton_Rouge%2C_Louisiana.jpg');

            UPDATE [Recipe]
SET [ImageUrl] = N'https://upload.wikimedia.org/wikipedia/commons/f/ff/Frysauce.JPG'
WHERE [Name] = N'Copycat Cane''s-Style Dipping Sauce'
  AND ([ImageUrl] IS NULL OR [ImageUrl] = N'https://upload.wikimedia.org/wikipedia/commons/thumb/f/ff/Frysauce.JPG/800px-Frysauce.JPG');

            UPDATE [Recipe]
SET [ImageUrl] = N'https://upload.wikimedia.org/wikipedia/commons/d/d8/Garlic_toast.jpg',
    [Description] = CASE WHEN [Description] LIKE N'%FranHogan%' THEN N'Thick white bread griddled in garlic butter until crisp and golden outside, soft inside. Illustrative photo: Scudsvlad / Wikimedia Commons (CC BY-SA 4.0).' ELSE [Description] END
WHERE [Name] = N'Garlic-Butter Texas Toast (Cane''s-Style)'
  AND ([ImageUrl] IS NULL OR [ImageUrl] = N'https://upload.wikimedia.org/wikipedia/commons/thumb/4/44/Toast_with_garlic_and_butter.jpg/800px-Toast_with_garlic_and_butter.jpg');

            UPDATE [Recipe]
SET [ImageUrl] = N'https://upload.wikimedia.org/wikipedia/commons/1/18/French_fries_crinkle_cut.jpg'
WHERE [Name] = N'Crispy Crinkle-Cut Fries (Cane''s-Style)'
  AND ([ImageUrl] IS NULL OR [ImageUrl] = N'https://upload.wikimedia.org/wikipedia/commons/thumb/1/18/French_fries_crinkle_cut.jpg/800px-French_fries_crinkle_cut.jpg');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep live image URLs and user-selected photos on rollback.
    }
}
