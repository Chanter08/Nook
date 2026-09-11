namespace Nook.Api.DTOs.Shopping;

public class ShoppingCatalogSuggestionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
}