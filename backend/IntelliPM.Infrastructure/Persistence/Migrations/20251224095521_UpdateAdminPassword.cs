using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hash pré-calculé pour le mot de passe "Admin@123456"
            // Généré le 2024-12-24 via Generate-AdminHash.ps1
            var passwordHash = "biHzOObpuGaVC6jsC9teyazRUcjMNma6cJHSxpCdJZA=";
            var passwordSalt = "YHXzonAHtbDO8nrY2Y7MeA==";
            
            migrationBuilder.Sql($@"
                -- Mettre à jour le mot de passe, email, et rôle de l'utilisateur admin existant
                UPDATE Users
                SET 
                    PasswordHash = '{passwordHash}',
                    PasswordSalt = '{passwordSalt}',
                    Email = 'admin@intellipm.local',
                    GlobalRole = 2, -- GlobalRole.Admin
                    FirstName = 'System',
                    LastName = 'Administrator',
                    IsActive = 1,
                    UpdatedAt = GETUTCDATE()
                WHERE Username = 'admin' OR Email = 'admin@intellipm.local';
                
                -- Si l'utilisateur admin n'existe pas, le créer
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin' OR Email = 'admin@intellipm.local')
                BEGIN
                    DECLARE @OrgId INT;
                    SELECT TOP 1 @OrgId = Id FROM Organizations ORDER BY Id;
                    
                    IF @OrgId IS NULL
                    BEGIN
                        -- Créer une organisation par défaut si elle n'existe pas
                        INSERT INTO Organizations (Name, CreatedAt)
                        VALUES ('Default Organization', GETUTCDATE());
                        SET @OrgId = SCOPE_IDENTITY();
                    END
                    
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
            // Note: On ne peut pas restaurer l'ancien mot de passe car on ne le connaît pas
            // Cette méthode peut être laissée vide ou supprimer l'utilisateur admin si nécessaire
            migrationBuilder.Sql(@"
                -- Optionnel: Supprimer l'utilisateur admin (décommenter si nécessaire)
                -- DELETE FROM Users WHERE Username = 'admin' OR Email = 'admin@intellipm.local';
            ");
        }
    }
}
