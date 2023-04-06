using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmartLock.Implementation.Data;

public class DbMigrationManager
{
    private readonly IServiceProvider _serviceProvider;

    public DbMigrationManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateDatabaseAsync()
    {
        ILogger<SmartLockDbContext> logger = null;

        try
        {
            logger = _serviceProvider.GetRequiredService<ILogger<SmartLockDbContext>>();
            await using var dbContext = _serviceProvider.GetRequiredService<SmartLockDbContext>();

            var applied = (await dbContext.GetService<IHistoryRepository>().GetAppliedMigrationsAsync())
                .Select(m => m.MigrationId);

            var total = dbContext.GetService<IMigrationsAssembly>()
                .Migrations.Select(m => m.Key);

            var hasPendingMigration = total.Except(applied).Any();
            if (hasPendingMigration)
                await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, ex.Message);
            Console.WriteLine(ex);
            throw;
        }
    }
}