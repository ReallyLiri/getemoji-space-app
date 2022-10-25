using GetEmoji.Handlers;
using GetEmoji.Models;
using GetEmoji.Services.Emoji;
using GetEmoji.Services.Space;
using JetBrains.Space.Common;
using Refit;

namespace GetEmoji;

public static class ServiceRegistration
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<IAppInstallationStore, AppInstallationStore>();
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

        builder.Services.AddSpaceWebHookHandler<WebhookHandler>();

        return builder;
    }
}