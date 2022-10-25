using Microsoft.EntityFrameworkCore;

namespace GetEmoji.DB;

public class AppInstallationStore : IAppInstallationStore
{
    private readonly IDbContextFactory<GetEmojiAppContext> _appContextFactory;

    public AppInstallationStore(IDbContextFactory<GetEmojiAppContext> appContextFactory)
    {
        _appContextFactory = appContextFactory;
    }

    public async Task RegisterAppInstallationAsync(AppInstallation appInstallation)
    {
        await using var appContext = await _appContextFactory.CreateDbContextAsync();
        await appContext.AddAsync(appInstallation);
    }

    public async Task<AppInstallation> GetAppInstallationAsync(string clientId)
    {
        await using var appContext = await _appContextFactory.CreateDbContextAsync();
        return await appContext.GetAsync(clientId);
    }
}