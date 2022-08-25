using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MathCore.Generators.Infrastructure.Extensions;

internal static class AttributeDataExtensions
{
    public static T? NamedArgument<T>(this AttributeData? attribute, string ArgumentName, T? DefaultValue = default)
    {
        switch (attribute)
        {
            case null: return DefaultValue;
            case { AttributeClass.Kind: SymbolKind.ErrorType }:
            {
                if (attribute is not { ApplicationSyntaxReference: { } syntax_ref } || syntax_ref.GetSyntax() is not AttributeSyntax { ArgumentList.Arguments: { Count: > 0 } arguments })
                    return DefaultValue;

                foreach (var argument in arguments)
                    if (argument.NameEquals is { Name.Identifier.ValueText: var argument_name } && argument_name == ArgumentName)
                        return (T)((LiteralExpressionSyntax)argument.Expression).Token.Value!;

                return DefaultValue;
            }
            default:
                foreach (var (name, value) in attribute.NamedArguments)
                    if (name == ArgumentName)
                        return (T?)value.Value;

                return DefaultValue;
        }
    }
}