using System.Text.Json.Serialization;

namespace Nook.Api.Integrations.Recipes.FatSecret.Models;

public class FatSecretSearchRecipe
{
    [JsonPropertyName("recipe_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("recipe_name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("recipe_description")]
    public string? Description { get; set; }

    [JsonPropertyName("recipe_image")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("recipe_types")]
    public FatSecretRecipeTypes? RecipeTypes { get; set; }
}

public class FatSecretRecipeTypes
{
    [JsonPropertyName("recipe_type")]
    public List<string> RecipeType { get; set; } = [];
}