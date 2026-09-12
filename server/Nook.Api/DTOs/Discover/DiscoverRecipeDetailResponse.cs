namespace Nook.Api.DTOs.Discover;

public class DiscoverRecipeDetailResponse
{
    public string Provider { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public string? Category { get; set; }
    public string? Cuisine { get; set; }

    public string? SourceUrl { get; set; }
    public string? VideoUrl { get; set; }

    public int? Calories { get; set; }
    public decimal? ProteinGrams { get; set; }
    public decimal? CarbohydrateGrams { get; set; }
    public decimal? FatGrams { get; set; }

    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public int? Servings { get; set; }
    public string? Difficulty { get; set; }

    public int? TotalTimeMinutes { get; set; }

    public List<DiscoverIngredientResponse> Ingredients { get; set; } = [];
    public List<DiscoverStepResponse> Steps { get; set; } = [];
}