namespace BusinessFlowApp.Core.Strategies;

using Microsoft.Extensions.Logging;
using BusinessFlowApp.Services;

/// <summary>
/// Универсальная стратегия выполнения для любого типа результата
/// </summary>
public class GenericFlowTypeStrategy<T> : IFlowTypeStrategy
{
    private readonly IBusinessFlowFactory _factory;
    private readonly FlowType _flowType;
    private readonly Func<T, FlowResult> _resultFactory;
    private readonly ILogger<GenericFlowTypeStrategy<T>> _logger;

    public GenericFlowTypeStrategy(
        IBusinessFlowFactory factory,
        FlowType flowType,
        Func<T, FlowResult> resultFactory,
        ILogger<GenericFlowTypeStrategy<T>> logger)
    {
        _factory = factory;
        _flowType = flowType;
        _resultFactory = resultFactory;
        _logger = logger;
    }

    public FlowType FlowType => _flowType;

    public async Task<FlowResult> ExecuteAsync(
        BusinessFlowContext context,
        CancellationToken cancellationToken = default)
    {
        var flow = _factory.GetByFlowTypeAndGroup<T>(context.Row.FlowType, context.Group);

        if (flow is null)
        {
            throw new InvalidOperationException(
                $"Не найдена реализация IBusinessFlow<{typeof(T).Name}> для FlowType={context.Row.FlowType}, Group={context.Group}");
        }

        var message = new Message(context.Stream);
        var result = await flow.BuildRequest(message, cancellationToken);
        return _resultFactory(result);
    }
}
