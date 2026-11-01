using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.DTOs.Pantry;
using Nook.Api.Models;

namespace Nook.Api.Services;

public class PantryService
{
    private static readonly HashSet<string> StorageLocations =
    [
        "Pantry",
        "Fridge",
        "Freezer",
        "Cleaning",
        "Bathroom",
        "Pet",
        "Medicine",
        "Other"
    ];

    private readonly NookDbContext _db;

    public PantryService(NookDbContext db)
    {
        _db = db;
    }

    public async Task<PantryResponse> GetPantryAsync()
    {
        var entities = await _db.PantryItems
            .AsNoTracking()
            .OrderBy(x => x.StorageLocation)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var items = entities
            .Select(ToResponse)
            .ToList();

        return new PantryResponse
        {
            ItemCount = items.Count,
            LowStockCount = items.Count(x => x.IsLowStock),
            ExpiringSoonCount = items.Count(x => x.IsExpiringSoon),
            Items = items
        };
    }

    public async Task<PantryItemResponse> AddItemAsync(
        UpsertPantryItemRequest request)
    {
        var result = await AddItemCoreAsync(
            request,
            source: "manual",
            purchaseDate: null);

        return result.Item;
    }

    public async Task<PantryItemResponse?> UpdateItemAsync(
        int itemId,
        UpsertPantryItemRequest request)
    {
        Validate(request);

        var entity = await _db.PantryItems
            .FirstOrDefaultAsync(x => x.PantryItemId == itemId);

        if (entity is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        var unit = NormalizeUnit(request.Unit);
        var quantity = NormalizeQuantity(request.Quantity, request.Unit);
        var threshold =
            NormalizeQuantity(request.LowStockThreshold, request.Unit);
        var storageLocation = NormalizeStorageLocation(
            request.StorageLocation);

        var catalogItem = await ResolveCatalogItemAsync(
            request.ShoppingCatalogItemId,
            name,
            unit,
            request.Category,
            request.Notes);

        entity.ShoppingCatalogItemId = catalogItem.ShoppingCatalogItemId;
        entity.Name = catalogItem.Name;
        entity.NormalizedName = NormalizeName(catalogItem.Name);
        entity.Quantity = quantity;
        entity.Unit = unit;
        entity.Category = Clean(request.Category) ?? catalogItem.Category;
        entity.StorageLocation = storageLocation;
        entity.ExpiryDate = request.ExpiryDate;
        entity.LowStockThreshold = threshold;
        entity.Notes = Clean(request.Notes);
        entity.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return ToResponse(entity);
    }

    public async Task<bool> DeleteItemAsync(int itemId)
    {
        var entity = await _db.PantryItems
            .FirstOrDefaultAsync(x => x.PantryItemId == itemId);

        if (entity is null)
        {
            return false;
        }

        _db.PantryItems.Remove(entity);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<ImportReceiptResult> ImportReceiptAsync(
        ImportReceiptRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new PantryValidationException(
                "Select at least one receipt item to import.");
        }

        var imported = 0;
        var merged = 0;
        var importedCatalogIds = new HashSet<int>();
        var importedNames = new HashSet<string>();

        foreach (var item in request.Items)
        {
            var upsert = new UpsertPantryItemRequest
            {
                Name = item.Name,
                Quantity = item.Quantity,
                Unit = item.Unit,
                Category = item.Category,
                StorageLocation = item.StorageLocation,
                Notes = item.Notes,
                ExpiryDate = null,
                LowStockThreshold = null
            };

            var result = await AddItemCoreAsync(
                upsert,
                source: "receipt",
                purchaseDate: request.PurchaseDate);

            if (result.Merged)
            {
                merged++;
            }
            else
            {
                imported++;
            }

            if (result.Item.ShoppingCatalogItemId.HasValue)
            {
                importedCatalogIds.Add(
                    result.Item.ShoppingCatalogItemId.Value);
            }

            importedNames.Add(NormalizeName(result.Item.Name));
        }

        var completed = await CompletePurchasedShoppingItemsAsync(
            importedCatalogIds,
            importedNames);

        return new ImportReceiptResult
        {
            Imported = imported,
            Merged = merged,
            ShoppingItemsCompleted = completed
        };
    }

    private async Task<(PantryItemResponse Item, bool Merged)> AddItemCoreAsync(
        UpsertPantryItemRequest request,
        string source,
        DateOnly? purchaseDate)
    {
        Validate(request);

        var name = request.Name.Trim();
        var unit = NormalizeUnit(request.Unit);
        var quantity = NormalizeQuantity(request.Quantity, request.Unit);
        var threshold =
            NormalizeQuantity(request.LowStockThreshold, request.Unit);
        var category = Clean(request.Category);
        var notes = Clean(request.Notes);
        var storageLocation = NormalizeStorageLocation(
            request.StorageLocation);

        var catalogItem = await ResolveCatalogItemAsync(
            request.ShoppingCatalogItemId,
            name,
            unit,
            category,
            notes);

        var normalizedName = NormalizeName(catalogItem.Name);

        var existing = await _db.PantryItems
            .FirstOrDefaultAsync(x =>
                x.NormalizedName == normalizedName &&
                x.Unit == unit &&
                x.StorageLocation == storageLocation &&
                x.ExpiryDate == request.ExpiryDate);

        if (existing is not null)
        {
            if (quantity.HasValue)
            {
                existing.Quantity =
                    (existing.Quantity ?? 0) + quantity.Value;
            }

            existing.Category ??= category ?? catalogItem.Category;
            existing.Notes ??= notes;
            existing.LowStockThreshold ??= threshold;
            if (purchaseDate.HasValue)
            {
                existing.LastPurchasedDate = purchaseDate;
            }

            existing.Source = source == "receipt"
                ? "receipt"
                : existing.Source;
            existing.UpdatedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return (ToResponse(existing), true);
        }

        var now = DateTime.UtcNow;

        var entity = new PantryItem
        {
            ShoppingCatalogItemId = catalogItem.ShoppingCatalogItemId,
            Name = catalogItem.Name,
            NormalizedName = normalizedName,
            Quantity = quantity,
            Unit = unit,
            Category = category ?? catalogItem.Category,
            StorageLocation = storageLocation,
            ExpiryDate = request.ExpiryDate,
            LowStockThreshold = threshold,
            Notes = notes,
            Source = source,
            LastPurchasedDate = purchaseDate,
            CreatedDate = now,
            UpdatedDate = now
        };

        _db.PantryItems.Add(entity);
        await _db.SaveChangesAsync();

        return (ToResponse(entity), false);
    }

    private async Task<int> CompletePurchasedShoppingItemsAsync(
        HashSet<int> catalogIds,
        HashSet<string> normalizedNames)
    {
        if (catalogIds.Count == 0 && normalizedNames.Count == 0)
        {
            return 0;
        }

        var activeItems = await _db.ShoppingListItems
            .Where(x => !x.IsCompleted)
            .ToListAsync();

        var matches = activeItems
            .Where(x =>
                (x.ShoppingCatalogItemId.HasValue &&
                 catalogIds.Contains(x.ShoppingCatalogItemId.Value)) ||
                normalizedNames.Contains(NormalizeName(x.Name)))
            .ToList();

        if (matches.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;

        foreach (var item in matches)
        {
            item.IsCompleted = true;
            item.CompletedDate = now;
        }

        await _db.SaveChangesAsync();

        return matches.Count;
    }

    private async Task<ShoppingCatalogItem> ResolveCatalogItemAsync(
        int? requestedId,
        string name,
        string? unit,
        string? category,
        string? notes)
    {
        ShoppingCatalogItem? catalogItem = null;

        if (requestedId.HasValue)
        {
            catalogItem = await _db.ShoppingCatalogItems
                .FirstOrDefaultAsync(x =>
                    x.ShoppingCatalogItemId == requestedId.Value);
        }

        var normalizedName = NormalizeName(name);

        catalogItem ??= _db.ShoppingCatalogItems.Local
            .FirstOrDefault(x => x.NormalizedName == normalizedName);

        catalogItem ??= await _db.ShoppingCatalogItems
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedName);

        if (catalogItem is not null)
        {
            return catalogItem;
        }

        catalogItem = new ShoppingCatalogItem
        {
            Name = name,
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

    private static PantryItemResponse ToResponse(PantryItem entity)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var soon = today.AddDays(7);

        return new PantryItemResponse
        {
            Id = entity.PantryItemId,
            ShoppingCatalogItemId = entity.ShoppingCatalogItemId,
            Name = entity.Name,
            Quantity = entity.Quantity,
            Unit = entity.Unit,
            Category = entity.Category,
            StorageLocation = entity.StorageLocation,
            ExpiryDate = entity.ExpiryDate,
            LowStockThreshold = entity.LowStockThreshold,
            Notes = entity.Notes,
            Source = entity.Source,
            LastPurchasedDate = entity.LastPurchasedDate,
            IsLowStock =
                entity.Quantity.HasValue &&
                entity.LowStockThreshold.HasValue &&
                entity.Quantity.Value <= entity.LowStockThreshold.Value,
            IsExpiringSoon =
                entity.ExpiryDate.HasValue &&
                entity.ExpiryDate.Value >= today &&
                entity.ExpiryDate.Value <= soon
        };
    }

    private static void Validate(UpsertPantryItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new PantryValidationException("Item name is required.");
        }

        if (request.Quantity.HasValue && request.Quantity.Value < 0)
        {
            throw new PantryValidationException(
                "Quantity cannot be negative.");
        }

        if (request.LowStockThreshold.HasValue &&
            request.LowStockThreshold.Value < 0)
        {
            throw new PantryValidationException(
                "Low-stock threshold cannot be negative.");
        }

        if (!StorageLocations.Contains(
                NormalizeStorageLocation(request.StorageLocation)))
        {
            throw new PantryValidationException(
                "Choose a valid storage location.");
        }
    }

    private static string NormalizeStorageLocation(string? location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return "Pantry";
        }

        var trimmed = location.Trim();

        return StorageLocations.FirstOrDefault(x =>
                   x.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
               ?? "Other";
    }

    private static string NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(
            " ",
            value.Trim()
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string? NormalizeUnit(string? unit)
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
            "millilitre" or "millilitres" or
            "milliliter" or "milliliters" or "ml" => "ml",
            "litre" or "litres" or "liter" or "liters" or "l" => "ml",
            "each" => "each",
            _ => normalized
        };
    }

    private static decimal? NormalizeQuantity(
        decimal? quantity,
        string? originalUnit)
    {
        if (!quantity.HasValue || string.IsNullOrWhiteSpace(originalUnit))
        {
            return quantity;
        }

        return originalUnit.Trim().ToLowerInvariant() switch
        {
            "kg" or "kilogram" or "kilograms" => quantity.Value * 1000,
            "l" or "litre" or "litres" or "liter" or "liters" =>
                quantity.Value * 1000,
            _ => quantity.Value
        };
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}

public class PantryValidationException : Exception
{
    public PantryValidationException(string message) : base(message) { }
}
