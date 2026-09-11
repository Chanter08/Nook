namespace Nook.Api.DTOs.Shopping;

public class ShoppingListResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public int CompletedCount { get; set; }
    public List<ShoppingListItemResponse> Items { get; set; } = [];
    public List<ShoppingListItemResponse> CompletedItems { get; set; } = [];
}