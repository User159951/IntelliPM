# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Disable NuGet fallback folders to avoid Windows path issues in Linux container
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV NUGET_XMLDOC_MODE=skip
ENV NUGET_FALLBACK_PACKAGES=""

# Copy solution and project files first (for better layer caching)
COPY IntelliPM.sln .
COPY IntelliPM.Domain/IntelliPM.Domain.csproj IntelliPM.Domain/
COPY IntelliPM.Application/IntelliPM.Application.csproj IntelliPM.Application/
COPY IntelliPM.Infrastructure/IntelliPM.Infrastructure.csproj IntelliPM.Infrastructure/
COPY IntelliPM.API/IntelliPM.API.csproj IntelliPM.API/

# Create NuGet.config to disable fallback folders (prevents Windows path errors in Linux container)
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

# Restore dependencies using the custom NuGet.config
RUN dotnet restore IntelliPM.API/IntelliPM.API.csproj --configfile /src/NuGet.config

# Copy all source files (but preserve our NuGet.config)
COPY . .

# Ensure NuGet.config is still present and correct after copying
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

# Clean restore cache to remove any Windows path references, then restore again
RUN dotnet nuget locals all --clear && \
    dotnet restore IntelliPM.API/IntelliPM.API.csproj --configfile /src/NuGet.config --force

# Build the solution - disable fallback folders via MSBuild property
RUN dotnet build IntelliPM.API/IntelliPM.API.csproj -c Release -o /app/build \
    --no-restore \
    /p:DisableImplicitNuGetFallbackFolder=true

# Publish the application - disable fallback folders via MSBuild property
RUN dotnet publish IntelliPM.API/IntelliPM.API.csproj -c Release -o /app/publish \
    /p:UseAppHost=false \
    --no-restore \
    /p:DisableImplicitNuGetFallbackFolder=true

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Copy published files from build stage
COPY --from=build /app/publish .

# Create data directory for healthchecks.db and other runtime data
RUN mkdir -p /app/data

# Expose ports
EXPOSE 80
EXPOSE 443

# Set entry point
ENTRYPOINT ["dotnet", "IntelliPM.API.dll"]

