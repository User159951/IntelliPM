# Guide : Migration pour créer un utilisateur administrateur permanent

## Vue d'ensemble

Cette migration crée un utilisateur administrateur permanent avec les identifiants :
- **Email** : `admin@intellipm.local`
- **Mot de passe** : `Admin@123456`
- **Rôle** : Admin (GlobalRole = 2)
- **Username** : `admin`

## Étapes d'exécution

### 1. Générer le hash du mot de passe

Vous avez deux options pour générer le hash :

#### Option A : Utiliser l'endpoint API (recommandé)

1. **Démarrer le backend** :
   ```powershell
   cd backend\IntelliPM.API
   dotnet run
   ```

2. **Appeler l'endpoint** dans un navigateur ou via curl :
   ```
   GET http://localhost:5001/api/dev/hash/generate
   ```
   
   Ou avec PowerShell :
   ```powershell
   Invoke-RestMethod -Uri "http://localhost:5001/api/dev/hash/generate" -Method Get
   ```

3. **Copier les valeurs `hash` et `salt`** retournées par l'API.

#### Option B : Utiliser un script console (alternative)

1. Créer un fichier temporaire `GenerateHash.cs` avec le contenu de `Scripts/GenerateAdminHash.cs`

2. Compiler et exécuter :
   ```powershell
   dotnet script GenerateHash.cs
   ```
   
   Ou créer un petit projet console et copier le code de `PasswordHasher`.

### 2. Mettre à jour la migration

1. Ouvrir le fichier de migration :
   ```
   backend/IntelliPM.Infrastructure/Persistence/Migrations/20251224090755_SeedAdminUser.cs
   ```

2. Remplacer les valeurs placeholder :
   ```csharp
   var passwordHash = "VOTRE_HASH_ICI";  // Copier la valeur hash de l'étape 1
   var passwordSalt = "VOTRE_SALT_ICI";  // Copier la valeur salt de l'étape 1
   ```

### 3. Appliquer la migration

```powershell
dotnet ef database update --project backend\IntelliPM.Infrastructure --startup-project backend\IntelliPM.API --context AppDbContext
```

### 4. Vérifier que l'admin a été créé

Vous pouvez vérifier dans la base de données SQL Server :

```sql
SELECT Username, Email, GlobalRole, IsActive, OrganizationId 
FROM Users 
WHERE Email = 'admin@intellipm.local';
```

Résultat attendu :
- Username: `admin`
- Email: `admin@intellipm.local`
- GlobalRole: `2` (Admin)
- IsActive: `1` (true)
- OrganizationId: ID de la première organisation

### 5. Tester la connexion

1. Accéder à l'interface de connexion de l'application
2. Se connecter avec :
   - **Email** : `admin@intellipm.local`
   - **Mot de passe** : `Admin@123456`
3. Vérifier l'accès à l'interface admin (`/admin/users`, `/admin/dashboard`)

### 6. Nettoyer (optionnel)

Après avoir appliqué la migration, vous pouvez supprimer :

- ~~`backend/IntelliPM.API/Controllers/AdminHashGeneratorController.cs`~~ (REMOVED - Security vulnerability)
- `backend/IntelliPM.API/Scripts/GenerateAdminHash.cs`
- `backend/IntelliPM.API/Scripts/ADMIN_USER_MIGRATION_GUIDE.md` (ce fichier)

**Note** : Le contrôleur `AdminHashGeneratorController` a été supprimé pour des raisons de sécurité. L'utilisateur admin est maintenant créé automatiquement via `DataSeeder.SeedDevelopmentAdminUserAsync()` dans `Program.cs`.

## Vérification de l'idempotence

La migration est idempotente grâce à la vérification `IF NOT EXISTS`. Vous pouvez l'appliquer plusieurs fois sans créer de doublons :

```powershell
# Tester que la migration peut être appliquée plusieurs fois
dotnet ef database update --project backend\IntelliPM.Infrastructure --startup-project backend\IntelliPM.API --context AppDbContext
```

## Rollback

Pour annuler la migration :

```powershell
dotnet ef database update <NOM_DE_LA_MIGRATION_PRECEDENTE> --project backend\IntelliPM.Infrastructure --startup-project backend\IntelliPM.API --context AppDbContext
```

Ou supprimer la migration complètement :

```powershell
dotnet ef migrations remove --project backend\IntelliPM.Infrastructure --startup-project backend\IntelliPM.API --context AppDbContext
```

**⚠️ Attention** : La méthode `Down()` supprimera l'utilisateur admin de la base de données.

## Structure de la table Users

Assurez-vous que la table Users a les colonnes suivantes (vérifié dans les migrations précédentes) :
- `Id` (int, Identity)
- `Username` (nvarchar)
- `Email` (nvarchar)
- `PasswordHash` (nvarchar(max))
- `PasswordSalt` (nvarchar(max))
- `FirstName` (nvarchar(max))
- `LastName` (nvarchar(max))
- `GlobalRole` (int) - Enum: User=1, Admin=2
- `OrganizationId` (int)
- `IsActive` (bit)
- `CreatedAt` (datetimeoffset)
- `UpdatedAt` (datetimeoffset)

## Dépannage

### Erreur : "OrganizationId is NULL"
Si aucune organisation n'existe, la migration créera automatiquement une organisation par défaut nommée "Default Organization".

### Erreur : "Violation de contrainte unique"
Si l'utilisateur admin existe déjà, la migration ne fera rien grâce à `IF NOT EXISTS`. C'est le comportement attendu.

### Erreur : "Invalid password hash"
Vérifiez que vous avez bien copié les valeurs `hash` et `salt` générées par l'endpoint ou le script, sans espaces ou caractères supplémentaires.

