using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nook.Api.Data;

#nullable disable

namespace Nook.Api.Data.Migrations;

[DbContext(typeof(NookDbContext))]
[Migration("20261025213000_AddChores")]
public partial class AddChores : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Chore",
            columns: table => new
            {
                ChoreId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                AssignedTo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                Recurrence = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastCompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Chore", x => x.ChoreId));

        migrationBuilder.CreateIndex(
            name: "IX_Chore_IsCompleted_DueDate",
            table: "Chore",
            columns: new[] { "IsCompleted", "DueDate" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Chore");
    }
}
