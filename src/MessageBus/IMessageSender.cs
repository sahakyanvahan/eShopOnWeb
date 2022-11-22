namespace MessageBus;

public interface IMessageSender
{
    Task SendMessageAsync(string messageBody);
}