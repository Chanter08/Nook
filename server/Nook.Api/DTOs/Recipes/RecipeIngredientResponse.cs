namespace Nook.Api.DTOs.Recipes;

public class RecipeIngredientResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
}