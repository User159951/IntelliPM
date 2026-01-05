using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationAndUserAIQuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationAIQuotas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    MonthlyTokenLimit = table.Column<long>(type: "bigint", nullable: false),
                    MonthlyRequestLimit = table.Column<int>(type: "int", nullable: true),
                    ResetDayOfMonth = table.Column<int>(type: "int", nullable: true),
                    IsAIEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationAIQuotas", x => x.Id);
                    table.CheckConstraint("CK_OrganizationAIQuotas_ResetDayOfMonth", "[ResetDayOfMonth] IS NULL OR ([ResetDayOfMonth] >= 1 AND [ResetDayOfMonth] <= 31)");
                    table.ForeignKey(
                        name: "FK_OrganizationAIQuotas_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserAIQuotas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    MonthlyTokenLimitOverride = table.Column<long>(type: "bigint", nullable: true),
                    MonthlyRequestLimitOverride = table.Column<int>(type: "int", nullable: true),
                    IsAIEnabledOverride = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAIQuotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAIQuotas_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAIQuotas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAIQuotas_OrganizationId",
                table: "OrganizationAIQuotas",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAIQuotas_OrganizationId",
                table: "UserAIQuotas",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAIQuotas_UserId",
                table: "UserAIQuotas",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationAIQuotas");

            migrationBuilder.DropTable(
                name: "UserAIQuotas");
        }
    }
}
