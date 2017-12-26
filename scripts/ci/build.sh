#!/bin/sh

finish() {
    rm -f "$dockerLog" || true
}

trap finish EXIT

scriptDir="$(dirname "$0")"
dotnetDir=~/docker
dotnetImagePath="$dotnetDir/dotnet_image.tar"
dotnetImageTag="microsoft/dotnet:latest"
dockerLog="$(mktemp)"

dotnet publish -c Release --version-suffix $("$scriptDir/version-suffix.sh") || exit 1
if [ -e "$dotnetImagePath" ]; then
    echo "Loading image from $dotnetImagePath..."
    docker load -i "$dotnetImagePath" || exit 1
fi

echo "Pulling image $dotnetImageTag..."
docker pull "$dotnetImageTag" | tee "$dockerLog" || exit 1

newDotnetImage=0
cat "$dockerLog" | grep "Downloaded newer image for $dotnetImageTag" > /dev/null && newDotnetImage=1

echo "Building image $DOCKER_TAG..."
docker build -t "$DOCKER_TAG" . || exit 1

if [ "$newDotnetImage" = "1" ]; then
    echo "Saving image for $dotnetImageTag to $dotnetImagePath..."
    if [ ! -e "$dotnetDir" ]; then mkdir -p "$dotnetDir" || exit 1; fi
    docker save microsoft/dotnet -o "$dotnetImagePath" || exit 1
fi

echo 'Build succeeded.'
