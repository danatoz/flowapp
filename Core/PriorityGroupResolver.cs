namespace BusinessFlowApp.Core;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BusinessFlowApp.Services;

/// <summary>
/// Резолвер на основе приоритета групп
/// Выбирает группу по порядку приоритета из конфигурации
/// </summary>
public class PriorityGroupResolver : XlsxGroupResolverBase
{
    private readonly string[] _priorityKeywords;

    public PriorityGroupResolver(
        IOptions<XlsxGroupResolverOptions> options,
        ILogger<PriorityGroupResolver> logger,
        string[]? priorityKeywords = null)
        : base(options, logger)
    {
        _priorityKeywords = priorityKeywords ?? new[] { "priority", "express", "vip", "премиум", "срочно" };
    }

    protected override string ResolveGroupFromHeaders(
        XlsxHeaders headers,
        IEnumerable<string> availableGroups)
    {
        var groupsList = availableGroups.ToList();

        // Стратегия 1: Поиск по ключевому столбцу "Priority" или "Приоритет"
        if (headers.KeyValues.TryGetValue("Priority", out var priorityValue) ||
            headers.KeyValues.TryGetValue("Приоритет", out priorityValue) ||
            headers.KeyValues.TryGetValue("PriorityLevel", out priorityValue))
        {
            if (!string.IsNullOrEmpty(priorityValue))
            {
                // Пытаемся найти группу по значению приоритета
                var matchedGroup = groupsList.FirstOrDefault(g =>
                    g.Equals(priorityValue, StringComparison.OrdinalIgnoreCase));

                if (matchedGroup != null)
                    return matchedGroup;

                // Если значение числовое, выбираем по приоритету
                if (int.TryParse(priorityValue, out var priorityLevel))
                {
                    // Чем выше число, тем выше приоритет
                    return priorityLevel > 1
                        ? groupsList.Last() // Высокий приоритет -> последняя группа (например, Express)
                        : groupsList.First(); // Низкий приоритет -> первая группа (например, Standard)
                }
            }
        }

        // Стратегия 2: Поиск ключевых слов в значениях
        foreach (var kv in headers.KeyValues)
        {
            if (!string.IsNullOrEmpty(kv.Value) &&
                _priorityKeywords.Any(k => kv.Value.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                // Возвращаем группу с наивысшим приоритетом
                return groupsList.Last();
            }
        }

        // Стратегия 3: Поиск ключевых слов в названиях столбцов
        foreach (var columnName in headers.ColumnNames)
        {
            if (_priorityKeywords.Any(k => columnName.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                return groupsList.Last();
            }
        }

        // По умолчанию: первая группа (наименьший приоритет)
        return groupsList.First();
    }
}
