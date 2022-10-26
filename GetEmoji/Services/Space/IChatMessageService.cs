using GetEmoji.Models;
using JetBrains.Space.Client;

namespace GetEmoji.Services.Space;

public interface IChatMessageService
{
    Task SendHelpMessageAsync(string clientId, string recipientUserId, Commands commands);
    Task SendErrorMessageAsync(string clientId, string recipientUserId, string errorMessage = null);
    Task SendNoResultsAsync(string clientId, string recipientUserId, string query);
    Task SendEmojisResultsAsync(string clientId, string recipientUserId, SearchRequest searchRequest, IReadOnlyCollection<EmojiDescriptor> emojis, bool hasMore);
    Task SendEmojiCreatedAsync(string clientId, string recipientUserId, EmojiDescriptor emoji);
    Task SendRequestPermissionsMessageAsync(string clientId, string recipientUserId);
    Task RemoveButtonFromMessageAsync(string clientId, MessageContext messageContext);
    Task DeleteMessageAsync(string clientId, MessageContext messageContext);
}