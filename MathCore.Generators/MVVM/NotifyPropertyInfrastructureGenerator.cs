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

            var command_source = GetICommandImplementationSourceText(name_space);
            context.AddSource("Command.g.cs", command_source);
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
internal class NotifyPropertyAttribute : System.Attribute
{
    public string PropertyName { get; init; }

    public bool INotifyPropertyChangesImplementation { get; set; } = true;
}  
""";

    public static SourceText GetCommandAttributeSourceText(string Namespace) => SourceText.From(GetCommandAttributeSource(Namespace), Encoding.UTF8);

    public static string GetCommandAttributeSource(string Namespace) => $$"""
// Auto-generated code at {{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}}
namespace {{Namespace}}.MVVM;

[System.AttributeUsage(System.AttributeTargets.Method)]
internal class CommandAttribute : System.Attribute 
{
    public string CommandName { get; set; }
}
""";

    public static SourceText GetICommandImplementationSourceText(string Namespace) => SourceText.From(GetICommandImplementationSource(Namespace), Encoding.UTF8);

    public static string GetICommandImplementationSource(string NameSpace) => $$"""
// Auto-generated code at {{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}}
#nullable enable
using System;
using System.Windows.Input;

namespace {{NameSpace}}.Commands.Base
{
    public abstract class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        protected virtual void OnCanExecuteChanged(EventArgs e) => CanExecuteChanged?.Invoke(this, e);

        bool ICommand.CanExecute(object? parameter) => CanExecute(parameter);

        void ICommand.Execute(object? parameter)
        {
            if(!CanExecute(parameter)) return;
            Execute(parameter);
        }

        protected virtual bool CanExecute(object? parameter) => true;

        protected abstract void Execute(object? parameter);
    }
}

namespace {{NameSpace}}.Commands
{
    public class LambdaCommand : Base.Command
    {
        private readonly Action<object?> _Execute;

        private readonly Func<object?, bool>? _CanExecute;

        public LambdaCommand(Action Execute, Func<bool>? CanExecute = null) 
            : this(_ => Execute(), CanExecute is null ? null : _ => CanExecute()) 
        {

        }

        public LambdaCommand(Action<object?> Execute, Func<object?, bool>? CanExecute = null)
        {
            _Execute = Execute;
            _CanExecute = CanExecute;
        }

        protected override bool CanExecute(object? parameter) => base.CanExecute(parameter) && _CanExecute?.Invoke(parameter) == true;

        protected override void Execute(object? parameter) => _Execute(parameter);
    }
}
""";
}