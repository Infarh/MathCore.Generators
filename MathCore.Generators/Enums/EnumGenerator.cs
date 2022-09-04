using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;
using System.ComponentModel;
using System.Text;
using System.Xml.Linq;

namespace MathCore.Generators.Enums;

[Generator]
public class EnumGenerator : IIncrementalGenerator
{
    private static SourceText GetEnumTransformAttributeSourceText() => SourceText.From($$"""
// Auto-generated code at {{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}}
namespace MathCore.Generators.Enums;

[System.AttributeUsage(System.AttributeTargets.Enum)]
[System.Diagnostics.Conditional("MATHCORE_GENERATORS_DEBUG")]
internal class EnumTransformAttribute : System.Attribute 
{
    public string ExtensionsClassName { get; set; }
}
""", Encoding.UTF8);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Добавить атрибут
        context.RegisterPostInitializationOutput(c => c.AddSource("EnumTransformAttribute.g.cs", GetEnumTransformAttributeSourceText()));

        // Фильтрация перечислений
        var enums = context.SyntaxProvider
           .CreateSyntaxProvider(
                static (n, _) => n is EnumDeclarationSyntax e && e.IsContainsAttributeLike("EnumTransform"),
                static (c, _) => c.Node as EnumDeclarationSyntax)
           .Where(static m => m is not null)
           .Select(static (m, _) => m!);

        // Генерация исходных кодов используя скомпилированные перечисления и перечисления из исходного кода
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(enums.Collect()),
            static (compilation, source) => Execute(source.Left, source.Right, compilation));
    }

    private static void Execute(
                                  Compilation Compilation,
        ImmutableArray<EnumDeclarationSyntax> Enums,
                      SourceProductionContext Context)
    {
        if (Enums.IsDefaultOrEmpty)
            return;

        if (GetTypesToGenerate(Compilation, Enums, Context.CancellationToken) is not { Count: > 0 } enum_to_generate)
            return;

        var result = GenerateExtensionsClasses(enum_to_generate);
        foreach (var (name, source) in result)
            Context.AddSource($"{name}.g.cs", source);
    }

    private static ICollection<EnumInfo> GetTypesToGenerate(
                               Compilation Compilation,
     ImmutableArray<EnumDeclarationSyntax> Enums,
                         CancellationToken Cancel)
    {
        const string attribute_name = "EnumTransformAttribute";

        var enums = new List<EnumInfo>();
        foreach (var enum_syntax in Enums)
        {
            Cancel.ThrowIfCancellationRequested();

            var model = Compilation.GetSemanticModel(enum_syntax.SyntaxTree);
            if (model.GetDeclaredSymbol(enum_syntax, Cancel) is not INamedTypeSymbol enum_symbol)
                continue;

            if (enum_symbol.GetAttribute(attribute_name) is not { } transform_attribute)
                continue;

            var extensions_class_name = transform_attribute.NamedArgument<string>("ExtensionsClassName");

            var members = enum_symbol.GetMembers();

            var fields_infos = new List<EnumFieldInfo>(members.Length);
            var enum_members = new List<string>(members.Length);
            var descriptions = new Dictionary<string, string>();
            foreach (var member in members)
                if (member is IFieldSymbol
                    {
                        ConstantValue: { },
                        Name: { } name,
                        DeclaringSyntaxReferences: [{ } syntax_ref]
                    })
                {
                    enum_members.Add(name);

                    var field_syntax = syntax_ref.GetSyntax();

                    var leading_trivia = field_syntax.GetLeadingTrivia().ToFullString();

                    var comment_xml_str = new StringBuilder("<comments>").LN();
                    foreach (var comment_str in leading_trivia.EnumLines())
                        comment_xml_str.LN(comment_str.TrimStart().TrimStart(' ', '/', '\r', '\n'));
                    comment_xml_str.Length -= Environment.NewLine.Length;
                    comment_xml_str.LN("</comments>");

                    var comment_xml = XDocument.Parse(comment_xml_str.ToString());

                    var summary_str = comment_xml.Descendants("summary").Select(n => n.Value.Trim()).JoinStringLN();
                    var remarks_str = comment_xml.Descendants("remarks").Select(n => n.Value.Trim()).JoinStringLN();

                    string? field_description = null;
                    if (member.GetAttribute(nameof(DescriptionAttribute)) is { ConstructorArguments: { Length: > 0 } arguments } &&
                        arguments.FirstOrDefault() is { Value: string description })
                        descriptions.Add(name, field_description = description);

                    fields_infos.Add(new(
                              Value: name,
                        Description: field_description,
                            Summary: summary_str,
                            Remarks: remarks_str
                        ));
                }

            var enum_name = enum_symbol.Name;
            var enum_namespace = enum_symbol.ContainingNamespace.ToDisplayString();
            var enum_access_modifiers = enum_syntax.EnumAccessModifiers().JoinStringSpace();
            enums.Add(new(
                               Name: enum_name,
                    AccessModifiers: enum_access_modifiers,
                      EnumNamespace: enum_namespace,
                ExtensionsClassName: extensions_class_name,
                             Values: enum_members,
                       Descriptions: descriptions,
                             Fields: fields_infos));
        }

        return enums;
    }

    private static IEnumerable<(string ExtensionsName, SourceText Source)> GenerateExtensionsClasses(IEnumerable<EnumInfo> Enums)
    {
        foreach (var (enum_name, enum_access_modifiers, enum_namespace, extensions_class_name, members, descriptions, fields) in Enums)
        {
            var class_name = extensions_class_name ?? $"{enum_name}Ex";

            var source = new StringBuilder()
               .Append("// Autogenerated code at {0:dd.MM.yyyy HH:mm:ss}", DateTime.Now)
               .Nullable()
               .AppendLine()
               .Namespace(enum_namespace)
               .AppendLine();

            if (enum_access_modifiers is { Length: > 0 })
                source.Append(enum_access_modifiers).Append(' ');

            source.Append("static class ").LN(class_name);
            source.Append('{').LN();

            GenerateIsDefined(source, enum_name, members);
            GenerateParse(source.LN(), enum_name, members);
            GenerateTryParse(source.LN(), enum_name, members);
            GenerateToStringFast(source.LN(), enum_name, members);
            GenerateGetDescription(source.LN(), enum_name, descriptions);
            GenerateGetNames(source.LN(), enum_name, members);
            GenerateGetValues(source.LN(), enum_name, members);
            GenerateEnumerateNames(source.LN(), enum_name, members);
            GenerateEnumerateValues(source.LN(), enum_name, members);
            GenerateGetValuesCount(source.LN(), members.Count);

            if (fields.Any(f => f.Summary is { Length: > 0 }))
                GenerateGetSummary(source.LN(), enum_name, fields.Where(f => f.Summary is { Length: > 0 }).Select(f => (f.Value, f.Summary!)));

            if (fields.Any(f => f.Remarks is { Length: > 0 }))
                GenerateGetRemarks(source.LN(), enum_name, fields.Where(f => f.Remarks is { Length: > 0 }).Select(f => (f.Value, f.Remarks!)));

            source.Append('}').LN();

#if DEBUG
            var source_test = source.ToNumeratedLinesString();
#endif

            yield return (enum_name, source.ToSource());
        }
    }

    private static void GenerateGetDescription(
                              StringBuilder Source,
                                     string EnumName,
        IReadOnlyDictionary<string, string> Descriptions)
    {
        if (Descriptions.Count == 0) return;

        Source.Append("    public static string GetDescription(this {0} value) => value switch", EnumName).LN();
        Source.Append("    {").LN();

        foreach (var (member, description) in Descriptions)
            Source.LN("        {0}.{1} => \"{2}\",", EnumName, member, description);

        Source.Append("        _ => value.ToString()").LN();
        Source.Append("    };").LN();
    }

    private static void GenerateToStringFast(StringBuilder Source, string EnumName, IEnumerable<string> Members)
    {
        Source.Append("    public static string ToStringFast(this {0} value) => value switch", EnumName).LN();
        Source.Append("    {").LN();

        foreach (var member in Members)
            Source.LN("        {0}.{1} => nameof({0}.{1}),", EnumName, member);

        Source.Append("        _ => value.ToString()").LN();
        Source.Append("    };").LN();
    }

    private static void GenerateParse(StringBuilder Source, string EnumName, IEnumerable<string> Members)
    {
        Source.Append("    public static {0} Parse(string str) => str switch", EnumName).LN();
        Source.Append("    {").LN();

        foreach (var member in Members)
            Source.LN("        nameof({0}.{1}) => {0}.{1},", EnumName, member);

        Source.Append("        _ => throw new System.ArgumentOutOfRangeException(nameof(str), str, $\"Значение {str} не поддерживается\")").LN();
        Source.Append("    };").LN();
    }

    private static void GenerateTryParse(StringBuilder Source, string EnumName, IEnumerable<string> Members)
    {
        Source.Append("    public static bool TryParse(string str, out {0} value)", EnumName).LN();
        Source.Append("    {").LN();
        Source.Append("        switch(str)").LN();
        Source.Append("        {").LN();
        Source.Append("            default:").LN();
        Source.Append("                value = default;").LN();
        Source.Append("                return false;").LN();

        foreach (var member in Members)
        {
            Source.LN();
            Source.LN("            case nameof({0}.{1}):", EnumName, member);
            Source.LN("                value = {0}.{1};", EnumName, member);
            Source.LN("                return true;");
        }

        Source.Append("        }").LN();
        Source.Append("    }").LN();
    }

    private static void GenerateIsDefined(StringBuilder Source, string EnumName, IEnumerable<string> Members)
    {
        Source.Append("    public static bool IsDefined(string str)").LN();
        Source.Append("    {").LN();
        Source.Append("        switch(str)").LN();
        Source.Append("        {").LN();
        Source.Append("            default:").LN();
        Source.Append("                return false;").LN();

        Source.LN();
        foreach (var member in Members)
            Source.LN("            case nameof({0}.{1}):", EnumName, member);
        Source.Append("                return true;").LN();

        Source.Append("        }").LN();
        Source.Append("    }").LN();
    }

    private static void GenerateGetNames(StringBuilder Source, string EnumName, IEnumerable<string> Members)
    {
        Source.Append("    public static string[] GetNames() => new[]").LN();
        Source.Append("    {").LN();

        foreach (var member in Members)
            Source.LN("        nameof({0}.{1}),", EnumName, member);

        Source.Append("    };").LN();
    }

    private static void GenerateEnumerateNames(StringBuilder Source, string EnumName, IEnumerable<string> Members)
    {
        Source.Append("    public static IEnumerable<string> EnumerateNames()").LN();
        Source.Append("    {").LN();

        foreach (var member in Members)
            Source.LN("        yield return nameof({0}.{1});", EnumName, member);

        Source.Append("    }").LN();
    }

    private static void GenerateGetValues(StringBuilder Source, string EnumName, IEnumerable<string> Members)
    {
        Source.Append("    public static {0}[] GetValues() => new[]", EnumName).LN();
        Source.Append("    {").LN();

        foreach (var member in Members)
            Source.LN("        {0}.{1},", EnumName, member);

        Source.Append("    };").LN();
    }

    private static void GenerateEnumerateValues(StringBuilder Source, string EnumName, IEnumerable<string> Members)
    {
        Source.Append("    public static IEnumerable<{0}> EnumerateValues()", EnumName).LN();
        Source.Append("    {").LN();

        foreach (var member in Members)
            Source.LN("        yield return {0}.{1};", EnumName, member);

        Source.Append("    }").LN();
    }

    private static void GenerateGetValuesCount(StringBuilder Source, int ValuesCount) => Source
       .LN("    public static int GetValuesCount() => {0};", ValuesCount);

    private static void GenerateGetSummary(
                                          StringBuilder Source,
                                                 string EnumName,
        IEnumerable<(string Field, string Summary)> Summaries)
    {
        Source.Append("    public static string? GetSummary(this {0} value) => value switch", EnumName).LN();
        Source.Append("    {").LN();

        foreach (var (field, summary) in Summaries)
            Source.LN("        {0}.{1} => @\"{2}\",", EnumName, field, summary.Replace("\"", "\"\""));

        Source.Append("        _ => null").LN();
        Source.Append("    };").LN();
    }

    private static void GenerateGetRemarks(
                                          StringBuilder Source,
                                                 string EnumName,
        IEnumerable<(string Field, string Remarks)> Remarks)
    {
        Source.Append("    public static string? GetRemarks(this {0} value) => value switch", EnumName).LN();
        Source.Append("    {").LN();

        foreach (var (field, remarks) in Remarks)
            Source.LN("        {0}.{1} => @\"{2}\",", EnumName, field, remarks.Replace("\"", "\"\""));

        Source.Append("        _ => null").LN();
        Source.Append("    };").LN();
    }
}