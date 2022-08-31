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
                if (attribute is not
                    {
                        ApplicationSyntaxReference: { } syntax_ref
                    } || syntax_ref.GetSyntax() is not AttributeSyntax
                    {
                        ArgumentList.Arguments: { Count: > 0 } arguments
                    })
                    return DefaultValue;

                foreach (var argument in arguments)
                    if (argument.NameEquals is { Name.Identifier.ValueText: var argument_name } && argument_name == ArgumentName)
                        //return (T)((LiteralExpressionSyntax)argument.Expression).Token.Value!;
                        switch (argument.Expression)
                        {
                            case LiteralExpressionSyntax { Token.Value: T t_value }:
                                return t_value;

                            case TypeOfExpressionSyntax { Type: IdentifierNameSyntax { Identifier.Value: var type_identifier } type_info } type_expr:
                                var qq1 = type_expr.ToFullString();
                                var qq2 = type_info.ToFullString();
                                return (T?)type_identifier;
                        }

                return DefaultValue;
            }
            default:
                foreach (var (name, value) in attribute.NamedArguments)
                    if (name == ArgumentName)
                        return (T?)value.Value;

                return DefaultValue;
        }
    }

    public static T? GetNamedArgumentNode<T>(this AttributeData? attribute, string ArgumentName)
        where T : SyntaxNode
    {
        if (attribute is not { ApplicationSyntaxReference: { } app_syntax_ref })
            return null;

        if (app_syntax_ref.GetSyntax() is not AttributeSyntax { ArgumentList.Arguments: { Count: > 0 } args })
            return null;

        foreach (var arg in args)
            if (arg is { NameEquals.Name.Identifier.ValueText: { } arg_name, Expression: T expr } && arg_name == ArgumentName)
                return expr;

        return null;
    }
}