using System.Net;
using System.Net.Http.Json;
using Nook.Api.DTOs.Discover;
using Nook.Api.Integrations.Recipes.RecipeApi.Models;

namespace Nook.Api.Integrations.Recipes.RecipeApi;

public class RecipeApiProvider : IRecipeDiscoveryProvider
{
    private const string ProviderName = "RecipeAPI";

    private readonly HttpClient _httpClient;

    public RecipeApiProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<DiscoverRecipeSummaryResponse>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var encodedQuery = Uri.EscapeDataString(query.Trim());

        var response = await _httpClient.GetFromJsonAsync<RecipeApiListResponse>(
            $"recipes?search={encodedQuery}&per_page=10",
            cancellationToken);

        return response?.Data
            .Select(MapSummary)
            .ToList() ?? [];
    }

    public async Task<DiscoverRecipeDetailResponse?> GetByIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(externalId, out var recipeId))
        {
            return null;
        }

        var response = await _httpClient.GetAsync(
            $"recipes/{recipeId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<RecipeApiSingleResponse>(
            cancellationToken: cancellationToken);

        return body?.Data is null
            ? null
            : MapDetail(body.Data);
    }

    private static DiscoverRecipeSummaryResponse MapSummary(
        RecipeApiRecipe recipe)
    {
        return new DiscoverRecipeSummaryResponse
        {
            Provider = ProviderName,
            ExternalId = recipe.Id.ToString(),
            Name = recipe.Name,
            ImageUrl = null,
            Category = FormatLabel(recipe.MealType),
            Cuisine = FormatLabel(recipe.Cuisine)
        };
    }

    private static DiscoverRecipeDetailResponse MapDetail(
        RecipeApiRecipe recipe)
    {
        return new DiscoverRecipeDetailResponse
        {
            Provider = ProviderName,
            ExternalId = recipe.Id.ToString(),

            Name = recipe.Name,
            Description = recipe.Description,
            ImageUrl = null,

            Category = FormatLabel(recipe.MealType),
            Cuisine = FormatLabel(recipe.Cuisine),
            Difficulty = FormatLabel(recipe.Difficulty),

            PrepTimeMinutes = recipe.PrepTime,
            CookTimeMinutes = recipe.CookTime,
            Servings = recipe.Servings,

            Calories = recipe.CaloriesPerServing,
            ProteinGrams = recipe.Protein,
            CarbohydrateGrams = null,
            FatGrams = null,

            SourceUrl = null,
            VideoUrl = null,

            Ingredients = recipe.Ingredients
                .Select(MapIngredient)
                .ToList(),

            Steps = recipe.Instructions
                .Select((instruction, index) =>
                    new DiscoverStepResponse
                    {
                        StepNumber = index + 1,
                        Instruction = instruction
                    })
                .ToList()
        };
    }

    private static DiscoverIngredientResponse MapIngredient(
        RecipeApiIngredient ingredient)
    {
        var (quantity, unit) = ConvertToMetric(
            ingredient.Quantity,
            ingredient.Unit);

        return new DiscoverIngredientResponse
        {
            Name = ingredient.Name,
            Quantity = quantity,
            Unit = unit,
            Notes = ingredient.Optional
                ? "Optional"
                : null
        };
    }

    private static (decimal? Quantity, string? Unit) ConvertToMetric(
        decimal? quantity,
        string? unit)
    {
        if (!quantity.HasValue || string.IsNullOrWhiteSpace(unit))
        {
            return (quantity, unit);
        }

        return unit.Trim().ToLowerInvariant() switch
        {
            "oz" => (
                Math.Round(quantity.Value * 28.3495m, 1),
                "g"
            ),

            "lb" or "lbs" => (
                Math.Round(quantity.Value * 453.592m, 1),
                "g"
            ),

            "fl oz" => (
                Math.Round(quantity.Value * 29.5735m, 1),
                "ml"
            ),

            "cup" or "cups" => (
                Math.Round(quantity.Value * 240m, 1),
                "ml"
            ),

            _ => (quantity, unit)
        };
    }

    private static string? FormatLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Join(
            " ",
            value
                .Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(word =>
                    char.ToUpperInvariant(word[0]) +
                    word[1..].ToLowerInvariant()));
    }
}