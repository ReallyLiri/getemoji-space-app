using JetBrains.Space.Client;

namespace GetEmoji.Services.Space;

public interface ISpaceClientProvider
{
    Task<UploadClient> GetUploadClientAsync(string clientId); 
    Task<EmojiClient> GetEmojiClientAsync(string clientId);
    Task<ChatClient> GetChatClientAsync(string clientId);
    Task<ApplicationClient> GetApplicationClientAsync(string clientId);
}