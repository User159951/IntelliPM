using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeadLetterQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeadLetterMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MovedToDlqAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TotalRetryAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeadLetterMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterMessages_EventType",
                table: "DeadLetterMessages",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterMessages_IdempotencyKey",
                table: "DeadLetterMessages",
                column: "IdempotencyKey",
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterMessages_MovedToDlqAt",
                table: "DeadLetterMessages",
                column: "MovedToDlqAt");

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterMessages_MovedToDlqAt_EventType",
                table: "DeadLetterMessages",
                columns: new[] { "MovedToDlqAt", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterMessages_OriginalMessageId",
                table: "DeadLetterMessages",
                column: "OriginalMessageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeadLetterMessages");
        }
    }
}
