using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        
        for (int i = 0; i < 10; i++)
        {
            var serviceBusMessage = new ServiceBusMessage(i.ToString());
            serviceBusMessage.MessageId = "MessageId";
            serviceBusMessage.SessionId = "SAME";
            //serviceBusMessage.SessionId = i.ToString();
            await sender.SendMessageAsync(serviceBusMessage);
            logger.LogWarning("SENT message {ServiceBusMessage} with sessionId {sessionId}", serviceBusMessage.Body, serviceBusMessage.SessionId);
        }
    }

    [FunctionName(nameof(PocServiceBusTriggerAsync))]
    public static void PocServiceBusTriggerAsync(
        [ServiceBusTrigger("%ServiceBusQueueName%", IsSessionsEnabled = true)] ServiceBusReceivedMessage serviceBusReceivedMessage, ILogger logger)
    {
        logger.LogError("RECIEVED message {Message} with sessionId {SessionId}", serviceBusReceivedMessage.Body, serviceBusReceivedMessage.SessionId);
    }
}

