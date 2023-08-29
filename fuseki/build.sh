#!/bin/bash

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"

# Load variables
export vars.sh

# Build fuseki docker image

SCRIPT_LOCATION=$(dirname $0)
BASE_LOCATION=$(dirname $SCRIPT_LOCATION)
BUILD_LOCATION="$BASE_LOCATION/build"

DOCKER_REPO="https://repo1.maven.org/maven2/org/apache/jena/jena-fuseki-docker"
FUSEKI_VER="4.9.0"
FUSEKI_LOCATION="$BUILD_LOCATION/jena-fuseki-docker-$FUSEKI_VER"
ZIP_LOCATION="$FUSEKI_LOCATION.zip"
IMAGE_URL="https://repo1.maven.org/maven2/org/apache/jena/jena-fuseki-docker/$FUSEKI_VER/jena-fuseki-docker-$FUSEKI_VER.zip"


# Checks
commands=("docker" "unzip")
for cmd in ${commands[@]}; do
    if ! command -v $cmd &> /dev/null; then
        >&2 echo "$cmd must be installed"
        exit 1
    fi
done

# Clean
[ -d "$BUILD_LOCATION" ] && rm -r $BUILD_LOCATION
mkdir -p $BUILD_LOCATION

# Download
echo "curl $IMAGE_URL -o $ZIP_LOCATION"
curl $IMAGE_URL -o $ZIP_LOCATION
echo $(cat $(basename "$ZIP_LOCATION.md5")) "$ZIP_LOCATION" | md5sum -c
unzip $ZIP_LOCATION -d $BUILD_LOCATION

# Build fuseki image
docker build --force-rm --platform linux/x86_64 --build-arg JENA_VERSION=$FUSEKI_VER -t $IMAGE_NAME $FUSEKI_LOCATION