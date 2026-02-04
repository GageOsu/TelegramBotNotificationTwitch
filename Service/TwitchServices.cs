using System.Net.Http.Headers;
using System.Text.Json;
using TelegramBotNotificationTwitch.Configuration;
using TelegramBotNotificationTwitch.Interface;
using TelegramBotNotificationTwitch.Model.JsonModel;
using static System.Console;

namespace TelegramBotNotificationTwitch.Service
{
    public class TwitchServices(
        IConfiguration configuration,
        HttpClient httpClient) : ITwitchService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpClient _httpClient = httpClient;

        public async Task<string> GetAccessTokenAsync()
        {
            var twitchUser = _configuration.GetSection("TwitchApi").Get<TwitchUserConfiguration>();
            if (twitchUser is not null)
            {

                var content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string,string>("client_id", twitchUser.CLIENT_ID),
                    new KeyValuePair<string,string>("client_secret", twitchUser.CLIENT_SECRET),
                    new KeyValuePair<string,string>("grant_type", "client_credentials")
                ]);

                var response = await _httpClient.PostAsync("https://id.twitch.tv/oauth2/token", content);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                if(doc.RootElement.TryGetProperty("access_token", out var token))
                {
                    return token.GetString()!;
                }
            }
            return null!;
        }

        public async Task<TwitchStream?> GetStreamStatusAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                WriteLine("Ошибка: не удалось получить токен");
                return null;
            }
            var twitchUser = _configuration.GetSection("TwitchApi").Get<TwitchUserConfiguration>();
            if (twitchUser is null)
            {
                return null;
            }
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.twitch.tv/helix/streams?user_id={twitchUser.CLIENT_BROADCAST}");

            request.Headers.Add("Client-ID", twitchUser.CLIENT_ID);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = await response.Content.ReadAsStringAsync();
                var streamResponse = JsonSerializer.Deserialize<StreamResponse>(json, options);
                if (streamResponse is null)
                {
                    return null;
                }
                var stream = streamResponse.Data.First();
                if(stream is not null)
                {
                    return stream;
                }
            }
            return null;
        }
    }
}