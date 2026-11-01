using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Nook.Api.DTOs.Pantry;

namespace Nook.Api.Services;

public class ReceiptScannerService
{
    private const long MaxReceiptBytes = 15 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ReceiptScannerService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<ReceiptScanResponse> ScanAsync(
        IFormFile receipt,
        CancellationToken cancellationToken = default)
    {
        if (receipt.Length == 0)
        {
            throw new ReceiptScanValidationException("Choose a receipt photo first.");
        }

        if (receipt.Length > MaxReceiptBytes)
        {
            throw new ReceiptScanValidationException("Receipt photos must be 15 MB or smaller.");
        }

        if (!AllowedContentTypes.Contains(receipt.ContentType))
        {
            throw new ReceiptScanValidationException(
                "Use a JPEG, PNG, or WebP receipt photo.");
        }

        var apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ReceiptScanNotConfiguredException(
                "Receipt scanning is not configured.");
        }

        var model = _configuration["OpenAI:ReceiptModel"];

        if (string.IsNullOrWhiteSpace(model))
        {
            model = "gpt-5.5";
        }

        await using var stream = receipt.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var imageUrl =
            $"data:{receipt.ContentType};base64,{Convert.ToBase64String(memory.ToArray())}";

        var requestBody = new
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
                            text =
                                """
                                Read this supermarket or household-shopping receipt and extract only purchased product lines.

                                Rules:
                                - Exclude totals, subtotals, VAT/tax, discounts, loyalty lines, payment lines, change, deposits, bag fees, and store metadata.
                                - Do not invent a product if the receipt text is unclear.
                                - Convert abbreviated receipt descriptions into a short human-readable product name only when reasonably confident.
                                - Quantity should be the number purchased. If the receipt gives no explicit quantity, use 1.
                                - Use a unit such as each, pack, box, bag, bottle, roll, g, kg, ml, or l when the receipt makes it clear; otherwise use null.
                                - Category must be one of Groceries, Household, Cleaning, Toiletries, Pets, Pharmacy, Other.
                                - StorageLocation must be one of Pantry, Fridge, Freezer, Cleaning, Bathroom, Pet, Medicine, Other.
                                - Food is normally Pantry, Fridge, or Freezer. Do not guess expiry dates.
                                - isLikelyInventoryItem should be false for lines that should not normally be tracked as household stock.
                                - confidence is from 0 to 1 and reflects confidence in the interpreted product name.
                                """
                        },
                        new
                        {
                            type = "input_image",
                            image_url = imageUrl,
                            detail = "high"
                        }
                    }
                }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "receipt_scan",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new
                        {
                            storeName = new { type = new[] { "string", "null" } },
                            purchaseDate = new
                            {
                                type = new[] { "string", "null" },
                                description = "ISO-8601 YYYY-MM-DD when visible, otherwise null."
                            },
                            currency = new { type = new[] { "string", "null" } },
                            items = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    properties = new
                                    {
                                        receiptText = new { type = "string" },
                                        name = new { type = "string" },
                                        quantity = new { type = "number" },
                                        unit = new { type = new[] { "string", "null" } },
                                        category = new
                                        {
                                            type = "string",
                                            @enum = new[]
                                            {
                                                "Groceries", "Household", "Cleaning",
                                                "Toiletries", "Pets", "Pharmacy", "Other"
                                            }
                                        },
                                        storageLocation = new
                                        {
                                            type = "string",
                                            @enum = new[]
                                            {
                                                "Pantry", "Fridge", "Freezer", "Cleaning",
                                                "Bathroom", "Pet", "Medicine", "Other"
                                            }
                                        },
                                        price = new { type = new[] { "number", "null" } },
                                        confidence = new
                                        {
                                            type = "number",
                                            minimum = 0,
                                            maximum = 1
                                        },
                                        isLikelyInventoryItem = new { type = "boolean" }
                                    },
                                    required = new[]
                                    {
                                        "receiptText", "name", "quantity", "unit",
                                        "category", "storageLocation", "price",
                                        "confidence", "isLikelyInventoryItem"
                                    }
                                }
                            }
                        },
                        required = new[] { "storeName", "purchaseDate", "currency", "items" }
                    }
                }
            }
        };

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/responses");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response = await client.SendAsync(request, cancellationToken);

        var responseText =
            await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ReceiptScanProviderException(
                $"Receipt analysis failed ({(int)response.StatusCode}).");
        }

        using var document = JsonDocument.Parse(responseText);

        var structuredJson = document.RootElement
            .GetProperty("output")
            .EnumerateArray()
            .Where(x =>
                x.TryGetProperty("type", out var type) &&
                type.GetString() == "message")
            .SelectMany(x =>
                x.TryGetProperty("content", out var content)
                    ? content.EnumerateArray().ToArray()
                    : [])
            .FirstOrDefault(x =>
                x.TryGetProperty("type", out var type) &&
                type.GetString() == "output_text");

        if (structuredJson.ValueKind == JsonValueKind.Undefined ||
            !structuredJson.TryGetProperty("text", out var textElement))
        {
            throw new ReceiptScanProviderException(
                "The receipt could not be interpreted.");
        }

        var result = JsonSerializer.Deserialize<ReceiptScanResponse>(
            textElement.GetString() ?? "{}",
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result is null)
        {
            throw new ReceiptScanProviderException(
                "The receipt could not be interpreted.");
        }

        return result;
    }
}

public class ReceiptScanValidationException : Exception
{
    public ReceiptScanValidationException(string message) : base(message) { }
}

public class ReceiptScanNotConfiguredException : Exception
{
    public ReceiptScanNotConfiguredException(string message) : base(message) { }
}

public class ReceiptScanProviderException : Exception
{
    public ReceiptScanProviderException(string message) : base(message) { }
}
