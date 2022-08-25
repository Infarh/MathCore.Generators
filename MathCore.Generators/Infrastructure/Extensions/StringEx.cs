namespace MathCore.Generators.Infrastructure.Extensions;

internal static class StringEx
{
    public static string TrimEnd(this string str, string EndString) => str.EndsWith(EndString)
        ? str[..^EndString.Length]
        : str;

    public static IEnumerable<string> EnumLines(this string str)
    {
        var reader = new StringReader(str);
        while (reader.ReadLine() is { } line)
            yield return line;
    }

    public static IEnumerable<T> EnumLines<T>(this string str, Func<string, T> selector) => str.EnumLines().Select(selector);

    public static IEnumerable<T> EnumLines<T>(this string str, Func<string, int, T> selector) => str.EnumLines().Select(selector);

}