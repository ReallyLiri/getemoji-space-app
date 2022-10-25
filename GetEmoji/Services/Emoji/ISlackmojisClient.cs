using Refit;

namespace GetEmoji.Services.Emoji;

public interface ISlackmojisClient
{
    const string BaseAddress = "https://slackmojis.com";
    
    [Get("")]
    Task<string> HomePageAsync();

    [Get("/emojis/search")]
    Task<string> SearchAsync(string query, int page);
}