using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using SmartLock.Domain;
using SmartLock.Domain.Locks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartLock.WebApi.HostedServices;

public class MqttHostedService : IHostedService, ILockConnector
{
    private readonly MqttServer _server;
    private readonly IClusterClient _orleansClient;
    private readonly ILogger<MqttHostedService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public MqttHostedService(
        MqttServer server, IClusterClient orleansClient, ILogger<MqttHostedService> logger)
    {
        _server = server;
        _orleansClient = orleansClient;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.ValidatingConnectionAsync += HandleValidatingConnectionAsync;
        _server.ClientConnectedAsync += HandleClientConnectedAsync;
        _server.ClientDisconnectedAsync += HandleClientDisconnectedAsync;
        _server.ClientSubscribedTopicAsync += HandleClientSubscribedHandlerAsync;
        _server.InterceptingPublishAsync += HandleInterceptingPublishAsync;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _server.ValidatingConnectionAsync -= HandleValidatingConnectionAsync;
        _server.ClientConnectedAsync -= HandleClientConnectedAsync;
        _server.ClientDisconnectedAsync -= HandleClientDisconnectedAsync;
        _server.ClientSubscribedTopicAsync -= HandleClientSubscribedHandlerAsync;
        _server.InterceptingPublishAsync -= HandleInterceptingPublishAsync;
        return Task.CompletedTask;
    }

    public async Task SendMessageAsync(string topic, object message)
    {
        var payload = JsonSerializer.Serialize(message, SerializerOptions);
        var builder = new MqttApplicationMessageBuilder();
        builder
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce);
        var mqttMessage = builder.Build();

        await _server.InjectApplicationMessage(new InjectedMqttApplicationMessage(mqttMessage)
        { SenderClientId = "SenderClientId" });
    }

    private Task HandleValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
    {
        if (!Guid.TryParse(arg.UserName, out _))
        {
            arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
        }

        return Task.CompletedTask;
    }

    private Task HandleClientConnectedAsync(ClientConnectedEventArgs eventArgs)
    {
        _logger.LogInformation($"Lock Connected: {eventArgs.ClientId}");

        return Task.CompletedTask;
    }

    private Task HandleClientDisconnectedAsync(ClientDisconnectedEventArgs eventArgs)
    {
        _logger.LogInformation($"Lock disconnected: {eventArgs.ClientId}");

        return Task.CompletedTask;
    }

    private Task HandleClientSubscribedHandlerAsync(ClientSubscribedTopicEventArgs eventArgs)
    {
        _logger.LogInformation($"Lock {eventArgs.ClientId} subscribed to topic {eventArgs.TopicFilter.Topic}");

        return Task.CompletedTask;
    }

    private Task HandleInterceptingPublishAsync(InterceptingPublishEventArgs arg)
    {
        try
        {
            if (!Guid.TryParse(arg.ClientId, out var lockId))
            {
                return Task.CompletedTask;
            }

            var payloadStr = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
            var message = JsonSerializer.Deserialize<MqttLockAccessRequest>(payloadStr, SerializerOptions);
            var (userId, userRoles) = GetUserIdAndRolesFromToken(message.JwtToken);

            var lockGrain = _orleansClient.GetGrain<ILockGrain>(lockId.ToString());
            lockGrain.RequestAccessAsync(userId, userRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return Task.CompletedTask;
    }

    private static (string userId, string[] userRoles) GetUserIdAndRolesFromToken(string jwtEncoded)
    {
        JwtSecurityTokenHandler handler = new();
        var tokenStr = handler.ReadJwtToken(jwtEncoded);
        var userId = tokenStr.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData)?.Value;
        var userRoles = tokenStr.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();

        return (userId, userRoles);
    }
}

public class MqttLockAccessRequest
{
    // Used JWT token as an identifier that employee's tag stored and receives from the lock
    public string JwtToken { get; set; }
}