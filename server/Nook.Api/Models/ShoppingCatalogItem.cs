namespace Nook.Api.Models;

public class ShoppingCatalogItem
{
    public int ShoppingCatalogItemId { get; set; }
    public string Name { get; set; } =
        string.Empty;
    public string NormalizedName { get; set; } =
        string.Empty;
    public string? DefaultUnit { get; set; }
    public string? Category { get; set; }
    public string? DefaultNotes { get; set; }
    public int UseCount { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public ICollection<ShoppingListItem> ShoppingListItems { get; set; }
    = new List<ShoppingListItem>();
}