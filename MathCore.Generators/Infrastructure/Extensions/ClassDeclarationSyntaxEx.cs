using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MathCore.Generators.Infrastructure.Extensions;

internal static class ClassDeclarationSyntaxEx
{
    public static bool IsPartial(this ClassDeclarationSyntax Class) => Class.Modifiers.Any(m => m.ValueText == "partial");

    public static bool IsStatic(this ClassDeclarationSyntax Class) => Class.Modifiers.Any(m => m.ValueText == "static");

    public static IEnumerable<string> EnumAccessModifiers(this ClassDeclarationSyntax Class) => Class.Modifiers
       .Select(static m => m.ValueText)
       .Where(static m => m is "public" or "private" or "internal");

    public static BaseNamespaceDeclarationSyntax? GetNamespace(this ClassDeclarationSyntax Class)
    {
        SyntaxNode? node = Class;
        while (node is { } and not BaseNamespaceDeclarationSyntax)
            node = node.Parent;
        return node as BaseNamespaceDeclarationSyntax;
    }
}