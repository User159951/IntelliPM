Write-Host "🔄 Rebuilding All Containers..." -ForegroundColor Cyan

# Backend
Write-Host "`n=== BACKEND ===" -ForegroundColor Magenta
.\rebuild-backend.ps1

# Frontend
Write-Host "`n=== FRONTEND ===" -ForegroundColor Magenta
.\rebuild-frontend.ps1

Write-Host "`n✅ All containers rebuilt successfully!" -ForegroundColor Green
