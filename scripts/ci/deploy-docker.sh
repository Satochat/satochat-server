#!/bin/sh

echo "Uploading images..."
echo "$DOCKER_PASSWORD" | docker login -u "$DOCKER_USERNAME" --password-stdin || exit 1
docker-compose -f docker-compose.yml -f docker-compose.ci.prod.yml push app || exit 1
