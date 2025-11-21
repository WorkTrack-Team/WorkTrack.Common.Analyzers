using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace WorkTrack.Common.Analyzers;

/// <summary>
/// Вспомогательные методы для анализаторов.
/// Централизует общую логику для устранения дублирования кода (DRY принцип).
/// </summary>
public static class AnalyzerHelpers
{
    private static readonly ConcurrentDictionary<string, bool> _testAssemblyCache = new();

    /// <summary>
    /// Проверяет, является ли сборка тестовой.
    /// Оптимизировано с использованием кэширования и SkipLocalsInit для производительности.
    /// </summary>
    /// <param name="compilation">Компиляция для проверки.</param>
    /// <returns>True, если сборка является тестовой.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTestAssembly(Compilation compilation)
    {
        if (compilation is null)
        {
            return false;
        }

        var assemblyName = compilation.AssemblyName ?? string.Empty;
        return _testAssemblyCache.GetOrAdd(
            key: assemblyName,
            valueFactory: name => name.IndexOf(value: "Tests", comparisonType: StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// Проверяет, является ли файл тестовым по пути.
    /// Оптимизировано с использованием Span для лучшей производительности.
    /// </summary>
    /// <param name="filePath">Путь к файлу.</param>
    /// <returns>True, если файл является тестовым.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTestFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(value: filePath))
        {
            return false;
        }

        // Используем Span для оптимизации производительности
        var span = filePath!.AsSpan();
        var testsSpan = "Tests".AsSpan();
        return span.Contains(value: testsSpan, comparisonType: StringComparison.OrdinalIgnoreCase);
    }
}

