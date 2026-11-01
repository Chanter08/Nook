namespace Nook.Api.Models;

public class PantryItem
{
    public int PantryItemId { get; set; }
    public int? ShoppingCatalogItemId { get; set; }
    public ShoppingCatalogItem? ShoppingCatalogItem { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public string StorageLocation { get; set; } = "Pantry";
    public DateOnly? ExpiryDate { get; set; }
    public decimal? LowStockThreshold { get; set; }
    public string? Notes { get; set; }
    public string Source { get; set; } = "manual";
    public DateOnly? LastPurchasedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
