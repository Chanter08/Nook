using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Nook.Api.Services;

public sealed class MaltaWeatherService
{
    private const double Latitude = 35.8997;
    private const double Longitude = 14.5147;
    private const string Timezone = "Europe/Malta";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;

    public MaltaWeatherService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    public async Task<GardenWeatherContext> GetContextAsync(CancellationToken cancellationToken = default)
    {
        var forecastTask = GetForecastAsync(cancellationToken);
        var climateTask = GetClimateBaselineAsync(cancellationToken);
        await Task.WhenAll(forecastTask, climateTask);

        var forecast = await forecastTask;
        var baseline = await climateTask;
        var todayIndex = forecast.Dates.FindIndex(x => x == DateOnly.FromDateTime(DateTime.Now));
        if (todayIndex < 0) todayIndex = Math.Min(3, forecast.Dates.Count - 1);

        double Sum(IEnumerable<double> values) => Math.Round(values.Sum(), 1);
        var pastRain = Sum(forecast.Precipitation.Take(todayIndex));
        var next48Rain = Sum(forecast.Precipitation.Skip(todayIndex).Take(2));
        var next7Rain = Sum(forecast.Precipitation.Skip(todayIndex).Take(7));

        var today = new GardenWeatherDay(
            forecast.Dates[todayIndex],
            forecast.TempMax[todayIndex],
            forecast.TempMin[todayIndex],
            forecast.Precipitation[todayIndex],
            forecast.PrecipProbability[todayIndex],
            forecast.Et0[todayIndex],
            forecast.WindMax[todayIndex]);

        return new GardenWeatherContext(
            "Malta",
            10,
            today,
            pastRain,
            next48Rain,
            next7Rain,
            baseline,
            forecast.Dates.Select((d, i) => new GardenWeatherDay(
                d, forecast.TempMax[i], forecast.TempMin[i], forecast.Precipitation[i],
                forecast.PrecipProbability[i], forecast.Et0[i], forecast.WindMax[i])).ToArray());
    }

    private async Task<WeatherSeries> GetForecastAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var daily = "temperature_2m_max,temperature_2m_min,precipitation_sum,precipitation_probability_max,et0_fao_evapotranspiration,wind_speed_10m_max";
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={Latitude.ToString(CultureInfo.InvariantCulture)}&longitude={Longitude.ToString(CultureInfo.InvariantCulture)}&daily={daily}&timezone={Uri.EscapeDataString(Timezone)}&past_days=3&forecast_days=7";
        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        return ReadSeries(document.RootElement.GetProperty("daily"), includeProbability: true);
    }

    private async Task<ClimateBaseline> GetClimateBaselineAsync(CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync("malta-climate-baseline-v1", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);

            var endYear = DateTime.UtcNow.Year - 1;
            var startYear = endYear - 9;
            var client = _httpClientFactory.CreateClient();
            var daily = "temperature_2m_max,temperature_2m_min,precipitation_sum,et0_fao_evapotranspiration";
            var url = $"https://archive-api.open-meteo.com/v1/archive?latitude={Latitude.ToString(CultureInfo.InvariantCulture)}&longitude={Longitude.ToString(CultureInfo.InvariantCulture)}&start_date={startYear}-01-01&end_date={endYear}-12-31&daily={daily}&timezone={Uri.EscapeDataString(Timezone)}&models=era5";
            using var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var dailyElement = document.RootElement.GetProperty("daily");
            var dates = ReadDates(dailyElement.GetProperty("time"));
            var max = ReadDoubles(dailyElement.GetProperty("temperature_2m_max"));
            var min = ReadDoubles(dailyElement.GetProperty("temperature_2m_min"));
            var rain = ReadDoubles(dailyElement.GetProperty("precipitation_sum"));
            var et0 = ReadDoubles(dailyElement.GetProperty("et0_fao_evapotranspiration"));

            var month = DateTime.Now.Month;
            var indexes = dates.Select((date, index) => (date, index)).Where(x => x.date.Month == month).Select(x => x.index).ToArray();
            double Average(IReadOnlyList<double> values) => indexes.Length == 0 ? 0 : Math.Round(indexes.Average(i => values[i]), 1);

            return new ClimateBaseline(
                startYear,
                endYear,
                month,
                Average(max),
                Average(min),
                Average(rain),
                Average(et0));
        }) ?? throw new InvalidOperationException("Could not build Malta climate baseline.");
    }

    private static WeatherSeries ReadSeries(JsonElement daily, bool includeProbability)
    {
        var dates = ReadDates(daily.GetProperty("time"));
        var count = dates.Count;
        return new WeatherSeries(
            dates,
            ReadDoubles(daily.GetProperty("temperature_2m_max")),
            ReadDoubles(daily.GetProperty("temperature_2m_min")),
            ReadDoubles(daily.GetProperty("precipitation_sum")),
            includeProbability ? ReadDoubles(daily.GetProperty("precipitation_probability_max")) : Enumerable.Repeat(0d, count).ToList(),
            ReadDoubles(daily.GetProperty("et0_fao_evapotranspiration")),
            ReadDoubles(daily.GetProperty("wind_speed_10m_max")));
    }

    private static List<DateOnly> ReadDates(JsonElement element) =>
        element.EnumerateArray().Select(x => DateOnly.Parse(x.GetString()!, CultureInfo.InvariantCulture)).ToList();

    private static List<double> ReadDoubles(JsonElement element) =>
        element.EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? 0d : x.GetDouble()).ToList();

    private sealed record WeatherSeries(
        List<DateOnly> Dates,
        List<double> TempMax,
        List<double> TempMin,
        List<double> Precipitation,
        List<double> PrecipProbability,
        List<double> Et0,
        List<double> WindMax);
}

public sealed record GardenWeatherContext(
    string Location,
    int BaselineYears,
    GardenWeatherDay Today,
    double RainPast3DaysMm,
    double RainNext48HoursMm,
    double RainNext7DaysMm,
    ClimateBaseline CurrentMonthBaseline,
    GardenWeatherDay[] Forecast);

public sealed record GardenWeatherDay(
    DateOnly Date,
    double TemperatureMaxC,
    double TemperatureMinC,
    double RainMm,
    double RainProbabilityPercent,
    double Et0Mm,
    double WindMaxKmh);

public sealed record ClimateBaseline(
    int StartYear,
    int EndYear,
    int Month,
    double AverageMaxTemperatureC,
    double AverageMinTemperatureC,
    double AverageDailyRainMm,
    double AverageDailyEt0Mm);
