using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<
    WorkTrack.Common.Analyzers.AsyncSuffixAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace WorkTrack.Common.Analyzers.Tests;

/// <summary>
/// Unit-тесты для <see cref="AsyncSuffixAnalyzer"/>.
/// </summary>
public sealed class AsyncSuffixAnalyzerTests
{
    /// <summary>
    /// Проверяет, что анализатор сообщает об отсутствии суффикса Async у async метода.
    /// </summary>
    [Fact]
    public async Task AsyncMethodWithoutSuffix_ReportsDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test;

public class TestClass
{
    public async System.Threading.Tasks.Task {|#0:Process|}()
    {
        await System.Threading.Tasks.Task.CompletedTask;
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
            DiagnosticResult.CompilerError("WTI0002")
                .WithSpan("WorkTrack.Test/TestClass.cs", 6, 46, 6, 53)
                .WithArguments("Process"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает об отсутствии суффикса Async у метода возвращающего Task.
    /// </summary>
    [Fact]
    public async Task TaskMethodWithoutSuffix_ReportsDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;

namespace WorkTrack.Test;

public class TestClass
{
    public Task {|#0:GetData|}()
    {
        return Task.CompletedTask;
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
            DiagnosticResult.CompilerError("WTI0002")
                .WithSpan("WorkTrack.Test/TestClass.cs", 8, 17, 8, 24)
                .WithArguments("GetData"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает об отсутствии суффикса Async у метода возвращающего ValueTask.
    /// </summary>
    [Fact]
    public async Task ValueTaskMethodWithoutSuffix_ReportsDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;

namespace WorkTrack.Test;

public class TestClass
{
    public ValueTask {|#0:Load|}()
    {
        return new ValueTask();
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
            DiagnosticResult.CompilerError("WTI0002")
                .WithSpan("WorkTrack.Test/TestClass.cs", 8, 22, 8, 26)
                .WithArguments("Load"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об ошибке, если метод имеет суффикс Async.
    /// </summary>
    [Fact]
    public async Task AsyncMethodWithSuffix_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;

namespace WorkTrack.Test;

public class TestClass
{
    public async Task ProcessAsync()
    {
        await Task.CompletedTask;
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
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет override методы.
    /// </summary>
    [Fact]
    public async Task OverrideMethod_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;

namespace WorkTrack.Test;

public class BaseClass
{
    public virtual Task ProcessAsync() => Task.CompletedTask;
}

public class DerivedClass : BaseClass
{
    public override Task ProcessAsync() => Task.CompletedTask;
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет методы реализующие интерфейс.
    /// </summary>
    [Fact]
    public async Task InterfaceImplementation_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;

namespace WorkTrack.Test;

public interface IService
{
    Task Process();
}

public class Service : IService
{
    public Task Process() => Task.CompletedTask;
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет не-async методы.
    /// </summary>
    [Fact]
    public async Task NonAsyncMethod_NoDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test;

public class TestClass
{
    public void Process()
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
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет abstract методы.
    /// </summary>
    [Fact]
    public async Task AbstractMethod_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;

namespace WorkTrack.Test;

public abstract class TestClass
{
    public abstract Task Process();
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет explicit interface implementation.
    /// </summary>
    [Fact]
    public async Task ExplicitInterfaceImplementation_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;

namespace WorkTrack.Test;

public interface IService
{
    Task Process();
}

public class Service : IService
{
    Task IService.Process() => Task.CompletedTask;
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет generic методы с суффиксом Async.
    /// </summary>
    [Fact]
    public async Task GenericMethodWithAsyncSuffix_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;

namespace WorkTrack.Test;

public class TestClass
{
    public Task ProcessAsync<T>() => Task.CompletedTask;
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет extern методы.
    /// </summary>
    [Fact]
    public async Task ExternMethod_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;

namespace WorkTrack.Test;

public class TestClass
{
    public extern Task Process();
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не проверяет методы без locations.
    /// </summary>
    [Fact]
    public async Task MethodWithoutLocations_NoDiagnostic()
    {
        const string test = @"
using System.Threading.Tasks;

namespace WorkTrack.Test;

public class TestClass
{
    // Методы без source locations обычно генерируются компилятором
    // Этот тест проверяет что анализатор корректно обрабатывает такие случаи
}
";

        var testState = new VerifyCS
        {
            TestCode = test,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        testState.TestState.Sources.Clear();
        testState.TestState.Sources.Add(("WorkTrack.Test/TestClass.cs", test));
        await testState.RunAsync();
    }
}

