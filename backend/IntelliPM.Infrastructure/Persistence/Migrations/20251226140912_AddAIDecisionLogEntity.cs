using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAIDecisionLogEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIDecisionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    DecisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AgentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Question = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reasoning = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TokensUsed = table.Column<int>(type: "int", nullable: false),
                    PromptTokens = table.Column<int>(type: "int", nullable: false),
                    CompletionTokens = table.Column<int>(type: "int", nullable: false),
                    InputData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutputData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlternativesConsidered = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: false),
                    RequiresHumanApproval = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedByHuman = table.Column<bool>(type: "bit", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    WasApplied = table.Column<bool>(type: "bit", nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ActualOutcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExecutionTimeMs = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIDecisionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIDecisionLogs_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AIDecisionLogs_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_AgentType",
                table: "AIDecisionLogs",
                column: "AgentType");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_ApprovedByUserId",
                table: "AIDecisionLogs",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_CreatedAt",
                table: "AIDecisionLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_DecisionId",
                table: "AIDecisionLogs",
                column: "DecisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_DecisionType",
                table: "AIDecisionLogs",
                column: "DecisionType");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_EntityType_EntityId",
                table: "AIDecisionLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_Organization_CreatedAt",
                table: "AIDecisionLogs",
                columns: new[] { "OrganizationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_OrganizationId",
                table: "AIDecisionLogs",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_PendingApprovals",
                table: "AIDecisionLogs",
                columns: new[] { "RequiresHumanApproval", "ApprovedByHuman", "CreatedAt" },
                filter: "[RequiresHumanApproval] = 1 AND [ApprovedByHuman] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_RequestedByUserId",
                table: "AIDecisionLogs",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_Status",
                table: "AIDecisionLogs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIDecisionLogs");
        }
    }
}
