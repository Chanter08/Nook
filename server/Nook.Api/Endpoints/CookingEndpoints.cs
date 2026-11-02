using Nook.Api.Services;

namespace Nook.Api.Endpoints;

public static class CookingEndpoints
{
    public static IEndpointRouteBuilder MapCookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipes");
        group.MapGet("/{recipeId:int}/cook/preview", Preview);
        group.MapPost("/{recipeId:int}/cook", Cook);
        group.MapGet("/{recipeId:int}/cooks", History);
        group.MapPost("/cooks/{cookingId:int}/undo", Undo);
        return app;
    }

    private static async Task<IResult> Preview(
        int recipeId, decimal? servings, RecipeCookingService service)
    {
        try { return Results.Ok(await service.GetPreviewAsync(recipeId, servings)); }
        catch (CookingException e) { return Error(e); }
    }

    private static async Task<IResult> Cook(
        int recipeId, CookRecipeRequest request, RecipeCookingService service)
    {
        try { return Results.Ok(await service.CookAsync(recipeId, request)); }
        catch (CookingException e) { return Error(e); }
    }

    private static async Task<IResult> History(int recipeId, RecipeCookingService service) =>
        Results.Ok(await service.GetHistoryAsync(recipeId));

    private static async Task<IResult> Undo(int cookingId, RecipeCookingService service)
    {
        try
        {
            await service.UndoAsync(cookingId);
            return Results.NoContent();
        }
        catch (CookingException e) { return Error(e); }
    }

    private static IResult Error(CookingException exception) =>
        Results.Json(new { message = exception.Message }, statusCode: exception.StatusCode);
}