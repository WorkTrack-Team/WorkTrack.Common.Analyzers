using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WorkTrack.Common.Analyzers;

/// <summary>
/// Контролирует наличие Guard.Against в начале публичных методов.
/// </summary>
[DiagnosticAnalyzer(firstLanguage: LanguageNames.CSharp)]
public sealed class GuardClauseAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Диагностический идентификатор.
    /// </summary>
    public const string DiagnosticId = "WTI0005";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Метод должен начинаться с Guard.Against",
        messageFormat: "Метод '{0}' должен начинаться с вызова Guard.Against для входных аргументов",
        category: "WorkTrack.Common.Guards",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Публичные и internal методы/конструкторы обязаны проверять входные аргументы через Guard.Against.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(analysisMode: GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            action: AnalyzeMethod,
            syntaxKinds: new[] { SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration });
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (AnalyzerHelpers.IsTestAssembly(compilation: context.Compilation))
        {
            return;
        }

        switch (context.Node)
        {
            case MethodDeclarationSyntax method when ShouldValidate(modifiers: method.Modifiers):
                if (!TryGetSymbol(context: context, method: method, symbol: out IMethodSymbol? methodSymbol) || !RequiresGuard(method: methodSymbol!))
                {
                    return;
                }
                ValidateBody(context: context, body: method.Body, parameters: method.ParameterList);
                break;
            case ConstructorDeclarationSyntax ctor when ShouldValidate(modifiers: ctor.Modifiers):
                if (!TryGetSymbol(context: context, ctor: ctor, symbol: out IMethodSymbol? ctorSymbol) || !RequiresGuard(method: ctorSymbol!))
                {
                    return;
                }
                ValidateBody(context: context, body: ctor.Body, parameters: ctor.ParameterList);
                break;
        }
    }

    private static bool ShouldValidate(SyntaxTokenList modifiers)
    {
        if (modifiers.Any(kind: SyntaxKind.PrivateKeyword) || modifiers.Any(kind: SyntaxKind.ProtectedKeyword))
        {
            return false;
        }

        return modifiers.Any(kind: SyntaxKind.PublicKeyword) || modifiers.Any(kind: SyntaxKind.InternalKeyword);
    }

    private static void ValidateBody(SyntaxNodeAnalysisContext context, BlockSyntax? body, ParameterListSyntax parameters)
    {
        if (body is null || body.Statements.Count == 0)
        {
            return;
        }

        if (parameters.Parameters.Count == 0)
        {
            return;
        }

        StatementSyntax firstStatement = body.Statements.First();
        if (firstStatement is not ExpressionStatementSyntax expressionStatement)
        {
            Report(context: context, location: parameters.GetLocation());
            return;
        }

        if (!IsGuardCall(expression: expressionStatement.Expression))
        {
            Report(context: context, location: expressionStatement.GetLocation());
        }
    }

    private static bool IsGuardCall(ExpressionSyntax expression)
    {
        if (expression is AwaitExpressionSyntax awaitExpression)
        {
            expression = awaitExpression.Expression;
        }

        if (expression is InvocationExpressionSyntax invocation && invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var guardType = memberAccess.Expression.ToString();
            var memberName = memberAccess.Name.Identifier.ValueText;
            return guardType is "Guard.Against" or "GuardAgainst" && !string.IsNullOrWhiteSpace(value: memberName);
        }

        return false;
    }

    private static void Report(SyntaxNodeAnalysisContext context, Location location)
    {
        var methodName = context.ContainingSymbol?.Name ?? "method";
        var diagnostic = DiagnosticFactory.CreateMissingGuardClause(
            rule: Rule,
            location: location,
            methodName: methodName);
        context.ReportDiagnostic(diagnostic: diagnostic);
    }


    private static bool TryGetSymbol(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method, out IMethodSymbol? symbol)
    {
        symbol = context.SemanticModel.GetDeclaredSymbol(declarationSyntax: method, cancellationToken: context.CancellationToken);
        return symbol is not null;
    }

    private static bool TryGetSymbol(SyntaxNodeAnalysisContext context, ConstructorDeclarationSyntax ctor, out IMethodSymbol? symbol)
    {
        symbol = context.SemanticModel.GetDeclaredSymbol(declarationSyntax: ctor, cancellationToken: context.CancellationToken);
        return symbol is not null;
    }

    private static bool RequiresGuard(IMethodSymbol method)
    {
        foreach (IParameterSymbol parameter in method.Parameters)
        {
            if (parameter.RefKind != RefKind.None)
            {
                continue;
            }

            if (parameter.Type.TypeKind == TypeKind.TypeParameter)
            {
                continue;
            }

            if (parameter.Type.SpecialType == SpecialType.System_String && parameter.NullableAnnotation != NullableAnnotation.Annotated)
            {
                return true;
            }

            if (parameter.Type.IsReferenceType && parameter.NullableAnnotation != NullableAnnotation.Annotated)
            {
                return true;
            }
        }

        return false;
    }
}
