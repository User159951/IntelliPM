# ==============================================
# Stage 1: Build
# ==============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Disable NuGet fallback folders
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV NUGET_XMLDOC_MODE=skip
ENV NUGET_FALLBACK_PACKAGES=""

# Copy solution and project files
COPY IntelliPM.sln .
COPY IntelliPM.Domain/IntelliPM.Domain.csproj IntelliPM.Domain/
COPY IntelliPM.Application/IntelliPM.Application.csproj IntelliPM.Application/
COPY IntelliPM.Infrastructure/IntelliPM.Infrastructure.csproj IntelliPM.Infrastructure/
COPY IntelliPM.API/IntelliPM.API.csproj IntelliPM.API/

# Create NuGet.config
RUN echo '<?xml version="1.0" encoding="utf-8"?>' > /src/NuGet.config && \
    echo '<configuration>' >> /src/NuGet.config && \
    echo '  <packageSources>' >> /src/NuGet.config && \
    echo '    <clear/>' >> /src/NuGet.config && \
    echo '    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />' >> /src/NuGet.config && \
    echo '  </packageSources>' >> /src/NuGet.config && \
    echo '  <fallbackPackageFolders>' >> /src/NuGet.config && \
    echo '    <clear/>' >> /src/NuGet.config && \
    echo '  </fallbackPackageFolders>' >> /src/NuGet.config && \
    echo '</configuration>' >> /src/NuGet.config

# Restore dependencies
RUN dotnet restore IntelliPM.API/IntelliPM.API.csproj --configfile /src/NuGet.config

# Copy all source files
COPY . .

# Clean and restore again
RUN dotnet nuget locals all --clear && \
    dotnet restore IntelliPM.API/IntelliPM.API.csproj --configfile /src/NuGet.config --force

# Build
RUN dotnet build IntelliPM.API/IntelliPM.API.csproj -c Release -o /app/build \
    --no-restore \
    /p:DisableImplicitNuGetFallbackFolder=true

# Publish
RUN dotnet publish IntelliPM.API/IntelliPM.API.csproj -c Release -o /app/publish \
    /p:UseAppHost=false \
    --no-restore \
    /p:DisableImplicitNuGetFallbackFolder=true

# ==============================================
# Stage 2: Runtime
# ==============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=build /app/publish .

# Create directories
RUN mkdir -p /app/data /app/logs /app/uploads

# Expose ports
EXPOSE 80
EXPOSE 443

# Set entry point
ENTRYPOINT ["dotnet", "IntelliPM.API.dll"]
