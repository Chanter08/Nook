namespace Nook.Api.DTOs.Recipes;

public class RecipeOptionsResponse
{
    public List<string> MealTypes { get; set; } = [];
    public List<string> Cuisines { get; set; } = [];
}