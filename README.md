# getEmoji

## Deploy to Heroku

```sh
docker build -t getemoji .
# verify it works:
docker run --rm -it -e PORT=5000 -p 5000:5000 --name getemoji getemoji
heroku container:push getemoji
heroku container:release getemoji
```
