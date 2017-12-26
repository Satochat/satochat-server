#!/bin/sh

finish() {
    echo "Stopping container $containerName..."
    docker stop "$containerName" > /dev/null || true
}

trap finish EXIT

httpPort=5000
dbProvider=sqlite
connectionString='Data Source=satochat.sqlite'
containerName=satochat-server-test

echo "Performing tests using container $containerName..."

echo "Running container $containerName..."
docker run --rm --name "$containerName" -d -p "$httpPort:5000" -e SATOCHAT_DB_PROVIDER="$dbProvider" -e SATOCHAT_DB_CONNECTION_STRING="$connectionString" "$DOCKER_TAG" || exit 1

echo 'Waiting for server to start...'
count=0
maxTries=30
statusCode=0
while [ $count -lt $maxTries ]; do
    count=$((count+1))
    sleep 1
    statusCode=$(curl -s -w '%{http_code}' "http://localhost:$httpPort/up") || continue
    echo $statusCode | grep -P '^\d+$' > /dev/null || continue
    statusCode=$((statusCode))
    if [ "$statusCode" = "0" ]; then continue; fi
    break
done

if [ "$count" = "$maxTries" ]; then
    echo "Aborting after $maxTries tries."
    docker logs "$containerName"
    exit 1;
fi

echo "Server responded with status code $statusCode."
if [ "$statusCode" != "200" ]; then echo 'Test failed.'; exit 1; fi

echo 'Test succeeded.'
