using Microsoft.Extensions.Logging;

namespace SmartLock.Implementation.Grains;

public class LoggingCallFilter : IIncomingGrainCallFilter
{
    private readonly ILogger<LoggingCallFilter> _logger;

    public LoggingCallFilter(ILogger<LoggingCallFilter> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(IIncomingGrainCallContext context)
    {
        try
        {
            await context.Invoke();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);
            throw;
        }
    }
}