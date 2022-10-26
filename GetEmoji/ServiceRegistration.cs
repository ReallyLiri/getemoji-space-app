using System.Text.RegularExpressions;
using GetEmoji.DB;
using GetEmoji.Handlers;
using GetEmoji.Services.Emoji;
using GetEmoji.Services.Space;
using JetBrains.Space.Common;
using Microsoft.EntityFrameworkCore;
using Refit;

namespace GetEmoji;

public static class ServiceRegistration
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<ISpaceClientProvider, SpaceClientProvider>();

        builder.Services.AddSingleton<Func<AppInstallation, Connection>>(
            serviceProvider =>
                appInstallation => new ClientCredentialsConnection(
                    new Uri(appInstallation.ServerUrl),
                    appInstallation.ClientId,
                    appInstallation.ClientSecret,
                    serviceProvider.GetService<IHttpClientFactory>().CreateClient()
                )
        );

        builder.Services.AddRefitClient<ISlackmojisClient>()
            .ConfigureHttpClient(_ => _.BaseAddress = new Uri(ISlackmojisClient.BaseAddress));

        builder.Services.AddSingleton<IEmojiSearchService, EmojiSearchService>();

        builder.Services.AddSingleton<ISlackmojisParser, SlackmojisParser>();

        builder.Services.AddSingleton<IChatMessageService, ChatMessageService>();

        builder.Services.AddSingleton<ISpaceEmojiCreateService, SpaceEmojiCreateService>();

        builder.Services.AddSingleton<IPermissionRequestService, PermissionRequestService>();
        
        builder.Services.AddSpaceWebHookHandler<WebhookHandler>();

        builder.Services.AddSingleton<IAppInstallationStore, AppInstallationStore>();
        builder.Services.AddDbContextFactory<GetEmojiAppContext>(options => options.UseNpgsql(GetConnectionString()));

        return builder;
    }

    private static string GetConnectionString()
    {
        var connectionSting = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        if (!string.IsNullOrEmpty(connectionSting))
        {
            return connectionSting;
        }

        var m = Regex.Match(Environment.GetEnvironmentVariable("DATABASE_URL")!, @"postgres://(.*):(.*)@(.*):(.*)/(.*)");
        return $"Server={m.Groups[3]};Port={m.Groups[4]};User Id={m.Groups[1]};Password={m.Groups[2]};Database={m.Groups[5]};sslmode=Prefer;Trust Server Certificate=true";
    }
}