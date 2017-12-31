#!/bin/sh

finish() {
    echo "Stopping containers..."
    docker-compose -f docker-compose.yml -f docker-compose.ci.test.yml down > /dev/null || true
}

trap finish EXIT

# These must match values in docker-compose*.yml
httpPort=5000
appServiceName=app

echo "Performing tests using containers..."

echo "Running containers..."
docker-compose -f docker-compose.yml -f docker-compose.ci.test.yml up -d

echo 'Waiting for server to start...'
count=0
maxTries=60
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
    docker-compose logs "$appServiceName"
    exit 1;
fi

echo "Server responded with status code $statusCode."
if [ "$statusCode" != "200" ]; then echo 'Test failed.'; exit 1; fi

echo 'Test succeeded.'
