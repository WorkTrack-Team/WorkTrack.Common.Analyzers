using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<
    WorkTrack.Common.Analyzers.NamespaceStyleAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace WorkTrack.Common.Analyzers.Tests;

/// <summary>
/// Unit-тесты для <see cref="NamespaceStyleAnalyzer"/>.
/// </summary>
public sealed class NamespaceStyleAnalyzerTests
{
    /// <summary>
    /// Проверяет, что анализатор сообщает об использовании обычного namespace вместо file-scoped.
    /// </summary>
    [Fact]
    public async Task TraditionalNamespace_ReportsDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test
{
    public class TestClass
    {
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
        
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError("WTI0006")
                .WithSpan("WorkTrack.Test/TestClass.cs", 2, 11, 2, 25)
                .WithArguments("WorkTrack.Test"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для file-scoped namespace.
    /// </summary>
    [Fact]
    public async Task FileScopedNamespace_NoDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test;

public class TestClass
{
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает о нескольких публичных типах в файле.
    /// </summary>
    [Fact]
    public async Task MultiplePublicTypes_ReportsDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test;

public class FirstClass
{
}

public class {|#0:SecondClass|}
{
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError(NamespaceStyleAnalyzer.DiagnosticIdSingleType)
                .WithSpan("WorkTrack.Test/TestClass.cs", 8, 14, 8, 25)
                .WithArguments("FirstClass, SecondClass"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для одного публичного типа.
    /// </summary>
    [Fact]
    public async Task SinglePublicType_NoDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test;

public class TestClass
{
}

private class HelperClass
{
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает о нескольких internal типах.
    /// </summary>
    [Fact]
    public async Task MultipleInternalTypes_ReportsDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test;

internal class FirstClass
{
}

internal class {|#0:SecondClass|}
{
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError(NamespaceStyleAnalyzer.DiagnosticIdSingleType)
                .WithSpan("WorkTrack.Test/TestClass.cs", 8, 16, 8, 27)
                .WithArguments("FirstClass, SecondClass"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для одного публичного типа с вложенными классами.
    /// </summary>
    [Fact]
    public async Task PrivateTypes_NoDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test;

public class PublicClass
{
    private class PrivateClass
    {
    }

    private class AnotherPrivateClass
    {
    }
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет тестовые файлы.
    /// </summary>
    [Fact]
    public async Task TestFile_NoDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test
{
    public class TestClass
    {
    }
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("Tests/TestClass.cs", test));
        await testState.RunAsync();
    }
}

