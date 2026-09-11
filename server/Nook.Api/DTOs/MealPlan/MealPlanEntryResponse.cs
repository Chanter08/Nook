namespace Nook.Api.DTOs.MealPlan;

public class MealPlanEntryResponse
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string MealType { get; set; } = string.Empty;
    public MealPlanRecipeResponse Recipe { get; set; } = new();
}

public class MealPlanRecipeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}