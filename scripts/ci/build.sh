#!/bin/bash

finish() {
    rm -f "$dockerLog" || true
}

trap finish EXIT

scriptDir="$(dirname "$0")"
imagesDir=~/docker
dotnetImagePath="$imagesDir/dotnet_image.tar"
dotnetImageTag="microsoft/dotnet:latest"
mysqlImagePath="$imagesDir/mysql_image.tar"
mysqlImageTag="library/mysql:latest"
imageCount=2
imagePaths=("$dotnetImagePath" "$mysqlImagePath")
imageTags=("$dotnetImageTag" "$mysqlImageTag")
newImages=
dockerLog="$(mktemp)"

dotnet publish -c Release --version-suffix $("$scriptDir/version-suffix.sh") || exit 1

# Load cached images
for ((i = 0; i < $imageCount; ++i)); do
    imagePath=${imagePaths[$i]}
    if [ -e "$imagePath" ]; then
        echo "Loading image from $imagePath..."
        docker load -i "$imagePath" || exit 1
    fi
done

# Pull/Update images
for ((i = 0; i < $imageCount; ++i)); do
    imageTag=${imageTags[$i]}

    echo "Pulling image $imageTag..."
    docker pull "$imageTag" | tee "$dockerLog" || exit 1

    newImages[$i]=0
    cat "$dockerLog" | grep "Downloaded newer image for $imageTag" > /dev/null && newImages[$i]=1
done

echo "Building image..."
docker-compose -f docker-compose.yml -f docker-compose.ci.prod.yml build || exit 1

# Save images in order to cache them
for ((i = 0; i < $imageCount; ++i)); do
    imagePath=${imagePaths[$i]}
    imageTag=${imageTags[$i]}
    newImage=${newImages[$i]}
    if [ "$newImage" = "1" ]; then
        echo "Saving image for $imageTag to $imagePath..."
        if [ ! -e "$imagesDir" ]; then mkdir -p "$imagesDir" || exit 1; fi
        docker save "$imageTag" -o "$imagePath" || exit 1
    fi
done

echo 'Build succeeded.'
