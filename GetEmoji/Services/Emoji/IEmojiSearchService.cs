using GetEmoji.Models;

namespace GetEmoji.Services.Emoji;

public interface IEmojiSearchService
{
    Task<(IReadOnlyCollection<EmojiDescriptor>, bool)> SearchAsync(SearchRequest request);
}