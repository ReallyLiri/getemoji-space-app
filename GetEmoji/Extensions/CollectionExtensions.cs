namespace GetEmoji.Extensions;

public static class CollectionExtensions
{
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumerable, T item, bool condition) 
        => condition ? enumerable.Append(item) : enumerable;
}