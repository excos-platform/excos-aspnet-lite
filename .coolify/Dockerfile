# Dockerfile for Excos.AspNetCore.Lite TestServer
# Uses .NET SDK with Node.js installed for integrated frontend build via MSBuild

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Install Node.js and yarn for frontend build (required by MSBuild target)
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g yarn && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy solution file and project files for restore (better layer caching)
COPY Excos.AspNetCore.Lite.slnx ./
COPY src/**/*.csproj src/
COPY tests/**/*.csproj tests/
# Copy frontend package files for yarn install during MSBuild
COPY src/Excos.AspNetCore.Lite/ClientApp/package.json src/Excos.AspNetCore.Lite/ClientApp/
COPY src/Excos.AspNetCore.Lite/ClientApp/yarn.lock src/Excos.AspNetCore.Lite/ClientApp/

# Restore dependencies (cached unless project files change)
RUN dotnet restore src/Excos.AspNetCore.Lite.TestServer/Excos.AspNetCore.Lite.TestServer.csproj

# Copy remaining source code
COPY src/ ./src/
COPY tests/ ./tests/

# Build and publish the application
# MSBuild will automatically run the BuildClientApp target which installs yarn deps and builds frontend
RUN dotnet publish src/Excos.AspNetCore.Lite.TestServer/Excos.AspNetCore.Lite.TestServer.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
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
