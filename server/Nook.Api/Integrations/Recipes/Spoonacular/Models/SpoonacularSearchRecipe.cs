using System.Text.Json.Serialization;

namespace Nook.Api.Integrations.Recipes.Spoonacular.Models;

public class SpoonacularSearchRecipe
{
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? ImageUrl { get; set; }
}