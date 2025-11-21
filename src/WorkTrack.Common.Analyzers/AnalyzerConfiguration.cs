using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WorkTrack.Common.Analyzers;

/// <summary>
/// Конфигурация анализатора, инкапсулирующая настройки из .editorconfig.
/// Применяет принцип инкапсуляции для централизованного управления конфигурацией.
/// </summary>
internal sealed class AnalyzerConfiguration
{
    /// <summary>
    /// Максимальное количество строк в теле метода.
    /// </summary>
    public int MaxMethodLines { get; set; } = 5;

    /// <summary>
    /// Префиксы проектов, на которые распространяется анализ.
    /// </summary>
    public IReadOnlyList<string> TargetPrefixes { get; set; } = new List<string> { "WorkTrack." };

    /// <summary>
    /// Исключать ли тестовые сборки из анализа.
    /// </summary>
    public bool ExcludeTests { get; set; } = true;

    /// <summary>
    /// Загружает конфигурацию из AnalyzerConfigOptions.
    /// </summary>
    /// <param name="optionsProvider">Провайдер опций конфигурации анализатора.</param>
    /// <param name="syntaxTree">Синтаксическое дерево для получения конфигурации.</param>
    /// <returns>Загруженная конфигурация.</returns>
    public static AnalyzerConfiguration LoadFrom(
        AnalyzerConfigOptionsProvider optionsProvider,
        SyntaxTree syntaxTree)
    {
        var options = optionsProvider.GetOptions(syntaxTree);
        var config = new AnalyzerConfiguration();

        // Загружаем MaxMethodLines
        const string maxLinesKey = "wt_common_method_length_max_lines";
        var fullMaxLinesKey = $"dotnet_analyzer_diagnostic.{maxLinesKey}";
        
        if (options.TryGetValue(key: fullMaxLinesKey, value: out var maxLinesValue) ||
            options.TryGetValue(key: maxLinesKey, value: out maxLinesValue))
        {
            if (int.TryParse(s: maxLinesValue, result: out var maxLines) && maxLines > 0)
            {
                config.MaxMethodLines = maxLines;
            }
        }

        // Загружаем TargetPrefixes
        const string targetPrefixesKey = "wt_common_analyzer_target_prefixes";
        if (options.TryGetValue(key: targetPrefixesKey, value: out var prefixesValue) && 
            !string.IsNullOrWhiteSpace(value: prefixesValue))
        {
            var prefixes = prefixesValue
                .Split(separator: new[] { ',', ';' }, options: StringSplitOptions.RemoveEmptyEntries)
                .Select(selector: s => s.Trim())
                .Where(predicate: s => !string.IsNullOrWhiteSpace(value: s))
                .ToList();
            
            if (prefixes.Count > 0)
            {
                config.TargetPrefixes = prefixes;
            }
        }

        return config;
    }
}

