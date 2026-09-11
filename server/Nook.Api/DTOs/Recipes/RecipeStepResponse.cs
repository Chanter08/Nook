namespace Nook.Api.DTOs.Recipes;

public class RecipeStepResponse
{
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
}