namespace BusinessFlowApp.Services;

using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using BusinessFlowApp.Core;

public class DecisionTable
{
    public List<DecisionTableRow> Rows { get; set; } = new();
}

public class DecisionTableRow
{
    public string Folder { get; set; } = string.Empty;
    public string Mailbox { get; set; } = string.Empty;
    public FlowType FlowType { get; set; }

    /// <summary>
    /// Список групп для динамического определения группы по содержимому XLSX
    /// </summary>
    public List<string> Groups { get; set; } = new();

    /// <summary>
    /// Имя резолвера для определения группы (опционально)
    /// </summary>
    public string? GroupResolver { get; set; }
}

public class DecisionTableOptions
{
    public const string SectionName = "DecisionTable";
    public List<DecisionTableRow> Rows { get; set; } = new();
}

/// <summary>
/// Опции для резолвера групп на основе XLSX файлов
/// </summary>
public class XlsxGroupResolverOptions
{
    public const string SectionName = "XlsxGroupResolverOptions";

    /// <summary>
    /// Максимальное количество строк для чтения при определении группы
    /// </summary>
    public int MaxRowsToRead { get; set; } = 10;

    /// <summary>
    /// Индекс строки заголовков (0-based)
    /// </summary>
    public int HeaderRowIndex { get; set; } = 0;
}
