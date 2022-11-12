using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MathCore.Generators.Infrastructure.Extensions;

internal static class FieldDeclarationSyntaxEx
{
    public static bool ExistAttribute(this FieldDeclarationSyntax field, string AttributeName) => field.AttributeLists.ExistAttribute(AttributeName);

    public static bool ContainsAttributeText(this FieldDeclarationSyntax field, string AttributeName) => field.AttributeLists.ContainsAttributeText(AttributeName);

    public static bool IsNotifyPropertyField(this FieldDeclarationSyntax field) => field.ContainsAttributeText("NotifyProperty");

    public static bool IsStatic(this FieldDeclarationSyntax field) => field.Modifiers.Any(static m => m.IsKind(SyntaxKind.StaticKeyword));

    public static bool IsReadonly(this FieldDeclarationSyntax field) => field.Modifiers.Any(static m => m.IsKind(SyntaxKind.ReadOnlyKeyword));
}

internal static class MethodDeclarationSyntaxEx
{
    public static bool ExistAttribute(this MethodDeclarationSyntax method, string AttributeName) => method.AttributeLists.ExistAttribute(AttributeName);

    public static bool ContainsAttributeText(this MethodDeclarationSyntax method, string AttributeName) => method.AttributeLists.ContainsAttributeText(AttributeName);

    public static bool IsCommandHandlerMethod(this MethodDeclarationSyntax method) => method.ContainsAttributeText("Command");

    public static bool IsStatic(this MethodDeclarationSyntax method) => method.Modifiers.Any(static m => m.IsKind(SyntaxKind.StaticKeyword));
}