using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

//Create queue with Duplicate detection
public class PocServiceBusTrigger
{
    private readonly IConfiguration _config;
    public PocServiceBusTrigger(IConfiguration config)
    {
        _config = config;
    }

    [FunctionName(nameof(PocTimerTrigger))]
    public async Task PocTimerTrigger(
        [TimerTrigger("* * 17 1 * *", RunOnStartup = true)] TimerInfo timerInfo, ILogger logger)
    {
        var serviceBusClient = new ServiceBusClient(_config.GetValue<string>("AzureWebJobsServiceBus"));
        await using var sender = serviceBusClient.CreateSender(_config.GetValue<string>("ServiceBusQueueName"));

        for (int i = 0; i < 10; i++)
        {
            var serviceBusMessage = new ServiceBusMessage { MessageId = "SameMessageId" };
            var serviceBusRandomMessage = new ServiceBusMessage { MessageId = Random.Shared.Next(100, 1000).ToString() };

            await sender.SendMessageAsync(serviceBusMessage);
            logger.LogWarning("SENT with SAME messageId {MessageId}", serviceBusMessage.MessageId);
            await sender.SendMessageAsync(serviceBusRandomMessage);
            logger.LogWarning("SENT with RANDOM messageId {MessageId}", serviceBusRandomMessage.MessageId);
        }
    }

    [FunctionName(nameof(PocServiceBusTriggerAsync))]
    public static void PocServiceBusTriggerAsync(
        [ServiceBusTrigger("%ServiceBusQueueName%")] ServiceBusReceivedMessage message, ILogger logger)
    {
        logger.LogError("RECIEVED message {MessageId}", message.MessageId);
    }
}

