namespace Nook.Api.DTOs;

public class AddRecipeToShoppingRequest
{
    public List<int> RecipeIngredientIds { get; set; } = [];
}