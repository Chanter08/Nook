using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Make the two-dinner migration resilient to a database whose old
/// (PlanDate, MealType) index was previously removed outside EF migrations.
/// Keep all existing meal plans; never discard entries to resolve conflicts.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261113204000_AllowTwoDinnersPerDay")]
public partial class AllowTwoDinnersPerDay : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- The existing production database can differ from the initial EF index history.
            -- Use a conditional DROP rather than assuming the old index exists.
            IF COL_LENGTH(N'dbo.MealPlanEntry', N'Slot') IS NULL
            BEGIN
                ALTER TABLE [MealPlanEntry]
                    ADD [Slot] int NOT NULL CONSTRAINT [DF_MealPlanEntry_Slot] DEFAULT (1);
            END;
            """);

        // SQL Server compiles each command before executing it. Keep the
        // column addition in its own batch so subsequent statements can
        // safely reference Slot, whether it already existed or was added.
        migrationBuilder.Sql(
            """
            -- If a previous manual index removal permitted duplicate meals, do not
            -- delete any entries. Fail clearly when there are more than the allowed
            -- one breakfast/lunch/snack or two dinners per day.
            IF EXISTS (
                SELECT 1 FROM [MealPlanEntry]
                GROUP BY [PlanDate], [MealType]
                HAVING COUNT(*) > CASE WHEN [MealType] = N'Dinner' THEN 2 ELSE 1 END
            )
            BEGIN
                THROW 51001, 'Meal plan has more entries than supported for a date and meal type. Resolve duplicates manually; no meals were deleted.', 1;
            END;

            -- Preserve two already-planned dinners, if present, and give them
            -- deterministic distinct slots before enforcing the new unique index.
            ;WITH DinnerSlots AS (
                SELECT [MealPlanEntryId],
                       ROW_NUMBER() OVER (
                           PARTITION BY [PlanDate] ORDER BY [MealPlanEntryId]
                       ) AS [NewSlot]
                FROM [MealPlanEntry]
                WHERE [MealType] = N'Dinner'
            )
            UPDATE m SET [Slot] = d.[NewSlot]
            FROM [MealPlanEntry] AS m
            INNER JOIN DinnerSlots AS d
                ON m.[MealPlanEntryId] = d.[MealPlanEntryId];

            IF EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID(N'[MealPlanEntry]')
                    AND name = N'IX_MealPlanEntry_PlanDate_MealType'
            )
            BEGIN
                DROP INDEX [IX_MealPlanEntry_PlanDate_MealType] ON [MealPlanEntry];
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID(N'[MealPlanEntry]')
                    AND name = N'IX_MealPlanEntry_PlanDate_MealType_Slot'
            )
            BEGIN
                CREATE UNIQUE INDEX [IX_MealPlanEntry_PlanDate_MealType_Slot]
                    ON [MealPlanEntry] ([PlanDate], [MealType], [Slot]);
            END;

            IF NOT EXISTS (
                SELECT 1 FROM sys.check_constraints
                WHERE parent_object_id = OBJECT_ID(N'[MealPlanEntry]')
                    AND name = N'CK_MealPlanEntry_DinnerSlot'
            )
            BEGIN
                ALTER TABLE [MealPlanEntry]
                    ADD CONSTRAINT [CK_MealPlanEntry_DinnerSlot]
                        CHECK (
                            ([MealType] = N'Dinner' AND [Slot] IN (1, 2))
                            OR ([MealType] <> N'Dinner' AND [Slot] = 1)
                        );
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- An unsafe rollback would discard or invalidate second dinners.
            IF EXISTS (SELECT 1 FROM [MealPlanEntry] WHERE [Slot] <> 1)
            BEGIN
                THROW 51002, 'Cannot revert two-dinner support while second dinners exist.', 1;
            END;

            IF EXISTS (
                SELECT 1 FROM sys.check_constraints
                WHERE parent_object_id = OBJECT_ID(N'[MealPlanEntry]')
                    AND name = N'CK_MealPlanEntry_DinnerSlot'
            )
                ALTER TABLE [MealPlanEntry] DROP CONSTRAINT [CK_MealPlanEntry_DinnerSlot];

            IF EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID(N'[MealPlanEntry]')
                    AND name = N'IX_MealPlanEntry_PlanDate_MealType_Slot'
            )
                DROP INDEX [IX_MealPlanEntry_PlanDate_MealType_Slot] ON [MealPlanEntry];
            """);

        // EF's SQL Server provider also removes the default constraint
        // attached to Slot before dropping the column.
        migrationBuilder.DropColumn(name: "Slot", table: "MealPlanEntry");

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID(N'[MealPlanEntry]')
                    AND name = N'IX_MealPlanEntry_PlanDate_MealType'
            )
                CREATE UNIQUE INDEX [IX_MealPlanEntry_PlanDate_MealType]
                    ON [MealPlanEntry] ([PlanDate], [MealType]);
            """);
    }
}
