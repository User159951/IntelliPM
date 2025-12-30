# PowerShell script to generate password hash for admin user migration
# Usage: .\Generate-AdminHash.ps1

$password = "Admin@123456"
$saltSize = 16
$hashSize = 32
$iterations = 10000

# Generate random salt
Add-Type -AssemblyName System.Security
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$salt = New-Object byte[] $saltSize
$rng.GetBytes($salt)

# Generate hash using PBKDF2
$pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($password, $salt, $iterations, [System.Security.Cryptography.HashAlgorithmName]::SHA256)
$hash = $pbkdf2.GetBytes($hashSize)

# Convert to Base64
$hashBase64 = [Convert]::ToBase64String($hash)
$saltBase64 = [Convert]::ToBase64String($salt)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Password Hash and Salt for 'Admin@123456'" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Hash: $hashBase64" -ForegroundColor Green
Write-Host "Salt: $saltBase64" -ForegroundColor Green
Write-Host ""
Write-Host "Copy these lines into SeedAdminUser.cs migration:" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Yellow
Write-Host "var passwordHash = `"$hashBase64`";" -ForegroundColor White
Write-Host "var passwordSalt = `"$saltBase64`";" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Yellow

# Cleanup
$pbkdf2.Dispose()
$rng.Dispose()

