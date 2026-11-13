using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>Keep existing meal plans and allow a second, independent dinner per day.</summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261113204000_AllowTwoDinnersPerDay")]
public partial class AllowTwoDinnersPerDay : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Slot", table: "MealPlanEntry", type: "int",
            nullable: false, defaultValue: 1);

        migrationBuilder.DropIndex(
            name: "IX_MealPlanEntry_PlanDate_MealType", table: "MealPlanEntry");

        migrationBuilder.CreateIndex(
            name: "IX_MealPlanEntry_PlanDate_MealType_Slot",
            table: "MealPlanEntry",
            columns: new[] { "PlanDate", "MealType", "Slot" },
            unique: true);

        migrationBuilder.Sql(
            """
            ALTER TABLE [MealPlanEntry]
            ADD CONSTRAINT [CK_MealPlanEntry_DinnerSlot]
                CHECK (([MealType] = N'Dinner' AND [Slot] IN (1, 2))
                    OR ([MealType] <> N'Dinner' AND [Slot] = 1));
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Don't silently discard an existing second dinner on rollback.
        migrationBuilder.Sql(
            """
            IF EXISTS (SELECT 1 FROM [MealPlanEntry] WHERE [Slot] <> 1)
                THROW 50001, 'Cannot revert two-dinner support while second dinners exist.', 1;
            ALTER TABLE [MealPlanEntry] DROP CONSTRAINT [CK_MealPlanEntry_DinnerSlot];
            """);

        migrationBuilder.DropIndex(
            name: "IX_MealPlanEntry_PlanDate_MealType_Slot", table: "MealPlanEntry");
        migrationBuilder.DropColumn(name: "Slot", table: "MealPlanEntry");
        migrationBuilder.CreateIndex(
            name: "IX_MealPlanEntry_PlanDate_MealType", table: "MealPlanEntry",
            columns: new[] { "PlanDate", "MealType" }, unique: true);
    }
}
