namespace Nook.Api.DTOs.Pantry;

public sealed class PantryItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public string Location { get; set; } = "Pantry";
    public DateOnly? ExpiryDate { get; set; }
    public DateTime AddedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public sealed class SavePantryItemRequest
{
    public string? Name { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public string? Location { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}

public sealed class BulkAddPantryItemsRequest
{
    public List<SavePantryItemRequest> Items { get; set; } = [];
}
