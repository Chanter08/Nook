using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.DTOs.Pantry;
using Nook.Api.Models;

namespace Nook.Api.Endpoints;

public static class PantryEndpoints
{
    private static readonly string[] AllowedLocations =
    [
        "Pantry",
        "Fridge",
        "Freezer",
        "Other"
    ];

    public static IEndpointRouteBuilder MapPantryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pantry");

        group.MapGet("", GetItems);
        group.MapPost("", AddItem);
        group.MapPost("/bulk", BulkAddItems);
        group.MapPut("/{id:int}", UpdateItem);
        group.MapDelete("/{id:int}", DeleteItem);

        return app;
    }

    private static async Task<IResult> GetItems(NookDbContext db)
    {
        var items = await db.PantryItems
            .AsNoTracking()
            .OrderBy(x => x.Location)
            .ThenBy(x => x.ExpiryDate == null)
            .ThenBy(x => x.ExpiryDate)
            .ThenBy(x => x.Name)
            .Select(x => ToResponse(x))
            .ToListAsync();

        return Results.Ok(items);
    }

    private static async Task<IResult> AddItem(SavePantryItemRequest request, NookDbContext db)
    {
        var validation = Validate(request);

        if (validation.Error is not null)
        {
            return Results.BadRequest(new { message = validation.Error });
        }

        var now = DateTime.UtcNow;
        var item = new PantryItem
        {
            Name = validation.Name!,
            Quantity = request.Quantity,
            Unit = NormalizeOptional(request.Unit),
            Location = validation.Location!,
            ExpiryDate = request.ExpiryDate,
            AddedDate = now,
            UpdatedDate = now
        };

        db.PantryItems.Add(item);
        await db.SaveChangesAsync();

        return Results.Created($"/api/pantry/{item.PantryItemId}", ToResponse(item));
    }

    private static async Task<IResult> BulkAddItems(BulkAddPantryItemsRequest request, NookDbContext db)
    {
        if (request.Items.Count is < 1 or > 100)
        {
            return Results.BadRequest(new { message = "Add between 1 and 100 pantry items at a time." });
        }

        var validated = new List<(SavePantryItemRequest Request, string Name, string Location)>();

        foreach (var requestItem in request.Items)
        {
            var validation = Validate(requestItem);

            if (validation.Error is not null)
            {
                return Results.BadRequest(new { message = validation.Error });
            }

            validated.Add((requestItem, validation.Name!, validation.Location!));
        }

        var now = DateTime.UtcNow;
        var items = validated.Select(x => new PantryItem
        {
            Name = x.Name,
            Quantity = x.Request.Quantity,
            Unit = NormalizeOptional(x.Request.Unit),
            Location = x.Location,
            ExpiryDate = x.Request.ExpiryDate,
            AddedDate = now,
            UpdatedDate = now
        }).ToList();

        db.PantryItems.AddRange(items);
        await db.SaveChangesAsync();

        return Results.Ok(items.Select(ToResponse).ToList());
    }

    private static async Task<IResult> UpdateItem(int id, SavePantryItemRequest request, NookDbContext db)
    {
        var item = await db.PantryItems.FindAsync(id);

        if (item is null)
        {
            return Results.NotFound(new { message = "Pantry item not found." });
        }

        var validation = Validate(request);

        if (validation.Error is not null)
        {
            return Results.BadRequest(new { message = validation.Error });
        }

        item.Name = validation.Name!;
        item.Quantity = request.Quantity;
        item.Unit = NormalizeOptional(request.Unit);
        item.Location = validation.Location!;
        item.ExpiryDate = request.ExpiryDate;
        item.UpdatedDate = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> DeleteItem(int id, NookDbContext db)
    {
        var item = await db.PantryItems.FindAsync(id);

        if (item is null)
        {
            return Results.NotFound(new { message = "Pantry item not found." });
        }

        db.PantryItems.Remove(item);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static (string? Name, string? Location, string? Error) Validate(SavePantryItemRequest request)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return (null, null, "Item name is required.");
        }

        if (name.Length > 150)
        {
            return (null, null, "Item name must be 150 characters or fewer.");
        }

        if (request.Quantity <= 0 || request.Quantity > 9999)
        {
            return (null, null, "Quantity must be greater than zero.");
        }

        var unit = NormalizeOptional(request.Unit);

        if (unit?.Length > 50)
        {
            return (null, null, "Unit must be 50 characters or fewer.");
        }

        var location = AllowedLocations.FirstOrDefault(x =>
            x.Equals(request.Location?.Trim() ?? "Pantry", StringComparison.OrdinalIgnoreCase));

        if (location is null)
        {
            return (null, null, "Location must be Pantry, Fridge, Freezer or Other.");
        }

        return (name, location, null);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static PantryItemResponse ToResponse(PantryItem item)
    {
        return new PantryItemResponse
        {
            Id = item.PantryItemId,
            Name = item.Name,
            Quantity = item.Quantity,
            Unit = item.Unit,
            Location = item.Location,
            ExpiryDate = item.ExpiryDate,
            AddedDate = item.AddedDate,
            UpdatedDate = item.UpdatedDate
        };
    }
}
