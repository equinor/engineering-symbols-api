
SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"

DATASET_NAME="ds"
DB_NAME="db1"
PORT="3030"
IMAGE_NAME="es-api-fuseki"

DB_PATH="$SCRIPT_DIR/databases/$DB_NAME"

echo "Using persistant db at: $DB_PATH"

# Create DB dir if not exists
if [ ! -d "$DB_PATH" ]; then
  mkdir -p "$DB_PATH"
fi

docker run --rm -p $PORT:3030 -v $DB_PATH:/tdb2 $IMAGE_NAME --tdb2 --update --loc /tdb2 /$DATASET_NAME