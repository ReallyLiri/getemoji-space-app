using GetEmoji.DB;
using JetBrains.Space.Client;
using JetBrains.Space.Common;

namespace GetEmoji.Services.Space;

public class SpaceClientProvider : ISpaceClientProvider
{
    private readonly IAppInstallationStore _appInstallationStore;
    private readonly Func<AppInstallation, Connection> _connectionBuilder;

    public SpaceClientProvider(IAppInstallationStore appInstallationStore, Func<AppInstallation, Connection> connectionBuilder)
    {
        _appInstallationStore = appInstallationStore;
        _connectionBuilder = connectionBuilder;
    }

    public async Task<UploadClient> GetUploadClientAsync(string clientId) => new(await GetConnectionAsync(clientId));

    public async Task<EmojiClient> GetEmojiClientAsync(string clientId) => new(await GetConnectionAsync(clientId));

    public async Task<ChatClient> GetChatClientAsync(string clientId) => new(await GetConnectionAsync(clientId));

    public async Task<ApplicationClient> GetApplicationClientAsync(string clientId) => new(await GetConnectionAsync(clientId));

    private async Task<Connection> GetConnectionAsync(string clientId)
        => _connectionBuilder(await _appInstallationStore.GetAppInstallationAsync(clientId));
}