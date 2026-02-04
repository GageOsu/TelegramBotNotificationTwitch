using TelegramBotNotificationTwitch.Interface;
using TelegramBotNotificationTwitch.Model;

namespace TelegramBotNotificationTwitch.Service;

public class SqliteEfStorageService(ApplicationContextService _context) : IStorage
{
    protected readonly ApplicationContextService _context = _context;

    public long Add(UserEntity user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return user.UserChatId;
    }

    public UserEntity GetUserById(long id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return null!;
        return user;
    }

    public List<UserEntity> GetUsers() 
    {
        return [.. _context.Users.Where(u => u.IsNotify == true)];
    }

    public bool Remove(long id)
    {
        var contact = _context.Users.Find(id);
        if (contact == null) return false;
        _context.Remove(contact);
        _context.SaveChanges();
        return true;
    }

    public long Update(UserEntity updateUser)
    {
        _context.Users.Update(updateUser);
        _context.SaveChanges();
        return updateUser.UserChatId;
    }
}