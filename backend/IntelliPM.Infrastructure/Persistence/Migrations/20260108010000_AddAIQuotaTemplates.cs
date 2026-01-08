using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAIQuotaTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create AIQuotaTemplates table
            migrationBuilder.CreateTable(
                name: "AIQuotaTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TierName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsSystemTemplate = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MaxTokensPerPeriod = table.Column<int>(type: "int", nullable: false),
                    MaxRequestsPerPeriod = table.Column<int>(type: "int", nullable: false),
                    MaxDecisionsPerPeriod = table.Column<int>(type: "int", nullable: false),
                    MaxCostPerPeriod = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AllowOverage = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OverageRate = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    DefaultAlertThresholdPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 80m),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIQuotaTemplates", x => x.Id);
                });

            // Create unique index on TierName (when not deleted)
            migrationBuilder.CreateIndex(
                name: "IX_AIQuotaTemplates_TierName",
                table: "AIQuotaTemplates",
                column: "TierName",
                unique: true,
                filter: "[DeletedAt] IS NULL");

            // Create index for active templates
            migrationBuilder.CreateIndex(
                name: "IX_AIQuotaTemplates_Active_DisplayOrder",
                table: "AIQuotaTemplates",
                columns: new[] { "IsActive", "DisplayOrder" },
                filter: "[IsActive] = 1 AND [DeletedAt] IS NULL");

            // Seed default templates
            var now = DateTimeOffset.UtcNow;
            
            migrationBuilder.InsertData(
                table: "AIQuotaTemplates",
                columns: new[] { "TierName", "Description", "IsActive", "IsSystemTemplate", "MaxTokensPerPeriod", "MaxRequestsPerPeriod", "MaxDecisionsPerPeriod", "MaxCostPerPeriod", "AllowOverage", "OverageRate", "DefaultAlertThresholdPercentage", "DisplayOrder", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { "Free", "Free tier for new users and organizations. Basic AI features with limited usage.", true, true, 100_000, 100, 50, 0m, false, 0m, 80m, 1, now, now },
                    { "Pro", "Professional tier for growing teams. Enhanced AI capabilities with higher limits.", true, true, 1_000_000, 1000, 500, 100m, true, 0.00002m, 80m, 2, now, now },
                    { "Enterprise", "Enterprise tier for large organizations. Unlimited scale with premium support.", true, true, 10_000_000, 10000, 5000, 1000m, true, 0.00001m, 80m, 3, now, now },
                    { "Disabled", "Disabled tier for organizations with AI features disabled. All limits set to zero.", true, true, 0, 0, 0, 0m, false, 0m, 80m, 999, now, now }
                });

            // Add TemplateId column to AIQuotas table (nullable initially for existing data)
            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "AIQuotas",
                type: "int",
                nullable: true);

            // Create index for TemplateId
            migrationBuilder.CreateIndex(
                name: "IX_AIQuotas_TemplateId",
                table: "AIQuotas",
                column: "TemplateId");

            // Update existing quotas to reference Free template by default
            migrationBuilder.Sql(@"
                UPDATE AIQuotas 
                SET TemplateId = (SELECT Id FROM AIQuotaTemplates WHERE TierName = 'Free' AND DeletedAt IS NULL)
                WHERE TemplateId IS NULL");

            // Now make TemplateId required
            migrationBuilder.AlterColumn<int>(
                name: "TemplateId",
                table: "AIQuotas",
                type: "int",
                nullable: false);

            // Remove default value from TierName (it should come from template)
            migrationBuilder.AlterColumn<string>(
                name: "TierName",
                table: "AIQuotas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldDefaultValue: "Free",
                defaultValue: null);

            // Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_AIQuotas_AIQuotaTemplates_TemplateId",
                table: "AIQuotas",
                column: "TemplateId",
                principalTable: "AIQuotaTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Update existing quotas to sync TierName from template
            migrationBuilder.Sql(@"
                UPDATE q
                SET q.TierName = t.TierName
                FROM AIQuotas q
                INNER JOIN AIQuotaTemplates t ON q.TemplateId = t.Id
                WHERE q.TierName != t.TierName OR q.TierName IS NULL");

            // Update Organization delete behavior to Restrict (it was Cascade, but now we need Restrict for FK cycles)
            migrationBuilder.DropForeignKey(
                name: "FK_AIQuotas_Organizations_OrganizationId",
                table: "AIQuotas");

            migrationBuilder.AddForeignKey(
                name: "FK_AIQuotas_Organizations_OrganizationId",
                table: "AIQuotas",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore Organization FK to Cascade
            migrationBuilder.DropForeignKey(
                name: "FK_AIQuotas_Organizations_OrganizationId",
                table: "AIQuotas");

            migrationBuilder.AddForeignKey(
                name: "FK_AIQuotas_Organizations_OrganizationId",
                table: "AIQuotas",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Drop FK to templates
            migrationBuilder.DropForeignKey(
                name: "FK_AIQuotas_AIQuotaTemplates_TemplateId",
                table: "AIQuotas");

            // Drop TemplateId index
            migrationBuilder.DropIndex(
                name: "IX_AIQuotas_TemplateId",
                table: "AIQuotas");

            // Restore default value for TierName
            migrationBuilder.AlterColumn<string>(
                name: "TierName",
                table: "AIQuotas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Free");

            // Drop TemplateId column
            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "AIQuotas");

            // Drop template table
            migrationBuilder.DropTable(
                name: "AIQuotaTemplates");
        }
    }
}

