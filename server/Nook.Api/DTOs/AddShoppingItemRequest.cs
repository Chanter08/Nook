namespace Nook.Api.DTOs;

public class AddShoppingItemRequest
{
    public int? ShoppingCatalogItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public string? Category { get; set; }
}