using Nook.Api.DTOs;
using Nook.Api.Services;

namespace Nook.Api.Endpoints;

public static class ShoppingEndpoints
{
    public static IEndpointRouteBuilder MapShoppingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/shopping");

        group.MapGet("", GetShoppingList);
        group.MapGet("/catalog/search", SearchCatalog);
        group.MapPost("/items", AddItem);
        group.MapPost("/from-recipe/{recipeId:int}", AddFromRecipe);
        group.MapPatch("/items/{itemId:int}/complete", CompleteItem);
        group.MapPatch("/items/{itemId:int}/uncomplete", UncompleteItem);
        group.MapDelete("/completed", ClearCompleted);

        return app;
    }

    private static async Task<IResult> GetShoppingList(ShoppingService service)
    {
        var result = await service.GetShoppingListAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> SearchCatalog(string? q, ShoppingService service)
    {
        var result = await service.SearchCatalogAsync(q);
        return Results.Ok(result);
    }

    private static async Task<IResult> AddItem(
        AddShoppingItemRequest request,
        ShoppingService service)
    {
        var result = await service.AddManualItemAsync(request);

        if (result.IsInvalid)
        {
            return Results.BadRequest(new { message = result.Message });
        }

        return Results.Ok(new
        {
            id = result.ItemId,
            name = result.Name,
            merged = result.Merged
        });
    }

    private static async Task<IResult> AddFromRecipe(
        int recipeId,
        AddRecipeToShoppingRequest request,
        ShoppingService service)
    {
        var result = await service.AddRecipeIngredientsAsync(recipeId, request);

        if (result.IsRecipeNotFound)
        {
            return Results.NotFound(new { message = result.Message });
        }

        if (result.IsInvalidSelection)
        {
            return Results.BadRequest(new { message = result.Message });
        }

        return Results.Ok(new
        {
            recipeId = result.RecipeId,
            recipeName = result.RecipeName,
            selected = result.Selected,
            added = result.Added,
            merged = result.Merged
        });
    }

    private static async Task<IResult> CompleteItem(int itemId, ShoppingService service)
    {
        var completed = await service.CompleteItemAsync(itemId);

        return completed
            ? Results.NoContent()
            : Results.NotFound(new { message = "Shopping item not found." });
    }

    private static async Task<IResult> UncompleteItem(int itemId, ShoppingService service)
    {
        var restored = await service.UncompleteItemAsync(itemId);

        return restored
            ? Results.NoContent()
            : Results.NotFound(new { message = "Shopping item not found." });
    }

    private static async Task<IResult> ClearCompleted(ShoppingService service)
    {
        var deleted = await service.ClearCompletedAsync();
        return Results.Ok(new { deleted });
    }
}