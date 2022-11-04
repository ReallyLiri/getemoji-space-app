namespace GetEmoji.Services.Emoji;

public static class EmojiNameSanitizer
{
    private static readonly ISet<char> AllowedSpecialChars = new HashSet<char>
    {
        '_', '-', '+'
    };

    public static string SanitizeName(string name)
        => string.Join("", name.Select(SanitizeChar));

    private static char SanitizeChar(char c) 
        => char.IsLetterOrDigit(c) || AllowedSpecialChars.Contains(c) 
            ? char.ToLowerInvariant(c) 
            : '-';
}