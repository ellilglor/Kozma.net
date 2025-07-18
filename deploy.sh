cd /root/Kozma.net
git pull

docker network inspect cogmaster-net >/dev/null 2>&1 || docker network create cogmaster-net

cd /root/Kozma.net/App
docker compose up -d
