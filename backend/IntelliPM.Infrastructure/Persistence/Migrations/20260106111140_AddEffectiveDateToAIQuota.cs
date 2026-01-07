using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEffectiveDateToAIQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EffectiveDate",
                table: "AIQuotas",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotas_EffectiveDate",
                table: "AIQuotas",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotas_Scheduled",
                table: "AIQuotas",
                columns: new[] { "IsActive", "EffectiveDate" },
                filter: "[IsActive] = 0 AND [EffectiveDate] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AIQuotas_EffectiveDate",
                table: "AIQuotas");

            migrationBuilder.DropIndex(
                name: "IX_AIQuotas_Scheduled",
                table: "AIQuotas");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "AIQuotas");
        }
    }
}
