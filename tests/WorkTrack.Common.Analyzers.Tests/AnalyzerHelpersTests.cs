using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace WorkTrack.Common.Analyzers.Tests;

/// <summary>
/// Unit-тесты для <see cref="AnalyzerHelpers"/>.
/// </summary>
public sealed class AnalyzerHelpersTests
{
    /// <summary>
    /// Проверяет, что IsTestAssembly корректно определяет тестовые сборки.
    /// </summary>
    [Fact]
    public void IsTestAssembly_WithTestsInName_ReturnsTrue()
    {
        // Arrange
        var compilation = CreateCompilation(assemblyName: "MyProject.Tests");

        // Act
        var result = AnalyzerHelpers.IsTestAssembly(compilation);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Проверяет, что IsTestAssembly возвращает false для обычных сборок.
    /// </summary>
    [Fact]
    public void IsTestAssembly_WithoutTestsInName_ReturnsFalse()
    {
        // Arrange
        var compilation = CreateCompilation(assemblyName: "MyProject");

        // Act
        var result = AnalyzerHelpers.IsTestAssembly(compilation);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Проверяет, что IsTestFile корректно определяет тестовые файлы.
    /// </summary>
    [Theory]
    [InlineData("Tests/MyTest.cs", true)]
    [InlineData("MyProject/Tests/Test.cs", true)]
    [InlineData("MyProject/Service.cs", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsTestFile_VariousPaths_ReturnsExpected(string? filePath, bool expected)
    {
        // Act
        var result = AnalyzerHelpers.IsTestFile(filePath);

        // Assert
        Assert.Equal(expected, result);
    }

    private static CSharpCompilation CreateCompilation(string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("public class Test { }");
        return CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: [syntaxTree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
    }
}

