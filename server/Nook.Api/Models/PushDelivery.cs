namespace Nook.Api.Models;
public class PushDelivery {
 public long Id { get; set; }
 public Guid SubscriptionId { get; set; }
 public string EventKey { get; set; } = "";
 public DateTime SentAtUtc { get; set; }
 public PushSubscription Subscription { get; set; } = null!;
}
