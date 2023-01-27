using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class PocServiceBusTrigger
{
    private readonly IConfiguration _config;
    public PocServiceBusTrigger(IConfiguration config)
    {
        _config = config;
    }

    [FunctionName(nameof(PocTimerTrigger))]
    public async Task PocTimerTrigger(
        [TimerTrigger("* * 17 * * *", RunOnStartup = true)] TimerInfo timerInfo, ILogger logger)
    {
        var serviceBusClient = new ServiceBusClient(_config.GetValue<string>("AzureWebJobsServiceBus"));
        await using var sender = serviceBusClient.CreateSender(_config.GetValue<string>("ServiceBusQueueName"));
        const string MESSAGEID = "1";

        for (int i = 0; i < 10; i++)
        {
            var serviceBusMessage = new ServiceBusMessage();
            serviceBusMessage.MessageId = MESSAGEID;

            var random = Random.Shared.Next(100, 1000).ToString();
            var serviceBusRandomMessage = new ServiceBusMessage(random);
            serviceBusRandomMessage.MessageId = random;

            await sender.SendMessageAsync(serviceBusMessage);
            await sender.SendMessageAsync(serviceBusRandomMessage);
            logger.LogWarning("SENT with SAME messageId {MessageId}", MESSAGEID);
            logger.LogWarning("SENT with RANDOM messageId {MessageId}", random);
        }
    }

    [FunctionName(nameof(PocServiceBusTriggerAsync))]
    public static void PocServiceBusTriggerAsync(
        [ServiceBusTrigger("%ServiceBusQueueName%")] ServiceBusReceivedMessage message, ILogger logger)
    {
        logger.LogError("RECIEVED message {MessageId}", message.MessageId);
    }
}

