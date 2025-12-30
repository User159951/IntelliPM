using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPermissionsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_Users_UserId",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "Roles",
                table: "ProjectMembers");

            migrationBuilder.AddColumn<int>(
                name: "GlobalRole",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvitedAt",
                table: "ProjectMembers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "InvitedById",
                table: "ProjectMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "ProjectMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GlobalRole = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invitations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invitations_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_InvitedById",
                table: "ProjectMembers",
                column: "InvitedById");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_CreatedById",
                table: "Invitations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_Email_IsUsed_ExpiresAt",
                table: "Invitations",
                columns: new[] { "Email", "IsUsed", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_ProjectId",
                table: "Invitations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_Token",
                table: "Invitations",
                column: "Token",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_Users_InvitedById",
                table: "ProjectMembers",
                column: "InvitedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_Users_UserId",
                table: "ProjectMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_Users_InvitedById",
                table: "ProjectMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_Users_UserId",
                table: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_InvitedById",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "GlobalRole",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InvitedAt",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "InvitedById",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ProjectMembers");

            migrationBuilder.AddColumn<string>(
                name: "Roles",
                table: "ProjectMembers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_Users_UserId",
                table: "ProjectMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
