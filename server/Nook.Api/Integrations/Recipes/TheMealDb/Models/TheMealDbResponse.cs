using System.Text.Json.Serialization;

namespace Nook.Api.Integrations.Recipes.TheMealDb.Models;

public class TheMealDbResponse
{
    [JsonPropertyName("meals")]
    public List<TheMealDbMeal>? Meals { get; set; }
}