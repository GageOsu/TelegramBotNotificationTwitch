using Telegram.Bot;
using TelegramBotNotificationTwitch.Interface;

namespace TelegramBotNotificationTwitch.Service;

public class ReceiverService(ITelegramBotClient botClient, UpdateHandler updateHandler, ILogger<ReceiverServiceBase<UpdateHandler>> logger)
    : ReceiverServiceBase<UpdateHandler>(botClient, updateHandler, logger);