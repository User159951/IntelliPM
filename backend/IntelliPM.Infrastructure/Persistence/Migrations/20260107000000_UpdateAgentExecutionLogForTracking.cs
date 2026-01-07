using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAgentExecutionLogForTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentType",
                table: "AgentExecutionLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<int>(
                name: "LinkedDecisionId",
                table: "AgentExecutionLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutionLogs_LinkedDecisionId",
                table: "AgentExecutionLogs",
                column: "LinkedDecisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentExecutionLogs_AIDecisionLogs_LinkedDecisionId",
                table: "AgentExecutionLogs",
                column: "LinkedDecisionId",
                principalTable: "AIDecisionLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentExecutionLogs_AIDecisionLogs_LinkedDecisionId",
                table: "AgentExecutionLogs");

            migrationBuilder.DropIndex(
                name: "IX_AgentExecutionLogs_LinkedDecisionId",
                table: "AgentExecutionLogs");

            migrationBuilder.DropColumn(
                name: "AgentType",
                table: "AgentExecutionLogs");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "AgentExecutionLogs");

            migrationBuilder.DropColumn(
                name: "TokensUsed",
                table: "AgentExecutionLogs");

            migrationBuilder.DropColumn(
                name: "LinkedDecisionId",
                table: "AgentExecutionLogs");
        }
    }
}

