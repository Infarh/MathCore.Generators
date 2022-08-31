namespace MathCore.Generators.Infrastructure.Extensions;

internal static class TypeInfoEx
{
    public static bool HasInterface(this TypeInfo type, string InterfaceName) => 
        type is { Type.AllInterfaces: { Length: > 0 } all_interfaces } &&
        Enumerable.Any(all_interfaces, i => i.ToDisplayString() == InterfaceName);
}