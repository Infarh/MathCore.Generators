namespace MathCore.Generators.Enums;

internal record struct EnumInfo(
    string Name,
    string AccessModifiers,
    string EnumNamespace,
    string? ExtensionsClassName,
    ICollection<string> Values,
    IReadOnlyDictionary<string, string> Descriptions,
    IReadOnlyCollection<EnumFieldInfo> Fields);

internal record struct EnumFieldInfo(
    string Value,
    string? Description,
    string? Summary,
    string? Remarks);