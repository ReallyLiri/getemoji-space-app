namespace GetEmoji.Models;

public record EmojiDescriptor(string Name, string Url)
{
    public override string ToString() => Name;
}