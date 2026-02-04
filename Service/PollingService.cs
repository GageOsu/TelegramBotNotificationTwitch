using TelegramBotNotificationTwitch.Interface;

namespace TelegramBotNotificationTwitch.Service;

// Compose Polling and ReceiverService implementations
public class PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger)
    : PollingServiceBase<ReceiverService>(serviceProvider, logger);