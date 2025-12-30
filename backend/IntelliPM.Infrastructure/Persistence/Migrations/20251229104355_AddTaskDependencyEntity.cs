using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDependencyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskDependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceTaskId = table.Column<int>(type: "int", nullable: false),
                    DependentTaskId = table.Column<int>(type: "int", nullable: false),
                    DependencyType = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskDependencies_ProjectTasks_DependentTaskId",
                        column: x => x.DependentTaskId,
                        principalTable: "ProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskDependencies_ProjectTasks_SourceTaskId",
                        column: x => x.SourceTaskId,
                        principalTable: "ProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskDependencies_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskDependencies_CreatedById",
                table: "TaskDependencies",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDependencies_DependentTask_Organization",
                table: "TaskDependencies",
                columns: new[] { "DependentTaskId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskDependencies_DependentTaskId",
                table: "TaskDependencies",
                column: "DependentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDependencies_OrganizationId",
                table: "TaskDependencies",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDependencies_Source_Dependent_Type_Unique",
                table: "TaskDependencies",
                columns: new[] { "SourceTaskId", "DependentTaskId", "DependencyType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskDependencies_SourceTask_Organization",
                table: "TaskDependencies",
                columns: new[] { "SourceTaskId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskDependencies_SourceTaskId",
                table: "TaskDependencies",
                column: "SourceTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskDependencies");
        }
    }
}
