using GetEmoji.Extensions;
using GetEmoji.Models;

namespace GetEmoji.Services.Space;

public class SpaceEmojiCreateService : ISpaceEmojiCreateService
{
    private readonly ILogger<SpaceEmojiCreateService> _logger;
    private readonly ISpaceClientProvider _spaceClientProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string UploadMediaPrefix = "emoji";

    public SpaceEmojiCreateService(
        ILogger<SpaceEmojiCreateService> logger,
        ISpaceClientProvider spaceClientProvider,
        IHttpClientFactory httpClientFactory
    )
    {
        _logger = logger;
        _spaceClientProvider = spaceClientProvider;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AddEmojiResult> UploadEmojiAsync(string clientId, EmojiDescriptor emoji)
    {
        try
        {
            _logger.LogInformation($"For client-id '{clientId}' adding emoji '{emoji}'");
            var emojiClient = await _spaceClientProvider.GetEmojiClientAsync(clientId);
            var emojiExists = await emojiClient.CheckIfEmojiExistsAsync(emoji.Name);
            if (emojiExists)
            {
                _logger.LogInformation($"Emoji '{emoji}' already exists");
                return AddEmojiResult.AlreadyExists;
            }

            var attachmentId = await UploadAsAttachmentAsync(clientId, emoji);
            if (string.IsNullOrEmpty(attachmentId))
            {
                _logger.LogInformation($"Failed to upload emoji '{emoji}'");
                return AddEmojiResult.Failed;
            }

            _logger.LogInformation($"Created attachment '{attachmentId}' from emoji '{emoji}'");
            await emojiClient.AddEmojiAsync(emoji.Name, attachmentId);
            return AddEmojiResult.Added;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Failed to create emoji '{emoji}'");
            return AddEmojiResult.Failed;
        }
    }

    private async Task<string> UploadAsAttachmentAsync(string clientId, EmojiDescriptor emoji)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        await using var contentStream = await httpClient.GetStreamAsync(emoji.Url);
        var uploadClient = await _spaceClientProvider.GetUploadClientAsync(clientId);
        return await uploadClient.UploadImageAsync(UploadMediaPrefix, GetFileName(emoji), contentStream);
    }

    private string GetFileName(EmojiDescriptor emoji) => emoji.Url.Split('/').Last().Split('?').First();
}