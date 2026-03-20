using Azure.Messaging.ServiceBus;
using EventHub.Cosmos;
using EventHub.FunctionApp.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EventHub.FunctionApp;

public class EventConsumerFunction(ILogger<EventConsumerFunction> logger, ICosmosEventWriter cosmosEventWriter)
{
    [Function(nameof(ConsumeEventCreated))]
    public async Task ConsumeEventCreated(
        [ServiceBusTrigger("queue.1", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        if (!EventDeserialization.TryDeserialize(message.Body, out var evt, out var jsonException))
        {
            if (jsonException is not null)
            {
                logger.LogWarning(
                    jsonException,
                    "Failed to deserialize Service Bus message to Event. MessageId={MessageId}, Subject={Subject}",
                    message.MessageId,
                    message.Subject);
            }
            else
            {
                logger.LogWarning(
                    "Service Bus message deserialized to null Event. MessageId={MessageId}, Subject={Subject}",
                    message.MessageId,
                    message.Subject);
            }

            return;
        }

        LogReceivedEvent(evt, message);

        var doc = CosmosEventDocument.FromEvent(
            evt.Id,
            evt.UserId,
            EventDeserialization.SerializeEventType(evt.Type),
            evt.Description,
            evt.CreatedAt);

        try
        {
            await cosmosEventWriter.UpsertAsync(doc, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to persist event to Cosmos DB. MessageId={MessageId}, EventId={EventId}",
                message.MessageId,
                evt.Id);
            throw;
        }
    }

    private void LogReceivedEvent(Event evt, ServiceBusReceivedMessage message)
    {
        logger.LogInformation(
            "Received event. EventId={EventId}, UserId={UserId}, Type={Type}, Description={Description}, CreatedAt={CreatedAt:o}, Subject={Subject}, MessageId={MessageId}",
            evt.Id,
            evt.UserId,
            evt.Type,
            evt.Description,
            evt.CreatedAt,
            message.Subject,
            message.MessageId);
    }
}
