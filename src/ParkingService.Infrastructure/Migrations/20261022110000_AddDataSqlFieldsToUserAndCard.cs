using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddDataSqlFieldsToUserAndCard : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Thêm các trường vào bảng Cards theo data.sql
        migrationBuilder.AddColumn<string>(
            name: "Type",
            table: "Cards",
            type: "TEXT",
            maxLength: 20,
            nullable: false,
            defaultValue: "Registered");

        migrationBuilder.AddColumn<string>(
            name: "VehiclePlate",
            table: "Cards",
            type: "TEXT",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "SkipValidateVehiclePlate",
            table: "Cards",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "Info",
            table: "Cards",
            type: "TEXT",
            maxLength: 250,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Phone",
            table: "Cards",
            type: "TEXT",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Room",
            table: "Cards",
            type: "TEXT",
            maxLength: 50,
            nullable: true);

        // 2. Thêm các trường vào bảng Users theo data.sql
        migrationBuilder.AddColumn<string>(
            name: "Username",
            table: "Users",
            type: "TEXT",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "RoleId",
            table: "Users",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Role",
            table: "Users",
            type: "TEXT",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Room",
            table: "Users",
            type: "TEXT",
            maxLength: 50,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Type",
            table: "Cards");

        migrationBuilder.DropColumn(
            name: "VehiclePlate",
            table: "Cards");

        migrationBuilder.DropColumn(
            name: "SkipValidateVehiclePlate",
            table: "Cards");

        migrationBuilder.DropColumn(
            name: "Info",
            table: "Cards");

        migrationBuilder.DropColumn(
            name: "Phone",
            table: "Cards");

        migrationBuilder.DropColumn(
            name: "Room",
            table: "Cards");

        migrationBuilder.DropColumn(
            name: "Username",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "RoleId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Role",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Room",
            table: "Users");
    }
}
