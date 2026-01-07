using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationIdToAgentExecutionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "AIDecisionLogs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostAccumulated",
                table: "AIDecisionLogs",
                type: "decimal(10,6)",
                precision: 10,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ExecutionStatus",
                table: "AIDecisionLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Success");

            migrationBuilder.AddColumn<string>(
                name: "AgentType",
                table: "AgentExecutionLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LinkedDecisionId",
                table: "AgentExecutionLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "AgentExecutionLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "AgentExecutionLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TokensUsed",
                table: "AgentExecutionLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_CorrelationId",
                table: "AIDecisionLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_ExecutionStatus",
                table: "AIDecisionLogs",
                column: "ExecutionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutionLogs_LinkedDecisionId",
                table: "AgentExecutionLogs",
                column: "LinkedDecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutionLogs_OrganizationId",
                table: "AgentExecutionLogs",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentExecutionLogs_AIDecisionLogs_LinkedDecisionId",
                table: "AgentExecutionLogs",
                column: "LinkedDecisionId",
                principalTable: "AIDecisionLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AgentExecutionLogs_Organizations_OrganizationId",
                table: "AgentExecutionLogs",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentExecutionLogs_AIDecisionLogs_LinkedDecisionId",
                table: "AgentExecutionLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AgentExecutionLogs_Organizations_OrganizationId",
                table: "AgentExecutionLogs");

            migrationBuilder.DropIndex(
                name: "IX_AIDecisionLogs_CorrelationId",
                table: "AIDecisionLogs");

            migrationBuilder.DropIndex(
                name: "IX_AIDecisionLogs_ExecutionStatus",
                table: "AIDecisionLogs");

            migrationBuilder.DropIndex(
                name: "IX_AgentExecutionLogs_LinkedDecisionId",
                table: "AgentExecutionLogs");

            migrationBuilder.DropIndex(
                name: "IX_AgentExecutionLogs_OrganizationId",
                table: "AgentExecutionLogs");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "AIDecisionLogs");

            migrationBuilder.DropColumn(
                name: "CostAccumulated",
                table: "AIDecisionLogs");

            migrationBuilder.DropColumn(
                name: "ExecutionStatus",
                table: "AIDecisionLogs");

            migrationBuilder.DropColumn(
                name: "AgentType",
                table: "AgentExecutionLogs");

            migrationBuilder.DropColumn(
                name: "LinkedDecisionId",
                table: "AgentExecutionLogs");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AgentExecutionLogs");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "AgentExecutionLogs");

            migrationBuilder.DropColumn(
                name: "TokensUsed",
                table: "AgentExecutionLogs");
        }
    }
}
