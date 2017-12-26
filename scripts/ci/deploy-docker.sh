#!/bin/sh

echo "Uploading image $DOCKER_TAG..."
echo "$DOCKER_PASSWORD" | docker login -u "$DOCKER_USERNAME" --password-stdin || exit 1
docker push "$DOCKER_TAG" || exit 1
