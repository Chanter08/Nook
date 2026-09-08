namespace Nook.Api.DTOs;

public class CreateRecipeIngredientRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
}