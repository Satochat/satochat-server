docker login -u "$DOCKER_USERNAME" -p "$DOCKER_PASSWORD" || exit 1
docker push "$DOCKER_TAG" || exit 1
