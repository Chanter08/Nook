using System.Text.Json.Serialization;

namespace Nook.Api.Integrations.Recipes.Spoonacular.Models;

public class SpoonacularSearchResponse
{
    [JsonPropertyName("results")]
    public List<SpoonacularSearchRecipe> Results { get; set; } = [];
}