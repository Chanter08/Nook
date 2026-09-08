namespace Nook.Api.Models;

public class RecipeCuisine
{
    public int RecipeId { get; set; }
    public int CuisineId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public Cuisine Cuisine { get; set; } = null!;
}