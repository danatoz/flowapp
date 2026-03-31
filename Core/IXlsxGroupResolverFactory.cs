namespace BusinessFlowApp.Core;

/// <summary>
/// Фабрика для создания именованных резолверов групп
/// </summary>
public interface IXlsxGroupResolverFactory
{
    /// <summary>
    /// Получает резолвер по имени
    /// </summary>
    /// <param name="resolverName">Имя резолвера (из конфигурации)</param>
    /// <returns>Резолвер или null если не найден</returns>
    IXlsxGroupResolver? GetResolver(string resolverName);
    
    /// <summary>
    /// Получает все зарегистрированные имена резолверов
    /// </summary>
    IEnumerable<string> GetRegisteredResolverNames();
}
