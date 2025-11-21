using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WorkTrack.Common.Analyzers;

/// <summary>
/// Требует file-scoped namespace и контролирует, что в файле только один публичный тип.
/// </summary>
[DiagnosticAnalyzer(firstLanguage: LanguageNames.CSharp)]
public sealed class NamespaceStyleAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Диагностический идентификатор правила file-scoped namespace.
    /// </summary>
    private const string _diagnosticIdFileScoped = "WTI0006";

    /// <summary>
    /// Диагностический идентификатор правила единственного публичного типа.
    /// </summary>
    public const string DiagnosticIdSingleType = "WTI0007";

    private static readonly DiagnosticDescriptor _fileScopedRule = new(
        id: _diagnosticIdFileScoped,
        title: "Используйте file-scoped namespace",
        messageFormat: "Namespace должен быть объявлен в file-scoped стиле",
        category: "WorkTrack.Common.Namespaces",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Все файлы должны использовать file-scoped namespace (namespace Foo.Bar;).");

    private static readonly DiagnosticDescriptor _singleTypeRule = new(
        id: DiagnosticIdSingleType,
        title: "Только один публичный тип в файле",
        messageFormat: "Файл содержит несколько публичных типов: {0}",
        category: "WorkTrack.Common.Structure",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Файл должен содержать максимум один публичный/интерфейсный тип.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_fileScopedRule, _singleTypeRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(analysisMode: GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(action: AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        if (AnalyzerHelpers.IsTestFile(filePath: context.Tree.FilePath))
        {
            return;
        }

        SyntaxNode root = context.Tree.GetRoot(cancellationToken: context.CancellationToken);
        EnforceFileScopedNamespace(context: context, root: root);
        EnforceSinglePublicType(context: context, root: root);
    }

    private static void EnforceFileScopedNamespace(SyntaxTreeAnalysisContext context, SyntaxNode root)
    {
        NamespaceDeclarationSyntax? namespaceDeclaration = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        if (namespaceDeclaration is null)
        {
            return;
        }

        Location location = namespaceDeclaration.Name.GetLocation();
        var diagnostic = DiagnosticFactory.CreateInvalidNamespaceStyle(
            rule: _fileScopedRule,
            location: location,
            namespaceName: namespaceDeclaration.Name.ToString());
        context.ReportDiagnostic(diagnostic: diagnostic);
    }

    private static void EnforceSinglePublicType(SyntaxTreeAnalysisContext context, SyntaxNode root)
    {
        var publicTypes = ExtractTopLevelTypes(root: root)
            .Where(predicate: type => type.Modifiers.Any(kind: SyntaxKind.PublicKeyword) || type.Modifiers.Any(kind: SyntaxKind.InternalKeyword))
            .ToList();

        if (publicTypes.Count <= 1)
        {
            return;
        }

        var names = string.Join(separator: ", ", values: publicTypes.Select(selector: t => t.Identifier.Text));
        var diagnostic = DiagnosticFactory.CreateInvalidNamespaceStyle(
            rule: _singleTypeRule,
            location: publicTypes[index: 1].Identifier.GetLocation(),
            namespaceName: names);
        context.ReportDiagnostic(diagnostic: diagnostic);
    }

    private static IEnumerable<TypeDeclarationSyntax> ExtractTopLevelTypes(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<TypeDeclarationSyntax>()
            .Where(predicate: type => type.Parent is CompilationUnitSyntax or FileScopedNamespaceDeclarationSyntax or NamespaceDeclarationSyntax);
    }

}
