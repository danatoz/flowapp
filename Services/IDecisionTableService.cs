namespace BusinessFlowApp.Services;

using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

public interface IDecisionTableService
{
    DecisionTableRow? GetRow(string mailbox, string folder);
}

public class DecisionTableService : IDecisionTableService
{
    private readonly IReadOnlyList<DecisionTableRow> _rows;

    public DecisionTableService(IOptions<DecisionTableOptions> options)
    {
        _rows = options.Value.Rows.AsReadOnly();
    }

    public DecisionTableRow? GetRow(string mailbox, string folder)
    {
        return _rows.FirstOrDefault(r =>
            r.Mailbox.Equals(mailbox, StringComparison.OrdinalIgnoreCase) &&
            r.Folder.Equals(folder, StringComparison.OrdinalIgnoreCase));
    }
}
