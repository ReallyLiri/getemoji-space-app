# getEmoji

![logo](https://i.imgur.com/gnZgeLs.png)

Search and immediately add emoji to your JetBrains Space chats!

Powered by [slackmojis](https://slackmojis.com)

![commands](https://i.imgur.com/EwPgByj.png)

![gif](https://i.imgur.com/M2ahQzk.gif)

## Debug locally

Copy [GetEmoji/Properties/launchSettings.template.json](GetEmoji/Properties/launchSettings.template.json) to `GetEmoji/Properties/launchSettings.json` and fill in the missing app installation details.

Use `ngrok` to communicate between Space and your server, use the endpoint `https://<ngrok>/api/space`.

## Install to Space

### Install manually

Grant `Add custom emoji, View custom emoji` permissions in the **Global Authorization** settings.

### Install from link

Open your browser to

```sh
BASE_URL="https://<ngrok>"
https://jetbrains.com/space/app/install-app?name=getEmoji&endpoint=$BASE_URL/api/space
```

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

## Run server with Docker

```sh
docker build -t getemoji .
docker run --rm -it -e PORT=5000 -p 5000:5000 --name getemoji getemoji
```
