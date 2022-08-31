namespace MathCore.Generators.Infrastructure.Extensions;

internal static class TypeInfoEx
{
    public static bool HasInterface(this TypeInfo type, string InterfaceName)
    {
        foreach (var @interface in type.Type.AllInterfaces)
            if (@interface.ToDisplayString() == InterfaceName)
                return true;

        return false;
    }
}