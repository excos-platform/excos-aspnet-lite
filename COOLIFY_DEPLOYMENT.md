# Coolify Deployment with Nixpacks

This document explains how to deploy the Excos.AspNetCore.Lite test server to Coolify using nixpacks, and addresses the conflict with .NET SDK's built-in container support.

## Overview

The repository contains two configuration files for deploying to Coolify:

- **`coolify.json`** - Tells Coolify to use nixpacks as the build pack
- **`nixpacks.toml`** - Configures how nixpacks should build and run the application

## Why Docker Container Publish Target Doesn't Work with Nixpacks

### The Conflict

The test server project (`Excos.AspNetCore.Lite.TestServer.csproj`) currently has the following settings:

```xml
<EnableSdkContainerSupport>true</EnableSdkContainerSupport>
<ContainerRepository>excos-lite-test-server</ContainerRepository>
<ContainerImageTag>latest</ContainerImageTag>
```

These settings enable **.NET SDK's built-in container publishing** feature (available since .NET 8), which allows you to create Docker containers directly using:

```bash
dotnet publish -t:PublishContainer
```

This creates a container image **without needing a Dockerfile**.

### Why This Conflicts with Nixpacks

**Nixpacks and .NET SDK container support are two different container build systems that cannot work together:**

1. **Nixpacks builds its own container**
   - Nixpacks orchestrates the entire container build process
   - It uses Nix packages to set up the build environment
   - It runs standard `dotnet publish` commands (WITHOUT the `-t:PublishContainer` flag)
   - It then packages the output into a container using its own layering logic

2. **.NET SDK container support builds containers during publish**
   - When `EnableSdkContainerSupport` is enabled, `dotnet publish -t:PublishContainer` creates a Docker image
   - The SDK chooses base images, layers, and container metadata automatically
   - It pushes directly to a container registry

3. **The conflict:**
   - Nixpacks runs `dotnet publish -c Release -o out` (without `-t:PublishContainer`)
   - This means the .NET SDK container features are **never triggered** during nixpacks builds
   - The `EnableSdkContainerSupport` setting and related properties are **ignored**
   - Having both configurations is confusing and may lead to unexpected behavior
   - If someone tries to use both systems, they'll end up with conflicting build processes

### Technical Details

When using nixpacks:
- The `dotnet publish` command in `nixpacks.toml` produces a directory of files (`out/`), not a container
- Nixpacks then creates its own container image with these files
- The container base image, layers, and runtime are controlled by Nixpacks, not the .NET SDK
- Container-specific MSBuild properties (`<Container*>`) are not processed or respected

When using .NET SDK container support:
- The `dotnet publish -t:PublishContainer` command builds AND containerizes in one step
- It uses MSBuild properties from the `.csproj` file to configure the container
- It produces a Docker image directly, ready to push to a registry
- No external build system like Nixpacks is needed

## Recommendations

### Option 1: Use Nixpacks (Recommended for Coolify)

If deploying to Coolify with nixpacks, **remove or disable** the .NET SDK container support settings from the `.csproj` file:

```xml
<!-- Remove or comment out these lines -->
<!--
<EnableSdkContainerSupport>true</EnableSdkContainerSupport>
<ContainerRepository>excos-lite-test-server</ContainerRepository>
<ContainerImageTag>latest</ContainerImageTag>
-->
```

This approach:
- ✅ Works seamlessly with Coolify/nixpacks
- ✅ Provides a consistent build process across different languages
- ✅ Eliminates confusion about which container system is being used
- ❌ Requires nixpacks configuration instead of .NET-native approach

### Option 2: Use .NET SDK Container Support (Alternative)

If you want to use .NET's built-in container support instead:

1. **Remove** `coolify.json` and `nixpacks.toml`
2. **Keep** the `EnableSdkContainerSupport` settings in `.csproj`
3. Use a **Dockerfile-based deployment** or direct container publishing:

```bash
dotnet publish -t:PublishContainer -c Release
```

This approach:
- ✅ Uses .NET-native container features
- ✅ Respects all MSBuild container properties
- ✅ Simpler for .NET-only projects
- ❌ Doesn't work with Coolify's nixpacks build pack
- ❌ Requires different CI/CD configuration

### Option 3: Keep Both (Not Recommended)

You could keep both configurations if:
- You use nixpacks for Coolify deployment
- You use .NET SDK containers for local development or other CI/CD pipelines
- You document clearly which approach is used where

However, this:
- ⚠️ Creates confusion about which system is "canonical"
- ⚠️ May lead to different behavior in different environments
- ⚠️ Requires maintaining two separate build configurations

## Current Configuration

The current `nixpacks.toml` configuration for Coolify deployment:

```toml
[phases.setup]
nixPkgs = ["dotnet-sdk_10"]

[phases.build]
cmds = [
  "dotnet restore",
  "dotnet publish src/Excos.AspNetCore.Lite.TestServer/Excos.AspNetCore.Lite.TestServer.csproj -c Release -o out --no-restore"
]

[start]
cmd = "dotnet out/Excos.AspNetCore.Lite.TestServer.dll"

[variables]
ASPNETCORE_URLS = "http://0.0.0.0:8080"
ASPNETCORE_ENVIRONMENT = "Production"
```

## Testing Locally

To test the nixpacks build process locally (if you have nixpacks installed):

```bash
# Build using nixpacks
nixpacks build . --name excos-test-server

# Run the container
docker run -p 8080:8080 excos-test-server
```

To test .NET SDK container support:

```bash
# Build and publish to local Docker
dotnet publish src/Excos.AspNetCore.Lite.TestServer -t:PublishContainer

# Run the container
docker run -p 8080:8080 excos-lite-test-server:latest
```

## References

- [.NET SDK Container Support Documentation](https://learn.microsoft.com/en-us/dotnet/core/containers/overview)
- [Nixpacks Documentation](https://nixpacks.com/)
- [Coolify Documentation](https://coolify.io/docs/)
