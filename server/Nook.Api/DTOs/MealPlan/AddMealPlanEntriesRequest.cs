namespace Nook.Api.DTOs.MealPlan;

public sealed class AddMealPlanEntriesRequest
{
    public DateOnly Date { get; set; }
    public string MealType { get; set; } = string.Empty;
    public List<int> RecipeIds { get; set; } = [];
}
