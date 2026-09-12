using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nook.Api.Integrations.Recipes.TheMealDb.Models;

public class TheMealDbMeal
{
    [JsonPropertyName("idMeal")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("strMeal")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("strCategory")]
    public string? Category { get; set; }

    [JsonPropertyName("strArea")]
    public string? Cuisine { get; set; }

    [JsonPropertyName("strInstructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("strMealThumb")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("strSource")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("strYoutube")]
    public string? VideoUrl { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extra { get; set; } = [];
}