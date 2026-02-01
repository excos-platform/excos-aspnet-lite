# Coolify Deployment Configuration

This directory contains the Docker-based deployment configuration for Coolify.

## Files

- **Dockerfile** - Single-stage Dockerfile with Node.js + .NET SDK for integrated build
- **docker-compose.yml** - Docker Compose configuration for Coolify

## Build Process

The Dockerfile uses a simplified 2-stage build that leverages MSBuild's integrated frontend build:

1. **Build Stage** (.NET SDK 10.0 + Node.js 20)
   - Installs Node.js and yarn into the .NET SDK image
   - Copies source files (src/ only, tests excluded)
   - Restores NuGet packages
   - Runs `dotnet publish` which triggers MSBuild's `BuildClientApp` target
   - The MSBuild target automatically runs `yarn install` and `yarn build`
   - Publishes the complete application

2. **Runtime Stage** (.NET ASP.NET 10.0 Alpine)
   - Minimal Alpine-based runtime image (~121MB vs 230MB with standard image)
   - Copies published application
   - Runs the application on port 8080

## Why This Approach?

Previously, we tried a 3-stage build with separate Node.js and .NET stages. However, the .NET project has integrated frontend build via MSBuild (using `Yarn.MSBuild` package), which requires Node.js/yarn to be available during the .NET build.

This simplified approach:
- Installs Node.js into the SDK image once
- Lets MSBuild handle the frontend build automatically
- Reduces complexity and potential sync issues
- Uses Alpine runtime for minimal image size (121MB)
- Test projects excluded from the build (not needed at runtime)

## Environment Variables

- `ASPNETCORE_URLS=http://+:8080` - Listen on all interfaces, port 8080
- `ASPNETCORE_ENVIRONMENT=Production` - Production environment

## DNS Configuration

The docker-compose.yml includes DNS servers (8.8.8.8, 8.8.4.4) to prevent DNS resolution failures during build. The yarn install command also includes `--network-timeout 100000` flag for resilience against temporary network issues.

If you encounter DNS errors during build, ensure your Coolify/Docker host has proper DNS configuration or network connectivity.

## Deployment

Configure Coolify manually to use the docker-compose file in this directory.
