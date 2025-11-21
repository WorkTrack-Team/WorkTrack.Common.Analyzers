; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|------
WTI0001 | WorkTrack.Common.CodeQuality | Error | Метод слишком длинный. Настраивается через .editorconfig: dotnet_analyzer_diagnostic.wt_common_method_length_max_lines
WTI0002 | WorkTrack.Common.AsyncNaming | Error | Асинхронный метод должен оканчиваться на Async
WTI0004 | WorkTrack.Common.Time | Error | Нельзя использовать DateTime/DateTimeOffset напрямую
WTI0005 | WorkTrack.Common.Guards | Error | Метод должен начинаться с Guard.Against
WTI0006 | WorkTrack.Common.Namespaces | Error | Используйте file-scoped namespace
WTI0007 | WorkTrack.Common.Structure | Error | Только один публичный тип в файле
