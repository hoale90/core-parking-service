using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                Gender = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Cards",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CardNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cards", x => x.Id);
                table.ForeignKey(
                    name: "FK_Cards_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Cards_CardNumber",
            table: "Cards",
            column: "CardNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Cards_UserId",
            table: "Cards",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Cards");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
