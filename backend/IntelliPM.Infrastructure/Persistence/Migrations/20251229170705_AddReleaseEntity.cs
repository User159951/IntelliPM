using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReleaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReleaseId",
                table: "Sprints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextMilestoneDueDate",
                table: "ProjectOverviewReadModels",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextMilestoneName",
                table: "ProjectOverviewReadModels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpcomingMilestonesCount",
                table: "ProjectOverviewReadModels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Releases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PlannedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualReleaseDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReleaseNotes = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    ChangeLog = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    IsPreRelease = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TagName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    ReleasedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Releases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Releases_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Releases_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Releases_Users_ReleasedById",
                        column: x => x.ReleasedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_ReleaseId",
                table: "Sprints",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_CreatedById",
                table: "Releases",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_OrganizationId",
                table: "Releases",
                column: "OrganizationId",
                filter: "[OrganizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_PlannedDate",
                table: "Releases",
                column: "PlannedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_Project_Status",
                table: "Releases",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Releases_Project_Version_Unique",
                table: "Releases",
                columns: new[] { "ProjectId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Releases_ProjectId",
                table: "Releases",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_ReleasedById",
                table: "Releases",
                column: "ReleasedById");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_Status",
                table: "Releases",
                column: "Status",
                filter: "[Status] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_Version",
                table: "Releases",
                column: "Version");

            migrationBuilder.AddForeignKey(
                name: "FK_Sprints_Releases_ReleaseId",
                table: "Sprints",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sprints_Releases_ReleaseId",
                table: "Sprints");

            migrationBuilder.DropTable(
                name: "Releases");

            migrationBuilder.DropIndex(
                name: "IX_Sprints_ReleaseId",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "ReleaseId",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "NextMilestoneDueDate",
                table: "ProjectOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "NextMilestoneName",
                table: "ProjectOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "UpcomingMilestonesCount",
                table: "ProjectOverviewReadModels");
        }
    }
}
