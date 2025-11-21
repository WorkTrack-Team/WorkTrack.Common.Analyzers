using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using WorkTrack.Common.Analyzers;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<WorkTrack.Common.Analyzers.MethodLengthAnalyzer>;
using AnalyzerTestState = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<
    WorkTrack.Common.Analyzers.MethodLengthAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace WorkTrack.Common.Analyzers.Tests;

/// <summary>
/// Unit-тесты для <see cref="MethodLengthAnalyzer"/>.
/// </summary>
public sealed class MethodLengthAnalyzerTests
{
    /// <summary>
    /// Проверяет, что анализатор сообщает о методе, превышающем лимит строк.
    /// </summary>
    [Fact]
    public async Task MethodExceedingLimit_ReportsDiagnostic()
    {
        var test = @"
namespace WorkTrack.Test;

public class TestClass
{
    public void {|#0:LongMethod|}()
    {
        var x = 1;
        var y = 2;
        var z = 3;
        var a = 4;
        var b = 5;
        var c = 6; // 6 lines > 5
    }
}
";

        var expected = VerifyCS.Diagnostic(MethodLengthAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments(6, 5);

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает о методе в пределах лимита.
    /// </summary>
    [Fact]
    public async Task MethodWithinLimit_NoDiagnostic()
    {
        var test = @"
namespace WorkTrack.Test;

public class TestClass
{
    public void ShortMethod()
    {
        var x = 1;
        var y = 2;
        var z = 3; // 3 lines <= 5
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    /// <summary>
    /// Проверяет, что expression-bodied методы не проверяются.
    /// </summary>
    [Fact]
    public async Task ExpressionBodiedMethod_NoDiagnostic()
    {
        var test = @"
namespace WorkTrack.Test;

public class TestClass
{
    public int GetValue() => 42;
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    /// <summary>
    /// Проверяет, что анализатор читает конфигурацию из .editorconfig.
    /// </summary>
    [Fact]
    public async Task CustomMaxLinesFromEditorConfig_RespectsConfiguration()
    {
        var test = @"
namespace WorkTrack.Test;

public class TestClass
{
    public void Method()
    {
        var x = 1;
        var y = 2;
        var z = 3;
        var a = 4;
        var b = 5;
        var c = 6;
        var d = 7; // 7 lines, но лимит увеличен до 10
    }
}
";

        var testState = new AnalyzerTestState
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
        };

        // Добавляем конфигурацию через .editorconfig
        testState.TestState.AnalyzerConfigFiles.Add(
            ("/.editorconfig", @"
root = true

[*.cs]
wt_common_method_length_max_lines = 10
"));

        await testState.RunAsync();
    }
}

