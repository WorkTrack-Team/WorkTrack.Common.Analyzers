using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using WorkTrack.Common.Analyzers;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<WorkTrack.Common.Analyzers.GuardClauseAnalyzer>;
using AnalyzerTestState = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<
    WorkTrack.Common.Analyzers.GuardClauseAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace WorkTrack.Common.Analyzers.Tests;

/// <summary>
/// Unit-тесты для <see cref="GuardClauseAnalyzer"/>.
/// </summary>
public sealed class GuardClauseAnalyzerTests
{
    /// <summary>
    /// Проверяет, что анализатор сообщает об отсутствии Guard.Against в публичном методе с параметрами.
    /// </summary>
    [Fact]
    public async Task PublicMethodWithoutGuard_ReportsDiagnostic()
    {
        var test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method(string {|#0:param|})
    {
        Console.WriteLine(param);
    }
}
";

        var expected = VerifyCS.Diagnostic(GuardClauseAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("Method");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке, если метод начинается с Guard.Against.
    /// </summary>
    [Fact]
    public async Task PublicMethodWithGuard_NoDiagnostic()
    {
        var test = @"
using Ardalis.GuardClauses;

namespace TestNamespace;

public class TestClass
{
    public void Method(string param)
    {
        Guard.Against.Null(param);
        Console.WriteLine(param);
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет приватные методы.
    /// </summary>
    [Fact]
    public async Task PrivateMethod_NoDiagnostic()
    {
        var test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    private void Method(string param)
    {
        Console.WriteLine(param);
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет методы без параметров.
    /// </summary>
    [Fact]
    public async Task MethodWithoutParameters_NoDiagnostic()
    {
        var test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method()
    {
        Console.WriteLine(""Hello"");
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}

