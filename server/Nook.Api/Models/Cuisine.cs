namespace Nook.Api.Models;

public class Cuisine
{
    public int CuisineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<RecipeCuisine> RecipeCuisines { get; set; }
        = new List<RecipeCuisine>();
}
