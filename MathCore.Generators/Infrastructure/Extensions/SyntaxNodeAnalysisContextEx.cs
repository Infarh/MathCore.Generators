using Microsoft.CodeAnalysis.Diagnostics;

namespace MathCore.Generators.Infrastructure.Extensions;

internal static class SyntaxNodeAnalysisContextEx
{
    public static void Report(this SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptior, Location location) => 
        context.ReportDiagnostic(Diagnostic.Create(descriptior, location));
}