using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace MathCore.Generators.MVVM;

// https://dennistretyakov.com/writing-first-roslyn-analyzer-and-codefix-provider

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ViewModelDiagnosticAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MVVM1001";
    private const string __Title = "Class should partial";
    private const string __MessageFormat = "Объявление класса должен быть с модификатором partial";
    private const string __Description = "Добавить partial";
    private const string __Category = "Usage";

    internal static readonly DiagnosticDescriptor MVVM0001NoPartialClass = new(
        DiagnosticId,
        __Title,
        __MessageFormat,
        __Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: __Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MVVM0001NoPartialClass);

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

        if (class_node.IsPartial())
            return;

        var class_location = class_node.GetLocation();
        var keyword_location = class_node.Keyword.GetLocation();
        var error_location = Location.Create(
            class_node.SyntaxTree,
            TextSpan.FromBounds(
                class_location.SourceSpan.Start,
                keyword_location.SourceSpan.End));
        //context.Report(MVVM0001NoPartialClass, keyword_location);
        context.Report(MVVM0001NoPartialClass, error_location);
    }
}

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ViewModelCodeFixProvider)), Shared]
public class ViewModelCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(ViewModelDiagnosticAnalyzer.MVVM0001NoPartialClass.Id);

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root  = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var node  = root!.FindNode(context.Span);
        var class_declaration = root
           .FindToken(context.Diagnostics[0].Location.SourceSpan.Start)
           .Parent!.AncestorsAndSelf()
           .OfType<ClassDeclarationSyntax>()
           .First();

        var document = context.Document;
        //var solution = document.Project.Solution;
        //var document_semantic_model = await document.GetSemanticModelAsync(context.CancellationToken);


        context.RegisterCodeFix(
            CodeAction.Create(
                "Добавить partial",
                createChangedDocument: cancel => FixAsync(document, root, class_declaration, cancel), 
                equivalenceKey: "Add partial"), 
            context.Diagnostics);
    }

    private static Task<Document> FixAsync(Document Document, SyntaxNode Root, ClassDeclarationSyntax Class, CancellationToken Cancel)
    {
        var modifiers = Class.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        var corrected_class = Class.WithModifiers(modifiers).NormalizeWhitespace();

        var corrected_root = Root.ReplaceNode(Class, corrected_class);

        return Task.FromResult(Document.WithSyntaxRoot(corrected_root));
    }

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
}