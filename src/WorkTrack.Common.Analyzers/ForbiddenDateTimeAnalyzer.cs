using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WorkTrack.Common.Analyzers;

/// <summary>
/// Предотвращает прямое обращение к DateTime/DateTimeOffset в продуктивном коде.
/// </summary>
[DiagnosticAnalyzer(firstLanguage: LanguageNames.CSharp)]
public sealed class ForbiddenDateTimeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Диагностический идентификатор правила.
    /// </summary>
    public const string DiagnosticId = "WTI0004";

    private static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticId,
        title: "Неиспользуемые напрямую DateTime/DateTimeOffset",
        messageFormat: "Нельзя использовать {0}. Используйте IClock/SystemClock.",
        category: "WorkTrack.Common.Time",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Вместо DateTime.UtcNow/Now нужно использовать IClock/SystemClock для удобства тестирования.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(analysisMode: GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(action: AnalyzeMemberAccess, syntaxKinds: SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        if (AnalyzerHelpers.IsTestAssembly(compilation: context.Compilation))
        {
            return;
        }

        var memberAccess = (MemberAccessExpressionSyntax)context.Node;
        var identifier = memberAccess.Name.Identifier.ValueText;
        if (!IsForbiddenProperty(identifier: identifier))
        {
            return;
        }

        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(expression: memberAccess).Symbol;
        if (symbol is not IPropertySymbol property)
        {
            return;
        }

        if (!IsDateTimeProperty(property: property))
        {
            return;
        }

        Location location = memberAccess.Name.GetLocation();
        if (!location.IsInSource)
        {
            return;
        }

        var fullyQualified = property.ContainingType.ToDisplayString(format: SymbolDisplayFormat.FullyQualifiedFormat) + "." + property.Name;
        var diagnostic = DiagnosticFactory.CreateForbiddenDateTimeProperty(
            rule: _rule,
            location: location,
            propertyName: fullyQualified);
        context.ReportDiagnostic(diagnostic: diagnostic);
    }

    private static readonly string[] ForbiddenProperties = ["Now", "UtcNow"];

    private static bool IsForbiddenProperty(string identifier)
    {
        return Array.IndexOf(array: ForbiddenProperties, value: identifier) >= 0;
    }

    private static bool IsDateTimeProperty(IPropertySymbol property)
    {
        var containingType = property.ContainingType.ToDisplayString();
        return containingType is "System.DateTime" or "System.DateTimeOffset";
    }
}
