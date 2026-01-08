using System;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.Json;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPermissionPoliciesForOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL to seed permission policies for all existing organizations
            // This migration ensures all organizations have an active policy with all permissions allowed
            // to maintain backward compatibility during the transition to deny-by-default security model
            
            var sql = @"
                -- Build JSON array of all permission names
                DECLARE @PermissionJson NVARCHAR(MAX);
                SELECT @PermissionJson = '[' + STUFF((
                    SELECT ',' + '""' + Name + '""'
                    FROM Permissions
                    ORDER BY Name
                    FOR XML PATH(''), TYPE
                ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') + ']';

                -- Create permission policies for all organizations that don't have one
                INSERT INTO OrganizationPermissionPolicies (OrganizationId, AllowedPermissionsJson, IsActive, CreatedAt)
                SELECT 
                    o.Id AS OrganizationId,
                    @PermissionJson AS AllowedPermissionsJson,
                    1 AS IsActive,
                    GETUTCDATE() AS CreatedAt
                FROM Organizations o
                WHERE NOT EXISTS (
                    SELECT 1 
                    FROM OrganizationPermissionPolicies opp 
                    WHERE opp.OrganizationId = o.Id
                );
            ";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all permission policies created by this migration
            // Note: This will only remove policies created at the exact timestamp of this migration
            // In practice, you may want to manually review which policies to remove
            migrationBuilder.Sql(@"
                DELETE FROM OrganizationPermissionPolicies
                WHERE CreatedAt >= '2026-01-08 00:00:00'
                AND CreatedAt < '2026-01-09 00:00:00';
            ");
        }
    }
}

