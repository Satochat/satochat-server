#!/bin/sh

scriptDir="$(dirname "$0")"
"$scriptDir/build.sh" || exit 1
"$scriptDir/test.sh" || exit 1
