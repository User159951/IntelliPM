using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationSettingsAndSeedBusinessConstants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create OrganizationSettings table
            migrationBuilder.CreateTable(
                name: "OrganizationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    UpdatedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationSettings_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationSettings_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSettings_OrganizationId_Key",
                table: "OrganizationSettings",
                columns: new[] { "OrganizationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSettings_UpdatedById",
                table: "OrganizationSettings",
                column: "UpdatedById");

            // Seed initial business constants as GlobalSettings using SQL
            migrationBuilder.Sql(@"
                -- Sprint Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('Sprint.MinDuration', '1', 'Minimum sprint duration in days', 'Business', GETUTCDATE()),
                    ('Sprint.MaxDuration', '30', 'Maximum sprint duration in days', 'Business', GETUTCDATE()),
                    ('Sprint.GoalMaxLength', '500', 'Maximum length for sprint goal', 'Business', GETUTCDATE()),
                    ('Sprint.MinCapacity', '1', 'Minimum sprint capacity', 'Business', GETUTCDATE()),
                    ('Sprint.MaxCapacity', '1000', 'Maximum sprint capacity', 'Business', GETUTCDATE());

                -- Attachment Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('Attachment.MaxFileSizeBytes', '10485760', 'Maximum file size in bytes (10 MB)', 'Business', GETUTCDATE()),
                    ('Attachment.MaxTotalSizePerEntity', '52428800', 'Maximum total size per entity in bytes (50 MB)', 'Business', GETUTCDATE());

                -- Project Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('Project.NameMaxLength', '200', 'Maximum length for project name', 'Business', GETUTCDATE()),
                    ('Project.DescriptionMaxLength', '2000', 'Maximum length for project description', 'Business', GETUTCDATE()),
                    ('Project.MinSprintDuration', '1', 'Minimum sprint duration in days', 'Business', GETUTCDATE()),
                    ('Project.MaxSprintDuration', '30', 'Maximum sprint duration in days', 'Business', GETUTCDATE());

                -- Task Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('Task.TitleMaxLength', '200', 'Maximum length for task title', 'Business', GETUTCDATE()),
                    ('Task.DescriptionMaxLength', '5000', 'Maximum length for task description', 'Business', GETUTCDATE()),
                    ('Task.MinStoryPoints', '0', 'Minimum story points', 'Business', GETUTCDATE()),
                    ('Task.MaxStoryPoints', '100', 'Maximum story points', 'Business', GETUTCDATE());

                -- Team Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('Team.NameMaxLength', '200', 'Maximum length for team name', 'Business', GETUTCDATE()),
                    ('Team.MinCapacity', '1', 'Minimum team capacity', 'Business', GETUTCDATE()),
                    ('Team.MaxCapacity', '10000', 'Maximum team capacity', 'Business', GETUTCDATE());

                -- Organization Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('Organization.MaxNameLength', '200', 'Maximum length for organization name', 'Business', GETUTCDATE()),
                    ('Organization.MinNameLength', '2', 'Minimum length for organization name', 'Business', GETUTCDATE());

                -- Comment Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('Comment.MaxContentLength', '5000', 'Maximum length for comment content', 'Business', GETUTCDATE()),
                    ('Comment.MaxNestingLevel', '3', 'Maximum nesting level for comment replies', 'Business', GETUTCDATE());

                -- Milestone Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('Milestone.MaxNameLength', '200', 'Maximum length for milestone name', 'Business', GETUTCDATE()),
                    ('Milestone.MaxDescriptionLength', '1000', 'Maximum length for milestone description', 'Business', GETUTCDATE()),
                    ('Milestone.MinProgress', '0', 'Minimum progress value (0%)', 'Business', GETUTCDATE()),
                    ('Milestone.MaxProgress', '100', 'Maximum progress value (100%)', 'Business', GETUTCDATE()),
                    ('Milestone.DefaultProgress', '0', 'Default progress value when milestone is created', 'Business', GETUTCDATE());

                -- Release Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('Release.MaxNameLength', '200', 'Maximum length for release name', 'Business', GETUTCDATE()),
                    ('Release.MaxVersionLength', '50', 'Maximum length for release version', 'Business', GETUTCDATE()),
                    ('Release.MaxDescriptionLength', '2000', 'Maximum length for release description', 'Business', GETUTCDATE()),
                    ('Release.MaxReleaseNotesLength', '5000', 'Maximum length for release notes', 'Business', GETUTCDATE()),
                    ('Release.MaxChangeLogLength', '5000', 'Maximum length for change log', 'Business', GETUTCDATE()),
                    ('Release.MaxTagNameLength', '100', 'Maximum length for tag name', 'Business', GETUTCDATE()),
                    ('Release.MinCodeCoverageThreshold', '80', 'Minimum code coverage threshold (80%)', 'Business', GETUTCDATE()),
                    ('Release.MaxOpenCriticalBugs', '0', 'Maximum number of open critical bugs allowed', 'Business', GETUTCDATE()),
                    ('Release.MaxOpenHighPriorityBugs', '3', 'Maximum number of open high priority bugs allowed', 'Business', GETUTCDATE());

                -- AI Quota Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('AIQuota.QuotaPeriodDays', '30', 'Number of days in a quota period (monthly)', 'Business', GETUTCDATE()),
                    ('AIQuota.DefaultAlertThreshold', '80', 'Default alert threshold percentage (80%)', 'Business', GETUTCDATE()),
                    ('AIQuota.CostPerToken', '0.00001', 'Cost per token ($0.01 per 1000 tokens)', 'Business', GETUTCDATE()),
                    ('AIQuota.Free.MaxTokensPerPeriod', '100000', 'Free tier: Maximum tokens per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Free.MaxRequestsPerPeriod', '100', 'Free tier: Maximum requests per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Free.MaxDecisionsPerPeriod', '50', 'Free tier: Maximum decisions per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Free.MaxCostPerPeriod', '0', 'Free tier: Maximum cost per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Free.AllowOverage', 'false', 'Free tier: Allow overage', 'Business', GETUTCDATE()),
                    ('AIQuota.Pro.MaxTokensPerPeriod', '1000000', 'Pro tier: Maximum tokens per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Pro.MaxRequestsPerPeriod', '1000', 'Pro tier: Maximum requests per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Pro.MaxDecisionsPerPeriod', '500', 'Pro tier: Maximum decisions per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Pro.MaxCostPerPeriod', '100', 'Pro tier: Maximum cost per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Pro.AllowOverage', 'true', 'Pro tier: Allow overage', 'Business', GETUTCDATE()),
                    ('AIQuota.Pro.OverageRate', '0.00002', 'Pro tier: Overage rate ($0.02 per 1000 tokens)', 'Business', GETUTCDATE()),
                    ('AIQuota.Enterprise.MaxTokensPerPeriod', '10000000', 'Enterprise tier: Maximum tokens per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Enterprise.MaxRequestsPerPeriod', '10000', 'Enterprise tier: Maximum requests per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Enterprise.MaxDecisionsPerPeriod', '5000', 'Enterprise tier: Maximum decisions per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Enterprise.MaxCostPerPeriod', '1000', 'Enterprise tier: Maximum cost per period', 'Business', GETUTCDATE()),
                    ('AIQuota.Enterprise.AllowOverage', 'true', 'Enterprise tier: Allow overage', 'Business', GETUTCDATE()),
                    ('AIQuota.Enterprise.OverageRate', '0.00001', 'Enterprise tier: Overage rate ($0.01 per 1000 tokens)', 'Business', GETUTCDATE());

                -- AI Decision Constants
                INSERT INTO GlobalSettings (Key, Value, Description, Category, CreatedAt)
                VALUES 
                    ('AIDecision.MinConfidenceScore', '0.7', 'Minimum confidence score for auto-applying decisions without human approval', 'Business', GETUTCDATE()),
                    ('AIDecision.MaxReasoningLength', '10000', 'Maximum length for reasoning text', 'Business', GETUTCDATE());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded data (optional - you may want to keep it)
            migrationBuilder.Sql(@"
                DELETE FROM GlobalSettings 
                WHERE Category = 'Business'
            ");

            migrationBuilder.DropTable(
                name: "OrganizationSettings");
        }
    }
}

