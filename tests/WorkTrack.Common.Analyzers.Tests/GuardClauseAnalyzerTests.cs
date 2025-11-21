using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<
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
        const string test = @"
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

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };
        // Устанавливаем явное имя файла для корректной работы анализатора
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError(GuardClauseAnalyzer.DiagnosticId)
                .WithSpan("TestNamespace/TestClass.cs", 10, 9, 10, 34)
                .WithArguments("Method"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке, если метод начинается с Guard.Against.
    /// </summary>
    [Fact]
    public async Task PublicMethodWithGuard_NoDiagnostic()
    {
        const string test = @"
using System;
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

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };
        // Устанавливаем явное имя файла для корректной работы анализатора
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет приватные методы.
    /// </summary>
    [Fact]
    public async Task PrivateMethod_NoDiagnostic()
    {
        const string test = @"
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

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };
        // Устанавливаем явное имя файла для корректной работы анализатора
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет методы без параметров.
    /// </summary>
    [Fact]
    public async Task MethodWithoutParameters_NoDiagnostic()
    {
        const string test = @"
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

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };
        // Устанавливаем явное имя файла для корректной работы анализатора
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает об отсутствии Guard.Against в конструкторе.
    /// </summary>
    [Fact]
    public async Task ConstructorWithoutGuard_ReportsDiagnostic()
    {
        const string test = @"
using System;
using Ardalis.GuardClauses;

namespace TestNamespace;

public class TestClass
{
    public TestClass(string {|#0:param|})
    {
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError(GuardClauseAnalyzer.DiagnosticId)
                .WithSpan("TestNamespace/TestClass.cs", 11, 9, 11, 34)
                .WithArguments(".ctor"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для метода с пустым телом.
    /// </summary>
    [Fact]
    public async Task MethodWithEmptyBody_NoDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method(string param)
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает об ошибке, если первое statement не является ExpressionStatement.
    /// </summary>
    [Fact]
    public async Task MethodWithNonExpressionFirstStatement_ReportsDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method(string {|#0:param|})
    {
        if (param == null) return;
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError(GuardClauseAnalyzer.DiagnosticId)
                .WithSpan("TestNamespace/TestClass.cs", 8, 23, 8, 37)
                .WithArguments("Method"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для метода с await Guard.Against.
    /// </summary>
    [Fact]
    public async Task MethodWithAwaitGuard_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;
using Ardalis.GuardClauses;

namespace TestNamespace;

public class TestClass
{
    public async Task Method(string param)
    {
        await Guard.Against.Null(param);
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для метода с ref параметрами.
    /// </summary>
    [Fact]
    public async Task MethodWithRefParameter_NoDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method(ref string param)
    {
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для метода с nullable параметрами.
    /// </summary>
    [Fact]
    public async Task MethodWithNullableParameter_NoDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method(string? param)
    {
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для метода с out параметрами.
    /// </summary>
    [Fact]
    public async Task MethodWithOutParameter_NoDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method(out string param)
    {
        param = string.Empty;
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для метода с generic параметрами.
    /// </summary>
    [Fact]
    public async Task MethodWithGenericParameter_NoDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method<T>(T param)
    {
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает об ошибке для internal метода.
    /// </summary>
    [Fact]
    public async Task InternalMethodWithoutGuard_ReportsDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    internal void Method(string {|#0:param|})
    {
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError(GuardClauseAnalyzer.DiagnosticId)
                .WithSpan("TestNamespace/TestClass.cs", 10, 9, 10, 34)
                .WithArguments("Method"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает об ошибке для метода с reference type параметром.
    /// </summary>
    [Fact]
    public async Task MethodWithReferenceTypeParameter_ReportsDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method(object {|#0:param|})
    {
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        testState.ExpectedDiagnostics.Add(
            DiagnosticResult.CompilerError(GuardClauseAnalyzer.DiagnosticId)
                .WithSpan("TestNamespace/TestClass.cs", 10, 9, 10, 34)
                .WithArguments("Method"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для метода с nullable reference type параметром.
    /// </summary>
    [Fact]
    public async Task MethodWithNullableReferenceType_NoDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method(object? param)
    {
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для protected метода.
    /// </summary>
    [Fact]
    public async Task ProtectedMethod_NoDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    protected void Method(string param)
    {
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке для метода с GuardAgainst (альтернативное имя).
    /// </summary>
    [Fact]
    public async Task MethodWithGuardAgainst_NoDiagnostic()
    {
        const string test = @"
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method(string param)
    {
        GuardAgainst.Null(param);
        Console.WriteLine(param);
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
        testState.TestState.Sources.Add(("TestNamespace/TestClass.cs", test));
        
        var guardAssembly = typeof(Ardalis.GuardClauses.Guard).Assembly;
        testState.TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(guardAssembly.Location));
        await testState.RunAsync();
    }
}

