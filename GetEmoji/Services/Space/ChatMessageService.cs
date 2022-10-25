using GetEmoji.Models;
using JetBrains.Space.Client;
using GetEmoji.Extensions;

namespace GetEmoji.Services.Space;

public class ChatMessageService : IChatMessageService
{
    private readonly ILogger<ChatMessageService> _logger;
    private readonly ISpaceClientProvider _spaceClientProvider;

    public ChatMessageService(ILogger<ChatMessageService> logger, ISpaceClientProvider spaceClientProvider)
    {
        _logger = logger;
        _spaceClientProvider = spaceClientProvider;
    }

    public async Task SendHelpMessageAsync(string clientId, string recipientUserId, Commands commands)
    {
        _logger.LogInformation($"Sending help message for client-id '{clientId}' and user '{recipientUserId}'");
        await SendMessageAsync(
            clientId,
            recipientUserId,
            BlockMessage(
                MessageSectionElement.MessageSection(
                    header: "List of available commands",
                    elements: new List<MessageBlockElement>
                    {
                        MessageBlockElement.MessageFields(
                            commands.CommandsItems
                                .Select(it => MessageFieldElement.MessageField(it.Name, it.Description))
                                .ToList<MessageFieldElement>())
                    }),
                new MessageOutline("GetEmoji Bot")
            )
        );
    }

    public async Task SendErrorMessageAsync(string clientId, string recipientUserId, string message = null)
    {
        await SendMessageAsync(
            clientId,
            recipientUserId,
            ChatMessage.Text(message ?? $"Something went wrong {SpaceBuiltinEmojis.Sad}")
        );
    }

    public async Task SendNoResultsAsync(string clientId, string recipientUserId, string query)
    {
        _logger.LogInformation($"Found no results for client-id '{clientId}' and user '{recipientUserId}' for query '{query}'");
        await SendMessageAsync(
            clientId,
            recipientUserId,
            ChatMessage.Text($"No results found for `{query}`")
        );
    }

    public async Task SendEmojisResultsAsync(
        string clientId,
        string recipientUserId,
        SearchRequest searchRequest,
        IReadOnlyCollection<EmojiDescriptor> emojis,
        bool hasMore
    )
    {
        _logger.LogInformation($"Found {emojis.Count} results for client-id '{clientId}' and user '{recipientUserId}' for query '{searchRequest.Query}'");
        foreach (var emoji in emojis)
        {
            await SendMessageAsync(
                clientId,
                recipientUserId,
                BlockMessage(
                    MessageSectionElement.MessageSection(
                        elements: GetEmojiMessageGroup(emoji).ToList(),
                        style: MessageStyle.PRIMARY
                    )
                )
            );
        }

        if (hasMore)
        {
            await SendMessageAsync(
                clientId,
                recipientUserId,
                BlockMessage(
                    MessageSectionElement.MessageSection(
                        elements: new List<MessageBlockElement>
                        {
                            GetMoreButton(searchRequest)
                        },
                        style: MessageStyle.SECONDARY
                    )
                )
            );
        }
    }

    public async Task SendEmojiCreatedAsync(string clientId, string recipientUserId, EmojiDescriptor emoji)
    {
        await SendMessageAsync(
            clientId,
            recipientUserId,
            ChatMessage.Text($"Emoji `:{emoji.Name}:` successfully added! :{emoji.Name}:")
        );
    }

    public async Task RemoveButtonFromMessageAsync(string clientId, MessageContext messageContext)
    {
        // It is assumed this message was created by SendEmojisResultsAsync

        if (messageContext.Body is not ChatMessageBlock chatMessageBlock || chatMessageBlock.Sections.Count != 1)
        {
            return;
        }

        if (chatMessageBlock.Sections.First() is not MessageSection section)
        {
            return;
        }

        var modifiedElements = new List<MessageBlockElement>();
        foreach (var element in section.Elements)
        {
            modifiedElements.Add(
                element is MessageControlGroup
                    ? MessageBlockElement.MessageText($"{SpaceBuiltinEmojis.Checkbox} Added to Space")
                    : element
            );
        }

        section.Elements = modifiedElements;

        var chatClient = await _spaceClientProvider.GetChatClientAsync(clientId);
        await chatClient.Messages.EditMessageAsync(
            ChannelIdentifier.Id(messageContext.ChannelId),
            ChatMessageIdentifier.InternalId(messageContext.MessageId),
            messageContext.Body
        );
    }

    public async Task DeleteMessageAsync(string clientId, MessageContext messageContext)
    {
        var chatClient = await _spaceClientProvider.GetChatClientAsync(clientId);
        await chatClient.Messages.DeleteMessageAsync(
            ChannelIdentifier.Id(messageContext.ChannelId),
            ChatMessageIdentifier.InternalId(messageContext.MessageId)
        );
    }

    private static ChatMessage BlockMessage(MessageSectionElement section, MessageOutlineBase outline = null) =>
        ChatMessage.Block(
            outline: outline,
            sections: new List<MessageSectionElement> { section }
        );

    private static IEnumerable<MessageBlockElement> GetEmojiMessageGroup(EmojiDescriptor emoji)
    {
        yield return MessageBlockElement.MessageText(
            $"`:{emoji.Name}:`",
            new MessageImage(emoji.Url)
        );
        yield return MessageBlockElement.MessageControlGroup(
            new List<MessageControlElement>
            {
                MessageControlElement.MessageButton(
                    "Add to Space",
                    MessageButtonStyle.PRIMARY,
                    MessageAction.Post(EmojiActions.AddEmojiActionId, emoji.ToJson())
                )
            }
        );
    }

    private static MessageBlockElement GetMoreButton(SearchRequest searchRequest) =>
        MessageBlockElement.MessageControlGroup(
            new List<MessageControlElement>
            {
                MessageControlElement.MessageButton(
                    "Get more",
                    MessageButtonStyle.PRIMARY,
                    MessageAction.Post(EmojiActions.GetMoreActionId, searchRequest.ToJson())
                )
            }
        );

    private async Task SendMessageAsync(string clientId, string recipientUserId, ChatMessage message)
    {
        var chatClient = await _spaceClientProvider.GetChatClientAsync(clientId);
        await chatClient.Messages.SendMessageAsync(
            recipient: MessageRecipient.Member(ProfileIdentifier.Id(recipientUserId)),
            content: message
        );
    }
}