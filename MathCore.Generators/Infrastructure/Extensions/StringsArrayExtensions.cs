namespace MathCore.Generators.Infrastructure.Extensions;

internal static class StringsArrayExtensions
{
    public static string JoinStringLN(this IEnumerable<string> strings) => strings.JoinString(Environment.NewLine);

    public static string JoinStringSpace(this IEnumerable<string> strings) => string.Join(" ", strings);

    public static string JoinStringComma(this IEnumerable<string> strings) => string.Join(",", strings);

    public static string JoinString(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);

    public static string JoinString(this IEnumerable<string> strings) => string.Concat(strings);
}