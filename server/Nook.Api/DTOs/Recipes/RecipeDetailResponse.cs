namespace Nook.Api.DTOs.Recipes;

public class RecipeDetailResponse : RecipeSummaryResponse
{
    public List<RecipeIngredientResponse> Ingredients { get; set; } = [];
    public List<RecipeStepResponse> Steps { get; set; } = [];
}