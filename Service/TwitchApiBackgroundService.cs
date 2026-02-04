using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotNotificationTwitch.Interface;
using static System.Console;

namespace TelegramBotNotificationTwitch.Service;

public class TwitchApiBackgroundService(
    ITwitchService twitchApi, 
    ITelegramBotClient botClient, 
    IServiceProvider serviceProvider) : BackgroundService
{
    // TODO: Сделать флаг обновляемым
    private static bool isStreamOnline = false;
    private readonly ITwitchService _twitchApi = twitchApi;
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var accessToken = await _twitchApi.GetAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken))
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var result =  await _twitchApi.GetStreamStatusAsync(accessToken);
            if (result != null && isStreamOnline == false) 
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IStorage>();

                
                var users = context.GetUsers();
                foreach (var user in users)
                {
                    await _botClient.SendMessage(user.UserChatId, $"{result.UserName} запустил стрим, {result.Title}", cancellationToken: stoppingToken);
                    await Task.CompletedTask;
                }
                isStreamOnline = true;
            }
            if (isStreamOnline == true && result == null)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IStorage>();
                var users = context.GetUsers();
                foreach (var user in users)
                {
                    await _botClient.SendMessage(user.UserChatId,"Стрим закончили", cancellationToken: stoppingToken);
                    await Task.CompletedTask;
                }
            }
            await Task.Delay(30000, cancellationToken: stoppingToken);
        }
    }
}