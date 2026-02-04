namespace TelegramBotNotificationTwitch.Model;

public class UserEntity
{
    public long Id { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public long UserChatId { get; set; }
    public bool IsNotify { get; set; }
}