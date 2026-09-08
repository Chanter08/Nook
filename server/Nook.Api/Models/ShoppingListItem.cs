namespace Nook.Api.Models;

public class ShoppingListItem
{
    public int ShoppingListItemId { get; set; }
    public int ShoppingListId { get; set; }
    public int? IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public string? Category { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public ShoppingList ShoppingList { get; set; } = null!;
    public Ingredient? Ingredient { get; set; }
    public int? ShoppingCatalogItemId { get; set; }
    public ShoppingCatalogItem? ShoppingCatalogItem { get; set; }
}