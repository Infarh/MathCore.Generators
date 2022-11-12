using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MathCore.Generators.Infrastructure.Extensions;

internal static class EnumDeclarationSyntaxEx
{
    public static bool IsContainsAttributeLike(this EnumDeclarationSyntax Enum, string AttributeName)
    {
        if (Enum.AttributeLists.Count == 0) return false;

        foreach (var attribute in Enum.AttributeLists.SelectMany(a => a.Attributes))
            if (attribute.Name.ToString().Contains(AttributeName))
                return true;

        return false;
    }

    public static IEnumerable<string> EnumAccessModifiers(this EnumDeclarationSyntax Class) => Class.Modifiers
       .Where(m => m.IsKind(SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword))
       .Select(m => m.ValueText);
}