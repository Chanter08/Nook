namespace Nook.Api.Services;

public sealed class PlantCareAdviceService
{
    public PlantCareAdvice Build(PlantAdviceInput plant, GardenWeatherContext weather)
    {
        var today = weather.Today;
        var reasons = new List<string>();
        var isCitrus = ContainsAny(plant.Species, "citrus") || ContainsAny(plant.Name, "lemon", "lime", "orange", "mandarin", "citrus");
        var likelyPotted = ContainsAny(plant.Notes, "pot", "potted", "container") || ContainsAny(plant.Location, "pot", "container");

        var daysSinceWatered = plant.LastWateredAtUtc is null
            ? (int?)null
            : Math.Max(0, (DateTime.UtcNow.Date - plant.LastWateredAtUtc.Value.Date).Days);

        string action;
        string title;
        string summary;

        if (weather.RainNext48HoursMm >= 4 || (today.RainProbabilityPercent >= 70 && today.RainMm >= 1))
        {
            action = "skip";
            title = "Hold off watering";
            summary = "Useful rain is expected soon. Let the weather do the watering first, then check the soil afterwards.";
            reasons.Add($"{weather.RainNext48HoursMm:0.#} mm of rain is forecast over the next 48 hours.");
        }
        else if (weather.RainPast3DaysMm >= 6)
        {
            action = "check";
            title = "Check before watering";
            summary = "There has been meaningful recent rain, so watering by schedule could leave the roots too wet.";
            reasons.Add($"{weather.RainPast3DaysMm:0.#} mm of rain fell over the last 3 days.");
        }
        else if (today.Et0Mm >= 4.5 || today.TemperatureMaxC >= 29 || today.WindMaxKmh >= 30)
        {
            action = "check";
            title = "Drying conditions — check today";
            summary = "Heat, wind or evapotranspiration are high enough that the pot/soil may dry faster than usual.";
            reasons.Add($"Today's drying demand (ET₀) is {today.Et0Mm:0.#} mm.");
            if (today.TemperatureMaxC >= 29) reasons.Add($"Maximum temperature is around {today.TemperatureMaxC:0.#}°C.");
            if (today.WindMaxKmh >= 30) reasons.Add($"Winds may reach about {today.WindMaxKmh:0.#} km/h.");
        }
        else if (daysSinceWatered is >= 4)
        {
            action = "check";
            title = "Check the soil today";
            summary = "There is no strong rain signal and it has been several days since the last logged watering.";
            reasons.Add($"Last watering was logged {daysSinceWatered} days ago.");
        }
        else
        {
            action = "monitor";
            title = "No automatic watering needed";
            summary = "Conditions are not strongly pushing toward either watering or skipping. Use the soil as the deciding factor.";
            if (daysSinceWatered is not null) reasons.Add($"Last watering was logged {daysSinceWatered} day{(daysSinceWatered == 1 ? "" : "s")} ago.");
        }

        if (!string.IsNullOrWhiteSpace(plant.LatestIssueSummary))
        {
            reasons.Add($"Latest photo note: {plant.LatestIssueSummary}");
            if (ContainsAny(plant.LatestIssueSummary, "wilting", "droop", "dry", "crispy"))
            {
                summary += " The latest photo also suggests checking moisture promptly rather than waiting for a fixed schedule.";
            }
            else if (ContainsAny(plant.LatestIssueSummary, "yellow", "root", "wet", "overwater"))
            {
                summary += " The latest photo makes over-watering worth ruling out before adding more water.";
            }
        }

        var routine = isCitrus
            ? likelyPotted
                ? "For potted citrus, check the top few centimetres of mix frequently in warm weather, but do not water every day automatically. Water thoroughly when that layer has dried, then let excess water drain."
                : "For citrus, use soil moisture rather than a fixed daily schedule. Deep watering followed by some drying is generally more useful than small daily drinks."
            : "Do not water purely because a day has passed. Check the soil first and use the weather trend plus the plant's appearance to decide.";

        var climateComparison = today.Et0Mm > weather.CurrentMonthBaseline.AverageDailyEt0Mm * 1.25
            ? "Today is drying faster than a typical day for this time of year."
            : today.Et0Mm < weather.CurrentMonthBaseline.AverageDailyEt0Mm * 0.75
                ? "Today is less drying than a typical day for this time of year."
                : "Today's drying demand is close to the recent 10-year seasonal norm.";

        return new PlantCareAdvice(
            action,
            title,
            summary,
            routine,
            climateComparison,
            reasons.ToArray(),
            daysSinceWatered,
            weather);
    }

    private static bool ContainsAny(string? value, params string[] needles)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return needles.Any(x => value.Contains(x, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record PlantAdviceInput(
    string Name,
    string? Species,
    string? Location,
    string? Notes,
    DateTime? LastWateredAtUtc,
    string? LatestObservationNotes,
    string? LatestIssueSummary);

public sealed record PlantCareAdvice(
    string Action,
    string Title,
    string Summary,
    string RoutineGuidance,
    string ClimateComparison,
    string[] Reasons,
    int? DaysSinceWatered,
    GardenWeatherContext Weather);
