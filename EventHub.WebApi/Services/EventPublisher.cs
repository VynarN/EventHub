using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;
using EventHub.WebApi.Interfaces.Services;
using EventHub.WebApi.Models.EMs;
using EventHub.WebApi.Options;
using Microsoft.Extensions.Options;

namespace EventHub.WebApi.Services;

public sealed class EventPublisher : IEventPublisher
{
    public const string EventCreatedSubject = "Event.Created";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private readonly ServiceBusClient _client;
    private readonly string _queueName;

    public EventPublisher(ServiceBusClient client, IOptions<ServiceBusOptions> options)
    {
        _client = client;
        _queueName = options.Value.QueueName;
    }

    public async Task PublishEventCreatedAsync(Event evt, CancellationToken cancellationToken = default)
    {
        await using var sender = _client.CreateSender(_queueName);
        var message = CreateEventCreatedMessage(evt);
        await sender.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
    }

    internal static ServiceBusMessage CreateEventCreatedMessage(Event evt)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(evt, SerializerOptions);
        return new ServiceBusMessage(bytes)
        {
            Subject = EventCreatedSubject,
            ContentType = "application/json",
        };
    }
}
