using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSuperAdminRoleAndPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hash pré-calculé pour le mot de passe "Super@dmin123456"
            // Généré le 2025-01-05
            var passwordHash = "jEG5KrUJ9U9HLc/hLxL743WfF90XlGcZ0zsdSsZ59dU=";
            var passwordSalt = "5ylOvMhSnFCL4jRFYC+P5Q==";
            
            migrationBuilder.Sql($@"
                -- Mettre à jour le GlobalRole de 2 à 3 (SuperAdmin) et le mot de passe pour l'utilisateur superadmin
                UPDATE Users
                SET 
                    GlobalRole = 3, -- GlobalRole.SuperAdmin
                    PasswordHash = '{passwordHash}',
                    PasswordSalt = '{passwordSalt}',
                    UpdatedAt = GETUTCDATE()
                WHERE Username = 'superadmin' OR (Email = 'superadmin@intellipm.com' AND GlobalRole = 2);
                
                -- Vérification : Afficher un message si l'utilisateur n'a pas été trouvé
                IF @@ROWCOUNT = 0
                BEGIN
                    PRINT 'Warning: No user with username ''superadmin'' or email ''superadmin@intellipm.com'' with GlobalRole = 2 was found.';
                END
                ELSE
                BEGIN
                    PRINT 'Success: SuperAdmin user updated successfully.';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: On ne peut pas restaurer l'ancien mot de passe car on ne le connaît pas
            // Cette méthode peut être laissée vide ou restaurer le GlobalRole à 2 si nécessaire
            migrationBuilder.Sql(@"
                -- Optionnel: Restaurer le GlobalRole à 2 (Admin) si nécessaire
                -- UPDATE Users
                -- SET GlobalRole = 2
                -- WHERE Username = 'superadmin';
            ");
        }
    }
}
