using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTeamEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AssignedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UnassignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTeams_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectTeams_Users_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_AssignedById",
                table: "ProjectTeams",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_IsActive",
                table: "ProjectTeams",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_OrganizationId",
                table: "ProjectTeams",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_ProjectId",
                table: "ProjectTeams",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_ProjectId_IsActive",
                table: "ProjectTeams",
                columns: new[] { "ProjectId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_ProjectId_TeamId",
                table: "ProjectTeams",
                columns: new[] { "ProjectId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_TeamId",
                table: "ProjectTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_TeamId_IsActive",
                table: "ProjectTeams",
                columns: new[] { "TeamId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTeams");
        }
    }
}
