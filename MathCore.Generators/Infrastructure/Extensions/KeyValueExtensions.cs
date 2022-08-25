namespace MathCore.Generators.Infrastructure.Extensions;

internal static class KeyValueExtensions
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> v, out TKey Key, out TValue Value)
    {
        Key = v.Key;
        Value = v.Value;
    }
}