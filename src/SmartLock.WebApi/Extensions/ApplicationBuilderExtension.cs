using SmartLock.Implementation.Data;

namespace SmartLock.WebApi.Extensions;

/// <summary>
///   Contains extensions for configuring routing on an <see cref="IApplicationBuilder"/>.
/// </summary>
internal static class ApplicationBuilderExtension
{
    /// <summary>
    ///   Applies pending migration to database.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <exception cref="ArgumentNullException">throw if applicationBuilder if null</exception>
    /// <returns>The same application builder so that multiple calls can be chained.</returns>
    public static IApplicationBuilder ApplyDatabaseMigrations(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        var migrationManager = services.GetRequiredService<DbMigrationManager>();
        migrationManager.MigrateDatabaseAsync().Wait();

        return app;
    }

    /// <summary>
    ///   Seeds the database with default data. <see cref="DbDataSeeder"/> class to see what data is seeded.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <exception cref="System.ArgumentNullException">app</exception>
    /// <returns>The same application builder so that multiple calls can be chained.</returns>
    public static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        var migrationManager = services.GetRequiredService<DbDataSeeder>();
        migrationManager.SeedDbAsync().Wait();

        return app;
    }
}