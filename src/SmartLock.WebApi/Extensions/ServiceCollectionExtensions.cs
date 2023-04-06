using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;
using SmartLock.Domain;
using SmartLock.Domain.Locks;
using SmartLock.Domain.Users;
using SmartLock.Implementation.Data;
using SmartLock.Implementation.Jwt;
using SmartLock.Implementation.Services;

namespace SmartLock.WebApi.Extensions;

/// <summary>
///   Extension methods for registering required services in an <see cref="IServiceCollection"/>.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    ///   Registers and configures implemented services for SmartLock in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <exception cref="ArgumentNullException">services</exception>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSmartLockServices(this IServiceCollection services)
    {
        services.AddScoped<ILockService, LockService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    /// <summary>
    ///   Registers DbContext and related services for applying migration and seeding data in the
    ///   <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///   In "appsettings.json" file the following sections should be available:
    ///   <para>"ConnectionStrings": { "DefaultConnection": "" }</para>
    /// </remarks>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The key/value application configuration.</param>
    /// <exception cref="ArgumentNullException">services</exception>
    /// <exception cref="ArgumentNullException">configuration</exception>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddEntityFramework(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        services.AddDbContextPool<SmartLockDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddTransient<DbMigrationManager>();
        services.AddTransient<DbDataSeeder>();

        return services;
    }

    /// <summary>
    ///   Registers and configures required services ASp.NET Core Identity system in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///   In "appsettings.json" file the following sections should be available:
    ///   <para>
    ///     "Jwt": { "Audience": "", "Issuer": "", "SecretKey": "", "ExpireDays": 0,
    ///     "ValidateAudience": true, "ValidateIssuer": true, "ValidateIssuerSigningKey": true,
    ///     "ValidateLifetime": true }
    ///   </para>
    /// </remarks>
    /// <param name="services">The services.</param>
    /// <param name="mvcBuilder">Adding parts to MVC.</param>
    /// <param name="configuration">The key/value application configuration.</param>
    /// <exception cref="ArgumentNullException">services</exception>
    /// <exception cref="ArgumentNullException">configuration</exception>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddIdentityAuthentication(this IServiceCollection services, IMvcBuilder mvcBuilder,
        IConfiguration configuration)
    {
        services.AddIdentity<UserEntity, RoleEntity>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<SmartLockDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        services.ConfigureApplicationCookie(options =>
        {
            options.Events = new CookieAuthenticationEvents()
            {
                OnRedirectToLogin = (ctx) =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        ctx.Response.StatusCode = 401;

                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = (ctx) =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        ctx.Response.StatusCode = 403;

                    return Task.CompletedTask;
                }
            };
        });

        services.AddDynamicAuthorization<SmartLockDbContext>(options =>
                options.DefaultAdminUser = DefaultData.ManagerEmail)
            .AddSqlServerStore(options =>
                options.ConnectionString = configuration.GetConnectionString("DefaultConnection"))
            .AddUi(mvcBuilder);

        return services;
    }

    /// <summary>
    ///   Registers required services for generating JWT token in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///   In "appsettings.json" file the following sections should be available:
    ///   <para>
    ///     "Jwt": { "Audience": "", "Issuer": "", "SecretKey": "", "ExpireDays": 0,
    ///     "ValidateAudience": true, "ValidateIssuer": true, "ValidateIssuerSigningKey": true,
    ///     "ValidateLifetime": true }
    ///   </para>
    /// </remarks>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The key/value application configuration.</param>
    /// <exception cref="ArgumentNullException">services</exception>
    /// <exception cref="ArgumentNullException">configuration</exception>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        JwtOptions jwtOptions = new();
        configuration.GetSection("Jwt").Bind(jwtOptions);
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                ValidateLifetime = jwtOptions.ValidateLifetime,
                ValidAudience = jwtOptions.Audience,
                ValidIssuer = jwtOptions.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.SecretKey))
            };
        });

        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        return services;
    }

    /// <summary>
    ///   Registers required services for generating JWT token in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///   In "appsettings.json" file the following sections should be available:
    ///   <para>
    ///     "Jwt": { "Audience": "", "Issuer": "", "SecretKey": "", "ExpireDays": 0,
    ///     "ValidateAudience": true, "ValidateIssuer": true, "ValidateIssuerSigningKey": true,
    ///     "ValidateLifetime": true }
    ///   </para>
    /// </remarks>
    /// <param name="services">The services.</param>
    /// <exception cref="ArgumentNullException">services</exception>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

        services.AddSwaggerGen(options =>
        {
            // JWT Bearer Authorization
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description =
                    "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.ExampleFilters();
        });

        return services;
    }

    /// <summary>
    ///   Registers and configures the MQTT services as hosted services in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///   In "appsettings.json" file the following sections should be available:
    ///   <para>"Host": { "MqttPort": 1234 }</para>
    /// </remarks>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The key/value application configuration.</param>
    /// <exception cref="ArgumentNullException">services</exception>
    /// <exception cref="ArgumentNullException">configuration</exception>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMqtt(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        services.AddHostedMqttServer(options =>
            {
                options.WithDefaultEndpointPort(configuration.GetValue<int>("Host:MqttPort"));
                options.WithDefaultEndpointBoundIPAddress(IPAddress.Any);
            })
            .AddMqttConnectionHandler()
            .AddMqttTcpServerAdapter();

        services.AddHostedService<MqttHostedService>();
        services.AddSingleton<ILockConnector, MqttHostedService>();

        return services;
    }
}