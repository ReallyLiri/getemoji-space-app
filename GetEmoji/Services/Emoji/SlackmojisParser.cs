using GetEmoji.Models;
using HtmlAgilityPack;

namespace GetEmoji.Services.Emoji;

public class SlackmojisParser : ISlackmojisParser
{
    public IReadOnlyCollection<EmojiDescriptor> ParseHomePageResponseAsync(string html, int limit)
        => ParseFromXPath(
            html,
            "html/body/ul[@class='groups']//img",
            emojis => emojis
                .OrderBy(_ => Guid.NewGuid())
                .Take(limit)
        );

    public IReadOnlyCollection<EmojiDescriptor> ParseSearchResponseAsync(string html, int skip, int limit)
        => ParseFromXPath(
            html,
            "ul/li/a[@class='downloader']//img",
            emojis => emojis
                .Skip(skip)
                .Take(limit)
        );

    private IReadOnlyCollection<EmojiDescriptor> ParseFromXPath(
        string html,
        string xpath,
        Func<IEnumerable<EmojiDescriptor>, IEnumerable<EmojiDescriptor>> transform
    )
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return transform(ParseImgNodes(doc.DocumentNode.SelectNodes(xpath))).ToList();
    }

    private static IEnumerable<EmojiDescriptor> ParseImgNodes(HtmlNodeCollection nodes) =>
        nodes?.Select(node => new EmojiDescriptor(
            EmojiNameSanitizer.SanitizeName(node.Attributes["title"].Value),
            node.Attributes["src"].Value
        )) ?? Enumerable.Empty<EmojiDescriptor>();
}