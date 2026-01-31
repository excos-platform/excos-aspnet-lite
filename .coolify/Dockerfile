# Multi-stage Dockerfile for Excos.AspNetCore.Lite TestServer

# Stage 1: Build frontend assets
FROM node:20-alpine AS frontend-build
WORKDIR /app/src/Excos.AspNetCore.Lite/ClientApp

# Copy package files and install dependencies
COPY src/Excos.AspNetCore.Lite/ClientApp/package.json ./
COPY src/Excos.AspNetCore.Lite/ClientApp/yarn.lock ./
RUN yarn install --frozen-lockfile

# Copy frontend source and build
COPY src/Excos.AspNetCore.Lite/ClientApp/ ./
RUN yarn build

# Stage 2: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build
WORKDIR /app

# Copy solution and project files
COPY Excos.AspNetCore.Lite.slnx ./
COPY src/Excos.AspNetCore.Lite/Excos.AspNetCore.Lite.csproj ./src/Excos.AspNetCore.Lite/
COPY src/Excos.AspNetCore.Lite.TestServer/Excos.AspNetCore.Lite.TestServer.csproj ./src/Excos.AspNetCore.Lite.TestServer/

# Restore dependencies
RUN dotnet restore src/Excos.AspNetCore.Lite.TestServer/Excos.AspNetCore.Lite.TestServer.csproj

# Copy all source code
COPY src/ ./src/

# Copy built frontend assets from previous stage to wwwroot
COPY --from=frontend-build /app/src/Excos.AspNetCore.Lite/wwwroot/app.js ./src/Excos.AspNetCore.Lite/wwwroot/app.js
COPY --from=frontend-build /app/src/Excos.AspNetCore.Lite/wwwroot/app.js.LICENSE.txt ./src/Excos.AspNetCore.Lite/wwwroot/app.js.LICENSE.txt

# Build and publish the application
RUN dotnet publish src/Excos.AspNetCore.Lite.TestServer/Excos.AspNetCore.Lite.TestServer.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:BuildClientApp=false

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published application
COPY --from=dotnet-build /app/publish ./

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "Excos.AspNetCore.Lite.TestServer.dll"]
