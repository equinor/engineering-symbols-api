#!/bin/bash

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"

IMAGE_NAME="es-api-fuseki"
DATASET_NAME="ds"
PORT="3030"
DB_NAME="db1"
DB_PATH="$SCRIPT_DIR/databases/$DB_NAME"