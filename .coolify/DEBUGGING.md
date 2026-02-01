# Debugging Yarn DNS Errors in Docker Build

## The Problem

You're seeing this error during Docker build:
```
error Error: getaddrinfo EAI_AGAIN registry.yarnpkg.com
```

This happens when MSBuild's `BuildClientApp` target runs `yarn install --frozen-lockfile` during the `dotnet publish` step.

## Diagnostic Steps

### 1. Verify Network Access in Build Container

Test if the build container has network access at all:

```dockerfile
# Add this temporary RUN command to your Dockerfile after installing Node.js/yarn:
RUN ping -c 3 8.8.8.8 || echo "No ping"; \
    nslookup registry.yarnpkg.com || echo "DNS failed"; \
    curl -I https://registry.yarnpkg.com || echo "HTTP failed"
```

**Expected results:**
- If ping fails → No network access at all
- If DNS fails → DNS resolution issue
- If HTTP fails → Firewall/proxy blocking

### 2. Check Coolify Build Environment Configuration

In your Coolify instance, check:

**A. Build isolation settings:**
- Does Coolify run builds in isolated network namespaces?
- Is there a setting to enable network access during build?
- Check Coolify server logs during build for network-related messages

**B. Docker daemon configuration on Coolify host:**
```bash
# SSH to Coolify host
cat /etc/docker/daemon.json

# Check if DNS is configured
# Should see something like:
{
  "dns": ["8.8.8.8", "1.1.1.1"]
}
```

**C. Firewall/network rules:**
```bash
# On Coolify host, test network access
docker run --rm alpine ping -c 3 registry.yarnpkg.com
docker run --rm alpine nslookup registry.yarnpkg.com
```

### 3. Test Build Phases Separately

Create a test Dockerfile to isolate the issue:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS test
WORKDIR /app

# Test 1: Can we install packages?
RUN apt-get update && apt-get install -y curl

# Test 2: Can we download from internet?
RUN curl -I https://www.google.com

# Test 3: Can we reach yarn registry?
RUN curl -I https://registry.yarnpkg.com

# Test 4: Install Node.js
ENV NODE_VERSION=20.18.1
RUN curl -fsSL https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.xz -o /tmp/node.tar.xz

# Test 5: Can yarn resolve DNS?
# ... install node and yarn as in main Dockerfile
# Then try: RUN yarn info react
```

Build this test Dockerfile and see which step fails.

### 4. Check for Proxy Requirements

Some corporate/cloud environments require proxy configuration:

```dockerfile
# Add before RUN commands that need network:
ENV HTTP_PROXY=http://your-proxy:port
ENV HTTPS_PROXY=http://your-proxy:port
ENV NO_PROXY=localhost,127.0.0.1
```

### 5. Inspect Coolify Build Logs

Get the full build log from Coolify (not just the error section) and look for:
- Network initialization messages
- DNS warnings
- Proxy-related errors
- Certificate verification issues
- Any messages about isolated/restricted network mode

## Common Causes & Solutions

### Cause 1: Coolify Uses Network-Isolated Builds

**Solution:** Configure Coolify to allow network access during build, or use build hooks to pre-build frontend before Docker build.

### Cause 2: DNS Resolution Blocked

**Solution:** Configure Docker daemon DNS on Coolify host:
```json
{
  "dns": ["8.8.8.8", "1.1.1.1"]
}
```

### Cause 3: Outbound HTTPS Blocked by Firewall

**Solution:** Check firewall rules on Coolify host, allow outbound HTTPS (port 443) to:
- registry.yarnpkg.com
- registry.npmjs.org

### Cause 4: IPv6 Issues

Try forcing IPv4:
```dockerfile
RUN echo 'precedence ::ffff:0:0/96 100' >> /etc/gai.conf
```

### Cause 5: Coolify Proxy/Cache Configuration

Check if Coolify has a package cache/proxy configured that might be misconfigured.

## Next Steps Based on Diagnosis

**If no network access during build:**
- Option A: Pre-build frontend outside Docker, copy pre-built files
- Option B: Commit frontend build artifacts to repository
- Option C: Use Coolify build hooks to run frontend build before Docker build

**If network access exists but DNS fails:**
- Configure Docker daemon DNS
- Check /etc/resolv.conf in build container
- Verify DNS servers are reachable from Coolify host

**If network access exists but yarn registry blocked:**
- Configure proxy settings
- Check firewall rules
- Use alternative registry mirror

## Recommended Debug Command

Add this to your Dockerfile temporarily to get detailed information:

```dockerfile
RUN echo "=== Network Debug Info ===" && \
    cat /etc/resolv.conf && \
    echo "=== DNS Test ===" && \
    nslookup registry.yarnpkg.com || echo "DNS failed" && \
    echo "=== Connectivity Test ===" && \
    curl -v https://registry.yarnpkg.com 2>&1 | head -20 || echo "Connection failed" && \
    echo "=== Environment ===" && \
    env | grep -i proxy
```

This will show you exactly what's failing and help determine the root cause.
