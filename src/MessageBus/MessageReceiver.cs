using Azure.Messaging.ServiceBus;

namespace MessageBus;
public class MessageReceiver : IMessageReceiver
{
    const string ServiceBusConnectionString = "Endpoint=sb://sb-exam-eastus-01.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=QdaI054hZLBRI0mixTezONzwks5CqUL/5jtnuBPn5y8=";
    const string QueueName = "orders";
    const string SubscriptionName = "reserved-orders-subscription";

    public async Task ReceiveMessageAsync()
    {
        var client = new ServiceBusClient(ServiceBusConnectionString);

        var processorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        };

        await using ServiceBusProcessor processor = client.CreateProcessor(QueueName, processorOptions);

        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;

        await processor.StartProcessingAsync();

        await processor.CloseAsync();
    }

    async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();

        await args.CompleteMessageAsync(args.Message);
    }

    Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}
