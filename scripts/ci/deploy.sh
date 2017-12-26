#!/bin/sh

scriptDir="$(dirname "$0")"
"$scriptDir/deploy-docker.sh" || exit 1
"$scriptDir/setup-server-ssh.sh" || exit 1
"$scriptDir/deploy-server.sh" || exit 1
