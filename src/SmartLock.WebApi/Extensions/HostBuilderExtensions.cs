using Orleans.Configuration;
using Orleans.Serialization;
using SmartLock.Implementation.Grains;

namespace SmartLock.WebApi.Extensions;

public static class HostBuilderExtensions
{
    private const string OrleansDistributedCounterStorageProvider = "OrleansDistributedCounterStorageProvider";

    public static IHostBuilder AddOrleans(this IHostBuilder builder)
    {
        builder.UseOrleans((ctx, siloBuilder) =>
        {
            var connection = ctx.Configuration.GetConnectionString("DefaultConnection");

            if (ctx.HostingEnvironment.IsDevelopment())
            {
                siloBuilder.UseLocalhostClustering();
                siloBuilder.AddMemoryGrainStorageAsDefault();
            }
            else
            {
                const string invariant = "System.Data.SqlClient";

                siloBuilder.UseAdoNetClustering(options =>
                {
                    options.Invariant = invariant;
                    options.ConnectionString = connection;
                }).Configure<ClusterOptions>(opt =>
                {
                    opt.ClusterId = "SmartLockCluster";
                    opt.ServiceId = "SmartLock";
                }).AddAdoNetGrainStorage(
                    "OrleansDistributedCounterStorageProvider",
                    opt =>
                    {
                        opt.ConnectionString = connection;
                        opt.Invariant = invariant;
                    });
            }

            siloBuilder.AddIncomingGrainCallFilter<LoggingCallFilter>();
            siloBuilder.Services.AddSerializer(serializerBuilder =>
            {
                serializerBuilder.AddNewtonsoftJsonSerializer(
                    isSupported: type => type.Namespace.StartsWith("SmartLock.Domain.Users"));
            });
        });

        return builder;
    }
}