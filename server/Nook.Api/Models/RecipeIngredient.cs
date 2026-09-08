namespace Nook.Api.Models;

public class RecipeIngredient
{
    public int RecipeIngredientId { get; set; }
    public int RecipeId { get; set; }
    public int IngredientId { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}