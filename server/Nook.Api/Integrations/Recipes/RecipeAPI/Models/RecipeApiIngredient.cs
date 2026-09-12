using System.Text.Json.Serialization;

namespace Nook.Api.Integrations.Recipes.RecipeApi.Models;

public class RecipeApiIngredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public bool Optional { get; set; }
}