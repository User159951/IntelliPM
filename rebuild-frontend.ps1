Write-Host "Rebuilding Frontend..." -ForegroundColor Cyan

# Stop and remove container
docker stop intellipm-frontend 2>$null
docker rm intellipm-frontend 2>$null

# Rebuild image
Write-Host "Building new image..." -ForegroundColor Yellow
Set-Location frontend
docker build -t intellipm-frontend .
Set-Location ..

# Run container
Write-Host "Starting container..." -ForegroundColor Green
docker run -d `
  --name intellipm-frontend `
  --network intellipm-network `
  -p 3001:5173 `
  -e VITE_API_BASE_URL=http://localhost:5001 `
  -e VITE_HMR_CLIENT_PORT=3001 `
  --restart unless-stopped `
  intellipm-frontend

Write-Host "Frontend rebuilt successfully!" -ForegroundColor Green
Write-Host "View logs with: docker logs -f intellipm-frontend" -ForegroundColor Cyan
