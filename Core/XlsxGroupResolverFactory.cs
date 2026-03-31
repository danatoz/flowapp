namespace BusinessFlowApp.Core;

/// <summary>
/// Фабрика резолверов групп
/// </summary>
public class XlsxGroupResolverFactory : IXlsxGroupResolverFactory
{
    private readonly Dictionary<string, IXlsxGroupResolver> _resolvers;

    public XlsxGroupResolverFactory(IEnumerable<IXlsxGroupResolver> resolvers)
    {
        _resolvers = resolvers
            .GroupBy(r => GetResolverName(r.GetType()))
            .ToDictionary(
                g => g.Key,
                g => g.First(),
                StringComparer.OrdinalIgnoreCase);
    }

    public IXlsxGroupResolver? GetResolver(string resolverName)
    {
        return string.IsNullOrEmpty(resolverName) 
            ? null 
            : _resolvers.GetValueOrDefault(resolverName);
    }

    public IEnumerable<string> GetRegisteredResolverNames() => _resolvers.Keys;

    private static string GetResolverName(Type resolverType)
    {
        var name = resolverType.Name;
        return name.EndsWith("GroupResolver", StringComparison.OrdinalIgnoreCase)
            ? name[..^"GroupResolver".Length]
            : name;
    }
}
