# Script pour tester si le mot de passe correspond au hash stocké
# Usage: .\Test-AdminPassword.ps1

$password = "Admin@123456"
$storedHash = "biHzOObpuGaVC6jsC9teyazRUcjMNma6cJHSxpCdJZA="
$storedSalt = "YHXzonAHtbDO8nrY2Y7MeA=="

$saltSize = 16
$hashSize = 32
$iterations = 10000

Write-Host "Test de vérification du mot de passe Admin@123456" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Hash stocké: $storedHash" -ForegroundColor Yellow
Write-Host "Salt stocké: $storedSalt" -ForegroundColor Yellow
Write-Host ""

# Vérifier le hash
Add-Type -AssemblyName System.Security
$saltBytes = [Convert]::FromBase64String($storedSalt)

$pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($password, $saltBytes, $iterations, [System.Security.Cryptography.HashAlgorithmName]::SHA256)
$computedHash = $pbkdf2.GetBytes($hashSize)
$computedHashBase64 = [Convert]::ToBase64String($computedHash)

Write-Host "Hash calculé: $computedHashBase64" -ForegroundColor Green
Write-Host ""

if ($computedHashBase64 -eq $storedHash) {
    Write-Host "✅ SUCCESS: Le mot de passe correspond au hash!" -ForegroundColor Green
} else {
    Write-Host "❌ ERROR: Le hash ne correspond pas!" -ForegroundColor Red
    Write-Host "Le mot de passe fourni ne correspond pas au hash stocké." -ForegroundColor Red
}

# Nettoyage
$pbkdf2.Dispose()

