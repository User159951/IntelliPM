using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToGlobalSettingsAndAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Category and UpdatedById to GlobalSettings
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "GlobalSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "General");

            migrationBuilder.AddColumn<int>(
                name: "UpdatedById",
                table: "GlobalSettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalSettings_UpdatedById",
                table: "GlobalSettings",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_GlobalSettings_Users_UpdatedById",
                table: "GlobalSettings",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Create AuditLogs table
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Changes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_GlobalSettings_Users_UpdatedById",
                table: "GlobalSettings");

            migrationBuilder.DropIndex(
                name: "IX_GlobalSettings_UpdatedById",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "GlobalSettings");
        }
    }
}

