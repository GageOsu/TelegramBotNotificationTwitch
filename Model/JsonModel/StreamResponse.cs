using System.Text.Json.Serialization;

namespace TelegramBotNotificationTwitch.Model.JsonModel
{
    public class StreamResponse
    {
        [JsonPropertyName("data")]
        public List<TwitchStream> Data { get; set; } = [];

        [JsonPropertyName("pagination")]
        public Pagination? Pagination { get; set; }
    }
}
