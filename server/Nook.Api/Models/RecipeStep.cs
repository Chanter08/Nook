namespace Nook.Api.Models;

public class RecipeStep
{
    public int RecipeStepId { get; set; }
    public int RecipeId { get; set; }
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public Recipe Recipe { get; set; } = null!;
}