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
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

app.MapFallbackToFile("index.html");

app.Run();