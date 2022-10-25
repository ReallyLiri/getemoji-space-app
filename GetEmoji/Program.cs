using GetEmoji;
using GetEmoji.DB;
using GetEmoji.Handlers;

var app = WebApplication.CreateBuilder(args)
    .RegisterServices()
    .Build();
app.MapSpaceWebHookHandler<WebhookHandler>("/api/space/receive");
app.MapGet("/", () => "getEmoji app is running 😋");

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GetEmojiAppContext>();
    await context.InitializeAsync(app.Configuration);
}

app.Run();
