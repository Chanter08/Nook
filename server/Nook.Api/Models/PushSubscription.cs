namespace Nook.Api.Models;
public class PushSubscription {
 public Guid Id { get; set; }
 public string AccessToken { get; set; } = "";
 public string Endpoint { get; set; } = "";
 public string P256dh { get; set; } = "";
 public string Auth { get; set; } = "";
 public bool MealsEnabled { get; set; } = true;
 public bool ChoresEnabled { get; set; } = true;
 public bool EventsEnabled { get; set; } = true;
 public bool PantryEnabled { get; set; } = true;
 public int ReminderHour { get; set; } = 9;
 public DateTime CreatedAtUtc { get; set; }
}
