using System.Net.Http.Json;
using Nook.Api.DTOs.Discover;
using Nook.Api.Integrations.Recipes.Spoonacular.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace Nook.Api.Integrations.Recipes.Spoonacular;

public class SpoonacularProvider : IRecipeDiscoveryProvider
{
    private const string ProviderName = "Spoonacular";

    private readonly HttpClient _httpClient;

    public SpoonacularProvider(HttpClient httpClient)
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

        var response =
            await _httpClient.GetFromJsonAsync<SpoonacularSearchResponse>(
                $"recipes/complexSearch" +
                $"?query={encodedQuery}" +
                $"&number=12" +
                $"&instructionsRequired=true",
                cancellationToken);

        return response?.Results
            .Select(recipe => new DiscoverRecipeSummaryResponse
            {
                Provider = ProviderName,
                ExternalId = recipe.Id.ToString(),
                Name = recipe.Name,
                ImageUrl = recipe.ImageUrl,
                Category = null,
                Cuisine = null
            })
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

        using var response = await _httpClient.GetAsync(
            $"recipes/{recipeId}/information?includeNutrition=true",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var recipe =
            await response.Content.ReadFromJsonAsync<SpoonacularRecipe>(
                cancellationToken: cancellationToken);

        if (recipe is null)
        {
            return null;
        }

        return new DiscoverRecipeDetailResponse
        {
            Provider = ProviderName,
            ExternalId = recipe.Id.ToString(),

            Name = recipe.Name,
            Description = CleanHtml(recipe.Summary),
            ImageUrl = recipe.Image,

            Category = recipe.DishTypes.FirstOrDefault(),
            Cuisine = recipe.Cuisines.FirstOrDefault(),

            SourceUrl = recipe.SourceUrl,
            VideoUrl = null,

            PrepTimeMinutes = recipe.PreparationMinutes,
            CookTimeMinutes = recipe.CookingMinutes,
            TotalTimeMinutes = recipe.ReadyInMinutes,
            Servings = recipe.Servings,

            Difficulty = null,

            Calories = GetNutrientInt(recipe, "Calories"),
            ProteinGrams = GetNutrient(recipe, "Protein"),
            CarbohydrateGrams = GetNutrient(recipe, "Carbohydrates"),
            FatGrams = GetNutrient(recipe, "Fat"),

            Ingredients = MapIngredients(recipe),
            Steps = MapSteps(recipe)
        };
    }

    private static List<DiscoverIngredientResponse> MapIngredients(
    SpoonacularRecipe recipe)
    {
        return recipe.ExtendedIngredients
            .Select(ingredient =>
            {
                var metric = ingredient.Measures?.Metric;

                return new DiscoverIngredientResponse
                {
                    Name = ingredient.Name,
                    Quantity = metric?.Amount ?? ingredient.Amount,
                    Unit = metric?.UnitShort ?? ingredient.Unit,
                    Notes = ingredient.Meta.Count > 0
                        ? string.Join(", ", ingredient.Meta)
                        : null
                };
            })
            .ToList();
    }

    private static List<DiscoverStepResponse> MapSteps(
        SpoonacularRecipe recipe)
    {
        var steps = recipe.AnalyzedInstructions
            .SelectMany(group => group.Steps)
            .Where(step => !string.IsNullOrWhiteSpace(step.Step))
            .ToList();

        return steps
            .Select((step, index) => new DiscoverStepResponse
            {
                StepNumber = index + 1,
                Instruction = step.Step.Trim()
            })
            .ToList();
    }

    private static decimal? GetNutrient(
        SpoonacularRecipe recipe,
        string name)
    {
        return recipe.Nutrition?.Nutrients
            .FirstOrDefault(nutrient =>
                nutrient.Name.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase))
            ?.Amount;
    }

    private static int? GetNutrientInt(
        SpoonacularRecipe recipe,
        string name)
    {
        var value = GetNutrient(recipe, name);

        return value.HasValue
            ? (int)Math.Round(value.Value)
            : null;
    }

    private static string? CleanHtml(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var withoutTags = Regex.Replace(
            value,
            "<.*?>",
            string.Empty);

        return System.Net.WebUtility.HtmlDecode(
            withoutTags).Trim();
    }
}