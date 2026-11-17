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

    public async Task<PlantAnalysisResult> AnalyzeAsync(
        IFormFile image,
        string? plantName,
        string? species,
        CancellationToken cancellationToken)
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

        var context = string.IsNullOrWhiteSpace(plantName) && string.IsNullOrWhiteSpace(species)
            ? "The plant has not been identified yet."
            : $"The user currently calls this plant '{plantName ?? "unnamed"}'. Recorded species: {species ?? "unknown"}. Treat these as hints, not guaranteed facts.";

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
                            Inspect this plant photo as a careful home-gardening assistant. {context}

                            Return BOTH:
                            1. Identification: the most likely common name and scientific name, a cultivar/variety only when visually supportable, confidence from 0 to 1, and up to three plausible alternatives.
                            2. Situation: what is visibly inferable about how/where it is growing (indoors/outdoors/unclear, pot/container/ground/unclear, apparent light exposure, visible soil-surface condition, growth stage, and overall visible condition).

                            Health rules:
                            - Identify only visible or reasonably inferable issues. Do not diagnose hidden root, nutrient, disease, or watering problems as certain from one photo.
                            - Focus on leaf colour, spotting, curling, wilting, pests, visible damage, soil/moisture clues, and general vigour.
                            - If the plant looks healthy, say so rather than inventing a problem.
                            - Give short, low-risk checks/actions.
                            - Urgent means the photo suggests rapid decline, severe pest/disease damage, or another issue needing prompt attention.
                            - If identification is weak, lower confidence and use alternatives instead of pretending certainty.
                            - A dwarf/grafted form or exact citrus cultivar is often impossible to prove from foliage alone; say null for variety when not supportable.
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
                            identification = new
                            {
                                type = "object",
                                additionalProperties = false,
                                properties = new
                                {
                                    commonName = new { type = new[] { "string", "null" } },
                                    scientificName = new { type = new[] { "string", "null" } },
                                    variety = new { type = new[] { "string", "null" } },
                                    confidence = new { type = "number", minimum = 0, maximum = 1 },
                                    alternatives = new { type = "array", items = new { type = "string" }, maxItems = 3 },
                                    notes = new { type = "string" }
                                },
                                required = new[] { "commonName", "scientificName", "variety", "confidence", "alternatives", "notes" }
                            },
                            situation = new
                            {
                                type = "object",
                                additionalProperties = false,
                                properties = new
                                {
                                    setting = new { type = "string", @enum = new[] { "outdoor", "indoor", "unclear" } },
                                    growingIn = new { type = "string", @enum = new[] { "pot", "container", "ground", "unclear" } },
                                    apparentLight = new { type = "string" },
                                    soilSurface = new { type = "string" },
                                    growthStage = new { type = "string" },
                                    visibleCondition = new { type = "string" }
                                },
                                required = new[] { "setting", "growingIn", "apparentLight", "soilSurface", "growthStage", "visibleCondition" }
                            },
                            status = new { type = "string", @enum = new[] { "healthy", "watch", "issue" } },
                            summary = new { type = "string" },
                            possibleIssues = new { type = "array", items = new { type = "string" } },
                            suggestedActions = new { type = "array", items = new { type = "string" } },
                            urgent = new { type = "boolean" }
                        },
                        required = new[] { "identification", "situation", "status", "summary", "possibleIssues", "suggestedActions", "urgent" }
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
                    if (part.TryGetProperty("type", out var type) &&
                        type.GetString() == "output_text" &&
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
            : JsonSerializer.Deserialize<PlantAnalysisResult>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? throw new PlantAnalysisException("The plant photo could not be interpreted.");
    }
}

public sealed record PlantIdentificationResult(
    string? CommonName,
    string? ScientificName,
    string? Variety,
    double Confidence,
    string[] Alternatives,
    string Notes);

public sealed record PlantSituationResult(
    string Setting,
    string GrowingIn,
    string ApparentLight,
    string SoilSurface,
    string GrowthStage,
    string VisibleCondition);

public sealed record PlantAnalysisResult(
    PlantIdentificationResult Identification,
    PlantSituationResult Situation,
    string Status,
    string Summary,
    string[] PossibleIssues,
    string[] SuggestedActions,
    bool Urgent);

public sealed class PlantAnalysisException(string message) : Exception(message);
