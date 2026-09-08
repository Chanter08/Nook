namespace Nook.Api.DTOs;

public class AddMealPlanEntryRequest
{
    public DateOnly Date { get; set; }
    public string MealType { get; set; } = string.Empty;
    public int RecipeId { get; set; }
}