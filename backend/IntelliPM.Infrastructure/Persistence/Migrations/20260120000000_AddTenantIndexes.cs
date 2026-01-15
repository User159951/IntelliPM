using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Composite indexes for ProjectTask (multi-tenant queries)
            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_OrganizationId_CreatedAt",
                table: "ProjectTasks",
                columns: new[] { "OrganizationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_OrganizationId_ProjectId",
                table: "ProjectTasks",
                columns: new[] { "OrganizationId", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_OrganizationId_AssigneeId",
                table: "ProjectTasks",
                columns: new[] { "OrganizationId", "AssigneeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_OrganizationId_Status",
                table: "ProjectTasks",
                columns: new[] { "OrganizationId", "Status" });

            // Single-column indexes for sorting on ProjectTask
            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_CreatedAt",
                table: "ProjectTasks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_UpdatedAt",
                table: "ProjectTasks",
                column: "UpdatedAt");

            // Composite index for Project (multi-tenant queries)
            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId_CreatedAt",
                table: "Projects",
                columns: new[] { "OrganizationId", "CreatedAt" });

            // Single-column indexes for sorting on Project
            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedAt",
                table: "Projects",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UpdatedAt",
                table: "Projects",
                column: "UpdatedAt");

            // Single-column index for sorting on User
            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop User index
            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            // Drop Project indexes
            migrationBuilder.DropIndex(
                name: "IX_Projects_UpdatedAt",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatedAt",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganizationId_CreatedAt",
                table: "Projects");

            // Drop ProjectTask indexes
            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_UpdatedAt",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_CreatedAt",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_OrganizationId_Status",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_OrganizationId_AssigneeId",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_OrganizationId_ProjectId",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_OrganizationId_CreatedAt",
                table: "ProjectTasks");
        }
    }
}
