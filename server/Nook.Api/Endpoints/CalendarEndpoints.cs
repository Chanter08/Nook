using Nook.Api.Services;

namespace Nook.Api.Endpoints;

public static class CalendarEndpoints
{
    public static IEndpointRouteBuilder MapCalendarEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/calendar");
        group.MapGet("/upcoming", GetUpcoming);
        return app;
    }

    private static async Task<IResult> GetUpcoming(CalendarService calendarService)
    {
        try
        {
            var events = await calendarService.GetUpcomingAsync();
            return Results.Ok(events);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}