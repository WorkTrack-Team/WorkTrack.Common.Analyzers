using Microsoft.CodeAnalysis.Testing;
using Xunit;
// Используем DefaultVerifier для совместимости с разными версиями xUnit
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<
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
        const string test = @"
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

        var testState = new VerifyCS 
        { 
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        
        // Устанавливаем явное имя файла с префиксом WorkTrack для срабатывания анализатора
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        
        // Добавляем конфигурацию для анализатора
        testState.TestState.AnalyzerConfigFiles.Add(
            ("/.editorconfig", @"
root = true

[*.cs]
wt_common_method_length_max_lines = 5
wt_common_analyzer_target_prefixes = WorkTrack.
"));
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError(MethodLengthAnalyzer.DiagnosticId)
                .WithSpan("WorkTrack.Test/TestClass.cs", 6, 17, 6, 27)
                .WithArguments(6, 5));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает о методе в пределах лимита.
    /// </summary>
    [Fact]
    public async Task MethodWithinLimit_NoDiagnostic()
    {
        const string test = @"
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

        var testState = new VerifyCS 
        { 
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        // Устанавливаем явное имя файла
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что expression-bodied методы не проверяются.
    /// </summary>
    [Fact]
    public async Task ExpressionBodiedMethod_NoDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test;

public class TestClass
{
    public int GetValue() => 42;
}
";

        var testState = new VerifyCS 
        { 
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        // Устанавливаем явное имя файла
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор читает конфигурацию из .editorconfig.
    /// </summary>
    [Fact]
    public async Task CustomMaxLinesFromEditorConfig_RespectsConfiguration()
    {
        const string test = @"
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

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        // Устанавливаем явное имя файла
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));

        // Добавляем конфигурацию через .editorconfig
        testState.TestState.AnalyzerConfigFiles.Add(
            ("/.editorconfig", @"
root = true

[*.cs]
wt_common_method_length_max_lines = 10
"));

        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор проверяет локальные функции.
    /// </summary>
    [Fact]
    public async Task LocalFunctionExceedingLimit_ReportsDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test;

public class TestClass
{
    public void ShortMethod()
    {
        void {|#0:LocalFunction|}()
        {
            var x = 1;
            var y = 2;
            var z = 3;
            var a = 4;
            var b = 5;
            var c = 6; // 6 lines > 5
        }
    }
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        
        testState.TestState.AnalyzerConfigFiles.Add(
            ("/.editorconfig", @"
root = true

[*.cs]
wt_common_method_length_max_lines = 5
wt_common_analyzer_target_prefixes = WorkTrack.
"));
        // Анализатор сообщает о двух диагностиках: для внешнего метода и локальной функции
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError(MethodLengthAnalyzer.DiagnosticId)
                .WithSpan("WorkTrack.Test/TestClass.cs", 6, 17, 6, 28)
                .WithArguments(9, 5));
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError(MethodLengthAnalyzer.DiagnosticId)
                .WithSpan("WorkTrack.Test/TestClass.cs", 8, 14, 8, 27)
                .WithArguments(6, 5));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет файлы без префикса WorkTrack.
    /// </summary>
    [Fact]
    public async Task FileWithoutTargetPrefix_NoDiagnostic()
    {
        const string test = @"
namespace Other.Test;

public class TestClass
{
    public void LongMethod()
    {
        var x = 1;
        var y = 2;
        var z = 3;
        var a = 4;
        var b = 5;
        var c = 6; // 6 lines > 5, но файл не в WorkTrack
    }
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("Other.Test/TestClass.cs", test));
        
        testState.TestState.AnalyzerConfigFiles.Add(
            ("/.editorconfig", @"
root = true

[*.cs]
wt_common_method_length_max_lines = 5
wt_common_analyzer_target_prefixes = WorkTrack.
"));
        await testState.RunAsync();
    }
}

