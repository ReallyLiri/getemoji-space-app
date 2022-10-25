using System.ComponentModel;
using GetEmoji.Models.Attributes;
using JetBrains.Annotations;

namespace GetEmoji.Models;

public enum EmojiCommands
{
    [CommandName("help")]
    [Description("Show this help")]
    [UsedImplicitly]
    Help,
    
    [CommandName("search")]
    [Description("Get emoji suggestions from text search, i.e `search stonks`")]
    Search,
    
    [CommandName("random")]
    [Description("Get a random bunch of emojis")]
    GetRandom,
}