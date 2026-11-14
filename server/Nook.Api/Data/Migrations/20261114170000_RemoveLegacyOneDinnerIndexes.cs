using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

/// <summary>
/// Remove obsolete date+meal-type UNIQUE constraints left behind under unexpected
/// names by older/manual database changes. The new (date, type, slot) uniqueness
/// remains in force and all existing meal plan records are preserved.
/// </summary>
[DbContext(typeof(NookDbContext))]
[Migration("20261114170000_RemoveLegacyOneDinnerIndexes")]
public partial class RemoveLegacyOneDinnerIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DECLARE @TableId int = OBJECT_ID(N'dbo.MealPlanEntry', N'U');
            IF @TableId IS NULL
                THROW 51010, 'MealPlanEntry table is missing.', 1;

            -- Only target unfiltered UNIQUE indexes/constraints whose key is
            -- *exactly* PlanDate and MealType. Never remove the PK, newer
            -- PlanDate/MealType/Slot index, or indexes on different keys.
            DECLARE @Legacy TABLE (
                [IndexName] sysname NOT NULL,
                [IsConstraint] bit NOT NULL
            );

            INSERT INTO @Legacy ([IndexName], [IsConstraint])
            SELECT i.[name], i.[is_unique_constraint]
            FROM sys.indexes AS i
            WHERE i.[object_id] = @TableId
              AND i.[is_unique] = 1
              AND i.[is_primary_key] = 0
              AND i.[has_filter] = 0
              AND (
                  SELECT COUNT(*)
                  FROM sys.index_columns AS ic
                  WHERE ic.[object_id] = i.[object_id]
                    AND ic.[index_id] = i.[index_id]
                    AND ic.[key_ordinal] > 0
              ) = 2
              AND (
                  SELECT COUNT(*)
                  FROM sys.index_columns AS ic
                  JOIN sys.columns AS c
                    ON c.[object_id] = ic.[object_id]
                    AND c.[column_id] = ic.[column_id]
                  WHERE ic.[object_id] = i.[object_id]
                    AND ic.[index_id] = i.[index_id]
                    AND ic.[key_ordinal] > 0
                    AND c.[name] IN (N'PlanDate', N'MealType')
              ) = 2;

            DECLARE @IndexName sysname;
            DECLARE @IsConstraint bit;
            DECLARE @Statement nvarchar(max);
            WHILE EXISTS (SELECT 1 FROM @Legacy)
            BEGIN
                SELECT TOP (1)
                    @IndexName = [IndexName],
                    @IsConstraint = [IsConstraint]
                FROM @Legacy ORDER BY [IndexName];

                IF @IsConstraint = 1
                    SET @Statement = N'ALTER TABLE [dbo].[MealPlanEntry] DROP CONSTRAINT ' + QUOTENAME(@IndexName);
                ELSE
                    SET @Statement = N'DROP INDEX ' + QUOTENAME(@IndexName) + N' ON [dbo].[MealPlanEntry]';

                PRINT N'Removing obsolete one-meal-per-type uniqueness: ' + QUOTENAME(@IndexName);
                EXEC sys.sp_executesql @Statement;
                DELETE FROM @Legacy WHERE [IndexName] = @IndexName;
            END;

            -- Assert the three-column unique index required by the API exists.
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [object_id] = @TableId
                  AND [name] = N'IX_MealPlanEntry_PlanDate_MealType_Slot'
                  AND [is_unique] = 1
            )
                THROW 51011, 'The two-dinner unique slot index is missing.', 1;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Do not restore a restrictive two-column index: it would invalidate
        // legitimate days with two dinners or silently destroy user data.
    }
}
