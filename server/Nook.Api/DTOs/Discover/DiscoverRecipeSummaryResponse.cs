namespace Nook.Api.DTOs.Discover;

public class DiscoverRecipeSummaryResponse
{
    public string Provider { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? Cuisine { get; set; }
}