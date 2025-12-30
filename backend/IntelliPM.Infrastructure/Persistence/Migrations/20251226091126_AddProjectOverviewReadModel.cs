using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectOverviewReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectOverviewReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProjectType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalMembers = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ActiveMembers = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TeamMembersJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    TotalSprints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ActiveSprintsCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CompletedSprintsCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CurrentSprintId = table.Column<int>(type: "int", nullable: true),
                    CurrentSprintName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TotalTasks = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CompletedTasks = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    InProgressTasks = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TodoTasks = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BlockedTasks = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OverdueTasks = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CompletedStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RemainingStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalDefects = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OpenDefects = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CriticalDefects = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AverageVelocity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    LastSprintVelocity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    VelocityTrendJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    ProjectHealth = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 100m),
                    HealthStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Unknown"),
                    RiskFactors = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    LastActivityAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActivitiesLast7Days = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ActivitiesLast30Days = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OverallProgress = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    SprintProgress = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    DaysUntilNextMilestone = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectOverviewReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectOverviewReadModels_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SprintSummaryReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SprintId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    SprintName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PlannedCapacity = table.Column<int>(type: "int", nullable: true),
                    TotalTasks = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CompletedTasks = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    InProgressTasks = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TodoTasks = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CompletedStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    InProgressStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RemainingStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CompletionPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    VelocityPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    CapacityUtilization = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    EstimatedDaysRemaining = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BurndownData = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    AverageVelocity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    IsOnTrack = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SprintSummaryReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SprintSummaryReadModels_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SprintSummaryReadModels_Sprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "Sprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskBoardReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    TodoCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    InProgressCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DoneCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalTaskCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TodoStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    InProgressStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DoneStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalStoryPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TodoTasks = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    InProgressTasks = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    DoneTasks = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskBoardReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskBoardReadModels_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOverviewReadModels_HealthStatus",
                table: "ProjectOverviewReadModels",
                column: "HealthStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOverviewReadModels_OrganizationId",
                table: "ProjectOverviewReadModels",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOverviewReadModels_OrganizationId_HealthStatus",
                table: "ProjectOverviewReadModels",
                columns: new[] { "OrganizationId", "HealthStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOverviewReadModels_OrganizationId_Status",
                table: "ProjectOverviewReadModels",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOverviewReadModels_ProjectId",
                table: "ProjectOverviewReadModels",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOverviewReadModels_Status",
                table: "ProjectOverviewReadModels",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SprintSummaryReadModels_OrganizationId",
                table: "SprintSummaryReadModels",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SprintSummaryReadModels_ProjectId",
                table: "SprintSummaryReadModels",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SprintSummaryReadModels_ProjectId_Status",
                table: "SprintSummaryReadModels",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SprintSummaryReadModels_SprintId",
                table: "SprintSummaryReadModels",
                column: "SprintId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SprintSummaryReadModels_Status",
                table: "SprintSummaryReadModels",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TaskBoardReadModels_OrganizationId",
                table: "TaskBoardReadModels",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskBoardReadModels_ProjectId",
                table: "TaskBoardReadModels",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskBoardReadModels_ProjectId_OrganizationId",
                table: "TaskBoardReadModels",
                columns: new[] { "ProjectId", "OrganizationId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectOverviewReadModels");

            migrationBuilder.DropTable(
                name: "SprintSummaryReadModels");

            migrationBuilder.DropTable(
                name: "TaskBoardReadModels");
        }
    }
}
