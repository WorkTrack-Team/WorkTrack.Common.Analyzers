using Microsoft.CodeAnalysis;

namespace WorkTrack.Common.Analyzers;

/// <summary>
/// Фабрика для создания диагностик анализаторов.
/// Применяет Factory Pattern для централизованного создания диагностик.
/// </summary>
internal static class DiagnosticFactory
{
    /// <summary>
    /// Создает диагностику для слишком длинного метода.
    /// </summary>
    /// <param name="rule">Правило диагностики.</param>
    /// <param name="location">Местоположение диагностики.</param>
    /// <param name="actualLines">Фактическое количество строк.</param>
    /// <param name="maxLines">Максимально допустимое количество строк.</param>
    /// <returns>Созданная диагностика.</returns>
    public static Diagnostic CreateMethodTooLong(
        DiagnosticDescriptor rule,
        Location location,
        int actualLines,
        int maxLines)
    {
        return Diagnostic.Create(
            descriptor: rule,
            location: location,
            messageArgs: [actualLines, maxLines]);
    }

    /// <summary>
    /// Создает диагностику для отсутствия суффикса Async.
    /// </summary>
    /// <param name="rule">Правило диагностики.</param>
    /// <param name="location">Местоположение диагностики.</param>
    /// <param name="methodName">Имя метода.</param>
    /// <returns>Созданная диагностика.</returns>
    public static Diagnostic CreateMissingAsyncSuffix(
        DiagnosticDescriptor rule,
        Location location,
        string methodName)
    {
        return Diagnostic.Create(
            descriptor: rule,
            location: location,
            messageArgs: methodName);
    }

    /// <summary>
    /// Создает диагностику для отсутствия Guard.Against.
    /// </summary>
    /// <param name="rule">Правило диагностики.</param>
    /// <param name="location">Местоположение диагностики.</param>
    /// <param name="methodName">Имя метода или конструктора.</param>
    /// <returns>Созданная диагностика.</returns>
    public static Diagnostic CreateMissingGuardClause(
        DiagnosticDescriptor rule,
        Location location,
        string methodName)
    {
        return Diagnostic.Create(
            descriptor: rule,
            location: location,
            messageArgs: methodName);
    }

    /// <summary>
    /// Создает диагностику для использования запрещенного DateTime свойства.
    /// </summary>
    /// <param name="rule">Правило диагностики.</param>
    /// <param name="location">Местоположение диагностики.</param>
    /// <param name="propertyName">Имя запрещенного свойства.</param>
    /// <returns>Созданная диагностика.</returns>
    public static Diagnostic CreateForbiddenDateTimeProperty(
        DiagnosticDescriptor rule,
        Location location,
        string propertyName)
    {
        return Diagnostic.Create(
            descriptor: rule,
            location: location,
            messageArgs: propertyName);
    }

    /// <summary>
    /// Создает диагностику для неправильного стиля пространства имен.
    /// </summary>
    /// <param name="rule">Правило диагностики.</param>
    /// <param name="location">Местоположение диагностики.</param>
    /// <param name="namespaceName">Имя пространства имен.</param>
    /// <returns>Созданная диагностика.</returns>
    public static Diagnostic CreateInvalidNamespaceStyle(
        DiagnosticDescriptor rule,
        Location location,
        string namespaceName)
    {
        return Diagnostic.Create(
            descriptor: rule,
            location: location,
            messageArgs: namespaceName);
    }
}

