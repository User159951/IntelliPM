using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMentionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mentions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    MentionedUserId = table.Column<int>(type: "int", nullable: false),
                    StartIndex = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: false),
                    MentionText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NotificationSent = table.Column<bool>(type: "bit", nullable: false),
                    NotificationSentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mentions_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mentions_Users_MentionedUserId",
                        column: x => x.MentionedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mentions_CommentId",
                table: "Mentions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Mentions_MentionedUserId",
                table: "Mentions",
                column: "MentionedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Mentions_NotificationSent_CreatedAt",
                table: "Mentions",
                columns: new[] { "NotificationSent", "CreatedAt" },
                filter: "[NotificationSent] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Mentions_OrganizationId",
                table: "Mentions",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mentions");
        }
    }
}
