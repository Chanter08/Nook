namespace Nook.Api.Models;

public class RecipeRecipeType
{
    public int RecipeId { get; set; }
    public int RecipeTypeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public RecipeType RecipeType { get; set; } = null!;
}