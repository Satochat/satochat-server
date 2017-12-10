#!/bin/sh

stable=$1
versionSuffix=
buildNumber=$TRAVIS_BUILD_NUMBER
branch=$(echo $TRAVIS_BRANCH | sed -e 's/\//-/g')
gitCommit=$(git describe --long --dirty --always)

if [ "$stable" != "stable" ]; then
    versionSuffix="ci.$buildNumber.$branch.$gitCommit"
fi

if [ ! -z "$versionSuffix" ]; then
    echo $versionSuffix
fi
