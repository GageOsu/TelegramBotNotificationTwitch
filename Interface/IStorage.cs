using TelegramBotNotificationTwitch.Model;

namespace TelegramBotNotificationTwitch.Interface;

public interface IStorage
{
    List<UserEntity> GetUsers();
    UserEntity GetUserById(long id);
    long Add(UserEntity user);
    long Update(UserEntity user);
    bool Remove(long id);
}