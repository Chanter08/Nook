using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

[DbContext(typeof(NookDbContext))]
[Migration("20261116211500_AddGardenPlants")]
public partial class AddGardenPlants : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'[dbo].[Plant]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Plant] (
                    [PlantId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Plant] PRIMARY KEY,
                    [Name] nvarchar(120) NOT NULL,
                    [Species] nvarchar(160) NULL,
                    [Variety] nvarchar(120) NULL,
                    [Location] nvarchar(120) NULL,
                    [AcquiredDate] date NULL,
                    [Notes] nvarchar(1000) NULL,
                    [CoverImageUrl] nvarchar(500) NULL,
                    [CreatedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_Plant_CreatedAtUtc] DEFAULT SYSUTCDATETIME()
                );
            END;

            IF OBJECT_ID(N'[dbo].[PlantObservation]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[PlantObservation] (
                    [PlantObservationId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_PlantObservation] PRIMARY KEY,
                    [PlantId] int NOT NULL,
                    [ObservedAtUtc] datetime2 NOT NULL,
                    [Notes] nvarchar(1500) NULL,
                    [ImageUrl] nvarchar(500) NULL,
                    [IssueSummary] nvarchar(500) NULL,
                    CONSTRAINT [FK_PlantObservation_Plant] FOREIGN KEY ([PlantId])
                        REFERENCES [dbo].[Plant]([PlantId]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_PlantObservation_PlantId_ObservedAtUtc]
                    ON [dbo].[PlantObservation]([PlantId], [ObservedAtUtc] DESC);
            END;

            IF OBJECT_ID(N'[dbo].[PlantCareEvent]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[PlantCareEvent] (
                    [PlantCareEventId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_PlantCareEvent] PRIMARY KEY,
                    [PlantId] int NOT NULL,
                    [EventType] nvarchar(40) NOT NULL,
                    [OccurredAtUtc] datetime2 NOT NULL,
                    [Notes] nvarchar(800) NULL,
                    CONSTRAINT [FK_PlantCareEvent_Plant] FOREIGN KEY ([PlantId])
                        REFERENCES [dbo].[Plant]([PlantId]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_PlantCareEvent_PlantId_OccurredAtUtc]
                    ON [dbo].[PlantCareEvent]([PlantId], [OccurredAtUtc] DESC);
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'[dbo].[PlantCareEvent]', N'U') IS NOT NULL DROP TABLE [dbo].[PlantCareEvent];
            IF OBJECT_ID(N'[dbo].[PlantObservation]', N'U') IS NOT NULL DROP TABLE [dbo].[PlantObservation];
            IF OBJECT_ID(N'[dbo].[Plant]', N'U') IS NOT NULL DROP TABLE [dbo].[Plant];
            """);
    }
}
