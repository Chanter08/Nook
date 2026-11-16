using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.Models;
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

builder.Services.AddScoped<WebPushSender>();
builder.Services.AddHostedService<PushReminderWorker>();
builder.Services.AddScoped<ShoppingService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<PantryService>();
builder.Services.AddScoped<RecipeCookingService>();
builder.Services.AddScoped<ReceiptScannerService>();
builder.Services.AddScoped<PlantAnalysisService>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("NookMobile", policy =>
    {
        policy
            .WithOrigins("capacitor://localhost", "http://localhost")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("NookMobile");

// Fail CI before deployment when the model and the last migration snapshot differ.
// This checks metadata only; it does not connect to or change the database.
if (args.Any(x => x.Equals("--verify-migrations", StringComparison.OrdinalIgnoreCase)))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<NookDbContext>();

    if (db.Database.HasPendingModelChanges())
    {
        throw new InvalidOperationException(
            "NookDbContext does not match the latest EF migration snapshot.");
    }

    Console.WriteLine("EF Core model matches the latest migration snapshot.");
    return;
}

if (args.Any(x => x.Equals("--migrate", StringComparison.OrdinalIgnoreCase)))
{
    await using var scope = app.Services.CreateAsyncScope();

    var db = scope.ServiceProvider.GetRequiredService<NookDbContext>();

    Console.WriteLine("Applying database migrations...");

    await db.Database.MigrateAsync();

    Console.WriteLine("Database migrations complete.");

    return;
}

// Production smoke check: prove the live SQL Server accepts two distinct dinners
// for the same date and meal type. The inserts are inside a rolled-back
// transaction so no test meal or shopping data persists.
if (args.Any(x => x.Equals("--verify-dinner-slots", StringComparison.OrdinalIgnoreCase)))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<NookDbContext>();

    var recipeIds = await db.Recipes.AsNoTracking()
        .Where(x => !x.IsArchived)
        .OrderBy(x => x.RecipeId)
        .Select(x => x.RecipeId)
        .Take(2)
        .ToListAsync();

    if (recipeIds.Count < 2)
        throw new InvalidOperationException("Dinner slot smoke test requires two existing recipes.");

    var candidate = new DateOnly(2099, 1, 1);
    var found = false;
    for (var offset = 0; offset < 100; offset++)
    {
        var date = candidate.AddDays(offset);
        if (!await db.MealPlanEntries.AnyAsync(x => x.PlanDate == date))
        {
            candidate = date;
            found = true;
            break;
        }
    }
    if (!found)
        throw new InvalidOperationException("No empty future date for the dinner-slot smoke test.");

    await using (var transaction = await db.Database.BeginTransactionAsync())
    {
        try
        {
            db.MealPlanEntries.AddRange(
                new MealPlanEntry
                {
                    PlanDate = candidate, MealType = "Dinner", Slot = 1,
                    RecipeId = recipeIds[0], CreatedDate = DateTime.UtcNow
                },
                new MealPlanEntry
                {
                    PlanDate = candidate, MealType = "Dinner", Slot = 2,
                    RecipeId = recipeIds[1], CreatedDate = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();

            var count = await db.MealPlanEntries.AsNoTracking()
                .CountAsync(x => x.PlanDate == candidate && x.MealType == "Dinner");
            if (count != 2)
                throw new InvalidOperationException($"Expected two dinner entries; found {count}.");
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    Console.WriteLine("Live database verified: two dinners saved together; test data rolled back.");
    return;
}

app.UseStaticFiles();

app.MapPushEndpoints();
app.MapCalendarEndpoints();
app.MapRecipeEndpoints();
app.MapShoppingEndpoints();
app.MapMealPlanEndpoints();
app.MapDiscoverEndpoints();
app.MapChoreEndpoints();
app.MapPantryEndpoints();
app.MapCookingEndpoints();
app.MapPlantEndpoints();

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