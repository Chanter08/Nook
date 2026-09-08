namespace Nook.Api.Models;

public class Ingredient
{
    public int IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DefaultUnit { get; set; }
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
        = new List<RecipeIngredient>();
}