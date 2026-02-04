using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TelegramBotNotificationTwitch.Configuration;
using TelegramBotNotificationTwitch.Interface;
using TelegramBotNotificationTwitch.Model;
using TelegramBotNotificationTwitch.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection("TelegramBot"));
var connect = builder.Configuration.GetConnectionString("connectionStringSqlite");
builder.Services.AddDbContext<ApplicationContextService>(options => options.UseSqlite(connect));

builder.Services.AddHttpClient("telegram_bot_client")
    .RemoveAllLoggers()
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        BotConfiguration? botConfiguration = sp.GetService<IOptions<BotConfiguration>>()?.Value;
        ArgumentNullException.ThrowIfNull(botConfiguration);
        TelegramBotClientOptions options = new(botConfiguration.BotToken);
        return new TelegramBotClient(options, httpClient);
    });
builder.Services.AddScoped<IStorage, SqliteEfStorageService>();
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<ReceiverService>();
builder.Services.AddHostedService<PollingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Telegram Api",
    });
});




builder.Services.AddHttpClient<ITwitchService, TwitchServices>();
builder.Services.AddHostedService<TwitchApiBackgroundService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/send", async ([FromBody]MessageUser messageUser, UpdateHandler updateHandler) =>
{
    await updateHandler.SendMsg(messageUser);
});

app.Run();



