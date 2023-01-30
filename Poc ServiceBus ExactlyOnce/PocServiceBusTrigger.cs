using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
//Create queue with Duplicate detection and session enabled
public class PocServiceBusTrigger
{
    private readonly IConfiguration _config;
    public PocServiceBusTrigger(IConfiguration config)
    {
        _config = config;
    }

    [FunctionName(nameof(PocTimerTriggerAsync))]
    public async Task PocTimerTriggerAsync(
        [TimerTrigger("* * 17 1 * *", RunOnStartup = true)] TimerInfo timerInfo, ILogger logger)
    {
        await SendMessagesAsync(logger);
    }

    [FunctionName(nameof(PocHttpTriggerAsync))]
    public async Task PocHttpTriggerAsync(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
    ILogger logger)
    {
        await SendMessagesAsync(logger);
    }

    [FunctionName(nameof(PocServiceBusTriggerAsync))]
    public static void PocServiceBusTriggerAsync(
        [ServiceBusTrigger("%ServiceBusQueueName%", IsSessionsEnabled = true)] ServiceBusReceivedMessage serviceBusReceivedMessage, ILogger logger)
    {
        logger.LogError("RECIEVED message {Message} with sessionId {SessionId}", serviceBusReceivedMessage.Body, serviceBusReceivedMessage.SessionId);
    }

    private async Task SendMessagesAsync(ILogger logger)
    {
        var serviceBusClient = new ServiceBusClient(_config.GetValue<string>("AzureWebJobsServiceBus"));
        var sender = serviceBusClient.CreateSender(_config.GetValue<string>("ServiceBusQueueName"));

        for (int i = 0; i < _config.GetValue<int>("NumberOfMessages"); i++)
        {
            var serviceBusMessage = new ServiceBusMessage(i.ToString());
            serviceBusMessage.MessageId = "MessageId";
            //serviceBusMessage.SessionId = "SAME";
            serviceBusMessage.SessionId = i.ToString();//Uncomment this and comment above to test with variable session id
            await sender.SendMessageAsync(serviceBusMessage);
            logger.LogWarning("SENT message {ServiceBusMessage} with sessionId {sessionId}", serviceBusMessage.Body, serviceBusMessage.SessionId);
        }
    }
}

