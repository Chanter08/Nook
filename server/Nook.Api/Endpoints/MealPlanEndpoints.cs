using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.DTOs;
using Nook.Api.Models;
using Nook.Api.DTOs.MealPlan;

namespace Nook.Api.Endpoints;

public static class MealPlanEndpoints
{
    private static readonly string[] AllowedMealTypes =
    [
        "Breakfast",
        "Lunch",
        "Dinner",
        "Snack"
    ];

    public static IEndpointRouteBuilder MapMealPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/meal-plan");

        group.MapGet("/week", GetWeek);
        group.MapPost("", AddEntry);
        group.MapPost("/batch", AddEntriesBatch);
        group.MapDelete("/{id:int}", DeleteEntry);
        group.MapPatch("/{id:int}/move", MoveEntry);

        return app;
    }

    private static async Task<IResult> GetWeek(DateOnly start, NookDbContext db)
    {
        var end = start.AddDays(7);

        var entries = await db.MealPlanEntries
            .AsNoTracking()
            .Where(x => x.PlanDate >= start && x.PlanDate < end)
            .OrderBy(x => x.PlanDate)
            .ThenBy(x => x.MealType)
            .ThenBy(x => x.Slot)
            .Select(x => new MealPlanEntryResponse
            {
                Id = x.MealPlanEntryId,
                Date = x.PlanDate,
                MealType = x.MealType,
                Recipe = new MealPlanRecipeResponse
                {
                    Id = x.Recipe.RecipeId,
                    Name = x.Recipe.Name,
                    ImageUrl = x.Recipe.ImageUrl
                }
            })
            .ToListAsync();

        return Results.Ok(entries);
    }


    private sealed record AddMealPlanEntriesRequest(DateOnly Date, string MealType, List<int> RecipeIds);

    /// <summary>
    /// Add one or two distinct dinner recipes in one atomic save.
    /// Other meals still have a capacity of one per date.
    /// </summary>
    private static async Task<IResult> AddEntriesBatch(
        AddMealPlanEntriesRequest request, NookDbContext db)
    {
        if (request.Date == default)
            return Results.BadRequest(new { message = "A valid date is required." });

        if (string.IsNullOrWhiteSpace(request.MealType) ||
            !AllowedMealTypes.Contains(request.MealType, StringComparer.OrdinalIgnoreCase))
            return Results.BadRequest(new { message = "Invalid meal type." });

        var mealType = AllowedMealTypes.First(type =>
            type.Equals(request.MealType, StringComparison.OrdinalIgnoreCase));
        var maxSlots = mealType == "Dinner" ? 2 : 1;
        var recipeIds = request.RecipeIds;

        if (recipeIds is null || recipeIds.Count == 0 || recipeIds.Count > maxSlots ||
            recipeIds.Any(id => id <= 0) || recipeIds.Distinct().Count() != recipeIds.Count)
            return Results.BadRequest(new {
                message = mealType == "Dinner"
                    ? "Choose one or two different dinner recipes."
                    : "Choose exactly one recipe for this meal."
            });

        var existing = await db.MealPlanEntries.AsNoTracking()
            .Where(x => x.PlanDate == request.Date && x.MealType == mealType)
            .Select(x => new { x.Slot, x.RecipeId })
            .ToListAsync();

        if (recipeIds.Any(id => existing.Any(item => item.RecipeId == id)))
            return Results.Conflict(new { message = "One of those recipes is already planned for this meal on that date." });

        var availableSlots = Enumerable.Range(1, maxSlots)
            .Where(slot => existing.All(item => item.Slot != slot))
            .Take(recipeIds.Count)
            .ToList();

        if (availableSlots.Count < recipeIds.Count)
            return Results.Conflict(new {
                message = mealType == "Dinner"
                    ? "This date already has two dinners. Remove one to add another."
                    : $"{mealType} is already planned for this date."
            });

        var activeRecipeIds = await db.Recipes.AsNoTracking()
            .Where(x => recipeIds.Contains(x.RecipeId) && !x.IsArchived)
            .Select(x => x.RecipeId)
            .ToListAsync();

        if (activeRecipeIds.Count != recipeIds.Count)
            return Results.NotFound(new { message = "One or more selected recipes could not be found." });

        var entries = recipeIds.Select((recipeId, index) => new MealPlanEntry
        {
            PlanDate = request.Date,
            MealType = mealType,
            RecipeId = recipeId,
            Slot = availableSlots[index],
            CreatedDate = DateTime.UtcNow
        }).ToList();

        db.MealPlanEntries.AddRange(entries);

        try
        {
            // SaveChanges uses a single transaction for all inserts: no half-planned dinners.
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new {
                message = "The database rejected these dinner slots. Please refresh and try again."
            });
        }

        return Results.Created("/api/meal-plan", new { ids = entries.Select(x => x.MealPlanEntryId).ToList() });
    }

    private static int FindAvailableSlot(string mealType, IReadOnlyCollection<int> occupiedSlots)
    {
        var maxSlots = mealType == "Dinner" ? 2 : 1;
        return Enumerable.Range(1, maxSlots).FirstOrDefault(slot => !occupiedSlots.Contains(slot));
    }

    private static async Task<IResult> AddEntry(AddMealPlanEntryRequest request, NookDbContext db)
    {
        if (request.Date == default)
            return Results.BadRequest(new { message = "A valid date is required." });

        if (!AllowedMealTypes.Contains(request.MealType, StringComparer.OrdinalIgnoreCase))
            return Results.BadRequest(new { message = "Invalid meal type." });

        var mealType = AllowedMealTypes.First(x =>
            x.Equals(request.MealType, StringComparison.OrdinalIgnoreCase));

        var recipeExists = await db.Recipes.AsNoTracking()
            .AnyAsync(x => x.RecipeId == request.RecipeId && !x.IsArchived);
        if (!recipeExists)
            return Results.NotFound(new { message = "Recipe not found." });

        var existing = await db.MealPlanEntries.AsNoTracking()
            .Where(x => x.PlanDate == request.Date && x.MealType == mealType)
            .Select(x => new { x.Slot, x.RecipeId })
            .ToListAsync();

        if (existing.Any(x => x.RecipeId == request.RecipeId))
            return Results.Conflict(new { message = "That recipe is already planned for this meal on this date." });

        var slot = FindAvailableSlot(mealType, existing.Select(x => x.Slot).ToArray());
        if (slot == 0)
            return Results.Conflict(new {
                message = mealType == "Dinner"
                    ? "Two dinners are already planned for this date."
                    : $"{mealType} is already planned for this date."
            });

        var entry = new MealPlanEntry
        {
            PlanDate = request.Date,
            MealType = mealType,
            RecipeId = request.RecipeId,
            Slot = slot,
            CreatedDate = DateTime.UtcNow
        };

        db.MealPlanEntries.Add(entry);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Unique (date, meal type, slot) index also protects concurrent requests.
            return Results.Conflict(new {
                message = "That meal slot was just taken. Refresh the plan and try again."
            });
        }

        return Results.Created(
            $"/api/meal-plan/{entry.MealPlanEntryId}",
            new CreateMealPlanEntryResponse { Id = entry.MealPlanEntryId });
    }

    private sealed record MoveMealPlanEntryRequest(DateOnly Date);

    private static async Task<IResult> MoveEntry(
        int id, MoveMealPlanEntryRequest request, NookDbContext db)
    {
        var entry = await db.MealPlanEntries.FindAsync(id);
        if (entry is null)
            return Results.NotFound(new { message = "Meal plan entry not found." });
        if (request.Date == default)
            return Results.BadRequest(new { message = "A valid destination date is required." });
        if (entry.PlanDate == request.Date)
            return Results.NoContent();

        var existing = await db.MealPlanEntries.AsNoTracking()
            .Where(x => x.MealPlanEntryId != id &&
                x.PlanDate == request.Date && x.MealType == entry.MealType)
            .Select(x => new { x.Slot, x.RecipeId })
            .ToListAsync();

        if (existing.Any(x => x.RecipeId == entry.RecipeId))
            return Results.Conflict(new { message = "That recipe is already planned for this meal on the destination date." });

        var slot = FindAvailableSlot(entry.MealType, existing.Select(x => x.Slot).ToArray());
        if (slot == 0)
            return Results.Conflict(new {
                message = entry.MealType == "Dinner"
                    ? "Two dinners are already planned for that date. Move or remove one first."
                    : $"{entry.MealType} is already planned for that day. Move it first."
            });

        entry.PlanDate = request.Date;
        entry.Slot = slot;
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new {
                message = "That meal slot is no longer available. Refresh and try again."
            });
        }
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteEntry(int id, NookDbContext db)
    {
        var entry = await db.MealPlanEntries.FindAsync(id);

        if (entry is null)
        {
            return Results.NotFound();
        }

        db.MealPlanEntries.Remove(entry);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}