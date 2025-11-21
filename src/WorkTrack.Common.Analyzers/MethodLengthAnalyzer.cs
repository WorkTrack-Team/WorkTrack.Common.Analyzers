using System;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WorkTrack.Common.Analyzers;

/// <summary>
/// Анализатор, гарантирующий, что тела методов не превышают пяти строк.
/// </summary>
[DiagnosticAnalyzer(firstLanguage: LanguageNames.CSharp)]
public sealed class MethodLengthAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Диагностический идентификатор, используемый для предупреждения о слишком длинных методах.
    /// </summary>
    public const string DiagnosticId = "WTI0001";

    private const int DefaultMaxBodyLines = 5;
    private const string MaxBodyLinesKey = "wt_common_method_length_max_lines";
    private const string TargetPrefixesKey = "wt_common_analyzer_target_prefixes";
    private static readonly string[] DefaultTargetPrefixes = ["WorkTrack."];

    private static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticId,
        title: "Метод слишком длинный",
        messageFormat: "Метод содержит {0} строк внутри тела. Допустимо не более {1} строк.",
        category: "WorkTrack.Common.CodeQuality",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Методы должны быть лаконичными, чтобы поддерживать читабельность и следовать архитектурным требованиям WorkTrack. Настройте через .editorconfig: dotnet_analyzer_diagnostic.wt_common_method_length_max_lines = число.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(analysisMode: GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(action: Analyze, syntaxKinds: new[]{SyntaxKind.MethodDeclaration, SyntaxKind.LocalFunctionStatement});
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (AnalyzerHelpers.IsTestAssembly(compilation: context.Compilation))
        {
            return;
        }

        if (!IsTargetedFile(context: context))
        {
            return;
        }

        switch (context.Node)
        {
            case MethodDeclarationSyntax method:
                Evaluate(body: method.Body, expressionBody: method.ExpressionBody, location: method.Identifier.GetLocation(), context: context);
                break;
            case LocalFunctionStatementSyntax localFunction:
                Evaluate(body: localFunction.Body, expressionBody: localFunction.ExpressionBody, location: localFunction.Identifier.GetLocation(), context: context);
                break;
        }
    }

    private static bool IsTargetedFile(SyntaxNodeAnalysisContext context)
    {
        var path = context.Node.SyntaxTree.FilePath;
        if (string.IsNullOrWhiteSpace(value: path))
        {
            return false;
        }

        var targetPrefixes = GetTargetPrefixes(context: context);
        return targetPrefixes.Any(predicate: prefix => path.Contains(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase));
    }

    private static string[] GetTargetPrefixes(SyntaxNodeAnalysisContext context)
    {
        var config = AnalyzerConfiguration.LoadFrom(
            optionsProvider: context.Options.AnalyzerConfigOptionsProvider,
            syntaxTree: context.Node.SyntaxTree);
        
        return config.TargetPrefixes.ToArray();
    }

    private static void Evaluate(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Location location, SyntaxNodeAnalysisContext context)
    {
        if (expressionBody is not null || body is null)
        {
            return;
        }

        var maxLines = GetMaxBodyLines(context: context);
        var openLine = body.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var closeLine = body.CloseBraceToken.GetLocation().GetLineSpan().EndLinePosition.Line;
        var length = closeLine - openLine - 1;

        if (length > maxLines)
        {
            var diagnostic = DiagnosticFactory.CreateMethodTooLong(
                rule: _rule,
                location: location,
                actualLines: length,
                maxLines: maxLines);
            context.ReportDiagnostic(diagnostic: diagnostic);
        }
    }

    private static int GetMaxBodyLines(SyntaxNodeAnalysisContext context)
    {
        var config = AnalyzerConfiguration.LoadFrom(
            optionsProvider: context.Options.AnalyzerConfigOptionsProvider,
            syntaxTree: context.Node.SyntaxTree);
        
        return config.MaxMethodLines;
    }
}
