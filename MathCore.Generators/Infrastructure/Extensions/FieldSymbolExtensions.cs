namespace MathCore.Generators.Infrastructure.Extensions;

internal static class FieldSymbolExtensions
{
    public static void Deconstruct(this IFieldSymbol field, out string FieldName, out string FieldType)
    {
        FieldName = field.Name;
        FieldType = field.Type.Name;
    }
}