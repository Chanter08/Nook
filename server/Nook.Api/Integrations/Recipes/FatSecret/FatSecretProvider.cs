using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nook.Api.DTOs.Discover;
using Nook.Api.Integrations.Recipes.FatSecret.Models;

namespace Nook.Api.Integrations.Recipes.FatSecret;

public class FatSecretProvider : IRecipeDiscoveryProvider
{
    private const string ProviderName = "FatSecret";

    private readonly HttpClient _httpClient;
    private readonly FatSecretTokenService _tokenService;

    public FatSecretProvider(
        HttpClient httpClient,
        FatSecretTokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    public async Task<List<DiscoverRecipeSummaryResponse>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        await AuthorizeAsync(cancellationToken);

        var encodedQuery = Uri.EscapeDataString(query.Trim());

        var response =
            await _httpClient.GetAsync(
                $"recipes/search/v3" +
                $"?search_expression={encodedQuery}" +
                $"&max_results=20" +
                $"&format=json",
                cancellationToken);

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        Console.WriteLine(raw);

        response.EnsureSuccessStatusCode();

        var result = System.Text.Json.JsonSerializer.Deserialize<FatSecretSearchResponse>(
        raw,
        new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Recipes?.Recipe
            .Select(recipe =>
                new DiscoverRecipeSummaryResponse
                {
                    Provider = ProviderName,
                    ExternalId = recipe.Id,
                    Name = recipe.Name,
                    ImageUrl = recipe.ImageUrl,
                    Category = recipe.RecipeTypes?
                        .RecipeType
                        .FirstOrDefault(),
                    Cuisine = null
                })
            .ToList() ?? [];
    }

    public Task<DiscoverRecipeDetailResponse?> GetByIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task AuthorizeAsync(
        CancellationToken cancellationToken)
    {
        var token =
            await _tokenService.GetAccessTokenAsync(
                cancellationToken);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
    }
}