namespace Nook.Api.Models;

public class ShoppingList
{
    public int ShoppingListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsArchived { get; set; }
    public ICollection<ShoppingListItem> Items { get; set; }
        = new List<ShoppingListItem>();
}