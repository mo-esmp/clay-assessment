using Serilog;
using SmartLock.WebApi.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// For production must use Azure Key Vault and for development should use Secret Manager tool.
//builder.Configuration.AddAzureKeyVault(
//    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
//    new DefaultAzureCredential(new DefaultAzureCredentialOptions
//    {
//        ManagedIdentityClientId = builder.Configuration["AzureADManagedIdentityClientId"]
//    }));

// Add services to the container.
var mvcBuilder = builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddRazorPages();
builder.Services.AddEntityFramework(builder.Configuration);
builder.Services.AddIdentityAuthentication(mvcBuilder, builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddMqtt(builder.Configuration);
builder.Services.AddSmartLockServices();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Host.AddOrleans();
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext());

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(builder.Configuration.GetValue<int>("Host:HttpPort"));
    options.ListenAnyIP(builder.Configuration.GetValue<int>("Host:HttpsPort"), opt => opt.UseHttps());
    options.ListenAnyIP(builder.Configuration.GetValue<int>("Host:MqttPort"), opt => opt.UseMqtt());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseApiExceptionHandling();

//if (app.Environment.IsDevelopment())
//{
app.ApplyDatabaseMigrations();
app.SeedDatabase();
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapRazorPages();

app.Run();

public partial class Program
{
}