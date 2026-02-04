using Microsoft.EntityFrameworkCore;
using TelegramBotNotificationTwitch.Model;

namespace TelegramBotNotificationTwitch.Service;

public class ApplicationContextService : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    public ApplicationContextService(DbContextOptions<ApplicationContextService> options) 
        : base (options)
    {
         Database.EnsureCreatedAsync();
    }
}