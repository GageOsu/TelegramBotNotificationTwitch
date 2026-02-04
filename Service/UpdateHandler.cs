
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNotificationTwitch.Interface;
using TelegramBotNotificationTwitch.Model;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TelegramBotNotificationTwitch.Service;

// TODO Разбить update handler 
// TODO Сделать единный inlineMarkup
public class UpdateHandler(
    ITelegramBotClient bot,
    ILogger<UpdateHandler> logger,
    IStorage context) : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient = bot;
    private readonly ILogger<UpdateHandler> _logger = logger;
    private readonly IStorage _context = context;

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }


    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { } message }                        => OnMessage(message),
            { EditedMessage: { } message }                  => OnMessage(message),
            { CallbackQuery: { } callbackQuery } => OnCallbackQuery(callbackQuery),
            _                                               => UnknownUpdateHandlerAsync(update)
        });
    }

    public async Task SendMsg(MessageUser message)
    {
        var users = _context.GetUsers();
        foreach (var user in users)
        {
            await _botClient.SendMessage(user.UserChatId, message.Text);
             await Task.CompletedTask;
        }
    }

    private async Task OnMessage(Message msg)
    {
        _logger.LogInformation("Receive message type: {MessageType}", msg.Type);
        if (msg.Text is not { } messageText)
            return;

        Message sentMessage = await (messageText.Split(' ')[0] switch
        {
            "/start" => SendInlineKeyboard(msg),
            _ => Usage(msg)
        });
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.Id);
    }

    async Task<Message> Usage(Message msg)
    {
        const string usage = """
                <b><u>Bot menu</u></b>:
                Привет, я бот для оповещение о начале трансляции, 
                я пока в разработке поэтому если нашел ошибку
                то напиши в лс группы канала.
                /start - начать пользоваться ботом или если какая-то ошибка попробуйте снова запустить бота
            """;
        return await _botClient.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }

    async Task<Message> SendInlineKeyboard(Message msg)
    {
        if (msg.From?.Id is null)
        {
            return await _botClient.SendMessage(msg.Chat, "Ошибка");
        }
        var id = msg.From.Id;
        var user = _context.GetUsers().FirstOrDefault(u => u.UserChatId == id);
        if (user is null)
        {
            var newUser = new UserEntity()
            {
                UserChatId = id,
                IsNotify = false
            };
            var result = _context.Add(newUser);
            if (result == id)
            {
                await _botClient.SendMessage(msg.Chat, "Пользователь успешно зарегистрирован!");
            }
        }
        // TODO: Сделать inlineMarkup константными и т.д
        var inlineMarkup = new InlineKeyboardMarkup()
            .AddNewRow("Включить оповещение о стримах")
            .AddNewRow()
                .AddButton(InlineKeyboardButton.WithUrl("Telegram", "https://t.me/gageosustory"))
                .AddButton(InlineKeyboardButton.WithUrl("Twitch", "https://www.twitch.tv/gage_osu"))
                .AddButton(InlineKeyboardButton.WithUrl("TikTok", "https://tiktok.com/@gage_osu"))
                .AddButton(InlineKeyboardButton.WithUrl("Нельзя", "https://www.instagram.com/gageosu?igsh=MXB1OHE0ZTB2NTl5Nw=="));
        return await _botClient.SendMessage(msg.Chat, "Выберите опцию:", replyMarkup: inlineMarkup);
    }

    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        var userId = callbackQuery.From.Id;
        var user = _context.GetUserById(userId);

        if (user is null) return;
        var text = "Уведомления включены ✅";
        if (callbackQuery.Data == text && callbackQuery.Message is not null)
        {
            user.IsNotify = false;
            _context.Update(user);
            var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow("Включить оповещение о стримах")
                .AddNewRow()
                .AddButton(InlineKeyboardButton.WithUrl("Telegram", "https://t.me/gageosustory"))
                .AddButton(InlineKeyboardButton.WithUrl("Twitch", "https://www.twitch.tv/gage_osu"))
                .AddButton(InlineKeyboardButton.WithUrl("TikTok", "https://tiktok.com/@gage_osu"))
                .AddButton(InlineKeyboardButton.WithUrl("Нельзя", "https://www.instagram.com/gageosu?igsh=MXB1OHE0ZTB2NTl5Nw=="));
            await _botClient.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, "Выберите опцию:", replyMarkup: inlineMarkup);
            //await bot.SendMessage(callbackQuery.Message!.Chat, $"Received {callbackQuery.Data}");
        }
        else if (callbackQuery.Data == "Включить оповещение о стримах" && callbackQuery.Message is not null)
        {
            user.IsNotify = true;
            _context.Update(user);
            var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow("Уведомления включены ✅")
                .AddNewRow()
                .AddButton(InlineKeyboardButton.WithUrl("Telegram", "https://t.me/gageosustory"))
                .AddButton(InlineKeyboardButton.WithUrl("Twitch", "https://www.twitch.tv/gage_osu"))
                .AddButton(InlineKeyboardButton.WithUrl("TikTok", "https://tiktok.com/@gage_osu"))
                .AddButton(InlineKeyboardButton.WithUrl("Нельзя", "https://www.instagram.com/gageosu?igsh=MXB1OHE0ZTB2NTl5Nw=="));
            await _botClient.EditMessageText(callbackQuery.Message.Chat.Id, callbackQuery.Message.Id, "Выберите опцию:", replyMarkup: inlineMarkup);
        }

    }

    private Task UnknownUpdateHandlerAsync(Telegram.Bot.Types.Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }


}