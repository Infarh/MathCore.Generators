namespace MathCore.Generators.Infrastructure.Extensions;

internal static class StringsArrayExtensions
{
    public static string JoinString(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);

    public static string JoinString(this IEnumerable<string> strings) => string.Concat(strings);
}