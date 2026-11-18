using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

[DbContext(typeof(NookDbContext))]
[Migration("20261118213000_AddPlantGrowingConditions")]
public partial class AddPlantGrowingConditions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH('dbo.Plant', 'GrowingIn') IS NULL
                ALTER TABLE [dbo].[Plant] ADD [GrowingIn] nvarchar(20) NULL;
            IF COL_LENGTH('dbo.Plant', 'RainExposure') IS NULL
                ALTER TABLE [dbo].[Plant] ADD [RainExposure] nvarchar(20) NULL;
            IF COL_LENGTH('dbo.Plant', 'SunExposure') IS NULL
                ALTER TABLE [dbo].[Plant] ADD [SunExposure] nvarchar(20) NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH('dbo.Plant', 'SunExposure') IS NOT NULL
                ALTER TABLE [dbo].[Plant] DROP COLUMN [SunExposure];
            IF COL_LENGTH('dbo.Plant', 'RainExposure') IS NOT NULL
                ALTER TABLE [dbo].[Plant] DROP COLUMN [RainExposure];
            IF COL_LENGTH('dbo.Plant', 'GrowingIn') IS NOT NULL
                ALTER TABLE [dbo].[Plant] DROP COLUMN [GrowingIn];
            """);
    }
}
