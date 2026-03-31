namespace BusinessFlowApp.Core;

using BusinessFlowApp.Models;
using BusinessFlowApp.Services;

/// <summary>
/// Контекст выполнения бизнес-процесса
/// </summary>
public record BusinessFlowContext(
    string Mailbox,
    string Folder,
    Stream Stream,
    DecisionTableRow Row,
    string Group);

/// <summary>
/// Стратегия выполнения для конкретного FlowType
/// </summary>
public interface IFlowTypeStrategy
{
    FlowType FlowType { get; }
    Task<FlowResult> ExecuteAsync(BusinessFlowContext context, CancellationToken cancellationToken = default);
}
