using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationIdToTenantEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIQuotas_Organizations_OrganizationId",
                table: "AIQuotas");

            migrationBuilder.DropIndex(
                name: "IX_AIDecisionLogs_PendingApprovals",
                table: "AIDecisionLogs");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Risks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocking",
                table: "QualityGates",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "QualityGates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Insights",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "DocumentStores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "BacklogItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Alerts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "TierName",
                table: "AIQuotas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Free");

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "AIQuotas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovalDeadline",
                table: "AIDecisionLogs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RejectedAt",
                table: "AIDecisionLogs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedByUserId",
                table: "AIDecisionLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusString",
                table: "AIDecisionLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Activities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Update existing records to use default organization (1) before adding foreign key constraints
            migrationBuilder.Sql(@"
                UPDATE Activities SET OrganizationId = 1 WHERE OrganizationId = 0;
                UPDATE Alerts SET OrganizationId = (SELECT OrganizationId FROM Projects WHERE Projects.Id = Alerts.ProjectId) WHERE OrganizationId = 0;
                UPDATE AuditLogs SET OrganizationId = COALESCE((SELECT OrganizationId FROM Users WHERE Users.Id = AuditLogs.UserId), 1) WHERE OrganizationId = 0;
                UPDATE DocumentStores SET OrganizationId = (SELECT OrganizationId FROM Projects WHERE Projects.Id = DocumentStores.ProjectId) WHERE OrganizationId = 0;
                UPDATE Insights SET OrganizationId = (SELECT OrganizationId FROM Projects WHERE Projects.Id = Insights.ProjectId) WHERE OrganizationId = 0;
                UPDATE QualityGates SET OrganizationId = (SELECT OrganizationId FROM Releases WHERE Releases.Id = QualityGates.ReleaseId) WHERE OrganizationId = 0;
                UPDATE Risks SET OrganizationId = (SELECT OrganizationId FROM Projects WHERE Projects.Id = Risks.ProjectId) WHERE OrganizationId = 0;
                UPDATE BacklogItem SET OrganizationId = (SELECT OrganizationId FROM Projects WHERE Projects.Id = BacklogItem.ProjectId) WHERE OrganizationId = 0;
            ");

            migrationBuilder.CreateTable(
                name: "AIDecisionApprovalPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    DecisionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiredRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsBlockingIfNotApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIDecisionApprovalPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIDecisionApprovalPolicies_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "RBACPolicyVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VersionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AppliedByUserId = table.Column<int>(type: "int", nullable: true),
                    PermissionsSnapshotJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    RolePermissionsSnapshotJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RBACPolicyVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RBACPolicyVersions_Users_AppliedByUserId",
                        column: x => x.AppliedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeedHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RecordsAffected = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WasAllowed = table.Column<bool>(type: "bit", nullable: false),
                    DenialReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitionAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitionAuditLogs_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitionAuditLogs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitionAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AllowedRolesJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RequiredConditionsJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitionRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Risks_OrganizationId",
                table: "Risks",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityGates_OrganizationId",
                table: "QualityGates",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Insights_OrganizationId",
                table: "Insights",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentStores_OrganizationId",
                table: "DocumentStores",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogItem_OrganizationId",
                table: "BacklogItem",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OrganizationId",
                table: "AuditLogs",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_OrganizationId",
                table: "Alerts",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotas_TemplateId",
                table: "AIQuotas",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_ApprovalDeadline_Status",
                table: "AIDecisionLogs",
                columns: new[] { "ApprovalDeadline", "Status" },
                filter: "[ApprovalDeadline] IS NOT NULL AND [Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_PendingApprovals",
                table: "AIDecisionLogs",
                columns: new[] { "RequiresHumanApproval", "Status", "CreatedAt" },
                filter: "[RequiresHumanApproval] = 1 AND [Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_RejectedByUserId",
                table: "AIDecisionLogs",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_OrganizationId",
                table: "Activities",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionApprovalPolicies_DecisionType",
                table: "AIDecisionApprovalPolicies",
                column: "DecisionType");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionApprovalPolicies_IsActive",
                table: "AIDecisionApprovalPolicies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionApprovalPolicies_Org_DecisionType_Active",
                table: "AIDecisionApprovalPolicies",
                columns: new[] { "OrganizationId", "DecisionType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionApprovalPolicies_RequiredRole",
                table: "AIDecisionApprovalPolicies",
                column: "RequiredRole");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotaTemplates_Active_DisplayOrder",
                table: "AIQuotaTemplates",
                columns: new[] { "IsActive", "DisplayOrder" },
                filter: "[IsActive] = 1 AND [DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuotaTemplates_TierName",
                table: "AIQuotaTemplates",
                column: "TierName",
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSettings_OrganizationId_Key",
                table: "OrganizationSettings",
                columns: new[] { "OrganizationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSettings_UpdatedById",
                table: "OrganizationSettings",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RBACPolicyVersions_AppliedAt",
                table: "RBACPolicyVersions",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RBACPolicyVersions_AppliedByUserId",
                table: "RBACPolicyVersions",
                column: "AppliedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RBACPolicyVersions_IsActive",
                table: "RBACPolicyVersions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RBACPolicyVersions_VersionNumber",
                table: "RBACPolicyVersions",
                column: "VersionNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SeedHistories_AppliedAt",
                table: "SeedHistories",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SeedHistories_SeedName",
                table: "SeedHistories",
                column: "SeedName");

            migrationBuilder.CreateIndex(
                name: "IX_SeedHistories_SeedName_Version",
                table: "SeedHistories",
                columns: new[] { "SeedName", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeedHistories_Success",
                table: "SeedHistories",
                column: "Success");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitionAuditLogs_AttemptedAt",
                table: "WorkflowTransitionAuditLogs",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitionAuditLogs_Entity",
                table: "WorkflowTransitionAuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitionAuditLogs_OrganizationId",
                table: "WorkflowTransitionAuditLogs",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitionAuditLogs_ProjectId",
                table: "WorkflowTransitionAuditLogs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitionAuditLogs_UserId",
                table: "WorkflowTransitionAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitionAuditLogs_WasAllowed",
                table: "WorkflowTransitionAuditLogs",
                column: "WasAllowed");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitionRules_EntityType",
                table: "WorkflowTransitionRules",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitionRules_Lookup",
                table: "WorkflowTransitionRules",
                columns: new[] { "EntityType", "FromStatus", "ToStatus", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Organizations_OrganizationId",
                table: "Activities",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AIDecisionLogs_Users_RejectedByUserId",
                table: "AIDecisionLogs",
                column: "RejectedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AIQuotas_AIQuotaTemplates_TemplateId",
                table: "AIQuotas",
                column: "TemplateId",
                principalTable: "AIQuotaTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AIQuotas_Organizations_OrganizationId",
                table: "AIQuotas",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Organizations_OrganizationId",
                table: "Alerts",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Organizations_OrganizationId",
                table: "AuditLogs",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BacklogItem_Organizations_OrganizationId",
                table: "BacklogItem",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentStores_Organizations_OrganizationId",
                table: "DocumentStores",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Insights_Organizations_OrganizationId",
                table: "Insights",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityGates_Organizations_OrganizationId",
                table: "QualityGates",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_Organizations_OrganizationId",
                table: "Risks",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Organizations_OrganizationId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_AIDecisionLogs_Users_RejectedByUserId",
                table: "AIDecisionLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AIQuotas_AIQuotaTemplates_TemplateId",
                table: "AIQuotas");

            migrationBuilder.DropForeignKey(
                name: "FK_AIQuotas_Organizations_OrganizationId",
                table: "AIQuotas");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Organizations_OrganizationId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Organizations_OrganizationId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_BacklogItem_Organizations_OrganizationId",
                table: "BacklogItem");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentStores_Organizations_OrganizationId",
                table: "DocumentStores");

            migrationBuilder.DropForeignKey(
                name: "FK_Insights_Organizations_OrganizationId",
                table: "Insights");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityGates_Organizations_OrganizationId",
                table: "QualityGates");

            migrationBuilder.DropForeignKey(
                name: "FK_Risks_Organizations_OrganizationId",
                table: "Risks");

            migrationBuilder.DropTable(
                name: "AIDecisionApprovalPolicies");

            migrationBuilder.DropTable(
                name: "AIQuotaTemplates");

            migrationBuilder.DropTable(
                name: "OrganizationSettings");

            migrationBuilder.DropTable(
                name: "RBACPolicyVersions");

            migrationBuilder.DropTable(
                name: "SeedHistories");

            migrationBuilder.DropTable(
                name: "WorkflowTransitionAuditLogs");

            migrationBuilder.DropTable(
                name: "WorkflowTransitionRules");

            migrationBuilder.DropIndex(
                name: "IX_Risks_OrganizationId",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_QualityGates_OrganizationId",
                table: "QualityGates");

            migrationBuilder.DropIndex(
                name: "IX_Insights_OrganizationId",
                table: "Insights");

            migrationBuilder.DropIndex(
                name: "IX_DocumentStores_OrganizationId",
                table: "DocumentStores");

            migrationBuilder.DropIndex(
                name: "IX_BacklogItem_OrganizationId",
                table: "BacklogItem");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_OrganizationId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_OrganizationId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_AIQuotas_TemplateId",
                table: "AIQuotas");

            migrationBuilder.DropIndex(
                name: "IX_AIDecisionLogs_ApprovalDeadline_Status",
                table: "AIDecisionLogs");

            migrationBuilder.DropIndex(
                name: "IX_AIDecisionLogs_PendingApprovals",
                table: "AIDecisionLogs");

            migrationBuilder.DropIndex(
                name: "IX_AIDecisionLogs_RejectedByUserId",
                table: "AIDecisionLogs");

            migrationBuilder.DropIndex(
                name: "IX_Activities_OrganizationId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "IsBlocking",
                table: "QualityGates");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "QualityGates");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Insights");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "DocumentStores");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "BacklogItem");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "AIQuotas");

            migrationBuilder.DropColumn(
                name: "ApprovalDeadline",
                table: "AIDecisionLogs");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "AIDecisionLogs");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                table: "AIDecisionLogs");

            migrationBuilder.DropColumn(
                name: "StatusString",
                table: "AIDecisionLogs");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Activities");

            migrationBuilder.AlterColumn<string>(
                name: "TierName",
                table: "AIQuotas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Free",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_AIDecisionLogs_PendingApprovals",
                table: "AIDecisionLogs",
                columns: new[] { "RequiresHumanApproval", "ApprovedByHuman", "CreatedAt" },
                filter: "[RequiresHumanApproval] = 1 AND [ApprovedByHuman] IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AIQuotas_Organizations_OrganizationId",
                table: "AIQuotas",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
