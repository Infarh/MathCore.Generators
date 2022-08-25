namespace MathCore.Generators.Infrastructure.Extensions;

internal static class StringEx
{
    public static string TrimEnd(this string str, string EndString) => str.EndsWith(EndString)
        ? str[..^EndString.Length]
        : str;
}