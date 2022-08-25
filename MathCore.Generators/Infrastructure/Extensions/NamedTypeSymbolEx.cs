namespace MathCore.Generators.Infrastructure.Extensions;

internal static class NamedTypeSymbolEx
{
    public static IEnumerable<IMethodSymbol> EnumCommandsMethods(this INamedTypeSymbol symbol) =>
        symbol.GetMembers().OfType<IMethodSymbol>().Where(method => !method.IsStatic && method.ReturnsVoid);
}