using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.DTOs;
using Nook.Api.Models;
using Nook.Api.Services;

namespace Nook.Api.Tests;

public class ShoppingServiceTests
{
    [Fact]
    public async Task RecipeMilk_MergesWithManualMilk_AndNormalizesLitresToMl()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<NookDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new NookDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new ShoppingService(db);

        await service.AddManualItemAsync(new AddShoppingItemRequest
        {
            Name = "Milk",
            Quantity = 1m,
            Unit = "l"
        });

        var recipeIngredient = await AddRecipeIngredientAsync(
            db, "Milk", 250m, "ml");

        var result = await service.AddRecipeIngredientsAsync(
            recipeIngredient.RecipeId,
            new AddRecipeToShoppingRequest
            {
                RecipeIngredientIds = [recipeIngredient.RecipeIngredientId]
            });

        db.ChangeTracker.Clear();

        var items = await db.ShoppingListItems
            .AsNoTracking()
            .ToListAsync();

        var item = Assert.Single(items);

        Assert.Equal(1250m, item.Quantity);
        Assert.Equal("ml", item.Unit);
        Assert.Null(item.IngredientId);
        Assert.True(result.Merged == 1);
    }

    [Fact]
    public async Task ManualKilograms_AndGrams_MergeIntoGrams()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<NookDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new NookDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new ShoppingService(db);

        await service.AddManualItemAsync(new AddShoppingItemRequest
        {
            Name = "Chicken",
            Quantity = 1m,
            Unit = "kg"
        });

        var result = await service.AddManualItemAsync(new AddShoppingItemRequest
        {
            Name = "Chicken",
            Quantity = 500m,
            Unit = "g"
        });

        db.ChangeTracker.Clear();

        var item = Assert.Single(
            await db.ShoppingListItems.AsNoTracking().ToListAsync());

        Assert.Equal(1500m, item.Quantity);
        Assert.Equal("g", item.Unit);
        Assert.True(result.Merged);
    }

    [Fact]
    public async Task SameItem_WithIncompatibleUnits_DoesNotMerge()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<NookDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new NookDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new ShoppingService(db);

        await service.AddManualItemAsync(new AddShoppingItemRequest
        {
            Name = "Milk",
            Quantity = 1m,
            Unit = "l"
        });

        var recipeIngredient = await AddRecipeIngredientAsync(
            db, "Milk", 250m, "g");

        await service.AddRecipeIngredientsAsync(
            recipeIngredient.RecipeId,
            new AddRecipeToShoppingRequest
            {
                RecipeIngredientIds = [recipeIngredient.RecipeIngredientId]
            });

        db.ChangeTracker.Clear();

        var items = await db.ShoppingListItems
            .AsNoTracking()
            .OrderBy(x => x.Unit)
            .ToListAsync();

        Assert.Equal(2, items.Count);

        Assert.Contains(items, x =>
            x.Unit == "ml" &&
            x.Quantity == 1000m);

        Assert.Contains(items, x =>
            x.Unit == "g" &&
            x.Quantity == 250m);
    }

    [Fact]
    public async Task CompletedItem_IsNotUsedAsMergeTarget()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<NookDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new NookDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new ShoppingService(db);

        await service.AddManualItemAsync(new AddShoppingItemRequest
        {
            Name = "Milk",
            Quantity = 1m,
            Unit = "l"
        });

        var original = await db.ShoppingListItems.SingleAsync();

        await service.CompleteItemAsync(original.ShoppingListItemId);

        var recipeIngredient = await AddRecipeIngredientAsync(
            db, "Milk", 250m, "ml");

        await service.AddRecipeIngredientsAsync(
            recipeIngredient.RecipeId,
            new AddRecipeToShoppingRequest
            {
                RecipeIngredientIds = [recipeIngredient.RecipeIngredientId]
            });

        db.ChangeTracker.Clear();

        var items = await db.ShoppingListItems
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(2, items.Count);

        var completed = Assert.Single(items.Where(x => x.IsCompleted));
        var active = Assert.Single(items.Where(x => !x.IsCompleted));

        Assert.Equal(1000m, completed.Quantity);
        Assert.Equal(250m, active.Quantity);
        Assert.NotNull(active.IngredientId);
    }

    private static async Task<RecipeIngredient> AddRecipeIngredientAsync(
        NookDbContext db,
        string name,
        decimal quantity,
        string unit)
    {
        var ingredient = new Ingredient
        {
            Name = name,
            DefaultUnit = unit
        };

        var recipe = new Recipe
        {
            Name = $"Recipe with {name}",
            CreatedDate = DateTime.UtcNow,
            IsArchived = false
        };

        var recipeIngredient = new RecipeIngredient
        {
            Ingredient = ingredient,
            Quantity = quantity,
            Unit = unit,
            SortOrder = 1
        };

        recipe.RecipeIngredients.Add(recipeIngredient);

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();

        return recipeIngredient;
    }
}