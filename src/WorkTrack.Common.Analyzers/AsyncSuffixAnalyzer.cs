using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WorkTrack.Common.Analyzers;

/// <summary>
/// Анализирует методы и требует суффикс Async для Task/ValueTask методов в прод-коде.
/// </summary>
[DiagnosticAnalyzer(firstLanguage: LanguageNames.CSharp)]
public sealed class AsyncSuffixAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Диагностический идентификатор правила.
    /// </summary>
    private const string DiagnosticId = "WTI0002";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Метод асинхронный, но не заканчивается на Async",
        messageFormat: "Асинхронный метод '{0}' должен оканчиваться суффиксом 'Async'. Переименуйте метод.",
        category: "WorkTrack.Common.AsyncNaming",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Все асинхронные методы в продуктивном коде должны заканчиваться на 'Async' для единообразия и читаемости.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(analysisMode: GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(action: AnalyzeMethod, symbolKinds: SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        if (AnalyzerHelpers.IsTestAssembly(compilation: context.Compilation))
        {
            return;
        }

        if (context.Symbol is not IMethodSymbol method)
        {
            return;
        }

        if (!ShouldAnalyzeMethod(method: method))
        {
            return;
        }

        if (!IsAsyncLike(method: method))
        {
            return;
        }

        if (method.Name.EndsWith(value: "Async", comparisonType: StringComparison.Ordinal))
        {
            return;
        }

        if (method.Locations.Length == 0)
        {
            return;
        }

        Location location = method.Locations[index: 0];
        if (!location.IsInSource)
        {
            return;
        }

        var diagnostic = DiagnosticFactory.CreateMissingAsyncSuffix(
            rule: Rule,
            location: location,
            methodName: method.Name);
        context.ReportDiagnostic(diagnostic: diagnostic);
    }

    private static bool ShouldAnalyzeMethod(IMethodSymbol method)
    {
        return method.MethodKind == MethodKind.Ordinary
               && method.Name.IndexOf(value: '<') < 0
               && method is { IsOverride: false, IsAbstract: false, IsExtern: false, IsImplicitlyDeclared: false }
               && !ImplementsInterfaceMember(method: method);
    }

    private static bool IsAsyncLike(IMethodSymbol method)
    {
        if (method.IsAsync)
        {
            return true;
        }

        var returnType = method.ReturnType as INamedTypeSymbol;
        var metadataName = returnType?.ConstructedFrom.ToDisplayString();
        return metadataName is "System.Threading.Tasks.Task" or "System.Threading.Tasks.ValueTask";
    }

    /// <summary>
    /// Проверяет, реализует ли метод член интерфейса.
    /// Оптимизировано для производительности на больших проектах.
    /// </summary>
    /// <param name="method">Символ метода для проверки.</param>
    /// <returns>True, если метод реализует член интерфейса.</returns>
    private static bool ImplementsInterfaceMember(IMethodSymbol method)
    {
        if (method.ExplicitInterfaceImplementations.Length > 0)
        {
            return true;
        }

        // Оптимизация: проверяем только прямые интерфейсы контейнера для улучшения производительности
        foreach (var @interface in method.ContainingType.Interfaces)
        {
            foreach (var member in @interface.GetMembers())
            {
                var implementation = method.ContainingType.FindImplementationForInterfaceMember(interfaceMember: member);
                if (SymbolEqualityComparer.Default.Equals(x: implementation, y: method))
                {
                    return true;
                }
            }
        }

        return false;
    }

}
