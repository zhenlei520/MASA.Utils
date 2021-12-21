namespace MASA.Utils.Caller.Core;

internal class DefaultCallerFactory : ICallerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<CallerRelations> _callers;

    public DefaultCallerFactory(IServiceProvider serviceProvider, List<CallerRelations> callers)
    {
        _serviceProvider = serviceProvider;
        _callers = callers;
    }

    public ICallerProvider CreateClient()
    {
        var caller = _callers.SingleOrDefault(c => c.IsDefault) ?? _callers.FirstOrDefault()!;
        return caller.Func.Invoke(_serviceProvider);
    }

    public ICallerProvider CreateClient(string name)
    {
        var caller = _callers.SingleOrDefault(c => c.Name == name);
        if (caller == null)
            throw new NotSupportedException($"Please make sure you have used {name} Caller");

        return caller.Func.Invoke(_serviceProvider);
    }
}