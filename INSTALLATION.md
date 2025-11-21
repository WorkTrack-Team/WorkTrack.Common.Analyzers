# Установка WorkTrack.Common.Analyzers

## ⚠️ Важно: buildTransitive применяется только к проектам с прямой ссылкой

Файл `buildTransitive/WorkTrack.Common.Analyzers.props` импортируется **только** в проекты, которые напрямую ссылаются на пакет через `PackageReference`. 

Это означает:
- ✅ Если добавить пакет в один проект → настройки применятся только к этому проекту
- ❌ Если добавить пакет в один проект → настройки **НЕ** применятся к другим проектам в solution

## Рекомендуемый подход: через Directory.Packages.props

### Шаг 1: Добавить версию пакета в Directory.Packages.props

В корне вашего solution создайте или обновите `Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageVersion Include="WorkTrack.Common.Analyzers" Version="1.0.0" />
    <!-- другие пакеты -->
  </ItemGroup>
</Project>
```

### Шаг 2: Добавить PackageReference в каждый проект

В каждом `.csproj`, который должен использовать анализаторы, добавьте:

```xml
<ItemGroup>
  <PackageReference Include="WorkTrack.Common.Analyzers" />
</ItemGroup>
```

### Шаг 3: Автоматизация (опционально)

Для добавления пакета во все проекты solution можно использовать скрипт:

```bash
# Получить список всех проектов и добавить пакет
dotnet sln list | grep "\.csproj$" | while read project; do
  dotnet add "$project" package WorkTrack.Common.Analyzers
done
```

Или через PowerShell:

```powershell
# Получить все проекты из solution
$projects = dotnet sln list | Select-String "\.csproj$"

foreach ($project in $projects) {
    dotnet add $project package WorkTrack.Common.Analyzers
}
```

## Альтернативный подход: через Directory.Build.props

Можно добавить `PackageReference` в корневой `Directory.Build.props`, но это менее гибко:

```xml
<Project>
  <ItemGroup>
    <!-- Применяется ко всем проектам в solution -->
    <PackageReference Include="WorkTrack.Common.Analyzers" />
  </ItemGroup>
</Project>
```

**Недостатки:**
- Нельзя исключить отдельные проекты
- Менее явное управление зависимостями
- Сложнее отследить, какие проекты используют пакет

## Исключение проектов из анализа

Если нужно исключить конкретный проект из анализа:

1. **Не добавляйте** `PackageReference` в этот проект
2. Или добавьте в `.csproj`:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);WTI0001;WTI0002;WTI0004;WTI0005;WTI0006;WTI0007</NoWarn>
</PropertyGroup>
```

## Проверка установки

После установки проверьте, что настройки применяются:

1. Соберите проект: `dotnet build`
2. Проверьте файл `obj/YourProject.csproj.nuget.g.props` - должен содержать импорт `WorkTrack.Common.Analyzers.props`
3. Попробуйте создать метод длиннее 5 строк - должна появиться ошибка WTI0001

## Пример для WorkTrack.Identity

```bash
# Добавить версию в Directory.Packages.props
echo '<PackageVersion Include="WorkTrack.Common.Analyzers" Version="1.0.0" />' >> WorkTrack.Identity/Directory.Packages.props

# Добавить во все проекты
cd WorkTrack.Identity
find . -name "*.csproj" -not -path "*/Analyzers/*" | xargs -I {} dotnet add {} package WorkTrack.Common.Analyzers
```

## Пример для WorkTrack.TemplateService

```bash
# Добавить версию в Directory.Packages.props
echo '<PackageVersion Include="WorkTrack.Common.Analyzers" Version="1.0.0" />' >> WorkTrack.TemplateService/Directory.Packages.props

# Добавить во все проекты
cd WorkTrack.TemplateService
find . -name "*.csproj" -not -path "*/Analyzers/*" | xargs -I {} dotnet add {} package WorkTrack.Common.Analyzers
```

