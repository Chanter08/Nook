namespace Nook.Api.Models;

// A cooking event is retained so stock changes can be reversed later.
public class CookedRecipe
{
    public int CookedRecipeId { get; set; }
    public int RecipeId { get; set; }
    public int? MealPlanEntryId { get; set; }
    public decimal Servings { get; set; }
    public DateTime CookedAtUtc { get; set; }
    public DateTime? UndoneAtUtc { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public ICollection<CookedIngredientUsage> Usages { get; set; } = new List<CookedIngredientUsage>();
}

public class CookedIngredientUsage
{
    public int CookedIngredientUsageId { get; set; }
    public int CookedRecipeId { get; set; }
    public int PantryItemId { get; set; }
    public int RecipeIngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public CookedRecipe CookedRecipe { get; set; } = null!;
    public PantryItem PantryItem { get; set; } = null!;
}