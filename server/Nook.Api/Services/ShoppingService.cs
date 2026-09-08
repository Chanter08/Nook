using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.DTOs;
using Nook.Api.Models;

namespace Nook.Api.Services;

public class ShoppingService
{
    private readonly NookDbContext _db;

    public ShoppingService(NookDbContext db)
    {
        _db = db;
    }

    public async Task<object> GetShoppingListAsync()
    {
        var shoppingList = await _db.ShoppingLists
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsArchived && x.Name == "Shopping");

        if (shoppingList is null)
        {
            return new
            {
                id = 0,
                name = "Shopping",
                itemCount = 0,
                completedCount = 0,
                items = Array.Empty<object>(),
                completedItems = Array.Empty<object>()
            };
        }

        var items = await _db.ShoppingListItems
            .AsNoTracking()
            .Where(x =>
                x.ShoppingListId == shoppingList.ShoppingListId &&
                !x.IsCompleted)
            .OrderBy(x => x.CreatedDate)
            .Select(x => new
            {
                id = x.ShoppingListItemId,
                ingredientId = x.IngredientId,
                shoppingCatalogItemId = x.ShoppingCatalogItemId,
                name = x.Name,
                quantity = x.Quantity,
                unit = x.Unit,
                notes = x.Notes,
                category = x.Category,
                origin = x.IngredientId != null ? "recipe" : "manual"
            })
            .ToListAsync();

        var completedItems = await _db.ShoppingListItems
            .AsNoTracking()
            .Where(x =>
                x.ShoppingListId == shoppingList.ShoppingListId &&
                x.IsCompleted)
            .OrderByDescending(x => x.CompletedDate)
            .Select(x => new
            {
                id = x.ShoppingListItemId,
                ingredientId = x.IngredientId,
                shoppingCatalogItemId = x.ShoppingCatalogItemId,
                name = x.Name,
                quantity = x.Quantity,
                unit = x.Unit,
                notes = x.Notes,
                category = x.Category,
                origin = x.IngredientId != null ? "recipe" : "manual"
            })
            .ToListAsync();

        return new
        {
            id = shoppingList.ShoppingListId,
            name = shoppingList.Name,
            itemCount = items.Count,
            completedCount = completedItems.Count,
            items,
            completedItems
        };
    }

    public async Task<object> SearchCatalogAsync(string? query)
    {
        var search = NormalizeCatalogName(query);

        if (string.IsNullOrWhiteSpace(search))
        {
            return Array.Empty<object>();
        }

        var results = await _db.ShoppingCatalogItems
            .AsNoTracking()
            .Where(x => x.NormalizedName.Contains(search))
            .OrderByDescending(x => x.NormalizedName.StartsWith(search))
            .ThenByDescending(x => x.LastUsedDate)
            .ThenByDescending(x => x.UseCount)
            .ThenBy(x => x.Name)
            .Take(8)
            .Select(x => new
            {
                id = x.ShoppingCatalogItemId,
                name = x.Name,
                unit = x.DefaultUnit,
                category = x.Category,
                notes = x.DefaultNotes
            })
            .ToListAsync();

        return results;
    }

    public async Task<AddManualShoppingItemResult> AddManualItemAsync(
        AddShoppingItemRequest request)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return AddManualShoppingItemResult.Invalid("Item name is required.");
        }

        if (request.Quantity.HasValue && request.Quantity.Value <= 0)
        {
            return AddManualShoppingItemResult.Invalid(
                "Quantity must be greater than zero.");
        }

        var shoppingList = await GetOrCreateShoppingListAsync();
        var unit = NormalizeShoppingUnit(request.Unit);
        var quantity = NormalizeShoppingQuantity(request.Quantity, request.Unit);

        var category = string.IsNullOrWhiteSpace(request.Category)
            ? null
            : request.Category.Trim();

        var notes = string.IsNullOrWhiteSpace(request.Notes)
            ? null
            : request.Notes.Trim();

        ShoppingCatalogItem? catalogItem = null;

        if (request.ShoppingCatalogItemId.HasValue)
        {
            catalogItem = await _db.ShoppingCatalogItems
                .FirstOrDefaultAsync(x =>
                    x.ShoppingCatalogItemId ==
                    request.ShoppingCatalogItemId.Value);
        }

        if (catalogItem is null)
        {
            var normalizedName = NormalizeCatalogName(name);

            catalogItem = await _db.ShoppingCatalogItems
                .FirstOrDefaultAsync(x =>
                    x.NormalizedName == normalizedName);
        }

        if (catalogItem is null)
        {
            catalogItem = await GetOrCreateCatalogItemAsync(
                name,
                unit,
                category,
                notes);
        }

        name = catalogItem.Name;
        unit ??= catalogItem.DefaultUnit;
        category ??= catalogItem.Category;
        notes ??= catalogItem.DefaultNotes;

        var existing = await _db.ShoppingListItems
            .FirstOrDefaultAsync(x =>
                x.ShoppingListId == shoppingList.ShoppingListId &&
                !x.IsCompleted &&
                x.ShoppingCatalogItemId == catalogItem.ShoppingCatalogItemId &&
                x.Unit == unit);

        if (existing is not null)
        {
            if (quantity.HasValue)
            {
                existing.Quantity =
                    (existing.Quantity ?? 0) + quantity.Value;
            }

            if (string.IsNullOrWhiteSpace(existing.Notes) &&
                !string.IsNullOrWhiteSpace(notes))
            {
                existing.Notes = notes;
            }

            if (string.IsNullOrWhiteSpace(existing.Category) &&
                !string.IsNullOrWhiteSpace(category))
            {
                existing.Category = category;
            }

            UpdateCatalogUsage(
                catalogItem,
                unit,
                category,
                notes);

            await _db.SaveChangesAsync();

            return AddManualShoppingItemResult.Success(
                existing.ShoppingListItemId,
                existing.Name,
                true);
        }

        var shoppingItem = new ShoppingListItem
        {
            ShoppingListId = shoppingList.ShoppingListId,
            IngredientId = null,
            ShoppingCatalogItemId = catalogItem.ShoppingCatalogItemId,
            Name = catalogItem.Name,
            Quantity = quantity,
            Unit = unit,
            Notes = notes,
            Category = category,
            IsCompleted = false,
            CreatedDate = DateTime.UtcNow,
            CompletedDate = null
        };

        _db.ShoppingListItems.Add(shoppingItem);

        UpdateCatalogUsage(
            catalogItem,
            unit,
            category,
            notes);

        await _db.SaveChangesAsync();

        return AddManualShoppingItemResult.Success(
            shoppingItem.ShoppingListItemId,
            shoppingItem.Name,
            false);
    }

    public async Task<AddRecipeToShoppingResult> AddRecipeIngredientsAsync(
        int recipeId,
        AddRecipeToShoppingRequest request)
    {
        if (request.RecipeIngredientIds is null ||
            request.RecipeIngredientIds.Count == 0)
        {
            return AddRecipeToShoppingResult.InvalidSelection(
                "Select at least one ingredient.");
        }

        var selectedIds = request.RecipeIngredientIds
            .Distinct()
            .ToHashSet();

        var recipe = await _db.Recipes
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r =>
                r.RecipeId == recipeId &&
                !r.IsArchived);

        if (recipe is null)
        {
            return AddRecipeToShoppingResult.RecipeNotFound();
        }

        var selectedIngredients = recipe.RecipeIngredients
            .Where(ri =>
                selectedIds.Contains(ri.RecipeIngredientId))
            .OrderBy(ri => ri.SortOrder)
            .ToList();

        if (selectedIngredients.Count == 0)
        {
            return AddRecipeToShoppingResult.InvalidSelection(
                "No valid ingredients were selected.");
        }

        var shoppingList = await GetOrCreateShoppingListAsync();

        var existingItems = await _db.ShoppingListItems
            .Where(x =>
                x.ShoppingListId == shoppingList.ShoppingListId &&
                !x.IsCompleted)
            .ToListAsync();

        var added = 0;
        var merged = 0;

        foreach (var recipeIngredient in selectedIngredients)
        {
            var ingredient = recipeIngredient.Ingredient;

            if (ingredient is null)
            {
                continue;
            }

            var normalizedUnit =
                NormalizeShoppingUnit(recipeIngredient.Unit);

            var normalizedQuantity =
                NormalizeShoppingQuantity(
                    recipeIngredient.Quantity,
                    recipeIngredient.Unit);

            var catalogItem = await GetOrCreateCatalogItemAsync(
                ingredient.Name,
                normalizedUnit,
                "Groceries",
                null);

            var existing = existingItems.FirstOrDefault(x =>
                x.IngredientId == ingredient.IngredientId &&
                NormalizeShoppingUnit(x.Unit) == normalizedUnit);

            if (existing is not null)
            {
                if (normalizedQuantity.HasValue)
                {
                    existing.Quantity =
                        (existing.Quantity ?? 0) +
                        normalizedQuantity.Value;
                }

                existing.ShoppingCatalogItemId ??=
                    catalogItem.ShoppingCatalogItemId;

                existing.Category ??= "Groceries";

                UpdateCatalogUsage(
                    catalogItem,
                    normalizedUnit,
                    "Groceries",
                    null);

                merged++;
                continue;
            }

            var shoppingItem = new ShoppingListItem
            {
                ShoppingListId = shoppingList.ShoppingListId,
                IngredientId = ingredient.IngredientId,
                ShoppingCatalogItemId =
                    catalogItem.ShoppingCatalogItemId,
                Name = ingredient.Name,
                Quantity = normalizedQuantity,
                Unit = normalizedUnit,
                Notes = null,
                Category = "Groceries",
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow,
                CompletedDate = null
            };

            _db.ShoppingListItems.Add(shoppingItem);
            existingItems.Add(shoppingItem);

            UpdateCatalogUsage(
                catalogItem,
                normalizedUnit,
                "Groceries",
                null);

            added++;
        }

        await _db.SaveChangesAsync();

        return AddRecipeToShoppingResult.Success(
            recipe.RecipeId,
            recipe.Name,
            selectedIngredients.Count,
            added,
            merged);
    }

    public async Task<bool> CompleteItemAsync(int itemId)
    {
        var item = await _db.ShoppingListItems
            .FirstOrDefaultAsync(x =>
                x.ShoppingListItemId == itemId);

        if (item is null)
        {
            return false;
        }

        if (!item.IsCompleted)
        {
            item.IsCompleted = true;
            item.CompletedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> UncompleteItemAsync(int itemId)
    {
        var item = await _db.ShoppingListItems
            .FirstOrDefaultAsync(x =>
                x.ShoppingListItemId == itemId);

        if (item is null)
        {
            return false;
        }

        item.IsCompleted = false;
        item.CompletedDate = null;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<int> ClearCompletedAsync()
    {
        var shoppingList = await _db.ShoppingLists
            .FirstOrDefaultAsync(x =>
                !x.IsArchived &&
                x.Name == "Shopping");

        if (shoppingList is null)
        {
            return 0;
        }

        var completedItems = await _db.ShoppingListItems
            .Where(x =>
                x.ShoppingListId == shoppingList.ShoppingListId &&
                x.IsCompleted)
            .ToListAsync();

        if (completedItems.Count == 0)
        {
            return 0;
        }

        _db.ShoppingListItems.RemoveRange(completedItems);

        await _db.SaveChangesAsync();

        return completedItems.Count;
    }

    private async Task<ShoppingList> GetOrCreateShoppingListAsync()
    {
        var shoppingList = await _db.ShoppingLists
            .FirstOrDefaultAsync(x =>
                !x.IsArchived &&
                x.Name == "Shopping");

        if (shoppingList is not null)
        {
            return shoppingList;
        }

        shoppingList = new ShoppingList
        {
            Name = "Shopping",
            CreatedDate = DateTime.UtcNow,
            IsArchived = false
        };

        _db.ShoppingLists.Add(shoppingList);

        await _db.SaveChangesAsync();

        return shoppingList;
    }

    private async Task<ShoppingCatalogItem> GetOrCreateCatalogItemAsync(
        string name,
        string? unit,
        string? category,
        string? notes)
    {
        var normalizedName = NormalizeCatalogName(name);

        var trackedItem = _db.ShoppingCatalogItems.Local
            .FirstOrDefault(x =>
                x.NormalizedName == normalizedName);

        if (trackedItem is not null)
        {
            return trackedItem;
        }

        var existingItem = await _db.ShoppingCatalogItems
            .FirstOrDefaultAsync(x =>
                x.NormalizedName == normalizedName);

        if (existingItem is not null)
        {
            return existingItem;
        }

        var catalogItem = new ShoppingCatalogItem
        {
            Name = name.Trim(),
            NormalizedName = normalizedName,
            DefaultUnit = unit,
            Category = category,
            DefaultNotes = notes,
            UseCount = 0,
            LastUsedDate = null,
            CreatedDate = DateTime.UtcNow
        };

        _db.ShoppingCatalogItems.Add(catalogItem);

        try
        {
            await _db.SaveChangesAsync();
            return catalogItem;
        }
        catch (DbUpdateException)
        {
            /*
             * Two household members could theoretically add the
             * same brand-new item at almost the same moment.
             * The unique NormalizedName index prevents duplicates.
             */
            _db.Entry(catalogItem).State = EntityState.Detached;

            var concurrentItem = await _db.ShoppingCatalogItems
                .FirstOrDefaultAsync(x =>
                    x.NormalizedName == normalizedName);

            if (concurrentItem is not null)
            {
                return concurrentItem;
            }

            throw;
        }
    }

    private static void UpdateCatalogUsage(
        ShoppingCatalogItem catalogItem,
        string? unit,
        string? category,
        string? notes)
    {
        catalogItem.UseCount++;
        catalogItem.LastUsedDate = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(unit))
        {
            catalogItem.DefaultUnit = unit;
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            catalogItem.Category = category;
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            catalogItem.DefaultNotes = notes;
        }
    }

    private static string NormalizeCatalogName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(
            " ",
            value
                .Trim()
                .ToLowerInvariant()
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries));
    }

    private static string? NormalizeShoppingUnit(string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
        {
            return null;
        }

        var normalized = unit.Trim().ToLowerInvariant();

        return normalized switch
        {
            "gram" or "grams" or "g" => "g",
            "kilogram" or "kilograms" or "kg" => "g",

            "millilitre" or
            "millilitres" or
            "milliliter" or
            "milliliters" or
            "ml" => "ml",

            "litre" or
            "litres" or
            "liter" or
            "liters" or
            "l" => "ml",

            "each" => "each",

            "clove" or
            "cloves" => "cloves",

            "tablespoon" or
            "tablespoons" or
            "tbsp" => "tbsp",

            "teaspoon" or
            "teaspoons" or
            "tsp" => "tsp",

            _ => normalized
        };
    }

    private static decimal? NormalizeShoppingQuantity(
        decimal? quantity,
        string? originalUnit)
    {
        if (!quantity.HasValue)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(originalUnit))
        {
            return quantity;
        }

        var unit = originalUnit.Trim().ToLowerInvariant();

        return unit switch
        {
            "kg" or
            "kilogram" or
            "kilograms" =>
                quantity.Value * 1000,

            "l" or
            "litre" or
            "litres" or
            "liter" or
            "liters" =>
                quantity.Value * 1000,

            _ => quantity.Value
        };
    }
}

public sealed class AddRecipeToShoppingResult
{
    public bool IsSuccess { get; init; }
    public bool IsRecipeNotFound { get; init; }
    public bool IsInvalidSelection { get; init; }
    public string? Message { get; init; }
    public int? RecipeId { get; init; }
    public string? RecipeName { get; init; }
    public int Selected { get; init; }
    public int Added { get; init; }
    public int Merged { get; init; }

    public static AddRecipeToShoppingResult Success(
        int recipeId,
        string recipeName,
        int selected,
        int added,
        int merged)
    {
        return new AddRecipeToShoppingResult
        {
            IsSuccess = true,
            RecipeId = recipeId,
            RecipeName = recipeName,
            Selected = selected,
            Added = added,
            Merged = merged
        };
    }

    public static AddRecipeToShoppingResult RecipeNotFound()
    {
        return new AddRecipeToShoppingResult
        {
            IsRecipeNotFound = true,
            Message = "Recipe not found."
        };
    }

    public static AddRecipeToShoppingResult InvalidSelection(
        string message)
    {
        return new AddRecipeToShoppingResult
        {
            IsInvalidSelection = true,
            Message = message
        };
    }
}

public sealed class AddManualShoppingItemResult
{
    public bool IsSuccess { get; init; }
    public bool IsInvalid { get; init; }
    public string? Message { get; init; }
    public int? ItemId { get; init; }
    public string? Name { get; init; }
    public bool Merged { get; init; }

    public static AddManualShoppingItemResult Success(
        int itemId,
        string name,
        bool merged)
    {
        return new AddManualShoppingItemResult
        {
            IsSuccess = true,
            ItemId = itemId,
            Name = name,
            Merged = merged
        };
    }

    public static AddManualShoppingItemResult Invalid(
        string message)
    {
        return new AddManualShoppingItemResult
        {
            IsInvalid = true,
            Message = message
        };
    }
}