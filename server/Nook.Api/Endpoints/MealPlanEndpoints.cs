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

    private static async Task<IResult> AddEntry(AddMealPlanEntryRequest request, NookDbContext db)
    {
        if (!AllowedMealTypes.Contains(request.MealType, StringComparer.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { message = "Invalid meal type." });
        }

        var mealType = AllowedMealTypes.First(x =>
            x.Equals(request.MealType, StringComparison.OrdinalIgnoreCase));

        var recipeExists = await db.Recipes
            .AsNoTracking()
            .AnyAsync(x => x.RecipeId == request.RecipeId && !x.IsArchived);

        if (!recipeExists)
        {
            return Results.NotFound(new { message = "Recipe not found." });
        }

        var slotExists = await db.MealPlanEntries
            .AnyAsync(x => x.PlanDate == request.Date && x.MealType == mealType);

        if (slotExists)
        {
            return Results.Conflict(new
            {
                message = $"{mealType} is already planned for this date."
            });
        }

        var entry = new MealPlanEntry
        {
            PlanDate = request.Date,
            MealType = mealType,
            RecipeId = request.RecipeId,
            CreatedDate = DateTime.UtcNow
        };

        db.MealPlanEntries.Add(entry);
        await db.SaveChangesAsync();

        return Results.Created(
            $"/api/meal-plan/{entry.MealPlanEntryId}",
            new CreateMealPlanEntryResponse
            {
                Id = entry.MealPlanEntryId
            });
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

        // Preserve meal type and never overwrite an occupied slot.
        var occupied = await db.MealPlanEntries.AnyAsync(x =>
            x.MealPlanEntryId != id &&
            x.PlanDate == request.Date &&
            x.MealType == entry.MealType);

        if (occupied)
            return Results.Conflict(new {
                message = $"{entry.MealType} is already planned for that day. Move it first."
            });

        entry.PlanDate = request.Date;
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // The unique (PlanDate, MealType) index guards concurrent moves.
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