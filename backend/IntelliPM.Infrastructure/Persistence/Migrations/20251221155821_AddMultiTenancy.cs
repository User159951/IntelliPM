using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Teams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Sprints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "ProjectTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Invitations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Defects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            // Create default organization and update existing data
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Organizations ON;
                INSERT INTO Organizations (Id, Name, CreatedAt)
                VALUES (1, 'Default Organization', GETUTCDATE());
                SET IDENTITY_INSERT Organizations OFF;
                
                UPDATE Users SET OrganizationId = 1 WHERE OrganizationId = 0;
                UPDATE Projects SET OrganizationId = 1 WHERE OrganizationId = 0;
                UPDATE Teams SET OrganizationId = 1 WHERE OrganizationId = 0;
                UPDATE Sprints SET OrganizationId = 1 WHERE OrganizationId = 0;
                UPDATE ProjectTasks SET OrganizationId = 1 WHERE OrganizationId = 0;
                UPDATE Defects SET OrganizationId = 1 WHERE OrganizationId = 0;
                UPDATE Notifications SET OrganizationId = 1 WHERE OrganizationId = 0;
                UPDATE Invitations SET OrganizationId = 1 WHERE OrganizationId = 0;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_OrganizationId",
                table: "Teams",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_OrganizationId",
                table: "Sprints",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_OrganizationId",
                table: "ProjectTasks",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId",
                table: "Projects",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_OrganizationId",
                table: "Notifications",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_OrganizationId",
                table: "Invitations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Defects_OrganizationId",
                table: "Defects",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Defects_Organizations_OrganizationId",
                table: "Defects",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Organizations_OrganizationId",
                table: "Invitations",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Organizations_OrganizationId",
                table: "Notifications",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organizations_OrganizationId",
                table: "Projects",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_Organizations_OrganizationId",
                table: "ProjectTasks",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sprints_Organizations_OrganizationId",
                table: "Sprints",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Organizations_OrganizationId",
                table: "Teams",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Defects_Organizations_OrganizationId",
                table: "Defects");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Organizations_OrganizationId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Organizations_OrganizationId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organizations_OrganizationId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_Organizations_OrganizationId",
                table: "ProjectTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Sprints_Organizations_OrganizationId",
                table: "Sprints");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Organizations_OrganizationId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Teams_OrganizationId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Sprints_OrganizationId",
                table: "Sprints");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_OrganizationId",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganizationId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_OrganizationId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_OrganizationId",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_Defects_OrganizationId",
                table: "Defects");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Defects");
        }
    }
}
