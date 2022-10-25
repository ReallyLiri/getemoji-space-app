# getEmoji

## Run DB locally

Deployed app uses Heroku Postgres. To achieve same functionality locally we can use docker:

```sh
docker run --restart unless-stopped -d \
  --name postgres \
  -p 5432:5432 \
  -e POSTGRES_USER=getemoji \
  -e POSTGRES_PASSWORD=getemoji \
  -e POSTGRES_DB=getemoji \
  postgres:latest
```

## Run Docker locally

```sh
docker build -t getemoji .
docker run --rm -it -e PORT=5000 -p 5000:5000 --name getemoji getemoji
```
