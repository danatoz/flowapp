namespace BusinessFlowApp.Core;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniExcelLibs;
using BusinessFlowApp.Services;

/// <summary>
/// Базовый класс для кастомных резолверов групп
/// Содержит общую логику чтения XLSX
/// </summary>
public abstract class XlsxGroupResolverBase : IXlsxGroupResolver
{
    private readonly XlsxGroupResolverOptions _options;
    protected readonly ILogger Logger;

    protected XlsxGroupResolverBase(
        IOptions<XlsxGroupResolverOptions> options,
        ILogger logger)
    {
        _options = options.Value;
        Logger = logger;
    }

    public async Task<string> ResolveAsync(
        Stream stream,
        IEnumerable<string> availableGroups,
        CancellationToken cancellationToken = default)
    {
        var headers = await ReadXlsxHeadersAsync(stream, cancellationToken);

        Logger.LogInformation(
            "[{Resolver}] XLSX заголовки: {Columns}, ключи: {Keys}",
            GetType().Name,
            string.Join(", ", headers.ColumnNames),
            string.Join(", ", headers.KeyValues.Keys));

        var group = ResolveGroupFromHeaders(headers, availableGroups);

        Logger.LogInformation("[{Resolver}] Определена группа: {Group}", GetType().Name, group);

        return group;
    }

    /// <summary>
    /// Читает заголовки XLSX без загрузки всего файла
    /// </summary>
    protected async Task<XlsxHeaders> ReadXlsxHeadersAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var columnNames = new List<string>();
        var keyValues = new Dictionary<string, string?>();

        stream.Position = 0;

        var rows = await MiniExcel.QueryAsync(stream, cancellationToken: cancellationToken);
        var rowsList = rows.Take(_options.MaxRowsToRead).ToList();

        if (!rowsList.Any())
        {
            return new XlsxHeaders(columnNames.AsReadOnly(), keyValues.AsReadOnly());
        }

        var headerRow = rowsList.First();
        foreach (var kvp in headerRow)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Key))
            {
                columnNames.Add(kvp.Key);
            }
        }

        foreach (var row in rowsList.Skip(1).Take(_options.MaxRowsToRead))
        {
            foreach (var kvp in row)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Key) && kvp.Value != null)
                {
                    var value = kvp.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        keyValues[kvp.Key] = value;
                    }
                }
            }
        }

        return new XlsxHeaders(columnNames.AsReadOnly(), keyValues.AsReadOnly());
    }

    /// <summary>
    /// Абстрактный метод определения группы (реализуется в наследниках)
    /// </summary>
    protected abstract string ResolveGroupFromHeaders(
        XlsxHeaders headers,
        IEnumerable<string> availableGroups);
}
