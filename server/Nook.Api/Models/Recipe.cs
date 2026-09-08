namespace Nook.Api.Models;

public class Recipe
{
    public int RecipeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public int? Servings { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsArchived { get; set; }
    public int? Calories { get; set; }
    public decimal? ProteinGrams { get; set; }
    public decimal? CarbohydrateGrams { get; set; }
    public decimal? FatGrams { get; set; }
    public ICollection<RecipeRecipeType> RecipeRecipeTypes { get; set; }
        = new List<RecipeRecipeType>();
    public ICollection<RecipeCuisine> RecipeCuisines { get; set; }
        = new List<RecipeCuisine>();
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    = new List<RecipeIngredient>();
    public ICollection<RecipeStep> RecipeSteps { get; set; }
    = new List<RecipeStep>();
    public ICollection<MealPlanEntry> MealPlanEntries { get; set; }
    = new List<MealPlanEntry>();
}
