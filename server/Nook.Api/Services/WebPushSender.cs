using System.Text.Json;
using Nook.Api.Models;
using WebPush;
namespace Nook.Api.Services;
public sealed class WebPushSender(IConfiguration config) {
 public async Task SendAsync(PushSubscription device, string title, string body, string url, CancellationToken ct) {
  var subject = config["WebPush:Subject"];
  var pub = config["WebPush:PublicKey"];
  var priv = config["WebPush:PrivateKey"];
  if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(pub) || string.IsNullOrWhiteSpace(priv))
   throw new InvalidOperationException("Missing VAPID configuration");
  using var client = new WebPushClient();
  await client.SendNotificationAsync(new WebPush.PushSubscription(device.Endpoint, device.P256dh, device.Auth),
   JsonSerializer.Serialize(new { title, body, url }), new VapidDetails(subject, pub, priv), ct);
 }
}
