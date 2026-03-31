namespace BusinessFlowApp.Core;

using Microsoft.Extensions.Logging;
using BusinessFlowApp.Services;

public interface IBusinessFlowFactory
{
    /// <summary>
    /// Поиск flow по одной группе
    /// </summary>
    IBusinessFlow<T>? GetByFlowTypeAndGroup<T>(FlowType flowType, string group);
}

public class BusinessFlowFactory : IBusinessFlowFactory
{
    private readonly IServiceProvider _serviceProvider;

    public BusinessFlowFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IBusinessFlow<T>? GetByFlowTypeAndGroup<T>(FlowType flowType, string group)
    {
        var flows = _serviceProvider.GetServices<IBusinessFlow<T>>();
        return flows.FirstOrDefault(f => f.FlowType == flowType && f.Group == group);
    }
}

public interface IBusinessFlowExecutor
{
    /// <summary>
    /// Выполнение с определением группы по XLSX файлу
    /// </summary>
    Task<FlowResult> ExecuteXlsxAsync(
        string mailbox,
        string folder,
        Stream xlsxStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполнение без определения группы (для обратной совместимости)
    /// </summary>
    Task<FlowResult> ExecuteAsync(
        string mailbox,
        string folder,
        Message message,
        CancellationToken cancellationToken = default);
}

public class BusinessFlowExecutor : IBusinessFlowExecutor
{
    private readonly IGroupResolutionService _groupResolutionService;
    private readonly IEnumerable<IFlowTypeStrategy> _strategies;

    public BusinessFlowExecutor(
        IGroupResolutionService groupResolutionService,
        IEnumerable<IFlowTypeStrategy> strategies)
    {
        _groupResolutionService = groupResolutionService;
        _strategies = strategies;
    }

    public async Task<FlowResult> ExecuteXlsxAsync(
        string mailbox,
        string folder,
        Stream xlsxStream,
        CancellationToken cancellationToken = default)
    {
        var context = await _groupResolutionService.ResolveAsync(
            mailbox, folder, xlsxStream, cancellationToken);

        return await ExecuteByStrategyAsync(context, cancellationToken);
    }

    public async Task<FlowResult> ExecuteAsync(
        string mailbox,
        string folder,
        Message message,
        CancellationToken cancellationToken = default)
    {
        var context = await _groupResolutionService.ResolveAsync(
            mailbox, folder, message.Stream, cancellationToken);

        return await ExecuteByStrategyAsync(context, cancellationToken);
    }

    private async Task<FlowResult> ExecuteByStrategyAsync(
        BusinessFlowContext context,
        CancellationToken cancellationToken)
    {
        var strategy = _strategies.FirstOrDefault(s => s.FlowType == context.Row.FlowType)
            ?? throw new InvalidOperationException($"Неподдерживаемый FlowType: {context.Row.FlowType}");

        return await strategy.ExecuteAsync(context, cancellationToken);
    }
}
