using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.DTOs.Chores;
using Nook.Api.Models;

namespace Nook.Api.Endpoints;

public static class ChoreEndpoints
{
    private static readonly string[] AllowedRecurrences = ["None", "Daily", "Weekly", "Monthly"];

    public static IEndpointRouteBuilder MapChoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chores");
        group.MapGet("", GetChores);
        group.MapPost("", CreateChore);
        group.MapPut("/{id:int}", UpdateChore);
        group.MapPatch("/{id:int}/complete", CompleteChore);
        group.MapPatch("/{id:int}/restore", RestoreChore);
        group.MapDelete("/{id:int}", DeleteChore);
        return app;
    }

    private static async Task<IResult> GetChores(NookDbContext db)
    {
        var chores = await db.Chores.AsNoTracking()
            .OrderBy(x => x.IsCompleted)
            .ThenBy(x => x.DueDate == null)
            .ThenBy(x => x.DueDate)
            .ThenBy(x => x.Title)
            .Select(x => new ChoreResponse
            {
                Id = x.ChoreId,
                Title = x.Title,
                AssignedTo = x.AssignedTo,
                DueDate = x.DueDate,
                Recurrence = x.Recurrence,
                IsCompleted = x.IsCompleted,
                CreatedDate = x.CreatedDate,
                LastCompletedDate = x.LastCompletedDate
            })
            .ToListAsync();

        return Results.Ok(chores);
    }

    private static async Task<IResult> CreateChore(CreateChoreRequest request, NookDbContext db)
    {
        var validation = Validate(request.Title, request.AssignedTo, request.Recurrence);
        if (validation.Error is not null) return Results.BadRequest(new { message = validation.Error });

        var chore = new Chore
        {
            Title = validation.Title!,
            AssignedTo = NormalizeOptional(request.AssignedTo),
            DueDate = request.DueDate,
            Recurrence = validation.Recurrence!,
            IsCompleted = false,
            CreatedDate = DateTime.UtcNow
        };

        db.Chores.Add(chore);
        await db.SaveChangesAsync();
        return Results.Created($"/api/chores/{chore.ChoreId}", ToResponse(chore));
    }

    private static async Task<IResult> UpdateChore(int id, UpdateChoreRequest request, NookDbContext db)
    {
        var chore = await db.Chores.FindAsync(id);
        if (chore is null) return Results.NotFound(new { message = "Chore not found." });

        var validation = Validate(request.Title, request.AssignedTo, request.Recurrence);
        if (validation.Error is not null) return Results.BadRequest(new { message = validation.Error });

        chore.Title = validation.Title!;
        chore.AssignedTo = NormalizeOptional(request.AssignedTo);
        chore.DueDate = request.DueDate;
        chore.Recurrence = validation.Recurrence!;
        await db.SaveChangesAsync();
        return Results.Ok(ToResponse(chore));
    }

    private static async Task<IResult> CompleteChore(int id, NookDbContext db)
    {
        var chore = await db.Chores.FindAsync(id);
        if (chore is null) return Results.NotFound(new { message = "Chore not found." });

        chore.LastCompletedDate = DateTime.UtcNow;

        if (chore.Recurrence == "None")
        {
            chore.IsCompleted = true;
        }
        else
        {
            chore.IsCompleted = false;
            chore.DueDate = GetNextDueDate(chore.DueDate, chore.Recurrence);
        }

        await db.SaveChangesAsync();
        return Results.Ok(ToResponse(chore));
    }

    private static async Task<IResult> RestoreChore(int id, NookDbContext db)
    {
        var chore = await db.Chores.FindAsync(id);
        if (chore is null) return Results.NotFound(new { message = "Chore not found." });

        if (chore.Recurrence != "None")
        {
            return Results.BadRequest(new { message = "Repeating chores stay active and automatically move to their next due date." });
        }

        chore.IsCompleted = false;
        chore.LastCompletedDate = null;
        await db.SaveChangesAsync();
        return Results.Ok(ToResponse(chore));
    }

    private static async Task<IResult> DeleteChore(int id, NookDbContext db)
    {
        var chore = await db.Chores.FindAsync(id);
        if (chore is null) return Results.NotFound(new { message = "Chore not found." });

        db.Chores.Remove(chore);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static (string? Title, string? Recurrence, string? Error) Validate(string? title, string? assignedTo, string? recurrence)
    {
        var normalizedTitle = title?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTitle)) return (null, null, "Chore title is required.");
        if (normalizedTitle.Length > 150) return (null, null, "Chore title must be 150 characters or fewer.");

        var normalizedAssignee = NormalizeOptional(assignedTo);
        if (normalizedAssignee?.Length > 80) return (null, null, "Assignee must be 80 characters or fewer.");

        var normalizedRecurrence = AllowedRecurrences.FirstOrDefault(x => x.Equals(recurrence?.Trim() ?? "None", StringComparison.OrdinalIgnoreCase));
        if (normalizedRecurrence is null) return (null, null, "Recurrence must be None, Daily, Weekly or Monthly.");

        return (normalizedTitle, normalizedRecurrence, null);
    }

    private static DateOnly GetNextDueDate(DateOnly? currentDueDate, string recurrence)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var next = currentDueDate ?? today;

        do
        {
            next = recurrence switch
            {
                "Daily" => next.AddDays(1),
                "Weekly" => next.AddDays(7),
                "Monthly" => next.AddMonths(1),
                _ => next
            };
        }
        while (next <= today);

        return next;
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ChoreResponse ToResponse(Chore chore) => new()
    {
        Id = chore.ChoreId,
        Title = chore.Title,
        AssignedTo = chore.AssignedTo,
        DueDate = chore.DueDate,
        Recurrence = chore.Recurrence,
        IsCompleted = chore.IsCompleted,
        CreatedDate = chore.CreatedDate,
        LastCompletedDate = chore.LastCompletedDate
    };
}
