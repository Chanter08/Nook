using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.Models;
using Nook.Api.Services;
namespace Nook.Api.Endpoints;

public static class PushEndpoints {
 public sealed record Keys(string P256dh,string Auth);
 public sealed record RegisterRequest(string Endpoint,Keys Keys);
 public sealed record Prefs(bool MealsEnabled,bool ChoresEnabled,bool EventsEnabled,bool PantryEnabled,int ReminderHour);

 public static IEndpointRouteBuilder MapPushEndpoints(this IEndpointRouteBuilder app) {
  var g=app.MapGroup("/api/push");
  g.MapGet("/public-key",(IConfiguration c) => string.IsNullOrWhiteSpace(c["WebPush:PublicKey"])
   ? Results.Problem("Web Push is not configured.",statusCode:503)
   : Results.Ok(new { publicKey=c["WebPush:PublicKey"] }));
  g.MapPost("/subscriptions",Register);
  g.MapGet("/subscriptions/{id:guid}",Read);
  g.MapPut("/subscriptions/{id:guid}",Save);
  g.MapDelete("/subscriptions/{id:guid}",Delete);
  g.MapPost("/subscriptions/{id:guid}/test",Test);
  return app;
 }
 private static object View(PushSubscription s) => new { s.Id,s.MealsEnabled,s.ChoresEnabled,s.EventsEnabled,s.PantryEnabled,s.ReminderHour };
 private static bool Authorized(HttpRequest req,PushSubscription s) {
  var value=req.Headers["X-Push-Token"].ToString();
  return value.Length==s.AccessToken.Length &&
   CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(value), System.Text.Encoding.UTF8.GetBytes(s.AccessToken));
 }
 private static async Task<IResult> Register(RegisterRequest data,NookDbContext db,IConfiguration config,CancellationToken ct) {
  if(new[]{"WebPush:PublicKey","WebPush:PrivateKey","WebPush:Subject"}.Any(k=>string.IsNullOrWhiteSpace(config[k])))
   return Results.Problem("Web Push is not configured.",statusCode:503);
  if(data.Keys is null || !Uri.TryCreate(data.Endpoint,UriKind.Absolute,out var uri) ||
   uri.Scheme!="https" || data.Endpoint.Length>2000 || string.IsNullOrWhiteSpace(data.Keys.P256dh) ||
   string.IsNullOrWhiteSpace(data.Keys.Auth) || data.Keys.P256dh.Length>512 || data.Keys.Auth.Length>256)
   return Results.BadRequest(new { message="Invalid push subscription." });
  var s=await db.PushSubscriptions.SingleOrDefaultAsync(x=>x.Endpoint==data.Endpoint,ct);
  if(s is null) {
   s=new PushSubscription { Id=Guid.NewGuid(),CreatedAtUtc=DateTime.UtcNow };
   db.PushSubscriptions.Add(s);
  }
  s.Endpoint=data.Endpoint; s.P256dh=data.Keys.P256dh; s.Auth=data.Keys.Auth;
  s.AccessToken=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
  await db.SaveChangesAsync(ct);
  return Results.Ok(new { s.Id,token=s.AccessToken,preferences=View(s) });
 }
 private static async Task<IResult> Read(Guid id,HttpRequest req,NookDbContext db,CancellationToken ct) {
  var s=await db.PushSubscriptions.FindAsync([id],ct);
  return s is null || !Authorized(req,s)?Results.NotFound():Results.Ok(View(s));
 }
 private static async Task<IResult> Save(Guid id,Prefs p,HttpRequest req,NookDbContext db,CancellationToken ct) {
  var s=await db.PushSubscriptions.FindAsync([id],ct);
  if(s is null || !Authorized(req,s)) return Results.NotFound();
  if(p.ReminderHour<0 || p.ReminderHour>23) return Results.BadRequest(new { message="Choose an hour from 0 to 23." });
  s.MealsEnabled=p.MealsEnabled;s.ChoresEnabled=p.ChoresEnabled;s.EventsEnabled=p.EventsEnabled;s.PantryEnabled=p.PantryEnabled;s.ReminderHour=p.ReminderHour;
  await db.SaveChangesAsync(ct);
  return Results.Ok(View(s));
 }
 private static async Task<IResult> Delete(Guid id,HttpRequest req,NookDbContext db,CancellationToken ct) {
  var s=await db.PushSubscriptions.FindAsync([id],ct);
  if(s is null || !Authorized(req,s)) return Results.NotFound();
  db.PushSubscriptions.Remove(s);await db.SaveChangesAsync(ct);return Results.NoContent();
 }
 private static async Task<IResult> Test(Guid id,HttpRequest req,NookDbContext db,WebPushSender sender,CancellationToken ct) {
  var s=await db.PushSubscriptions.FindAsync([id],ct);
  if(s is null || !Authorized(req,s))return Results.NotFound();
  try { await sender.SendAsync(s,"Nook notifications are ready","You can receive reminders on this device.","/us",ct);return Results.NoContent(); }
  catch(WebPush.WebPushException ex) when(ex.StatusCode is System.Net.HttpStatusCode.Gone or System.Net.HttpStatusCode.NotFound) {
   db.PushSubscriptions.Remove(s);await db.SaveChangesAsync(ct);
   return Results.Problem("Subscription expired; enable again.",statusCode:410);
  }
  catch(Exception) { return Results.Problem("Push test failed.",statusCode:502); }
 }
}
