using Azure.Messaging.ServiceBus;

namespace MessageBus;
public class MessageSender
{
    const string ServiceBusConnectionString = "Endpoint=sb://sb-exam-eastus-01.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=QdaI054hZLBRI0mixTezONzwks5CqUL/5jtnuBPn5y8=";
    const string QueueName = "orders";
    const string TopicName = "reserved-orders";
    const string SubscriptionName = "reserved-orders-subscription";

    public async Task SendMessageAsync(string messageBody)
    {
        await using var client = new ServiceBusClient(ServiceBusConnectionString);

        await using ServiceBusSender sender = client.CreateSender(QueueName);
        try
        {
            var message = new ServiceBusMessage(messageBody);
            Console.WriteLine($"Sending message: {messageBody}");
            await sender.SendMessageAsync(message);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
        }
        finally
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}
