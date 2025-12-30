using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hash pré-calculé pour le mot de passe "Admin@123456"
            // Généré le 2024-12-24 via Generate-AdminHash.ps1
            var passwordHash = "biHzOObpuGaVC6jsC9teyazRUcjMNma6cJHSxpCdJZA=";
            var passwordSalt = "YHXzonAHtbDO8nrY2Y7MeA==";
            
            migrationBuilder.Sql($@"
                DECLARE @OrgId INT;
                SELECT TOP 1 @OrgId = Id FROM Organizations ORDER BY Id;
                
                IF @OrgId IS NULL
                BEGIN
                    -- Créer une organisation par défaut si elle n'existe pas
                    INSERT INTO Organizations (Name, CreatedAt)
                    VALUES ('Default Organization', GETUTCDATE());
                    SET @OrgId = SCOPE_IDENTITY();
                END
                
                -- Insérer l'utilisateur admin uniquement s'il n'existe pas déjà (vérifier email ET username)
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@intellipm.local' OR Username = 'admin')
                BEGIN
                    INSERT INTO Users (
                        Username, 
                        Email, 
                        PasswordHash, 
                        PasswordSalt, 
                        FirstName,
                        LastName,
                        GlobalRole, 
                        OrganizationId, 
                        IsActive,
                        CreatedAt,
                        UpdatedAt
                    )
                    VALUES (
                        'admin', 
                        'admin@intellipm.local', 
                        '{passwordHash}', 
                        '{passwordSalt}',
                        'System',
                        'Administrator',
                        2, -- GlobalRole.Admin
                        @OrgId, 
                        1, -- IsActive = true
                        GETUTCDATE(),
                        GETUTCDATE()
                    );
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM Users WHERE Email = 'admin@intellipm.local';
            ");
        }
    }
}
