using TelegramBotNotificationTwitch.Model.JsonModel;

namespace TelegramBotNotificationTwitch.Interface;

public interface ITwitchService
{
    Task<string> GetAccessTokenAsync();

    Task<TwitchStream?> GetStreamStatusAsync(string accessToken);
}