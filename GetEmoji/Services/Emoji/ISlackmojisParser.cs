using GetEmoji.Models;

namespace GetEmoji.Services.Emoji;

public interface ISlackmojisParser
{
    IReadOnlyCollection<EmojiDescriptor> ParseHomePageResponseAsync(string html, int limit);
    IReadOnlyCollection<EmojiDescriptor> ParseSearchResponseAsync(string html, int skip, int limit);
}