using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;
#nullable disable
namespace Nook.Api.Data.Migrations;
[DbContext(typeof(NookDbContext))]
[Migration("20261108100000_AddWebPushReminders")]
public partial class AddWebPushReminders : Migration {
 protected override void Up(MigrationBuilder b) {
  b.CreateTable("PushSubscription", columns: table => new {
   Id=table.Column<Guid>("uniqueidentifier",nullable:false),
   AccessToken=table.Column<string>("nvarchar(64)",maxLength:64,nullable:false),
   Endpoint=table.Column<string>("nvarchar(2000)",maxLength:2000,nullable:false),
   P256dh=table.Column<string>("nvarchar(512)",maxLength:512,nullable:false),
   Auth=table.Column<string>("nvarchar(256)",maxLength:256,nullable:false),
   MealsEnabled=table.Column<bool>("bit",nullable:false),
   ChoresEnabled=table.Column<bool>("bit",nullable:false),
   EventsEnabled=table.Column<bool>("bit",nullable:false),
   PantryEnabled=table.Column<bool>("bit",nullable:false),
   ReminderHour=table.Column<int>("int",nullable:false),
   CreatedAtUtc=table.Column<DateTime>("datetime2",nullable:false)
  },constraints:table=>table.PrimaryKey("PK_PushSubscription",x=>x.Id));
  b.CreateIndex("IX_PushSubscription_Endpoint","PushSubscription","Endpoint",unique:true);
  b.CreateTable("PushDelivery",columns: table => new {
   Id=table.Column<long>("bigint",nullable:false).Annotation("SqlServer:Identity","1, 1"),
   SubscriptionId=table.Column<Guid>("uniqueidentifier",nullable:false),
   EventKey=table.Column<string>("nvarchar(300)",maxLength:300,nullable:false),
   SentAtUtc=table.Column<DateTime>("datetime2",nullable:false)
  },constraints:table=>{
   table.PrimaryKey("PK_PushDelivery",x=>x.Id);
   table.ForeignKey("FK_PushDelivery_PushSubscription_SubscriptionId",x=>x.SubscriptionId,"PushSubscription","Id",onDelete:ReferentialAction.Cascade);
  });
  b.CreateIndex("IX_PushDelivery_SubscriptionId_EventKey","PushDelivery",new[]{"SubscriptionId","EventKey"},unique:true);
 }
 protected override void Down(MigrationBuilder b) {
  b.DropTable("PushDelivery");b.DropTable("PushSubscription");
 }
}
