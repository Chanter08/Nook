using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Nook.Api.Integrations.Recipes.FatSecret;

public class FatSecretTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    private string? _accessToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FatSecretTokenService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string> GetAccessTokenAsync(
        CancellationToken cancellationToken = default)
    {
        if (_accessToken is not null &&
            DateTimeOffset.UtcNow < _expiresAt)
        {
            return _accessToken;
        }

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_accessToken is not null &&
                DateTimeOffset.UtcNow < _expiresAt)
            {
                return _accessToken;
            }

            var clientId = _configuration["FatSecret:ClientId"];
            var clientSecret = _configuration["FatSecret:ClientSecret"];

            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new InvalidOperationException(
                    "FatSecret credentials are not configured.");
            }

            using var client = _httpClientFactory.CreateClient();

            var credentials = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(
                    $"{clientId}:{clientSecret}"));

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    credentials);

            using var content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["scope"] = "basic"
                });

            using var response = await client.PostAsync(
                "https://oauth.fatsecret.com/connect/token",
                content,
                cancellationToken);

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"FatSecret authentication failed ({(int)response.StatusCode}): {raw}"
                );
            }

            var token = System.Text.Json.JsonSerializer.Deserialize<FatSecretTokenResponse>(
                raw,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (token is null ||
                string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new InvalidOperationException(
                    "FatSecret did not return an access token.");
            }

            _accessToken = token.AccessToken;

            // Refresh five minutes before the actual expiry.
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(
                Math.Max(token.ExpiresIn - 300, 60));

            return _accessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private class FatSecretTokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}