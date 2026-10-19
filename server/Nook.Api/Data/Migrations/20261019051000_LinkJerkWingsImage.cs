using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Links the user-supplied jerk wings photo from the web root images folder
/// to the existing recipe, without modifying any other recipes.
/// The file must be deployed at wwwroot/images/recipes/jerk-bbq-wings.jpeg.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261019051000_LinkJerkWingsImage")]
public partial class LinkJerkWingsImage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE [Recipe]
            SET [ImageUrl] = N'/images/recipes/jerk-bbq-wings.jpeg'
            WHERE LOWER([Name]) = LOWER(N'Sticky Jamaican Jerk Chicken Wings');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep an image linked to recipes on rollback rather than
        // clobbering an image URL changed after initial deployment.
    }
}
