namespace BusinessFlowApp.Services;

using Microsoft.Extensions.Logging;
using BusinessFlowApp.Core;

/// <summary>
/// Resolved группу для выполнения бизнес-процесса
/// </summary>
public interface IGroupResolutionService
{
    Task<BusinessFlowContext> ResolveAsync(
        string mailbox,
        string folder,
        Stream stream,
        CancellationToken cancellationToken = default);
}

public class GroupResolutionService : IGroupResolutionService
{
    private readonly IDecisionTableService _decisionTableService;
    private readonly IXlsxGroupResolverFactory _resolverFactory;
    private readonly DefaultXlsxGroupResolver _defaultResolver;

    public GroupResolutionService(
        IDecisionTableService decisionTableService,
        IXlsxGroupResolverFactory resolverFactory,
        DefaultXlsxGroupResolver defaultResolver)
    {
        _decisionTableService = decisionTableService;
        _resolverFactory = resolverFactory;
        _defaultResolver = defaultResolver;
    }

    public async Task<BusinessFlowContext> ResolveAsync(
        string mailbox,
        string folder,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        var row = _decisionTableService.GetRow(mailbox, folder)
            ?? throw new InvalidOperationException(
                $"Не найдена строка в DecisionTable для Mailbox={mailbox}, Folder={folder}");

        if (!row.Groups.Any())
        {
            throw new InvalidOperationException(
                $"Не указаны группы для Mailbox={mailbox}, Folder={folder}");
        }

        var group = await ResolveGroupAsync(stream, row.Groups, row.GroupResolver, cancellationToken);
        stream.Position = 0;

        return new BusinessFlowContext(mailbox, folder, stream, row, group);
    }

    private async Task<string> ResolveGroupAsync(
        Stream stream,
        IEnumerable<string> availableGroups,
        string? resolverName,
        CancellationToken cancellationToken)
    {
        if (availableGroups.Count() == 1)
        {
            return availableGroups.First();
        }

        if (!string.IsNullOrEmpty(resolverName))
        {
            var resolver = _resolverFactory.GetResolver(resolverName)
                ?? new ContextualXlsxGroupResolver(_resolverFactory, resolverName);

            return await resolver.ResolveAsync(stream, availableGroups, cancellationToken);
        }

        return await _defaultResolver.ResolveAsync(stream, availableGroups, cancellationToken);
    }
}
