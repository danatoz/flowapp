namespace BusinessFlowApp.Core;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BusinessFlowApp.Services;

/// <summary>
/// Резолвер на основе точного соответствия значения в столбце "Group" или "Группа"
/// </summary>
public class ExactMatchGroupResolver : XlsxGroupResolverBase
{
    public ExactMatchGroupResolver(
        IOptions<XlsxGroupResolverOptions> options,
        ILogger<ExactMatchGroupResolver> logger)
        : base(options, logger)
    {
    }

    protected override string ResolveGroupFromHeaders(
        XlsxHeaders headers,
        IEnumerable<string> availableGroups)
    {
        var groupsList = availableGroups.ToList();

        // Стратегия: Точное совпадение по столбцам "Group", "Группа", "Type"
        var groupColumnNames = new[] { "Group", "Группа", "Type", "Тип", "FlowType", "FlowGroup" };

        foreach (var columnName in groupColumnNames)
        {
            if (headers.KeyValues.TryGetValue(columnName, out var groupValue) &&
                !string.IsNullOrEmpty(groupValue))
            {
                var matchedGroup = groupsList.FirstOrDefault(g =>
                    g.Equals(groupValue, StringComparison.OrdinalIgnoreCase));

                if (matchedGroup != null)
                {
                    Logger.LogInformation(
                        "[{Resolver}] Найдено точное совпадение группы: {Group} (из столбца {Column})",
                        GetType().Name,
                        matchedGroup,
                        columnName);
                    return matchedGroup;
                }
            }
        }

        // Если точное совпадение не найдено, пробуем частичное
        foreach (var group in groupsList)
        {
            foreach (var kv in headers.KeyValues)
            {
                if (!string.IsNullOrEmpty(kv.Value) &&
                    kv.Value.Contains(group, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogInformation(
                        "[{Resolver}] Найдено частичное совпадение группы: {Group} (в значении {Key}={Value})",
                        GetType().Name,
                        group,
                        kv.Key,
                        kv.Value);
                    return group;
                }
            }
        }

        // По умолчанию: первая группа
        return groupsList.First();
    }
}
