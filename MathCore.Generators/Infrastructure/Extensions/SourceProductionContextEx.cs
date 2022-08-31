namespace MathCore.Generators.Infrastructure.Extensions;

internal static class SourceProductionContextEx
{
    public static void Warning(
        this SourceProductionContext context,
           string Id,
           string Title,
           string Category,
           string Message,
        Location? Location = null,
             bool IsEnabledByDefault = true,
              int WarningLevel = 1) => 
        context.ReportDiagnostic(Diagnostic.Create(
                            id: Id,
                      category: Category,
                       message: Message,
                      severity: DiagnosticSeverity.Warning,
               defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: IsEnabledByDefault,
                  warningLevel: WarningLevel,
                         title: Title, 
                      location: Location));

    public static void Error(
        this SourceProductionContext context,
           string Id,
           string Title,
           string Category,
           string Message,
        Location? Location = null,
             bool IsEnabledByDefault = true,
              int WarningLevel = 0) => 
        context.ReportDiagnostic(Diagnostic.Create(
                            id: Id,
                      category: Category,
                       message: Message,
                      severity: DiagnosticSeverity.Error,
               defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: IsEnabledByDefault,
                  warningLevel: WarningLevel,
                         title: Title, 
                      location: Location));
}