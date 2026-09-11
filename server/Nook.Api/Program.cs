using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.Endpoints;
using Nook.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddDbContext<NookDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("NookDb")
    )
);

builder.Services.AddScoped<ShoppingService>();
builder.Services.AddScoped<CalendarService>();

var app = builder.Build();

if (args.Any(x => x.Equals("--migrate", StringComparison.OrdinalIgnoreCase)))
{
    await using var scope = app.Services.CreateAsyncScope();

    var db = scope.ServiceProvider.GetRequiredService<NookDbContext>();

    Console.WriteLine("Applying database migrations...");

    await db.Database.MigrateAsync();

    Console.WriteLine("Database migrations complete.");

    return;
}

app.UseStaticFiles();

app.MapCalendarEndpoints();
app.MapRecipeEndpoints();
app.MapShoppingEndpoints();
app.MapMealPlanEndpoints();

app.MapGet("/health", async (NookDbContext db) =>
{
    var databaseAvailable = await db.Database.CanConnectAsync();

    return databaseAvailable
        ? Results.Ok(new { status = "healthy" })
        : Results.Json(
            new { status = "unhealthy", database = "unavailable" },
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
});

app.MapFallbackToFile("index.html");

app.Run();