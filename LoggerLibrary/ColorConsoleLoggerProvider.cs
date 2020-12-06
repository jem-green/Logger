using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

public sealed class ColorConsoleLoggerProvider : ILoggerProvider
{
    private readonly ColorConsoleLoggerConfiguration _config;
    private readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers = new ConcurrentDictionary<string, ColorConsoleLogger>();

    public ColorConsoleLoggerProvider(ColorConsoleLoggerConfiguration config)
    {
        _config = config;
    }

    public ILogger CreateLogger(string categoryName)
    {
        string name = "";
        ColorConsoleLogger logger = new ColorConsoleLogger(name, _config);

        _loggers.GetOrAdd(categoryName, logger);
        return(_loggers.GetOrAdd(categoryName, logger));
    }
    public void Dispose()
    {
        _loggers.Clear();
    }
}