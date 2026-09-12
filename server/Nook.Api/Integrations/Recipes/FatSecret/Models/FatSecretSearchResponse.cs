using System.Text.Json.Serialization;

namespace Nook.Api.Integrations.Recipes.FatSecret.Models;

public class FatSecretSearchResponse
{
    [JsonPropertyName("recipes")]
    public FatSecretRecipesResult? Recipes { get; set; }
}

public class FatSecretRecipesResult
{
    [JsonPropertyName("recipe")]
    public List<FatSecretSearchRecipe> Recipe { get; set; } = [];
}