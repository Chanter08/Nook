using System.Text.Json.Serialization;

namespace Nook.Api.Integrations.Recipes.RecipeApi.Models;

public class RecipeApiRecipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Difficulty { get; set; }

    [JsonPropertyName("meal_type")]
    public string? MealType { get; set; }

    public string? Cuisine { get; set; }

    [JsonPropertyName("dietary_tags")]
    public List<string> DietaryTags { get; set; } = [];

    public int? Servings { get; set; }

    [JsonPropertyName("prep_time")]
    public int? PrepTime { get; set; }

    [JsonPropertyName("cook_time")]
    public int? CookTime { get; set; }

    [JsonPropertyName("calories_per_serving")]
    public int? CaloriesPerServing { get; set; }

    public decimal? Protein { get; set; }

    public List<string> Instructions { get; set; } = [];
    public List<RecipeApiIngredient> Ingredients { get; set; } = [];
}