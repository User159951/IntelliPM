using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAIQuotaManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RetrospectiveNotes",
                table: "Sprints",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserAIQuotaOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PeriodStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PeriodEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MaxTokensPerPeriod = table.Column<int>(type: "int", nullable: true),
                    MaxRequestsPerPeriod = table.Column<int>(type: "int", nullable: true),
                    MaxDecisionsPerPeriod = table.Column<int>(type: "int", nullable: true),
                    MaxCostPerPeriod = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAIQuotaOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAIQuotaOverrides_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAIQuotaOverrides_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAIQuotaOverrides_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserAIUsageCounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PeriodStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PeriodEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TokensUsed = table.Column<int>(type: "int", nullable: false),
                    RequestsUsed = table.Column<int>(type: "int", nullable: false),
                    DecisionsMade = table.Column<int>(type: "int", nullable: false),
                    CostAccumulated = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastAggregatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAIUsageCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAIUsageCounters_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAIUsageCounters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAIQuotaOverrides_CreatedByUserId",
                table: "UserAIQuotaOverrides",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAIQuotaOverrides_Org_User",
                table: "UserAIQuotaOverrides",
                columns: new[] { "OrganizationId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAIQuotaOverrides_OrganizationId",
                table: "UserAIQuotaOverrides",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAIQuotaOverrides_User_Period",
                table: "UserAIQuotaOverrides",
                columns: new[] { "UserId", "PeriodStartDate", "PeriodEndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAIQuotaOverrides_UserId",
                table: "UserAIQuotaOverrides",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAIUsageCounters_Org_User",
                table: "UserAIUsageCounters",
                columns: new[] { "OrganizationId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAIUsageCounters_OrganizationId",
                table: "UserAIUsageCounters",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAIUsageCounters_User_Period",
                table: "UserAIUsageCounters",
                columns: new[] { "UserId", "PeriodStartDate", "PeriodEndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAIUsageCounters_UserId",
                table: "UserAIUsageCounters",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAIQuotaOverrides");

            migrationBuilder.DropTable(
                name: "UserAIUsageCounters");

            migrationBuilder.DropColumn(
                name: "RetrospectiveNotes",
                table: "Sprints");
        }
    }
}
