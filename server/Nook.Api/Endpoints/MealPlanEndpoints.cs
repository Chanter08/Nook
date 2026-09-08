using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.DTOs;
using Nook.Api.Models;

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
            .Select(x => new
            {
                id = x.MealPlanEntryId,
                date = x.PlanDate,
                mealType = x.MealType,
                recipe = new
                {
                    id = x.Recipe.RecipeId,
                    name = x.Recipe.Name,
                    imageUrl = x.Recipe.ImageUrl
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

        return Results.Created($"/api/meal-plan/{entry.MealPlanEntryId}", new
        {
            id = entry.MealPlanEntryId
        });
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