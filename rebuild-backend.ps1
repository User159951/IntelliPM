Write-Host "Rebuilding Backend..." -ForegroundColor Cyan

# Stop and remove container
docker stop intellipm-backend 2>$null
docker rm intellipm-backend 2>$null

# Rebuild image
Write-Host "Building new image..." -ForegroundColor Yellow
Set-Location backend
docker build -t intellipm-backend -f IntelliPM.API/Dockerfile .
Set-Location ..

# Run container
Write-Host "Starting container..." -ForegroundColor Green
docker run -d `
  --name intellipm-backend `
  --network intellipm-network `
  -p 5001:80 `
  -e ASPNETCORE_ENVIRONMENT=Development `
  -e ASPNETCORE_URLS=http://+:80 `
  -e "ConnectionStrings__SqlServer=Server=intellipm-sqlserver,1433;Database=IntelliPM;User Id=sa;Password=A7p9w1n3@!;Encrypt=True;TrustServerCertificate=True;" `
  -e "ConnectionStrings__DefaultConnection=Server=intellipm-sqlserver,1433;Database=IntelliPM;User Id=sa;Password=A7p9w1n3@!;Encrypt=True;TrustServerCertificate=True;" `
  -e "ConnectionStrings__VectorDb=Host=intellipm-postgres;Port=5432;Username=postgres;Password=A7p9w1n3@!;Database=intellipm_vector;Pooling=true;Maximum Pool Size=20;" `
  -e Ollama__Endpoint=http://intellipm-ollama:11434 `
  -e Ollama__Model=llama3.2:3b `
  -e Jwt__SecretKey=NTQyNmMxMjAtZDZmNi00ODIwLWE3ZGEtMWE2NTg5NjJhZjQwODVjMmFkZTgtYjBlZS00NWE3LTk5NDgtZmU3YzkwODY3M2Y4 `
  -e Jwt__Issuer=IntelliPM `
  -e Jwt__Audience=IntelliPM `
  -e Email__SmtpHost=smtp-relay.brevo.com `
  -e Email__SmtpPort=587 `
  -e Email__SmtpUsername=9ed34a001@smtp-brevo.com `
  -e Email__SmtpPassword=xsmtpsib-1f3b42834147585c7df4b3a1d5a308ec9979c8ed2a9503d679d75b12d68e150b-FsYRF9RIKg26VXGL `
  -e Email__FromEmail=mohamedelmahdi.touimy@gmail.com `
  -e Email__FromName=IntelliPM `
  --restart unless-stopped `
  intellipm-backend

Write-Host "Backend rebuilt successfully!" -ForegroundColor Green
Write-Host "View logs with: docker logs -f intellipm-backend" -ForegroundColor Cyan
