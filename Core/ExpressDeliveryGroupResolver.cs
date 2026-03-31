namespace BusinessFlowApp.Core;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BusinessFlowApp.Services;

/// <summary>
/// Резолвер для Express доставки
/// Приоритет: Express > Standard
/// </summary>
public class ExpressDeliveryGroupResolver : XlsxGroupResolverBase
{
    public ExpressDeliveryGroupResolver(
        IOptions<XlsxGroupResolverOptions> options,
        ILogger<ExpressDeliveryGroupResolver> logger)
        : base(options, logger)
    {
    }

    protected override string ResolveGroupFromHeaders(
        XlsxHeaders headers,
        IEnumerable<string> availableGroups)
    {
        var groupsList = availableGroups.ToList();

        // Стратегия 1: Поиск по значению "Express" в ключевых полях
        var expressKeywords = new[] { "express", "быстро", "срочно", "priority" };
        foreach (var kv in headers.KeyValues)
        {
            if (!string.IsNullOrEmpty(kv.Value) &&
                expressKeywords.Any(k => kv.Value.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                if (groupsList.Contains("Express", StringComparer.OrdinalIgnoreCase))
                {
                    return "Express";
                }
            }
        }

        // Стратегия 2: Поиск столбцов с "Express" в названии
        if (headers.ColumnNames.Any(c =>
            c.Contains("express", StringComparison.OrdinalIgnoreCase) ||
            c.Contains("срочно", StringComparison.OrdinalIgnoreCase)))
        {
            if (groupsList.Contains("Express", StringComparer.OrdinalIgnoreCase))
            {
                return "Express";
            }
        }

        // Стратегия 3: Если есть столбец "DeliveryType" со значением "Express"
        if (headers.KeyValues.TryGetValue("DeliveryType", out var deliveryType) ||
            headers.KeyValues.TryGetValue("ТипДоставки", out deliveryType))
        {
            if (!string.IsNullOrEmpty(deliveryType) &&
                deliveryType.Contains("Express", StringComparison.OrdinalIgnoreCase))
            {
                if (groupsList.Contains("Express", StringComparer.OrdinalIgnoreCase))
                {
                    return "Express";
                }
            }
        }

        // По умолчанию: Standard если доступен, иначе первая группа
        if (groupsList.Contains("Standard", StringComparer.OrdinalIgnoreCase))
        {
            return "Standard";
        }

        return groupsList.First();
    }
}
