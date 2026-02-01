# Dockerfile for Excos.AspNetCore.Lite TestServer
# Uses .NET SDK with Node.js installed for integrated frontend build via MSBuild

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Install Node.js and yarn for frontend build (required by MSBuild target)
# Download Node.js 20 LTS binary directly from official source
ENV NODE_VERSION=20.18.1
RUN apt-get update && \
    apt-get install -y curl xz-utils && \
    curl -fsSL https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.xz -o /tmp/node.tar.xz && \
    mkdir -p /usr/local/lib/nodejs && \
    tar -xJf /tmp/node.tar.xz -C /usr/local/lib/nodejs && \
    rm /tmp/node.tar.xz && \
    ln -s /usr/local/lib/nodejs/node-v${NODE_VERSION}-linux-x64/bin/node /usr/local/bin/node && \
    ln -s /usr/local/lib/nodejs/node-v${NODE_VERSION}-linux-x64/bin/npm /usr/local/bin/npm && \
    ln -s /usr/local/lib/nodejs/node-v${NODE_VERSION}-linux-x64/bin/npx /usr/local/bin/npx && \
    npm install -g yarn && \
    ln -s /usr/local/lib/nodejs/node-v${NODE_VERSION}-linux-x64/bin/yarn /usr/local/bin/yarn && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy solution file and project files for restore (better layer caching)
COPY Excos.AspNetCore.Lite.slnx ./

# Copy project files - Docker doesn't support ** globbing, so we copy explicitly
COPY src/Excos.AspNetCore.Lite/Excos.AspNetCore.Lite.csproj src/Excos.AspNetCore.Lite/
COPY src/Excos.AspNetCore.Lite.TestServer/Excos.AspNetCore.Lite.TestServer.csproj src/Excos.AspNetCore.Lite.TestServer/

# Copy frontend package files and install dependencies BEFORE copying source
# This allows yarn install to run in its own Docker layer and be cached
COPY src/Excos.AspNetCore.Lite/ClientApp/package.json src/Excos.AspNetCore.Lite/ClientApp/
COPY src/Excos.AspNetCore.Lite/ClientApp/yarn.lock src/Excos.AspNetCore.Lite/ClientApp/
RUN cd src/Excos.AspNetCore.Lite/ClientApp && yarn install --frozen-lockfile

# Restore dependencies (cached unless project files change)
RUN dotnet restore src/Excos.AspNetCore.Lite.TestServer/Excos.AspNetCore.Lite.TestServer.csproj

# Copy remaining source code (only src, not tests)
COPY src/ ./src/

# Build and publish the application
# MSBuild will run the BuildClientApp target, but yarn install will be fast (node_modules exists)
RUN dotnet publish src/Excos.AspNetCore.Lite.TestServer/Excos.AspNetCore.Lite.TestServer.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime image - using Alpine variant for minimal size (~121MB vs 230MB)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Copy published application
COPY --from=build /app/publish ./

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "Excos.AspNetCore.Lite.TestServer.dll"]
