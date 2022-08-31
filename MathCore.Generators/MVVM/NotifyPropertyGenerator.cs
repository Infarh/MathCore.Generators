﻿using FieldSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax;
using ClassSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.Text;

namespace MathCore.Generators.MVVM;

[Generator]
public class NotifyPropertyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider
           .CreateSyntaxProvider(
                static (node, _) => node is ClassSyntax { Members: var members } @class
                    && @class.IsPartial()
                    && !@class.IsStatic()
                    && members.OfType<FieldSyntax>().Any(static field => !field.IsStatic() && !field.IsReadonly() && field.IsNotifyPropertyField()),
                static (context, _) => context.Node as ClassSyntax)
           .Where(static s => s is not null)
           .Collect();

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(classes),
            static (compilation, source) => Execute(source.Left, source.Right!, compilation));
    }

    private static void Execute(Compilation Compilation, ImmutableArray<ClassSyntax> Classes, SourceProductionContext Context)
    {
        if (Classes.IsDefaultOrEmpty)
            return;

        foreach (var class_syntax in Classes)
        {
            var model = Compilation.GetSemanticModel(class_syntax.SyntaxTree);

            if (model.GetDeclaredSymbol(class_syntax) is not { IsStatic: false } class_symbol)
                continue;

            var class_name = class_symbol.Name;
            var class_namespace = class_symbol.ContainingNamespace.Name;

            var access_modifiers = class_syntax.EnumAccessModifiers().JoinString(" ");

            var is_npc = IsNotifyPropertyChangedImplemented(class_symbol);

            if (!is_npc && class_symbol
                   .GetMembers()
                   .OfType<IFieldSymbol>()
                   .Any(
                        static field => !field.IsReadOnly && !field.IsStatic
                            && field.GetAttributeLike("NotifyProperty") is { } attribute
                            && attribute.NamedArgument("INotifyPropertyChangesImplementation", true)))
            {
                Context.AddSource(
                    $"{class_name}.INotifyPropertyChanged.g.cs",
                    GetINPCImplementationSource(class_namespace, class_name, access_modifiers));
                is_npc = true;
            }

            var source = new StringBuilder("// Auto-generated code at ")
               .AppendLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"))
               .AppendLine("#nullable disable")
               .Namespace(class_namespace)
               .AppendLine();

            if (access_modifiers is { Length: > 0 })
                source.Append(access_modifiers).Append(' ');
            source.Append("partial class ").AppendLine(class_name);
            source.AppendLine("{");

            var is_first = true;
            foreach (var field in class_symbol.GetMembers().OfType<IFieldSymbol>().Where(static field => !field.IsReadOnly && !field.IsStatic))
                if (field.GetAttributeLike("NotifyProperty") is { } attribute)
                {
                    if (is_first)
                        is_first = false;
                    else
                        source.AppendLine();

                    var field_name = field.Name;
                    var property_name = attribute.NamedArgument<string>("PropertyName")
                        ?? GetPropertyName(field_name);

                    var field_syntax = class_syntax.Members
                       .OfType<FieldSyntax>()
                       .First(v => v.Declaration.Variables.Any(f => f.Identifier.Text == field_name));

                    //var vvv = field_syntax.GetLeadingTrivia().Where(v => v.IsKind(SyntaxKind.SingleLineCommentTrivia)).ToArray();

                    var comment = field_syntax.GetLeadingTrivia().ToString();

                    var property_type = field.Type.ToDisplayString();

                    source.AddNotifyProperty(property_type, field_name, property_name, !is_npc, comment);
                }

            source.AppendLine("}");

#if DEBUG
            var source_test = source.EnumLines(static (s, i) => $"{i + 1,3}|{s}").JoinString(Environment.NewLine);
#endif

            Context.AddSource($"{class_name}.properties.g.cs", source.ToSource());
        }
    }

    private static bool IsNotifyPropertyChangedImplemented(INamedTypeSymbol ClassDefinition)
    {
        for (var class_definition = ClassDefinition; class_definition is not null; class_definition = class_definition.BaseType)
        {
            var interfaces = class_definition.Interfaces;
            if (interfaces.Length > 0 && interfaces.Any(static v => v.Name == nameof(INotifyPropertyChanged)))
                return true;
        }

        return false;
    }

    private static string GetPropertyName(string FieldName) => Regex.Replace(FieldName.TrimStart('_'), "(^|_)[a-zA-Z]", m => char.ToUpper(m.Value.Length == 1 ? m.Value[0] : m.Value[1]).ToString());

    private static SourceText GetINPCImplementationSource(string Namespace, string Class, string AccessModifier) => SourceText.From($$"""
// Auto-generated code at {{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}}
#nullable enable
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace {{Namespace}};

{{(AccessModifier is { Length: > 0 } ? $"{AccessModifier} " : null)}}partial class {{Class}} : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
    }

    protected virtual bool Set<T>([NotNullIfNotNull("value")] ref T field, T value, [CallerMemberName] string PropertyName = null!)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(PropertyName);
        return true;
    }
}
""", Encoding.UTF8);
}