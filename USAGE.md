# Использование WorkTrack.Common.Analyzers

## Автоматическое применение через NuGet

При установке пакета `WorkTrack.Common.Analyzers` настройки автоматически применяются через файл `buildTransitive/WorkTrack.Common.Analyzers.props`. Вам **не нужно** вручную импортировать или копировать `Directory.Build.props`.

## Как работают несколько Directory.Build.props

MSBuild импортирует файлы в следующем порядке (от раннего к позднему):

1. **`buildTransitive/*.props` из NuGet пакетов** - `WorkTrack.Common.Analyzers.props` (автоматически при установке пакета)
2. **`Directory.Build.props` из иерархии директорий** - снизу вверх по структуре папок
3. **Проект `.csproj`** - финальные настройки

**Важно**: Настройки из более поздних источников **переопределяют** настройки из более ранних.

## Переопределение настроек

### ⚠️ Важно: Пакет нужно добавить в каждый проект

Файл `buildTransitive/WorkTrack.Common.Analyzers.props` импортируется **только** в проекты с прямой ссылкой на пакет. Для анализа всех проектов в solution нужно добавить `PackageReference` в каждый `.csproj`.

### Вариант 1: Через Directory.Packages.props (рекомендуется)

1. **Добавьте версию в `Directory.Packages.props`** (в корне solution):

```xml
<Project>
  <ItemGroup>
    <PackageVersion Include="WorkTrack.Common.Analyzers" Version="1.0.0" />
  </ItemGroup>
</Project>
```

2. **Добавьте ссылку в каждый `.csproj`**:

```xml
<ItemGroup>
  <PackageReference Include="WorkTrack.Common.Analyzers" />
</ItemGroup>
```

Или используйте скрипт для автоматизации:

```bash
# Добавить во все проекты solution
dotnet sln list | grep "\.csproj$" | xargs -I {} dotnet add {} package WorkTrack.Common.Analyzers
```

Затем создайте локальный `Directory.Build.props` только для переопределения нужных настроек:

```xml
<Project>
  <!-- Переопределяем нужные настройки -->
  <PropertyGroup>
    <!-- Например, изменить TargetFramework -->
    <TargetFramework>net8.0</TargetFramework>
    
    <!-- Или отключить TreatWarningsAsErrors для Debug -->
    <TreatWarningsAsErrors Condition="'$(Configuration)' == 'Debug'">false</TreatWarningsAsErrors>
  </PropertyGroup>

  <!-- Добавить дополнительные NoWarn -->
  <PropertyGroup Condition="$([System.String]::Copy('$(MSBuildProjectName)').EndsWith('Tests'))">
    <NoWarn>$(NoWarn);CA2007;MA0004</NoWarn>
  </PropertyGroup>
</Project>
```

### Вариант 2: Использование как проект (для разработки)

Если используете пакет как проект для разработки, импортируйте через `<Import>`:

```xml
<Project>
  <!-- Импортируем общие настройки -->
  <Import Project="../../WorkTrack.Common.Analyzers/Directory.Build.props" 
          Condition="Exists('../../WorkTrack.Common.Analyzers/Directory.Build.props')" />

  <!-- Переопределяем нужные настройки -->
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

### Вариант 3: Условные настройки

Используйте условия для разных конфигураций:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
</PropertyGroup>
```

## Что можно переопределить

✅ **Можно безопасно переопределить:**
- `TargetFramework`
- `TreatWarningsAsErrors`
- `NoWarn`
- `StyleCopEnabled`
- `GenerateDocumentationFile`
- Любые другие свойства

✅ **Анализаторы подключаются автоматически:**
- `WorkTrack.Common.Analyzers` (через ProjectReference)
- `Microsoft.CodeAnalysis.NetAnalyzers`
- `Meziantou.Analyzer`
- `Roslynator.Analyzers`
- `AsyncFixer`

⚠️ **Не нужно дублировать:**
- ProjectReference на анализаторы (подключаются автоматически)
- PackageReference на стандартные анализаторы (подключаются автоматически)

## Пример полного Directory.Build.props для сервиса

```xml
<Project>
  <!-- 
    НЕ нужно импортировать WorkTrack.Common.Analyzers.props вручную!
    Он автоматически импортируется при установке пакета через buildTransitive.
  -->

  <!-- Специфичные настройки сервиса (переопределяют настройки из пакета) -->
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <!-- Дополнительные настройки для тестов -->
  <PropertyGroup Condition="$([System.String]::Copy('$(MSBuildProjectName)').EndsWith('Tests'))">
    <NoWarn>$(NoWarn);WTI0001;CA2007</NoWarn>
  </PropertyGroup>

  <!-- Подключение stylecop.json сервиса (если отличается от общего) -->
  <ItemGroup Condition="Exists('$(MSBuildThisFileDirectory)stylecop.json')">
    <AdditionalFiles Include="$(MSBuildThisFileDirectory)stylecop.json" />
  </ItemGroup>
</Project>
```

## Порядок применения настроек

1. **`buildTransitive/WorkTrack.Common.Analyzers.props` из NuGet** - базовые настройки (автоматически)
2. **Ваш сервис/Directory.Build.props** - переопределения и дополнения
3. **Проект .csproj** - финальные настройки (имеют наивысший приоритет)

Это гарантирует, что анализаторы всегда подключены при установке пакета, но вы можете гибко настраивать параметры сборки через локальный `Directory.Build.props`.

