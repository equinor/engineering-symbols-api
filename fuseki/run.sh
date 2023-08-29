#!/bin/bash

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"

# Load variables
source  $SCRIPT_DIR/vars.sh

echo "Using persistant db at: $DB_PATH"

# Create DB dir if not exists
if [ ! -d "$DB_PATH" ]; then
  mkdir -p "$DB_PATH"
fi

docker run --rm -p $PORT:3030 -v $DB_PATH:/tdb2 $IMAGE_NAME --tdb2 --update --loc /tdb2 /$DATASET_NAME
