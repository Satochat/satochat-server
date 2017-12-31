#!/bin/sh

finish() {
    echo "Stopping containers..."
    if [ ! -z "$remoteTempDir" ]; then
        rm -rf "$remoteTempDir" > /dev/null || true
    fi
}

trap finish EXIT

echo "Deploying to $DEPLOY_APPSERVER_SSH_HOST..."

remoteTempDir=$(ssh "$DEPLOY_APPSERVER_SSH_HOST" "mktemp -d") || exit 1
echo "Remote temporary directory: $remoteTempDir"

cat docker-compose.ci.prod.yml | envsubst | tee docker-compose.ci.prod.yml
scp docker-compose.yml docker-compose.ci.prod.yml Dockerfile "$DEPLOY_APPSERVER_SSH_HOST:$remoteTempDir" || exit 1

echo "Stopping containers..."
ssh "$DEPLOY_APPSERVER_SSH_HOST" "cd '$remoteTempDir' && docker-compose -p '$COMPOSE_PROJECT_NAME' -f docker-compose.yml -f docker-compose.ci.prod.yml down" || exit 1

echo "Running containers..."
ssh "$DEPLOY_APPSERVER_SSH_HOST" "cd '$remoteTempDir' && docker-compose -p '$COMPOSE_PROJECT_NAME' -f docker-compose.yml -f docker-compose.ci.prod.yml up -d" || exit 1

echo "Performing tests against deployed site ($SATOCHAT_WAN_HOST)..."

echo 'Waiting for server to start...'
count=0
maxTries=60
statusCode=0
while [ $count -lt $maxTries ]; do
    count=$((count+1))
    sleep 1
    statusCode=$(curl -s -w '%{http_code}' "https://$SATOCHAT_WAN_HOST/up") || continue
    echo $statusCode | grep -P '^\d+$' > /dev/null || continue
    statusCode=$((statusCode))
    if [ "$statusCode" = "0" ]; then continue; fi
    break
done

if [ "$count" = "$maxTries" ]; then
    echo "Aborting after $maxTries tries."
    exit 1;
fi

echo "Server responded with status code $statusCode."
if [ "$statusCode" != "200" ]; then echo 'Test failed.'; exit 1; fi

# TODO: Roll back on failure

echo 'Test succeeded.'
echo 'Deployment succeeded.'
