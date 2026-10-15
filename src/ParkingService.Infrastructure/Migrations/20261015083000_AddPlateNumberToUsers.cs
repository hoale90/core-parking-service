using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPlateNumberToUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PlateNumber",
            table: "Users",
            type: "TEXT",
            maxLength: 20,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PlateNumber",
            table: "Users");
    }
}
