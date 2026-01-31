# Coolify Deployment Configuration

This directory contains the Docker-based deployment configuration for Coolify.

## Files

- **Dockerfile** - Multi-stage Dockerfile that builds the frontend and backend
- **docker-compose.yml** - Docker Compose configuration for Coolify

## Build Process

The Dockerfile uses a multi-stage build:

1. **Stage 1: Frontend Build** (Node.js 20 Alpine)
   - Installs frontend dependencies with yarn
   - Builds the React/TypeScript frontend using webpack
   - Outputs to `wwwroot/app.js`

2. **Stage 2: .NET Build** (dotnet-sdk:10.0)
   - Restores NuGet packages
   - Copies built frontend assets
   - Publishes the .NET application with `/p:BuildClientApp=false`

3. **Stage 3: Runtime** (dotnet-aspnet:10.0)
   - Minimal runtime image
   - Runs the application on port 8080

## Environment Variables

- `ASPNETCORE_URLS=http://+:8080` - Listen on all interfaces, port 8080
- `ASPNETCORE_ENVIRONMENT=Production` - Production environment

## Deployment

Coolify will automatically use these files based on the `coolify.json` configuration in the repository root.
