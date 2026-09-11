namespace Nook.Api.DTOs.Calendar;

public class CalendarEventResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool AllDay { get; set; }
}