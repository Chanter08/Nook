using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.Endpoints;
using Nook.Api.Integrations.Recipes.Spoonacular;
using Nook.Api.Services;
using Nook.Api.Integrations.Recipes.FatSecret;
using Nook.Api.Integrations.Recipes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddDbContext<NookDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("NookDb")
    )
);

builder.Services.AddScoped<ShoppingService>();
builder.Services.AddScoped<CalendarService>();

builder.Services.AddSingleton<FatSecretTokenService>();

builder.Services.AddHttpClient<FatSecretProvider>(client =>
{
    client.BaseAddress = new Uri(
        "https://platform.fatsecret.com/rest/");
});

builder.Services.AddHttpClient<SpoonacularProvider>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();

    var apiKey = configuration["Spoonacular:RapidApiKey"];
    var apiHost = configuration["Spoonacular:RapidApiHost"];

    if (string.IsNullOrWhiteSpace(apiKey) ||
        string.IsNullOrWhiteSpace(apiHost))
    {
        throw new InvalidOperationException(
            "Spoonacular RapidAPI configuration is missing."
        );
    }

    client.BaseAddress = new Uri($"https://{apiHost}/");

    client.DefaultRequestHeaders.Add(
        "X-RapidAPI-Key",
        apiKey
    );

    client.DefaultRequestHeaders.Add(
        "X-RapidAPI-Host",
        apiHost
    );
});

builder.Services.AddScoped<IRecipeDiscoveryProvider>(services =>
    services.GetRequiredService<SpoonacularProvider>());

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
app.MapDiscoverEndpoints();

app.MapGet("/health", async (NookDbContext db) =>
{
    var databaseAvailable = await db.Database.CanConnectAsync();

    return databaseAvailable
        ? Results.Ok(new { status = "healthy" })
        : Results.Json(
            new
            {
                status = "unhealthy",
                database = "unavailable"
            },
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
});

app.MapFallbackToFile("index.html");

app.Run();