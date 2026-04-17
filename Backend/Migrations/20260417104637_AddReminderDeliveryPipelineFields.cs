using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIStudyPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderDeliveryPipelineFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryAttempts",
                table: "Reminders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryStatus",
                table: "Reminders",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDeliveryAttemptAtUtc",
                table: "Reminders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastDeliveryError",
                table: "Reminders",
                type: "varchar(800)",
                maxLength: 800,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE `Reminders` SET `DeliveryStatus` = 'Pending' WHERE `DeliveryStatus` = '' OR `DeliveryStatus` IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_DeliveryStatus_ReminderDateTime",
                table: "Reminders",
                columns: new[] { "DeliveryStatus", "ReminderDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reminders_DeliveryStatus_ReminderDateTime",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "DeliveryAttempts",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "DeliveryStatus",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "LastDeliveryAttemptAtUtc",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "LastDeliveryError",
                table: "Reminders");
        }
    }
}
