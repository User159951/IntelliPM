# Guide de test de connexion admin

## 1. Vérifier que l'utilisateur admin existe

Appelez cet endpoint pour vérifier :
```
GET https://samatha-waxier-shauna.ngrok-free.dev/api/dev/hash/check-admin
```

**Résultat attendu :**
```json
{
  "exists": true,
  "username": "admin",
  "email": "admin@intellipm.local",
  "globalRole": "Admin",
  "isActive": true,
  "organizationId": 1,
  "passwordMatch": true,
  "note": "Password 'Admin@123456' matches the hash"
}
```

## 2. Tester la connexion

### Via l'interface web :
- Aller sur la page de login
- **Username** : `admin` (ou `admin@intellipm.local`)
- **Password** : `Admin@123456`
- Cliquer sur "Sign in"

### Via curl/Postman :
```bash
POST https://samatha-waxier-shauna.ngrok-free.dev/api/v1/Auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123456"
}
```

**Résultat attendu :**
```json
{
  "userId": 1,
  "username": "admin",
  "email": "admin@intellipm.local",
  "roles": ["Developer"],
  "message": "Logged in successfully"
}
```

## 3. Si la connexion échoue

### Vérifier les logs du backend
Les logs devraient indiquer :
- `User with username or email 'admin' not found` → L'utilisateur n'existe pas
- `User account is inactive` → Le compte est désactivé
- `Invalid password` → Le mot de passe ne correspond pas

### Solutions

#### Si l'utilisateur n'existe pas :
```sql
-- Vérifier si l'utilisateur existe
SELECT Username, Email, GlobalRole, IsActive, OrganizationId 
FROM Users 
WHERE Username = 'admin' OR Email = 'admin@intellipm.local';

-- Si l'utilisateur n'existe pas, appliquer la migration :
dotnet ef database update --project backend\IntelliPM.Infrastructure --startup-project backend\IntelliPM.API --context AppDbContext
```

#### Si le mot de passe ne correspond pas :
1. Vérifier le hash dans la base de données :
```sql
SELECT Username, Email, PasswordHash, PasswordSalt 
FROM Users 
WHERE Username = 'admin';
```

2. Appliquer la migration `UpdateAdminPassword` :
```bash
dotnet ef database update --project backend\IntelliPM.Infrastructure --startup-project backend\IntelliPM.API --context AppDbContext
```

#### Si le compte est inactif :
```sql
UPDATE Users 
SET IsActive = 1 
WHERE Username = 'admin';
```

## 4. Vérifier que le backend fonctionne

```bash
# Health check
GET https://samatha-waxier-shauna.ngrok-free.dev/api/health/live

# Devrait retourner : 200 OK
```

