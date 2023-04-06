using Microsoft.Extensions.DependencyInjection;
using SmartLock.Implementation.Data;

namespace SmartLock.Implementation.Grains;

public abstract class GrainBase : Grain
{
    private readonly IServiceProvider _serviceProvider;

    protected GrainBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected SmartLockDbContext GetDbContext()
    {
        var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<SmartLockDbContext>();
    }
}