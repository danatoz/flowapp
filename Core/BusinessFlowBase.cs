namespace BusinessFlowApp.Core;

using BusinessFlowApp.Services;

/// <summary>
/// Базовый класс для BusinessFlow с общей логикой выполнения
/// </summary>
public abstract class BusinessFlowBase<T> : IBusinessFlow<T>
{
    private readonly IBusinessFlowHandler _handler;

    protected BusinessFlowBase(IBusinessFlowHandler handler)
    {
        _handler = handler;
    }

    public abstract FlowType FlowType { get; }
    public abstract string Group { get; }

    public Task<T> BuildRequest(Message message, CancellationToken cancellationToken = default)
    {
        return _handler.ExecuteWithLogging(
            FlowType,
            Group,
            ct => ExecuteAsync(message, ct),
            cancellationToken);
    }

    protected abstract Task<T> ExecuteAsync(Message message, CancellationToken cancellationToken);
}
