namespace Nook.Api.Models;

public class PantryItem
{
    public int PantryItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public string Location { get; set; } = "Pantry";
    public DateOnly? ExpiryDate { get; set; }
    public DateTime AddedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
