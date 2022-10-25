using GetEmoji.Models;

namespace GetEmoji.Services.Space;

public class AppInstallationStore : IAppInstallationStore
{
    private readonly AppInstallation _appInstallation;
    
    public AppInstallationStore(IConfiguration configuration)
    {
        _appInstallation = new AppInstallation(
            configuration.GetValue<string>("SERVER_URL"),
            configuration.GetValue<string>("CLIENT_ID"),
            configuration.GetValue<string>("CLIENT_SECRET")
        );
    }

    public Task RegisterAppInstallationAsync(AppInstallation appInstallation)
    {
        throw new NotImplementedException();
    }

    public Task<AppInstallation> GetAppInstallationAsync(string clientId) 
        => Task.FromResult(_appInstallation);
}