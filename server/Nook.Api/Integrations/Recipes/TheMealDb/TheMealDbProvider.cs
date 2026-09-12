using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Nook.Api.DTOs.Discover;
using Nook.Api.Integrations.Recipes.TheMealDb.Models;

namespace Nook.Api.Integrations.Recipes.TheMealDb;

public class TheMealDbProvider : IRecipeDiscoveryProvider
{
    private const string ProviderName = "TheMealDB";

    private readonly HttpClient _httpClient;

    public TheMealDbProvider(HttpClient httpClient)
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

        var response = await _httpClient.GetFromJsonAsync<TheMealDbResponse>(
            $"search.php?s={encodedQuery}",
            cancellationToken);

        if (response?.Meals is null)
        {
            return [];
        }

        return response.Meals
            .Select(meal => new DiscoverRecipeSummaryResponse
            {
                Provider = ProviderName,
                ExternalId = meal.Id,
                Name = meal.Name,
                ImageUrl = meal.ImageUrl,
                Category = meal.Category,
                Cuisine = meal.Cuisine
            })
            .ToList();
    }

    public async Task<DiscoverRecipeDetailResponse?> GetByIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            return null;
        }

        var encodedId = Uri.EscapeDataString(externalId.Trim());

        var response = await _httpClient.GetFromJsonAsync<TheMealDbResponse>(
            $"lookup.php?i={encodedId}",
            cancellationToken);

        var meal = response?.Meals?.FirstOrDefault();

        if (meal is null)
        {
            return null;
        }

        return new DiscoverRecipeDetailResponse
        {
            Provider = ProviderName,
            ExternalId = meal.Id,
            Name = meal.Name,
            Description = null,
            ImageUrl = meal.ImageUrl,
            Category = meal.Category,
            Cuisine = meal.Cuisine,
            SourceUrl = meal.SourceUrl,
            VideoUrl = meal.VideoUrl,
            Ingredients = MapIngredients(meal),
            Steps = MapSteps(meal.Instructions)
        };
    }

    private static List<DiscoverIngredientResponse> MapIngredients(
        TheMealDbMeal meal)
    {
        var ingredients = new List<DiscoverIngredientResponse>();

        for (var index = 1; index <= 20; index++)
        {
            var ingredient = GetExtraString(
                meal.Extra,
                $"strIngredient{index}");

            if (string.IsNullOrWhiteSpace(ingredient))
            {
                continue;
            }

            var measure = GetExtraString(
                meal.Extra,
                $"strMeasure{index}");

            ingredients.Add(
                ParseIngredient(
                    ingredient.Trim(),
                    measure?.Trim()));
        }

        return ingredients;
    }

    private static DiscoverIngredientResponse ParseIngredient(
        string name,
        string? measure)
    {
        if (string.IsNullOrWhiteSpace(measure))
        {
            return new DiscoverIngredientResponse
            {
                Name = name
            };
        }

        var normalizedMeasure = measure
            .Replace("½", "1/2")
            .Replace("¼", "1/4")
            .Replace("¾", "3/4")
            .Trim();

        var match = Regex.Match(
            normalizedMeasure,
            @"^(?<quantity>\d+\s+\d+/\d+|\d+/\d+|\d+(?:\.\d+)?)\s*(?<rest>.*)$");

        if (!match.Success)
        {
            return new DiscoverIngredientResponse
            {
                Name = name,
                Notes = measure
            };
        }

        var quantity = ParseQuantity(
            match.Groups["quantity"].Value);

        if (!quantity.HasValue)
        {
            return new DiscoverIngredientResponse
            {
                Name = name,
                Notes = measure
            };
        }

        var remainder = match.Groups["rest"].Value.Trim();

        var (unit, notes) = ParseUnit(remainder);

        var metric = ConvertToMetric(
            quantity.Value,
            unit);

        return new DiscoverIngredientResponse
        {
            Name = name,
            Quantity = metric.Quantity,
            Unit = metric.Unit,
            Notes = notes
        };
    }

    private static decimal? ParseQuantity(string value)
    {
        value = value.Trim();

        if (value.Contains(' '))
        {
            var parts = value.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 &&
                decimal.TryParse(
                    parts[0],
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var whole))
            {
                var fraction = ParseFraction(parts[1]);

                if (fraction.HasValue)
                {
                    return whole + fraction.Value;
                }
            }
        }

        if (value.Contains('/'))
        {
            return ParseFraction(value);
        }

        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var number)
                ? number
                : null;
    }

    private static decimal? ParseFraction(string value)
    {
        var parts = value.Split('/');

        if (parts.Length != 2)
        {
            return null;
        }

        if (!decimal.TryParse(
                parts[0],
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var numerator))
        {
            return null;
        }

        if (!decimal.TryParse(
                parts[1],
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var denominator))
        {
            return null;
        }

        if (denominator == 0)
        {
            return null;
        }

        return numerator / denominator;
    }

    private static (string? Unit, string? Notes) ParseUnit(
        string remainder)
    {
        var lower = remainder.Trim().ToLowerInvariant();

        if (
            lower == "fl oz" ||
            lower.StartsWith("fl oz ")
        )
        {
            var lowerNotes = lower.Length > 5
                ? remainder[5..].Trim()
                : null;

            return (
                "fl oz",
                string.IsNullOrWhiteSpace(lowerNotes)
                    ? null
                    : lowerNotes
            );
        }
        if (string.IsNullOrWhiteSpace(remainder))
        {
            return (null, null);
        }

        var parts = remainder.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return (null, null);
        }

        var candidate = parts[0]
            .Trim()
            .TrimEnd(',')
            .ToLowerInvariant();

        var unit = candidate switch
        {
            "g" or "gram" or "grams" => "g",
            "kg" or "kilogram" or "kilograms" => "kg",
            "ml" or "millilitre" or "millilitres" or
                "milliliter" or "milliliters" => "ml",
            "l" or "litre" or "litres" or
                "liter" or "liters" => "l",
            "tsp" or "teaspoon" or "teaspoons" => "tsp",
            "tbsp" or "tablespoon" or "tablespoons" => "tbsp",
            "cup" or "cups" => "cup",
            "clove" or "cloves" => "cloves",
            "oz" or "ounce" or "ounces" => "oz",
            "lb" or "lbs" or "pound" or "pounds" => "lb",
            _ => null
        };

        if (unit is null)
        {
            return (null, remainder);
        }

        var notes = parts.Length > 1
            ? string.Join(" ", parts.Skip(1))
            : null;

        return (
            unit,
            string.IsNullOrWhiteSpace(notes) ? null : notes);
    }

    private static List<DiscoverStepResponse> MapSteps(
        string? instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions))
        {
            return [];
        }

        var lines = instructions
            .Split(
                ["\r\n", "\n"],
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (lines.Count == 0)
        {
            return [];
        }

        return lines
            .Select((instruction, index) =>
                new DiscoverStepResponse
                {
                    StepNumber = index + 1,
                    Instruction = instruction
                })
            .ToList();
    }

    private static string? GetExtraString(
        Dictionary<string, JsonElement> extra,
        string propertyName)
    {
        if (!extra.TryGetValue(
                propertyName,
                out var value))
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }


    private static (decimal Quantity, string? Unit) ConvertToMetric(
        decimal quantity,
        string? unit)
    {
        return unit switch
        {
            "oz" => (
                Math.Round(quantity * 28.3495m, 1),
                "g"
            ),

            "lb" => (
                Math.Round(quantity * 453.592m, 1),
                "g"
            ),

            "fl oz" => (
                Math.Round(quantity * 29.5735m, 1),
                "ml"
            ),

            "cup" => (
                Math.Round(quantity * 240m, 1),
                "ml"
            ),

            _ => (quantity, unit)
        };
    }
}