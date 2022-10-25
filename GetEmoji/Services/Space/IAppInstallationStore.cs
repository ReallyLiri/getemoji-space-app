using GetEmoji.Models;

namespace GetEmoji.Services.Space;

public interface IAppInstallationStore
{
    Task RegisterAppInstallationAsync(AppInstallation appInstallation);
    Task<AppInstallation> GetAppInstallationAsync(string clientId);
}
