using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAIQuotaEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIQuotas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    TierName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Free"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PeriodStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PeriodEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MaxTokensPerPeriod = table.Column<int>(type: "int", nullable: false),
                    MaxRequestsPerPeriod = table.Column<int>(type: "int", nullable: false),
                    MaxDecisionsPerPeriod = table.Column<int>(type: "int", nullable: false),
                    MaxCostPerPeriod = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TokensUsed = table.Column<int>(type: "int", nullable: false),
                    RequestsUsed = table.Column<int>(type: "int", nullable: false),
                    DecisionsMade = table.Column<int>(type: "int", nullable: false),
                    CostAccumulated = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    UsageByAgentJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    UsageByDecisionTypeJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    EnforceQuota = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsQuotaExceeded = table.Column<bool>(type: "bit", nullable: false),
                    QuotaExceededAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    QuotaExceededReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlertThresholdPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 80m),
                    AlertSent = table.Column<bool>(type: "bit", nullable: false),
                    AlertSentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AllowOverage = table.Column<bool>(type: "bit", nullable: false),
                    OverageRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    OverageTokensUsed = table.Column<int>(type: "int", nullable: false),
                    OverageCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastResetAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BillingReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIQuotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIQuotas_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotas_Active_PeriodEndDate",
                table: "AIQuotas",
                columns: new[] { "IsActive", "PeriodEndDate" },
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotas_IsQuotaExceeded",
                table: "AIQuotas",
                column: "IsQuotaExceeded");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotas_Organization_Active",
                table: "AIQuotas",
                columns: new[] { "OrganizationId", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotas_OrganizationId",
                table: "AIQuotas",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotas_PeriodEndDate",
                table: "AIQuotas",
                column: "PeriodEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotas_TierName",
                table: "AIQuotas",
                column: "TierName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIQuotas");
        }
    }
}
