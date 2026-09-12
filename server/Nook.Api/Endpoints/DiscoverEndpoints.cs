using Nook.Api.Integrations.Recipes;

namespace Nook.Api.Endpoints;

public static class DiscoverEndpoints
{
    public static IEndpointRouteBuilder MapDiscoverEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/discover");

        group.MapGet("/search", Search);
        group.MapGet("/{externalId}", GetById);

        return app;
    }

    private static async Task<IResult> Search(
        string? q,
        IRecipeDiscoveryProvider provider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.Ok(Array.Empty<object>());
        }

        var recipes = await provider.SearchAsync(
            q,
            cancellationToken);

        return Results.Ok(recipes);
    }

    private static async Task<IResult> GetById(
        string externalId,
        IRecipeDiscoveryProvider provider,
        CancellationToken cancellationToken)
    {
        var recipe = await provider.GetByIdAsync(
            externalId,
            cancellationToken);

        return recipe is null
            ? Results.NotFound(
                new { message = "Recipe not found." })
            : Results.Ok(recipe);
    }
}