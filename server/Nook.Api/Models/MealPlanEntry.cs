namespace Nook.Api.Models;

public class MealPlanEntry
{
    public int MealPlanEntryId { get; set; }
    public DateOnly PlanDate { get; set; }
    public string MealType { get; set; } = string.Empty;
    public int RecipeId { get; set; }
    public DateTime CreatedDate { get; set; }
    public Recipe Recipe { get; set; } = null!;
}