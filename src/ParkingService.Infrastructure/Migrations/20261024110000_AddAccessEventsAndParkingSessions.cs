using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddAccessEventsAndParkingSessions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AccessEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CardNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                CardId = table.Column<Guid>(type: "TEXT", nullable: true),
                UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                Direction = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                LaneCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                OcrPlateNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                PlateNormalized = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                ExpectedPlateNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                PlateSimilarityPercent = table.Column<double>(type: "REAL", nullable: true),
                SkipValidatePlate = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                Decision = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                ReasonCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                SnapshotPath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                EventTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccessEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_AccessEvents_Cards_CardId",
                    column: x => x.CardId,
                    principalTable: "Cards",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_AccessEvents_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "ParkingSessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CardNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                CardId = table.Column<Guid>(type: "TEXT", nullable: true),
                UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                PlateNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                EntryTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                ExitTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                DurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                EntryEventId = table.Column<Guid>(type: "TEXT", nullable: true),
                ExitEventId = table.Column<Guid>(type: "TEXT", nullable: true),
                EntrySnapshotPath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                ExitSnapshotPath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ParkingSessions", x => x.Id);
                table.ForeignKey(
                    name: "FK_ParkingSessions_AccessEvents_EntryEventId",
                    column: x => x.EntryEventId,
                    principalTable: "AccessEvents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_ParkingSessions_AccessEvents_ExitEventId",
                    column: x => x.ExitEventId,
                    principalTable: "AccessEvents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_ParkingSessions_Cards_CardId",
                    column: x => x.CardId,
                    principalTable: "Cards",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_ParkingSessions_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AccessEvents_CardId",
            table: "AccessEvents",
            column: "CardId");

        migrationBuilder.CreateIndex(
            name: "IX_AccessEvents_CardNumber",
            table: "AccessEvents",
            column: "CardNumber");

        migrationBuilder.CreateIndex(
            name: "IX_AccessEvents_EventTime",
            table: "AccessEvents",
            column: "EventTime");

        migrationBuilder.CreateIndex(
            name: "IX_AccessEvents_UserId",
            table: "AccessEvents",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_ParkingSessions_CardId",
            table: "ParkingSessions",
            column: "CardId");

        migrationBuilder.CreateIndex(
            name: "IX_ParkingSessions_CardNumber",
            table: "ParkingSessions",
            column: "CardNumber");

        migrationBuilder.CreateIndex(
            name: "IX_ParkingSessions_EntryEventId",
            table: "ParkingSessions",
            column: "EntryEventId");

        migrationBuilder.CreateIndex(
            name: "IX_ParkingSessions_ExitEventId",
            table: "ParkingSessions",
            column: "ExitEventId");

        migrationBuilder.CreateIndex(
            name: "IX_ParkingSessions_PlateNumber",
            table: "ParkingSessions",
            column: "PlateNumber");

        migrationBuilder.CreateIndex(
            name: "IX_ParkingSessions_Status",
            table: "ParkingSessions",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_ParkingSessions_UserId",
            table: "ParkingSessions",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ParkingSessions");

        migrationBuilder.DropTable(
            name: "AccessEvents");
    }
}
