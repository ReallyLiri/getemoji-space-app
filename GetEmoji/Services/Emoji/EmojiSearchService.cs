using GetEmoji.Extensions;
using GetEmoji.Models;
using Microsoft.Extensions.Caching.Memory;

namespace GetEmoji.Services.Emoji;

public class EmojiSearchService : IEmojiSearchService
{
    private readonly ISlackmojisClient _slackmojisClient;
    private readonly ISlackmojisParser _slackmojisParser;

    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions
    {
        SizeLimit = 1024
    });

    private static readonly TimeSpan CacheItemExpiration = TimeSpan.FromHours(1);

    private const int SlackmojisPageSize = 24;
    private const int TargetPageSize = 4;

    public EmojiSearchService(ISlackmojisClient slackmojisClient, ISlackmojisParser slackmojisParser)
    {
        _slackmojisClient = slackmojisClient;
        _slackmojisParser = slackmojisParser;
    }

    public async Task<(IReadOnlyCollection<EmojiDescriptor>, bool)> SearchAsync(SearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            var html = await _cache.GetOrAddAsync(
                nameof(_slackmojisClient.HomePageAsync),
                async () => await _slackmojisClient.HomePageAsync(),
                CacheItemExpiration
            );
            var emojis = _slackmojisParser.ParseHomePageResponseAsync(html, TargetPageSize);
            return (emojis, true);
        }
        else
        {
            var emojis = await _cache.GetOrAddAsync(
                request,
                async () =>
                {
                    var slackmojisPageNumber = (request.PageNumber * TargetPageSize) / SlackmojisPageSize;
                    var slackmojisPageOffset = (request.PageNumber * TargetPageSize) % SlackmojisPageSize;
                    var html = await _slackmojisClient.SearchAsync(request.Query, slackmojisPageNumber);
                    return _slackmojisParser.ParseSearchResponseAsync(html, slackmojisPageOffset, TargetPageSize);
                },
                CacheItemExpiration
            );
            return (emojis, emojis.Count >= TargetPageSize);
        }
    }
}