using GetEmoji;
using GetEmoji.Handlers;

var app = WebApplication.CreateBuilder(args)
    .RegisterServices()
    .Build();
app.MapSpaceWebHookHandler<WebhookHandler>("/api/space/receive");
app.MapGet("/", () => "Space app is running.");
app.Run();