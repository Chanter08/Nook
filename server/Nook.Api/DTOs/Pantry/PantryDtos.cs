namespace Nook.Api.DTOs.Pantry;

public class PantryResponse
{
    public int ItemCount { get; set; }
    public int LowStockCount { get; set; }
    public int ExpiringSoonCount { get; set; }
    public List<PantryItemResponse> Items { get; set; } = [];
}

public class PantryItemResponse
{
    public int Id { get; set; }
    public int? ShoppingCatalogItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public string StorageLocation { get; set; } = "Pantry";
    public DateOnly? ExpiryDate { get; set; }
    public decimal? LowStockThreshold { get; set; }
    public string? Notes { get; set; }
    public string Source { get; set; } = "manual";
    public DateOnly? LastPurchasedDate { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsExpiringSoon { get; set; }
}

public class UpsertPantryItemRequest
{
    public int? ShoppingCatalogItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public string StorageLocation { get; set; } = "Pantry";
    public DateOnly? ExpiryDate { get; set; }
    public decimal? LowStockThreshold { get; set; }
    public string? Notes { get; set; }
}

public class ReceiptScanResponse
{
    public string? StoreName { get; set; }
    public string? PurchaseDate { get; set; }
    public string? Currency { get; set; }
    public List<ReceiptScanItemResponse> Items { get; set; } = [];
}

public class ReceiptScanItemResponse
{
    public string ReceiptText { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public string Category { get; set; } = "Groceries";
    public string StorageLocation { get; set; } = "Pantry";
    public decimal? Price { get; set; }
    public decimal Confidence { get; set; }
    public bool IsLikelyInventoryItem { get; set; }
}

public class ImportReceiptRequest
{
    public DateOnly? PurchaseDate { get; set; }
    public List<ImportReceiptItemRequest> Items { get; set; } = [];
}

public class ImportReceiptItemRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public string StorageLocation { get; set; } = "Pantry";
    public string? Notes { get; set; }
}

public class ImportReceiptResult
{
    public int Imported { get; set; }
    public int Merged { get; set; }
    public int ShoppingItemsCompleted { get; set; }
}
