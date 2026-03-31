namespace BusinessFlowApp.Core;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniExcelLibs;
using BusinessFlowApp.Models;
using BusinessFlowApp.Services;

public enum FlowType
{
    Payments,
    Delivery
}

/// <summary>
/// Базовый класс для результатов выполнения BusinessFlow
/// </summary>
public abstract record FlowResult
{
    public FlowType FlowType { get; init; }

    protected FlowResult(FlowType flowType)
    {
        FlowType = flowType;
    }
}

public sealed record PaymentFlowResult(PaymentResult Payment) : FlowResult(FlowType.Payments)
{
    public bool Success => Payment.Success;
    public string TransactionId => Payment.TransactionId;
    public string MessageId => Payment.MessageId;
}

public sealed record DeliveryFlowResult(DeliveryResult Delivery) : FlowResult(FlowType.Delivery)
{
    public int EstimatedDays => Delivery.EstimatedDays;
    public decimal Cost => Delivery.Cost;
    public string MessageId => Delivery.MessageId;
}

public interface IBusinessFlow<T>
{
    FlowType FlowType { get; }
    string Group { get; }
    Task<T> BuildRequest(Message message, CancellationToken cancellationToken = default);
}

public interface IBusinessFlowHandler
{
    Task<T> ExecuteWithLogging<T>(
        FlowType flowType,
        string group,
        Func<CancellationToken, Task<T>> executeFunc,
        CancellationToken cancellationToken = default);
}

public class BusinessFlowHandler : IBusinessFlowHandler
{
    private readonly ILogger<BusinessFlowHandler> _logger;

    public BusinessFlowHandler(ILogger<BusinessFlowHandler> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteWithLogging<T>(
        FlowType flowType,
        string group,
        Func<CancellationToken, Task<T>> executeFunc,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Start FlowType: {FlowType} Group: {Group}.", flowType, group);
        try
        {
            return await executeFunc(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Some error");
            throw;
        }
    }
}

public record Message(Stream Stream);

/// <summary>
/// Результат парсинга заголовков XLSX
/// </summary>
public record XlsxHeaders(
    IReadOnlyList<string> ColumnNames,
    IReadOnlyDictionary<string, string?> KeyValues);

/// <summary>
/// Определяет группу на основе заголовков XLSX файла
/// </summary>
public interface IXlsxGroupResolver
{
    /// <summary>
    /// Возвращает конкретную группу из списка доступных
    /// </summary>
    /// <param name="stream">Stream с XLSX файлом</param>
    /// <param name="availableGroups">Список групп из конфига</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Определённая группа</returns>
    Task<string> ResolveAsync(
        Stream stream,
        IEnumerable<string> availableGroups,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Универсальный резолвер, который делегирует работу кастомным резолверам через фабрику
/// </summary>
public class XlsxHeadersGroupResolver : IXlsxGroupResolver
{
    private readonly IXlsxGroupResolverFactory _resolverFactory;
    private readonly IXlsxGroupResolver _defaultResolver;
    private readonly ILogger<XlsxHeadersGroupResolver> _logger;

    public XlsxHeadersGroupResolver(
        IXlsxGroupResolverFactory resolverFactory,
        DefaultXlsxGroupResolver defaultResolver,
        ILogger<XlsxHeadersGroupResolver> logger)
    {
        _resolverFactory = resolverFactory;
        _defaultResolver = defaultResolver;
        _logger = logger;
    }

    public async Task<string> ResolveAsync(
        Stream stream,
        IEnumerable<string> availableGroups,
        CancellationToken cancellationToken = default)
    {
        // Делегируем работу резолверу по умолчанию (базовая логика)
        _logger.LogInformation("Используется резолвер по умолчанию (базовая логика)");
        return await _defaultResolver.ResolveAsync(stream, availableGroups, cancellationToken);
    }
}

/// <summary>
/// Контекстный резолвер, который выбирает стратегию на основе конфигурации
/// </summary>
public class ContextualXlsxGroupResolver : IXlsxGroupResolver
{
    private readonly IXlsxGroupResolverFactory _resolverFactory;
    private readonly string? _resolverName;
    private readonly ILogger<ContextualXlsxGroupResolver> _logger;

    public ContextualXlsxGroupResolver(
        IXlsxGroupResolverFactory resolverFactory,
        string? resolverName = null,
        ILogger<ContextualXlsxGroupResolver>? logger = null)
    {
        _resolverFactory = resolverFactory;
        _resolverName = resolverName;
        _logger = logger ?? new Logger<ContextualXlsxGroupResolver>(new LoggerFactory());
    }

    public async Task<string> ResolveAsync(
        Stream stream,
        IEnumerable<string> availableGroups,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_resolverName))
        {
            var resolver = _resolverFactory.GetResolver(_resolverName);
            if (resolver != null)
            {
                _logger.LogInformation("Используется кастомный резолвер: {ResolverName}", _resolverName);
                return await resolver.ResolveAsync(stream, availableGroups, cancellationToken);
            }

            _logger.LogWarning("Резолвер '{ResolverName}' не найден, используется резолвер по умолчанию", _resolverName);
        }

        // Fallback: используем первый доступный резолвер или базовую логику
        var availableResolvers = _resolverFactory.GetRegisteredResolverNames().ToList();
        if (availableResolvers.Any())
        {
            var fallbackResolver = _resolverFactory.GetResolver(availableResolvers.First());
            if (fallbackResolver != null)
            {
                return await fallbackResolver.ResolveAsync(stream, availableGroups, cancellationToken);
            }
        }

        // Если ничего не найдено, возвращаем первую группу
        return availableGroups.First();
    }
}

/// <summary>
/// Резолвер по умолчанию: возвращает первую группу
/// </summary>
public class DefaultXlsxGroupResolver : IXlsxGroupResolver
{
    public Task<string> ResolveAsync(
        Stream stream,
        IEnumerable<string> availableGroups,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(availableGroups.First());
    }
}
