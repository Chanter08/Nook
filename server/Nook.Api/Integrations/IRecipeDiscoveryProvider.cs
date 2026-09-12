using Nook.Api.DTOs.Discover;

namespace Nook.Api.Integrations.Recipes;

public interface IRecipeDiscoveryProvider
{
    Task<List<DiscoverRecipeSummaryResponse>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);

    Task<DiscoverRecipeDetailResponse?> GetByIdAsync(
        string externalId,
        CancellationToken cancellationToken = default);
}