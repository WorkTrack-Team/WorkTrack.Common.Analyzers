using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<
    WorkTrack.Common.Analyzers.ForbiddenDateTimeAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace WorkTrack.Common.Analyzers.Tests;

/// <summary>
/// Unit-тесты для <see cref="ForbiddenDateTimeAnalyzer"/>.
/// </summary>
public sealed class ForbiddenDateTimeAnalyzerTests
{
    /// <summary>
    /// Проверяет, что анализатор сообщает об использовании DateTime.Now.
    /// </summary>
    [Fact]
    public async Task DateTimeNow_ReportsDiagnostic()
    {
        const string test = @"
using System;

namespace WorkTrack.Test;

public class TestClass
{
    public void Method()
    {
        var now = DateTime.{|#0:Now|};
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
            DiagnosticResult.CompilerError(ForbiddenDateTimeAnalyzer.DiagnosticId)
                .WithSpan("WorkTrack.Test/TestClass.cs", 10, 28, 10, 31)
                .WithArguments("global::System.DateTime.Now"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает об использовании DateTime.UtcNow.
    /// </summary>
    [Fact]
    public async Task DateTimeUtcNow_ReportsDiagnostic()
    {
        const string test = @"
using System;

namespace WorkTrack.Test;

public class TestClass
{
    public void Method()
    {
        var utcNow = DateTime.{|#0:UtcNow|};
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
            DiagnosticResult.CompilerError(ForbiddenDateTimeAnalyzer.DiagnosticId)
                .WithSpan("WorkTrack.Test/TestClass.cs", 10, 31, 10, 37)
                .WithArguments("global::System.DateTime.UtcNow"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает об использовании DateTimeOffset.Now.
    /// </summary>
    [Fact]
    public async Task DateTimeOffsetNow_ReportsDiagnostic()
    {
        const string test = @"
using System;

namespace WorkTrack.Test;

public class TestClass
{
    public void Method()
    {
        var now = DateTimeOffset.{|#0:Now|};
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
            DiagnosticResult.CompilerError(ForbiddenDateTimeAnalyzer.DiagnosticId)
                .WithSpan("WorkTrack.Test/TestClass.cs", 10, 34, 10, 37)
                .WithArguments("global::System.DateTimeOffset.Now"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор сообщает об использовании DateTimeOffset.UtcNow.
    /// </summary>
    [Fact]
    public async Task DateTimeOffsetUtcNow_ReportsDiagnostic()
    {
        const string test = @"
using System;

namespace WorkTrack.Test;

public class TestClass
{
    public void Method()
    {
        var utcNow = DateTimeOffset.{|#0:UtcNow|};
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
            DiagnosticResult.CompilerError(ForbiddenDateTimeAnalyzer.DiagnosticId)
                .WithSpan("WorkTrack.Test/TestClass.cs", 10, 37, 10, 43)
                .WithArguments("global::System.DateTimeOffset.UtcNow"));
        await testState.RunAsync();
    }

    /// <summary>
    /// Проверяет, что анализатор не сообщает об использовании других свойств DateTime.
    /// </summary>
    [Fact]
    public async Task DateTimeOtherProperty_NoDiagnostic()
    {
        const string test = @"
using System;

namespace WorkTrack.Test;

public class TestClass
{
    public void Method()
    {
        var min = DateTime.MinValue;
        var max = DateTime.MaxValue;
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
    /// Проверяет, что анализатор не сообщает об использовании других типов.
    /// </summary>
    [Fact]
    public async Task OtherTypeProperty_NoDiagnostic()
    {
        const string test = @"
namespace WorkTrack.Test;

public class TestClass
{
    public void Method()
    {
        var value = string.Empty;
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
    /// Проверяет, что анализатор не сообщает об ошибке, если символ не является property.
    /// </summary>
    [Fact]
    public async Task NonPropertyMemberAccess_NoDiagnostic()
    {
        const string test = @"
using System;

namespace WorkTrack.Test;

public class TestClass
{
    public void Method()
    {
        var dt = DateTime.Parse(""2024-01-01"");
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

}

