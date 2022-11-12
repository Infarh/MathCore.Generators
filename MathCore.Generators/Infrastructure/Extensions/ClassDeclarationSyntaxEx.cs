using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MathCore.Generators.Infrastructure.Extensions;

internal static class ClassDeclarationSyntaxEx
{
    public static bool IsPartial(this ClassDeclarationSyntax Class) => Class.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

    public static bool IsStatic(this ClassDeclarationSyntax Class) => Class.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

    public static IEnumerable<string> EnumAccessModifiers(this ClassDeclarationSyntax Class) => Class.Modifiers
       .Where(static m => m.IsKind(SyntaxKind.PublicKeyword, SyntaxKind.PrivateKeyword, SyntaxKind.InternalKeyword))
       .Select(static m => m.ValueText);

    public static BaseNamespaceDeclarationSyntax? GetNamespace(this ClassDeclarationSyntax Class)
    {
        SyntaxNode? node = Class;
        while (node is { } and not BaseNamespaceDeclarationSyntax)
            node = node.Parent;
        return node as BaseNamespaceDeclarationSyntax;
    }
}