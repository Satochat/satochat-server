#!/bin/sh

echo "Setting up SSH configuration..."

scriptDir="$(dirname "$0")"
DEPLOY_GATEWAY_SSH_IDENTITY_FILE=~/.ssh/appserver
DEPLOY_GATEWAY_SSH_IDENTITY_FILE_REPLACEMENT=$(echo "$DEPLOY_GATEWAY_SSH_IDENTITY_FILE" | sed 's/\//\\\//g')
DEPLOY_APPSERVER_SSH_IDENTITY_FILE=$DEPLOY_GATEWAY_SSH_IDENTITY_FILE
DEPLOY_APPSERVER_SSH_IDENTITY_FILE_REPLACEMENT=$DEPLOY_GATEWAY_SSH_IDENTITY_FILE_REPLACEMENT

mkdir ~/.ssh/ 2> /dev/null || true
echo "$DEPLOY_APPSERVER_SSH_PRIVATE_KEY" | base64 -d >> "$DEPLOY_APPSERVER_SSH_IDENTITY_FILE" || exit 1
chmod 0600 "$DEPLOY_APPSERVER_SSH_IDENTITY_FILE" | exit 1
cat "$scriptDir/ssh-config" >> ~/.ssh/config || exit 1

sshConfig=$( \
    cat ~/.ssh/config | \
    sed "s/{GATEWAY_HOST}/$DEPLOY_GATEWAY_SSH_HOST/g" | \
    sed "s/{WAN_HOST}/$DEPLOY_WAN_SSH_HOST/g" | \
    sed "s/{GATEWAY_USER}/$DEPLOY_GATEWAY_SSH_USER/g" | \
    sed "s/{GATEWAY_IDENTITY_FILE}/$DEPLOY_GATEWAY_SSH_IDENTITY_FILE_REPLACEMENT/g" | \
    sed "s/{APPSERVER_HOST}/$DEPLOY_APPSERVER_SSH_HOST/g" | \
    sed "s/{APPSERVER_USER}/$DEPLOY_APPSERVER_SSH_USER/g" | \
    sed "s/{APPSERVER_IDENTITY_FILE}/$DEPLOY_APPSERVER_SSH_IDENTITY_FILE_REPLACEMENT/g" | \
    base64)
echo "$sshConfig" | base64 -d > ~/.ssh/config || exit 1
chmod 0600 ~/.ssh/config | exit 1

echo "Establishing SSH connection to $DEPLOY_APPSERVER_SSH_HOST..."
ssh "$DEPLOY_APPSERVER_SSH_HOST" 'echo OK' || exit 1
