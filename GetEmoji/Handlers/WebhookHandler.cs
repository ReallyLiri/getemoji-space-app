using System.ComponentModel;
using GetEmoji.DB;
using GetEmoji.Extensions;
using GetEmoji.Models;
using GetEmoji.Models.Attributes;
using GetEmoji.Services.Emoji;
using GetEmoji.Services.Space;
using JetBrains.Annotations;
using JetBrains.Space.AspNetCore.Experimental.WebHooks;
using JetBrains.Space.Client;
using Commands = JetBrains.Space.Client.Commands;

namespace GetEmoji.Handlers;

[UsedImplicitly]
public class WebhookHandler : SpaceWebHookHandler
{
    private readonly ILogger<WebhookHandler> _logger;
    private readonly IAppInstallationStore _appInstallationStore;
    private readonly IChatMessageService _chatMessageService;
    private readonly IEmojiSearchService _emojiSearchService;
    private readonly ISpaceEmojiCreateService _spaceEmojiCreateService;

    private static readonly string SearchCommandPrefix = EmojiCommands.Search.GetText<CommandNameAttribute>(_ => _.Name);
    private static readonly string RandomCommandPrefix = EmojiCommands.GetRandom.GetText<CommandNameAttribute>(_ => _.Name);

    public WebhookHandler(
        ILogger<WebhookHandler> logger,
        IAppInstallationStore appInstallationStore,
        IChatMessageService chatMessageService,
        IEmojiSearchService emojiSearchService,
        ISpaceEmojiCreateService spaceEmojiCreateService
    )
    {
        _logger = logger;
        _appInstallationStore = appInstallationStore;
        _chatMessageService = chatMessageService;
        _emojiSearchService = emojiSearchService;
        _spaceEmojiCreateService = spaceEmojiCreateService;
    }

    public override Task<ApplicationExecutionResult> HandleInitAsync(InitPayload payload)
    {
        _logger.LogInformation($"Received {nameof(InitPayload)} event for client-id '{payload.ClientId}'");
        _appInstallationStore.RegisterAppInstallationAsync(new AppInstallation(
            payload.ServerUrl,
            payload.ClientId,
            payload.ClientSecret
        ));
        return base.HandleInitAsync(payload);
    }

    public override async Task<Commands> HandleListCommandsAsync(ListCommandsPayload payload) =>
        new(
            Enum.GetValues<EmojiCommands>()
                .Select(command => new CommandDetail(
                    command.GetText<CommandNameAttribute>(_ => _.Name),
                    command.GetText<DescriptionAttribute>(_ => _.Description))
                )
                .ToList()
        );

    public override async Task HandleMessageAsync(MessagePayload payload)
    {
        _logger.LogInformation($"Received {nameof(MessagePayload)} for client-id '{payload.ClientId}'");
        var messageText = payload.Message.Body as ChatMessageText;
        if (string.IsNullOrEmpty(messageText?.Text))
        {
            _logger.LogInformation("Message is empty");
            return;
        }

        var fullText = messageText.Text.Trim().TrimStart('/');
        if (fullText.StartsWith(SearchCommandPrefix))
        {
            var query = fullText[SearchCommandPrefix.Length..].Trim();
            await GetEmojisAsync(payload.ClientId, payload.UserId, new SearchRequest(query, 0));

            return;
        }

        if (fullText.StartsWith(RandomCommandPrefix))
        {
            await GetEmojisAsync(payload.ClientId, payload.UserId, new SearchRequest(string.Empty, 0));
            return;
        }

        await _chatMessageService.SendHelpMessageAsync(
            payload.ClientId,
            payload.UserId,
            await HandleListCommandsAsync(new ListCommandsPayload { UserId = payload.UserId })
        );
    }

    private async Task GetEmojisAsync(string clientId, string userId, SearchRequest searchRequest)
    {
        var (emojis, hasMore) = await _emojiSearchService.SearchAsync(searchRequest);
        if (emojis.Any())
        {
            await _chatMessageService.SendEmojisResultsAsync(
                clientId,
                userId,
                searchRequest,
                emojis,
                hasMore
            );
        }
        else
        {
            await _chatMessageService.SendNoResultsAsync(
                clientId,
                userId,
                searchRequest.Query
            );
        }
    }

    public override async Task<AppUserActionExecutionResult> HandleMessageActionAsync(MessageActionPayload payload)
    {
        _logger.LogInformation($"Received action: {payload.ActionId} x {payload.ActionValue}");

        switch (payload.ActionId)
        {
            case EmojiActions.AddEmojiActionId:
                await HandleAddEmojiActionAsync(payload);
                break;

            case EmojiActions.GetMoreActionId:
                var searchRequest = payload.ActionValue.FromJson<SearchRequest>();
                await _chatMessageService.DeleteMessageAsync(payload.ClientId, payload.Message);
                await GetEmojisAsync(payload.ClientId, payload.UserId, new SearchRequest(searchRequest.Query, searchRequest.PageNumber + 1));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(payload.ActionId), payload.ActionId, null);
        }

        return await base.HandleMessageActionAsync(payload);
    }

    private async Task HandleAddEmojiActionAsync(MessageActionPayload payload)
    {
        var emoji = payload.ActionValue.FromJson<EmojiDescriptor>();
        var result = await _spaceEmojiCreateService.UploadEmojiAsync(payload.ClientId, emoji);
        switch (result)
        {
            case AddEmojiResult.AlreadyExists:
                await _chatMessageService.SendErrorMessageAsync(
                    payload.ClientId,
                    payload.UserId,
                    $"Sorry! an emoji by the name of `${emoji.Name}` already exists {SpaceBuiltinEmojis.Conflict}"
                );
                break;
            case AddEmojiResult.Failed:
                await _chatMessageService.SendErrorMessageAsync(payload.ClientId, payload.UserId);
                break;
            case AddEmojiResult.Added:
                await _chatMessageService.SendEmojiCreatedAsync(payload.ClientId, payload.UserId, emoji);
                await _chatMessageService.RemoveButtonFromMessageAsync(payload.ClientId, payload.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }
}