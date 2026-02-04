namespace TelegramBotNotificationTwitch.Interface;

public interface IReceiverService
{
    Task ReceiveAsync(CancellationToken stoppingToken);
}