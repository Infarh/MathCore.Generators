using MethodSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax;
using ClassSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;
using System.Collections.Immutable;
using System.Text;

namespace MathCore.Generators.MVVM;

[Generator]
public class CommandGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is ClassSyntax { Members: var members } @class
                    && @class.IsPartial()
                    && !@class.IsStatic()
                    && members.OfType<MethodSyntax>().Any(method => !method.IsStatic() && method.IsCommandHandlerMethod()),
                static (context, _) => context.Node as ClassSyntax)
           .Where(static c => c is not null);

        var compilations = context.CompilationProvider.Combine(classes.Collect());

        context.RegisterSourceOutput(
            compilations,
            static (compilation, source) => Execute(source.Left, source.Right!, compilation));
    }

    private static void Execute(Compilation Compilation, ImmutableArray<ClassSyntax> Classes, SourceProductionContext Context)
    {
        if (Classes.IsDefaultOrEmpty)
            return;

        foreach (var class_syntax in Classes)
        {
            var model = Compilation.GetSemanticModel(class_syntax.SyntaxTree);

            if (model.GetDeclaredSymbol(class_syntax) is not INamedTypeSymbol { IsStatic: false } class_symbol)
                continue;

            var class_name = class_symbol.Name;
            var class_namespace = class_symbol.ContainingNamespace.Name;

            var access_modifiers = class_syntax.EnumAccessModifiers().JoinString(" ");

            var source = new StringBuilder("// Auto-generated code at ")
               .AppendLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"))
               .AppendLine("#nullable disable")
               .Using("System.Windows.Input")
               .Using("MathCore.Generated.MVVM.Commands")
               .AppendLine()
               .Namespace(class_namespace)
               .AppendLine();

            if (access_modifiers is { Length: > 0 })
                source.Append(access_modifiers).Append(' ');
            source.Append("partial class {0}", class_name).LN();
            source.Append("{").LN();

            var is_first = true;
            foreach (var execute_method in class_symbol.EnumCommandsMethods())
                if (execute_method.GetAttributeLike("Command") is { } attribute)
                {
                    if (is_first)
                        is_first = false;
                    else
                        source.LN();

                    //Debugger.Launch();

                    var execute_method_name = execute_method.Name;
                    var command_name = attribute.NamedArgument<string>("CommandName")
                        ?? GetCommandName(execute_method_name);

                    var command_name_trimmed = command_name.EndsWith("Command")
                        ? command_name[..^7]
                        : command_name;

                    string? can_execute_method_name = null;
                    if (class_symbol
                           .GetMembers($"Can{command_name_trimmed}Execute")
                           .OfType<IMethodSymbol>().FirstOrDefault(m => m.Parameters is { Length: 1 }) is { } can_execute_method
                        && can_execute_method.HasAttributeLike("Command"))
                        can_execute_method_name = can_execute_method.Name;

                    if (can_execute_method_name is null && class_symbol
                           .GetMembers()
                           .OfType<IMethodSymbol>()
                           .FirstOrDefault(m => m.GetAttributeLike("Command") is { } attr 
                                && attr.NamedArgument<string>("CommandName") == command_name 
                                && m.ReturnType.Name == "Boolean") is { } other_can_execute_method)
                        can_execute_method_name = other_can_execute_method.Name;

                    source.Append("    #region {0}", command_name).LN();
                    source.LN();
                    source.Append("    private ICommand _{0};", command_name).LN();

                    source.LN();
                    source.Append("    public ICommand {0} => _{0} ??=", command_name).LN();
                    source.Append("        new LambdaCommand(").Append(execute_method_name);
                    if (can_execute_method_name is not null)
                        source.Append(", ").Append(can_execute_method_name);
                    source.Append(");").LN();

                    source.LN();
                    source.Append("    #endregion").LN();
                }

            source.AppendLine("}");

#if DEBUG
            var source_test = source.EnumLines((s, i) => $"{i + 1,3}|{s}").JoinString(Environment.NewLine);
#endif

            Context.AddSource($"{class_name}.commands.g.cs", source.ToSource());
        }
    }

    private static string GetCommandName(string MethodName)
    {
        var result = MethodName;

        if (result.Length > 2 && result.StartsWith("On"))
            result = result[2..];

        if (result.Length > 8 && result.EndsWith("Executed"))
            result = result[..^8];

        return result.EndsWith("Command") ? result : $"{result}Command";
    }
}