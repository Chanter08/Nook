namespace Nook.Api.Models;

public class RecipeType
{
    public int RecipeTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<RecipeRecipeType> RecipeRecipeTypes { get; set; }
        = new List<RecipeRecipeType>();
}
