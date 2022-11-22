namespace MessageBus;

public interface IMessageReceiver
{
    Task ReceiveMessageAsync();
}
