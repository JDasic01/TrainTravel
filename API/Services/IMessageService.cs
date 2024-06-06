public interface IMessageService<T>
{
    Task SendMessageAsync(T message, string channel);
    Task<T> ReceiveMessageAsync(string channel);
}