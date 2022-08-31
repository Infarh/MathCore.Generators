using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace MathCore.Generators.MVVM;

[Generator]
public class NotifyPropertyInfrastructureGenerator : ISourceGenerator
{
    private static readonly DiagnosticDescriptor __InvalidWarning = new(
        id: "MNPChAttrGEN001",
        title: "Add attribute err",
        messageFormat: "Add attribute error '{0}'.",
        category: "NotifyPropertyGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            //string name_space;
            //if (context.Compilation.GetEntryPoint(context.CancellationToken) is not { ContainingNamespace: { } program_namespace })
            //    name_space = "MathCore.Generated.MVVM";
            //else
            //{
            //    name_space = program_namespace.ToDisplayString();

            //    if (name_space is null or "" or "<global namespace>")
            //        name_space = "MathCore.Generated.MVVM";
            //    else if (!name_space.EndsWith(".MVVM"))
            //        name_space += ".MVVM";
            //}

            const string name_space = "MathCore.Generated.MVVM";

            var attribute_source = GetNotifyPropertyAttributeSourceText(name_space.TrimEnd(".MVVM"));
            context.AddSource("NotifyPropertyAttribute.g.cs", attribute_source);

            var command_attribute_source = GetCommandAttributeSourceText(name_space.TrimEnd(".MVVM"));
            context.AddSource("CommandAttribute.g.cs", command_attribute_source);
        }
        catch (Exception e)
        {
            context.ReportDiagnostic(Diagnostic.Create(__InvalidWarning, Location.None, e.Message));
        }
    }

    public static SourceText GetNotifyPropertyAttributeSourceText(string Namespace) => SourceText.From(GetNotifyPropertyAttributeSource(Namespace), Encoding.UTF8);

    public static string GetNotifyPropertyAttributeSource(string NameSpace) => $$"""
// Auto-generated code at {{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}}
namespace {{NameSpace}}.MVVM;

[System.AttributeUsage(System.AttributeTargets.Field)]
//[System.Diagnostics.Conditional("MATHCORE_GENERATORS_DEBUG")] 
internal class NotifyPropertyAttribute : System.Attribute
{
    public string PropertyName { get; init; }

    public bool INotifyPropertyChangesImplementation { get; set; } = true;
}  
""";

    public static SourceText GetCommandAttributeSourceText(string Namespace) => SourceText.From(GetCommandAttributeSource(Namespace), Encoding.UTF8);

    public static string GetCommandAttributeSource(string Namespace) => $$"""
// Auto-generated code at {{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}}
#nullable enable
namespace {{Namespace}}.MVVM;

[System.AttributeUsage(System.AttributeTargets.Method)]
//[System.Diagnostics.Conditional("MATHCORE_GENERATORS_DEBUG")] 
internal class CommandAttribute : System.Attribute 
{
    public string? CommandName { get; set; }

    public string? CanExecuteMethodName { get; set; }

    public Type? CommandType { get; set; }
}
""";
}