# Coolify Deployment Configuration

This directory contains the Docker-based deployment configuration for Coolify.

## Files

- **Dockerfile** - Single-stage Dockerfile with Node.js + .NET SDK for integrated build
- **docker-compose.yml** - Docker Compose configuration for Coolify

## Build Process

The Dockerfile uses a simplified 2-stage build that leverages MSBuild's integrated frontend build:

1. **Build Stage** (.NET SDK 10.0 + Node.js 20)
   - Installs Node.js and yarn into the .NET SDK image
   - Copies project files and restores NuGet packages
   - **Copies package.json/yarn.lock and runs yarn install in separate layer**
   - Copies remaining source files (src/ only, tests excluded)
   - Runs `dotnet publish` which triggers MSBuild's `BuildClientApp` target
   - MSBuild's yarn install runs quickly (node_modules already exists)
   - MSBuild runs `yarn build` to compile frontend
   - Publishes the complete application

2. **Runtime Stage** (.NET ASP.NET 10.0 Alpine)
   - Minimal Alpine-based runtime image (~121MB vs 230MB with standard image)
   - Copies published application
   - Runs the application on port 8080

## Why This Approach?

Previously, we tried a 3-stage build with separate Node.js and .NET stages. However, the .NET project has integrated frontend build via MSBuild (using `Yarn.MSBuild` package), which requires Node.js/yarn to be available during the .NET build.

This simplified approach:
- Installs Node.js into the SDK image once
- Pre-installs yarn dependencies in a cached Docker layer (before source copy)
- Lets MSBuild handle the frontend build automatically
- Reduces complexity and potential sync issues
- Uses Alpine runtime for minimal image size (121MB)
- Test projects excluded from the build (not needed at runtime)
- More resilient to network issues (yarn dependencies installed early)

## Environment Variables

- `ASPNETCORE_URLS=http://+:8080` - Listen on all interfaces, port 8080
- `ASPNETCORE_ENVIRONMENT=Production` - Production environment

## Troubleshooting

### Yarn DNS Errors (getaddrinfo EAI_AGAIN)

If you encounter `getaddrinfo EAI_AGAIN registry.yarnpkg.com` errors during Docker build, this is usually caused by IPv6 DNS resolution issues. 

**Solution**: The Dockerfile includes `ENV NODE_OPTIONS=--dns-result-order=ipv4first` which forces Node.js to prefer IPv4 for DNS lookups.

If the issue persists:
- Check Docker daemon DNS configuration on your Coolify host
- Verify network connectivity from build containers
- Consider using a DNS server that supports both IPv4 and IPv6

### Port Already in Use

The docker-compose.yml does not expose ports to the host, expecting you to use a reverse proxy (like Coolify's built-in proxy). If you need direct access, add:
```yaml
ports:
  - "8080:8080"
```

## Deployment

Configure Coolify manually to use the docker-compose file in this directory.
