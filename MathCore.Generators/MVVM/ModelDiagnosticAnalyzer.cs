using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MathCore.Generators.MVVM;

// https://dennistretyakov.com/writing-first-roslyn-analyzer-and-codefix-provider

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ViewModelDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private const string __DiagnosticId = "MVVMModelDiagnostic";
    private const string __Title = "Class mast be partial";
    private const string __MessageFormat = "Класс должен быть partial";
    private const string __Description = "Добавить partial";
    private const string __Category = "Usage";

    private static readonly DiagnosticDescriptor __Rule = new(
        __DiagnosticId,
        __Title,
        __MessageFormat,
        __Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: __Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(__Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeCommandsInNoPartialClass, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeCommandsInNoPartialClass(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax
        {
            AttributeLists: { Count: > 0 } attributes, 
            Parent: ClassDeclarationSyntax { } class_node
        })
            return;

        if (!attributes
               .SelectMany(a => a.Attributes)
               .Any(a => a.Name.ToFullString() is "Command" or "CommandAttribute"))
            return;

        if(class_node.IsPartial())
            return;

        context.Report(__Rule, class_node.GetLocation());
    }
}

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ViewModelCodeFixProvider)), Shared]
public class ViewModelCodeFixProvider : CodeFixProvider
{
    internal static readonly DiagnosticDescriptor MVVM0001NoPartialClass = new(
        id: "MVVM1001",
        title: "Model class should partial",
        messageFormat: "{0} class should partial",
        category: "MVVM",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<string> FixableDiagnosticIds { get; }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        //context.RegisterCodeFix(CommandsInNoPartialClass, Diagnostic.Create("MVVMcf001", "MVVM", "No partial class", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true));
    }
}