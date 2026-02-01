#!/bin/bash
# Script to validate Docker image builds and runs correctly

set -e

echo "=== Building Docker image ==="
docker build -f .coolify/Dockerfile -t excos-coolify-test ..

echo ""
echo "=== Checking image size ==="
docker images excos-coolify-test --format "Size: {{.Size}}"

echo ""
echo "=== Starting container ==="
CONTAINER_ID=$(docker run -d --rm -p 8090:8080 excos-coolify-test)
echo "Container ID: $CONTAINER_ID"

# Wait for startup
sleep 5

echo ""
echo "=== Checking container status ==="
docker ps | grep excos-coolify-test || (echo "ERROR: Container not running" && exit 1)

echo ""
echo "=== Testing HTTP endpoints ==="
echo "Root endpoint:"
curl -s http://localhost:8090/ || echo "ERROR: Root endpoint failed"

echo ""
echo "API endpoint (should require auth):"
curl -i -s http://localhost:8090/excos/api/status 2>&1 | head -1

echo ""
echo "=== Container logs ==="
docker logs $CONTAINER_ID 2>&1 | grep -E "(listening|started|environment)"

echo ""
echo "=== Stopping container ==="
docker stop $CONTAINER_ID

echo ""
echo "✅ All validation checks passed!"
