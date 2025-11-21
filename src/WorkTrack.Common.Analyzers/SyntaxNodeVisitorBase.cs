using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WorkTrack.Common.Analyzers;

/// <summary>
/// Базовый класс для визиторов синтаксических узлов.
/// Применяет Visitor Pattern для обхода синтаксического дерева.
/// </summary>
/// <typeparam name="TContext">Тип контекста анализа.</typeparam>
internal abstract class SyntaxNodeVisitorBase<TContext> : CSharpSyntaxVisitor<Diagnostic?>
    where TContext : struct
{
    /// <summary>
    /// Контекст анализа.
    /// </summary>
    protected TContext Context { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="context">Контекст анализа.</param>
    protected SyntaxNodeVisitorBase(TContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Анализирует узел метода.
    /// </summary>
    /// <param name="node">Узел объявления метода.</param>
    /// <returns>Диагностика или null, если проблем не обнаружено.</returns>
    public override Diagnostic? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        return AnalyzeMethod(node);
    }

    /// <summary>
    /// Анализирует локальную функцию.
    /// </summary>
    /// <param name="node">Узел локальной функции.</param>
    /// <returns>Диагностика или null, если проблем не обнаружено.</returns>
    public override Diagnostic? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        return AnalyzeLocalFunction(node);
    }

    /// <summary>
    /// Анализирует объявление метода.
    /// </summary>
    /// <param name="node">Узел объявления метода.</param>
    /// <returns>Диагностика или null, если проблем не обнаружено.</returns>
    protected abstract Diagnostic? AnalyzeMethod(MethodDeclarationSyntax node);

    /// <summary>
    /// Анализирует локальную функцию.
    /// </summary>
    /// <param name="node">Узел локальной функции.</param>
    /// <returns>Диагностика или null, если проблем не обнаружено.</returns>
    protected virtual Diagnostic? AnalyzeLocalFunction(LocalFunctionStatementSyntax node) => null;
}

