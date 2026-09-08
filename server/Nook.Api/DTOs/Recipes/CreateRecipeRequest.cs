namespace Nook.Api.DTOs;

public class CreateRecipeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public int? Servings { get; set; }
    public string? ImageUrl { get; set; }
    public int? Calories { get; set; }
    public decimal? ProteinGrams { get; set; }
    public decimal? CarbohydrateGrams { get; set; }
    public decimal? FatGrams { get; set; }
    public List<string> MealTypes { get; set; } = [];
    public List<string> Cuisines { get; set; } = [];
    public List<CreateRecipeIngredientRequest> Ingredients { get; set; } = [];
    public List<CreateRecipeStepRequest> Steps { get; set; } = [];
}