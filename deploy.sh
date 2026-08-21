cd "$(dirname "$0")"
git pull

docker network inspect cogmaster-net >/dev/null 2>&1 || docker network create cogmaster-net

cd "$(dirname "$0")/App"
docker compose up -d
