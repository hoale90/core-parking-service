using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSkipValidatePlateToCards : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "SkipValidatePlate",
            table: "Cards",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SkipValidatePlate",
            table: "Cards");
    }
}
