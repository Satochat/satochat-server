#!/bin/sh

containerName=satochat-server
dbHostName="$SATOCHAT_DB_HOST"
dbHostLine=$(ssh "$DEPLOY_APPSERVER_SSH_HOST" "host -4 -t A $dbHostName")
dbHostIp=$(echo $dbHostLine | cut -d ' ' -f 4)
echo $dbHostIp | grep -P '^(\d{1,3}\.?){4}$' > /dev/null || (echo "IP address ($dbHostIp) for $dbHostName appears to be wrong."; exit 1)
dbHostMapping="$dbHostName:$dbHostIp"

echo "Database host mapping: $dbHostMapping"
echo "Deploying to $DEPLOY_APPSERVER_SSH_HOST..."

echo "Pulling image $DOCKER_TAG..."
ssh "$DEPLOY_APPSERVER_SSH_HOST" "docker pull '$DOCKER_TAG'" || exit 1

ssh "$DEPLOY_APPSERVER_SSH_HOST" "docker ps -q -f name='^/$containerName$'" | grep .
if [ $? = 0 ]; then
    echo "Stopping container $containerName..."
    ssh "$DEPLOY_APPSERVER_SSH_HOST" "docker stop '$containerName'" || exit 1
fi

ssh "$DEPLOY_APPSERVER_SSH_HOST" "docker ps -q -a -f name='^/$containerName$'" | grep .
if [ $? = 0 ]; then
    echo "Removing container $containerName..."
    ssh "$DEPLOY_APPSERVER_SSH_HOST" "docker rm '$containerName'" || exit 1
fi

echo "Creating container $containerName..."
ssh "$DEPLOY_APPSERVER_SSH_HOST" "docker create --restart unless-stopped --name '$containerName' --add-host '$dbHostMapping' -p '$SATOCHAT_HTTP_PORT:5000' -e SATOCHAT_DB_PROVIDER='$SATOCHAT_DB_PROVIDER' -e SATOCHAT_DB_CONNECTION_STRING='$SATOCHAT_DB_CONNECTION_STRING' '$DOCKER_TAG'" || exit 1

echo "Running container $containerName..."
ssh "$DEPLOY_APPSERVER_SSH_HOST" "docker start '$containerName'" || exit 1

echo "Performing tests against deployed site ($SATOCHAT_WAN_HOST)..."

echo 'Waiting for server to start...'
count=0
maxTries=30
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
