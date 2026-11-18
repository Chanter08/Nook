namespace Nook.Api.Services;

public sealed class PlantCareAdviceService
{
    public PlantCareAdvice Build(PlantAdviceInput plant, GardenWeatherContext weather)
    {
        var today = weather.Today;
        var isPotted = plant.GrowingIn?.Equals("pot", StringComparison.OrdinalIgnoreCase) == true;
        var exposedToRain = plant.RainExposure?.Equals("exposed", StringComparison.OrdinalIgnoreCase) == true;
        var sheltered = plant.RainExposure is "sheltered" or "indoor";
        var fullSun = plant.SunExposure?.Equals("full", StringComparison.OrdinalIgnoreCase) == true;
        var category = IdentifyCareGroup(plant);
        var reasons = new List<string>();
        var checks = new List<string>();
        var missing = new List<string>();

        if (plant.GrowingIn is null) missing.Add("Pot or ground");
        if (plant.RainExposure is null) missing.Add("Rain exposure");
        if (plant.SunExposure is null) missing.Add("Sunlight");

        var daysSinceWatered = plant.LastWateredAtUtc is null
            ? (int?)null
            : Math.Max(0, (DateTime.UtcNow.Date - plant.LastWateredAtUtc.Value.Date).Days);

        var highDryingDemand = today.Et0Mm >= 4.5 || today.TemperatureMaxC >= 29 || today.WindMaxKmh >= 30;
        var recentRain = weather.RainPast3DaysMm >= 6;
        var rainExpected = weather.RainNext48HoursMm >= 4 ||
                           (today.RainProbabilityPercent >= 70 && today.RainMm >= 1);

        var moistureCheck = category switch
        {
            "citrus" => "Check moisture about 4–5 cm below the surface. If dry there, water thoroughly and let excess water drain.",
            "moisture-loving herbs" => "Check the upper 1–2 cm of soil. Basil, mint and parsley prefer more consistent moisture; water when it starts to dry, not when it is soggy.",
            "Mediterranean herbs" => "Check below the surface and let the mix dry more between waterings. Rosemary, thyme, sage and lavender dislike persistently wet roots.",
            "succulents" => "Allow the potting mix to dry well before watering. Do not water simply because several days have passed.",
            _ => "Feel a few centimetres below the surface, not just the top crust. Water only if the root-zone moisture suits this plant."
        };
        checks.Add(moistureCheck);

        string action;
        string title;
        string summary;

        if (exposedToRain && (rainExpected || recentRain))
        {
            action = "check";
            title = "Check soil before adding water";
            summary = "This plant receives rainfall, so recent or expected rain may reduce its watering needs. Confirm moisture at root depth before deciding.";
            if (rainExpected)
                reasons.Add($"Forecast rain over the next 48 hours: {weather.RainNext48HoursMm:0.#} mm (not a guarantee the pot will receive that amount).");
            if (recentRain)
                reasons.Add($"Recorded rain over the last 3 days: {weather.RainPast3DaysMm:0.#} mm.");
        }
        else if (highDryingDemand && (isPotted || fullSun || plant.GrowingIn is null))
        {
            action = "check";
            title = "Check moisture today";
            summary = "Warm, windy or dry conditions may be drying this plant faster. Check the soil instead of watering automatically.";
            reasons.Add($"Today's ET₀ is {today.Et0Mm:0.#} mm; max temperature {today.TemperatureMaxC:0.#}°C; wind up to {today.WindMaxKmh:0.#} km/h.");
        }
        else if (daysSinceWatered is >= 4 && category != "succulents")
        {
            action = "check";
            title = "Check soil moisture today";
            summary = "It has been several days since a watering was logged, but the correct interval depends on plant type, pot size and actual soil moisture.";
            reasons.Add($"Last logged watering: {daysSinceWatered} days ago.");
        }
        else
        {
            action = "monitor";
            title = "Monitor — don't water by calendar";
            summary = "There is not enough evidence to recommend watering. Let the soil moisture and the plant's condition decide.";
            if (daysSinceWatered is not null)
                reasons.Add($"Last logged watering: {daysSinceWatered} day{(daysSinceWatered == 1 ? "" : "s")} ago.");
        }

        if (sheltered)
        {
            reasons.Add("Rainfall does not directly water this plant because its position is sheltered or indoors.");
            if (rainExpected && action == "monitor")
                checks.Add("Do not count the outdoor rain forecast as watering for this plant.");
        }
        else if (plant.RainExposure is null)
        {
            reasons.Add("Rain exposure is not set. Forecast rainfall has not been assumed to water this plant.");
        }

        if (isPotted)
        {
            reasons.Add("Plants in pots have limited root volume and can dry faster than garden soil.");
            checks.Add("Make sure excess water drains freely; don't leave roots sitting in water.");
        }

        if (fullSun && today.TemperatureMaxC >= 29 && category == "moisture-loving herbs")
            checks.Add("If leaves wilt in strong afternoon sun, check moisture and consider temporary afternoon shade rather than assuming they need more water.");

        if (!string.IsNullOrWhiteSpace(plant.LatestIssueSummary))
        {
            reasons.Add($"Latest saved plant issue (may be outdated): {plant.LatestIssueSummary}");
            if (ContainsAny(plant.LatestIssueSummary, "yellow", "root", "wet", "overwater", "mushy"))
                checks.Add("Because yellowing or wet-root symptoms were noted, rule out soggy soil and drainage problems before watering.");
            if (ContainsAny(plant.LatestIssueSummary, "wilting", "droop", "dry", "crispy"))
                checks.Add("Wilting can happen with either dry or damaged/wet roots. Check below-surface moisture before acting.");
        }

        string routine = category switch
        {
            "citrus" => isPotted
                ? "Potted citrus needs even moisture and free drainage. Check more often in hot weather; avoid small daily splashes and standing water."
                : "Citrus prefers deep, well-timed watering and well-drained soil. Check the root zone rather than relying on a fixed schedule.",
            "moisture-loving herbs" => "Keep basil, mint and parsley from drying out completely, especially in small pots. Check soil regularly and water thoroughly when it begins to dry.",
            "Mediterranean herbs" => "Rosemary, thyme, sage, oregano and lavender generally prefer a freely draining mix and more drying time between waterings.",
            "succulents" => "Cacti and succulents generally need a dry-down between waterings; prolonged wet conditions can harm roots.",
            _ => "Plant type is not yet confidently known. Check soil and drainage, review new growth and leaf changes, and avoid a fixed watering interval."
        };

        var baselineEt0 = weather.CurrentMonthBaseline.AverageDailyEt0Mm;
        var climateComparison = baselineEt0 <= 0.1
            ? "Seasonal drying comparison is unavailable."
            : today.Et0Mm > baselineEt0 * 1.25
                ? "Today has higher drying demand than is typical for this month."
                : today.Et0Mm < baselineEt0 * 0.75
                    ? "Today has lower drying demand than is typical for this month."
                    : "Today's drying demand is near the recent seasonal average.";

        if (missing.Count > 0)
            checks.Add("Complete the growing conditions in this plant's profile to make advice more specific.");

        return new PlantCareAdvice(
            action, title, summary, routine, climateComparison,
            reasons.ToArray(), daysSinceWatered, weather, category, checks.ToArray(), missing.ToArray());
    }

    private static string IdentifyCareGroup(PlantAdviceInput plant)
    {
        var text = $"{plant.Name} {plant.Species}".ToLowerInvariant();
        if (ContainsAny(text, "citrus", "lemon", "lime", "mandarin", "orange tree", "kumquat", "grapefruit"))
            return "citrus";
        if (ContainsAny(text, "basil", "ocimum", "mint", "mentha", "parsley", "petroselinum", "coriander", "cilantro"))
            return "moisture-loving herbs";
        if (ContainsAny(text, "rosemary", "salvia rosmarinus", "rosmarinus", "thyme", "thymus", "oregano", "origanum", "lavender", "lavandula", "sage", "salvia officinalis"))
            return "Mediterranean herbs";
        if (ContainsAny(text, "cactus", "cacti", "succulent", "aloe", "echeveria", "crassula", "jade plant"))
            return "succulents";
        return "unclassified";
    }

    private static bool ContainsAny(string? value, params string[] needles) =>
        !string.IsNullOrWhiteSpace(value) &&
        needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
}

public sealed record PlantAdviceInput(
    string Name,
    string? Species,
    string? Location,
    string? Notes,
    DateTime? LastWateredAtUtc,
    string? LatestObservationNotes,
    string? LatestIssueSummary,
    string? GrowingIn,
    string? RainExposure,
    string? SunExposure);

public sealed record PlantCareAdvice(
    string Action,
    string Title,
    string Summary,
    string RoutineGuidance,
    string ClimateComparison,
    string[] Reasons,
    int? DaysSinceWatered,
    GardenWeatherContext Weather,
    string CareGroup,
    string[] QuickChecks,
    string[] MissingProfileInfo);
