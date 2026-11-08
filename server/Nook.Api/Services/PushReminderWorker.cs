using System.Net;
using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.Models;
namespace Nook.Api.Services;

/// <summary>Single-host reminder dispatcher; scale-out requires a distributed lease/outbox.</summary>
public sealed class PushReminderWorker(IServiceScopeFactory scopes,IConfiguration config,ILogger<PushReminderWorker> log): BackgroundService {
 private static readonly TimeZoneInfo Zone=TimeZoneInfo.FindSystemTimeZoneById("Europe/Malta");
 protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
  using var timer=new PeriodicTimer(TimeSpan.FromMinutes(5));
  while(!stoppingToken.IsCancellationRequested) {
   try { await Dispatch(stoppingToken); }
   catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) { break; }
   catch(Exception ex) { log.LogError(ex,"Push reminder scan failed"); }
   if(!await timer.WaitForNextTickAsync(stoppingToken))break;
  }
 }
 private async Task Dispatch(CancellationToken ct) {
  if(new[]{"WebPush:PublicKey","WebPush:PrivateKey","WebPush:Subject"}.Any(k=>string.IsNullOrWhiteSpace(config[k])))return;
  var local=TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,Zone);
  var today=DateOnly.FromDateTime(local);
  var tomorrow=today.AddDays(1);
  await using var scope=scopes.CreateAsyncScope();
  var db=scope.ServiceProvider.GetRequiredService<NookDbContext>();
  var sender=scope.ServiceProvider.GetRequiredService<WebPushSender>();
  var devices=await db.PushSubscriptions.Where(x=>x.ReminderHour==local.Hour).ToListAsync(ct);
  if(devices.Count==0)return;
  var reminders=new List<(string kind,string key,string title,string body,string url)>();
  if(devices.Any(x=>x.MealsEnabled)) {
   var meals=await db.MealPlanEntries.AsNoTracking().Include(x=>x.Recipe).Where(x=>x.PlanDate==today).ToListAsync(ct);
   foreach(var m in meals)reminders.Add(("meal",$"meal:{today:yyyyMMdd}:{m.MealPlanEntryId}:{m.RecipeId}","Today's meal plan",$"{m.MealType}: {m.Recipe.Name}","/plans"));
  }
  if(devices.Any(x=>x.ChoresEnabled)) {
   var chores=await db.Chores.AsNoTracking().Where(x=>!x.IsCompleted && x.DueDate==today).ToListAsync(ct);
   foreach(var c in chores)reminders.Add(("chore",$"chore:{today:yyyyMMdd}:{c.ChoreId}","Chore due today",c.Title,"/us"));
  }
  if(devices.Any(x=>x.PantryEnabled)) {
   var items=await db.PantryItems.AsNoTracking().Where(x=>x.ExpiryDate==tomorrow && (x.Quantity==null || x.Quantity>0)).ToListAsync(ct);
   foreach(var i in items)reminders.Add(("pantry",$"pantry:{tomorrow:yyyyMMdd}:{i.PantryItemId}","Expires tomorrow",i.Name,"/pantry"));
  }
  if(devices.Any(x=>x.EventsEnabled)) {
   try {
    var events=await scope.ServiceProvider.GetRequiredService<CalendarService>().GetUpcomingAsync();
    foreach(var e in events) {
     var utc=DateTime.SpecifyKind(e.Start,DateTimeKind.Utc);
     var eventDay=DateOnly.FromDateTime(e.AllDay?utc:TimeZoneInfo.ConvertTimeFromUtc(utc,Zone));
     if(eventDay==tomorrow) reminders.Add(("event",$"event:{tomorrow:yyyyMMdd}:{e.Id}:{e.Start.Ticks}","Event tomorrow",e.Title,"/plans"));
    }
   } catch(Exception ex) { log.LogWarning(ex,"Calendar unavailable for event reminders"); }
  }
  foreach(var d in devices)foreach(var r in reminders) {
   if(r.kind=="meal"&&!d.MealsEnabled || r.kind=="chore"&&!d.ChoresEnabled ||
      r.kind=="pantry"&&!d.PantryEnabled || r.kind=="event"&&!d.EventsEnabled)continue;
   var key=r.key.Length<=300?r.key:Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(r.key)));
   if(await db.PushDeliveries.AnyAsync(x=>x.SubscriptionId==d.Id&&x.EventKey==key,ct))continue;
   try {
    await sender.SendAsync(d,r.title,r.body,r.url,ct);
    db.PushDeliveries.Add(new PushDelivery {SubscriptionId=d.Id,EventKey=key,SentAtUtc=DateTime.UtcNow});
    await db.SaveChangesAsync(ct);
   } catch(WebPush.WebPushException ex) when(ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound) {
    db.PushSubscriptions.Remove(d);await db.SaveChangesAsync(ct);break;
   } catch(Exception ex) { log.LogWarning(ex,"Push delivery failed for {SubscriptionId}",d.Id); }
  }
 }
}
