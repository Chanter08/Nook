using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Nook.Api.DTOs.Calendar;

namespace Nook.Api.Services;

public class CalendarService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public CalendarService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<CalendarEventResponse>> GetUpcomingAsync()
    {
        var calendarUrl = _configuration["GoogleCalendar:IcalUrl"];

        if (string.IsNullOrWhiteSpace(calendarUrl))
        {
            throw new InvalidOperationException(
                "Google Calendar iCal URL has not been configured.");
        }

        var httpClient = _httpClientFactory.CreateClient();
        var ics = await httpClient.GetStringAsync(calendarUrl);
        var calendar = Calendar.Load(ics);

        if (calendar is null)
        {
            throw new InvalidOperationException(
                "Could not read Google Calendar.");
        }

        var now = new CalDateTime(DateTime.UtcNow, "UTC", true);
        var thirtyDaysFromNow = new CalDateTime(
            DateTime.UtcNow.AddDays(30),
            "UTC",
            true);

        var events = calendar
            .GetOccurrences(now)
            .TakeWhileBefore(thirtyDaysFromNow)
            .Where(x => x.Source is CalendarEvent)
            .Select(x =>
            {
                var calendarEvent = (CalendarEvent)x.Source;
                var start = x.Period.StartTime.AsUtc;
                var end = x.Period.EffectiveEndTime?.AsUtc
                    ?? x.Period.EndTime?.AsUtc
                    ?? start;

                return new CalendarEventResponse
                {
                    Id = calendarEvent.Uid ?? string.Empty,
                    Title = calendarEvent.Summary ?? "Untitled event",
                    Start = start,
                    End = end,
                    AllDay = !x.Period.StartTime.HasTime
                };
            })
            .OrderBy(x => x.Start)
            .ToList();

        return events;
    }
}