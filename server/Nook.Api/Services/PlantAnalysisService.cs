using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Nook.Api.Services;

public sealed class PlantAnalysisService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public PlantAnalysisService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<PlantAnalysisResult> AnalyzeAsync(IFormFile image, string? plantName, string? species, CancellationToken cancellationToken)
    {
        if (image.Length == 0 || image.Length > 10 * 1024 * 1024)
            throw new PlantAnalysisException("Choose a plant photo up to 10 MB.");

        if (image.ContentType is not ("image/jpeg" or "image/png" or "image/webp"))
            throw new PlantAnalysisException("Use a JPEG, PNG, or WebP plant photo.");

        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new PlantAnalysisException("Plant photo analysis is not configured.");

        var model = _configuration["OpenAI:PlantModel"]
            ?? _configuration["OpenAI:ReceiptModel"]
            ?? "gpt-5.5";

        await using var source = image.OpenReadStream();
        using var memory = new MemoryStream();
        await source.CopyToAsync(memory, cancellationToken);
        var imageUrl = $"data:{image.ContentType};base64,{Convert.ToBase64String(memory.ToArray())}";
        var context = string.IsNullOrWhiteSpace(plantName)
            ? "The plant name/species is not known."
            : $"The user calls this plant '{plantName}'. Species: {species ?? "unknown"}.";

        var body = new
        {
            model,
            input = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "input_text",
                            text = $"""
                            Inspect this plant photo as a home-gardening assistant. {context}

                            Identify only visible or reasonably inferable issues. Do not claim certainty from one photo.
                            Focus on leaf color, spotting, curling, wilting, pests, visible damage, soil/moisture clues, and general vigor.
                            Give a short practical assessment and low-risk next checks/actions.
                            If the plant looks healthy, say so rather than inventing a problem.
                            Urgent means the photo suggests rapid decline, severe pest/disease damage, or another issue needing prompt attention.
                            """
                        },
                        new { type = "input_image", image_url = imageUrl, detail = "high" }
                    }
                }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "plant_analysis",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new
                        {
                            status = new { type = "string", @enum = new[] { "healthy", "watch", "issue" } },
                            summary = new { type = "string" },
                            possibleIssues = new { type = "array", items = new { type = "string" } },
                            suggestedActions = new { type = "array", items = new { type = "string" } },
                            urgent = new { type = "boolean" }
                        },
                        required = new[] { "status", "summary", "possibleIssues", "suggestedActions", "urgent" }
                    }
                }
            }
        };

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new PlantAnalysisException($"Plant analysis failed ({(int)response.StatusCode}).");

        using var document = JsonDocument.Parse(responseText);
        string? json = null;
        if (document.RootElement.TryGetProperty("output", out var output))
        {
            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty("content", out var content)) continue;
                foreach (var part in content.EnumerateArray())
                {
                    if (part.TryGetProperty("type", out var type) && type.GetString() == "output_text" &&
                        part.TryGetProperty("text", out var text))
                    {
                        json = text.GetString();
                        break;
                    }
                }
                if (json is not null) break;
            }
        }

        var result = string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<PlantAnalysisResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? throw new PlantAnalysisException("The plant photo could not be interpreted.");
    }
}

public sealed record PlantAnalysisResult(string Status, string Summary, string[] PossibleIssues, string[] SuggestedActions, bool Urgent);
public sealed class PlantAnalysisException(string message) : Exception(message);
