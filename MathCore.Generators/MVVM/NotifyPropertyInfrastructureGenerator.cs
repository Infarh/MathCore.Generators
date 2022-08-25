using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace MathCore.Generators.MVVM;

[Generator]
public class NotifyPropertyInfrastructureGenerator : ISourceGenerator
{
    public const string NotifyAttributeName = "NotifyProperty";
    public const string NotifyAttributePropertyName = "PropertyName";
    public const string NotifyAttributeINotifyPropertyChangesImplementation = "INotifyPropertyChangesImplementation";
    public const string CommandAttributeName = "Command";
    public const string CommandName = "CommandName";

    public const string NotifyPropertyAttributeName = $"{NotifyAttributeName}Attribute";
    public const string CommandAttributeAttributeName = $"{CommandAttributeName}Attribute";


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
            if (context.Compilation.GetEntryPoint(context.CancellationToken) is not { ContainingNamespace: { } program_namespace })
                return;

            var name_space = program_namespace.ToDisplayString();

            var attribute_source = GetNotifyPropertyAttributeSourceText(name_space);
            context.AddSource($"NotifyPropertyAttribute[{DateTime.Now:yy-MM-ddTHH-mm-ss}].g.cs", attribute_source);

            var command_source = GetICommandImplementationSourceText("Generated.MVVM");
            context.AddSource($"Command[{DateTime.Now:yy-MM-ddTHH-mm-ss}].g.cs", command_source);

            var command_attribute_source = GetCommandAttributeSourceText(name_space);
            context.AddSource($"CommandAttribute[{DateTime.Now:yy-MM-ddTHH-mm-ss}].g.cs", command_attribute_source);
        }
        catch (Exception e)
        {
            context.ReportDiagnostic(Diagnostic.Create(__InvalidWarning, Location.None, e.Message));
        }
    }

    public static SourceText GetNotifyPropertyAttributeSourceText(string Namespace) => SourceText.From(GetNotifyPropertyAttributeSource(Namespace), Encoding.UTF8);

    public static string GetNotifyPropertyAttributeSource(string NameSpace) =>
        $@"// Auto-generated code at {DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}
namespace {NameSpace}.MVVM;

[System.AttributeUsage(System.AttributeTargets.Field)]
internal class {NotifyPropertyAttributeName} : System.Attribute
{{
    public string {NotifyAttributePropertyName} {{ get; init; }}

    public bool {NotifyAttributeINotifyPropertyChangesImplementation} {{ get; set; }} = true;
}}";

    public static SourceText GetICommandImplementationSourceText(string Namespace) => SourceText.From(GetICommandImplementationSource(Namespace), Encoding.UTF8);

    public static string GetICommandImplementationSource(string NameSpace) =>
        $@"// Auto-generated code at {DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}
#nullable enable
using System;
using System.Windows.Input;

namespace {NameSpace}.Commands.Base
{{
    public abstract class Command : ICommand
    {{
        public event EventHandler? CanExecuteChanged;

        protected virtual void OnCanExecuteChanged(EventArgs e) => CanExecuteChanged?.Invoke(this, e);

        bool ICommand.CanExecute(object? parameter) => CanExecute(parameter);

        void ICommand.Execute(object? parameter)
        {{
            if(!CanExecute(parameter)) return;
            Execute(parameter);
        }}

        protected virtual bool CanExecute(object? parameter) => true;

        protected abstract void Execute(object? parameter);
    }}
}}

namespace {NameSpace}.Commands
{{
    public class LambdaCommand : Base.Command
    {{
        private readonly Action<object?> _Execute;

        private readonly Func<object?, bool>? _CanExecute;

        public LambdaCommand(Action Execute, Func<bool>? CanExecute = null) 
            : this(_ => Execute(), CanExecute is null ? null : _ => CanExecute()) 
        {{

        }}

        public LambdaCommand(Action<object?> Execute, Func<object?, bool>? CanExecute = null)
        {{
            _Execute = Execute;
            _CanExecute = CanExecute;
        }}

        protected override bool CanExecute(object? parameter) => base.CanExecute(parameter) && _CanExecute?.Invoke(parameter) == true;

        protected override void Execute(object? parameter) => _Execute(parameter);
    }}
}}
";

    public static SourceText GetCommandAttributeSourceText(string Namespace) => SourceText.From(GetCommandAttributeSource(Namespace), Encoding.UTF8);

    public static string GetCommandAttributeSource(string Namespace) =>
        $@"// Auto-generated code at {DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}
namespace {Namespace}.MVVM;

[System.AttributeUsage(System.AttributeTargets.Method)]
internal class {CommandAttributeAttributeName} : System.Attribute 
{{ 
    public string {CommandName} {{ get; set; }}
}}";
}