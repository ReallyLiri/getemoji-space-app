using GetEmoji.Models;

namespace GetEmoji.Services.Space;

public interface ISpaceEmojiCreateService
{
    Task<AddEmojiResult> UploadEmojiAsync(string clientId, EmojiDescriptor emoji);
}