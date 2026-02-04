using System.Text.Json.Serialization;

namespace TelegramBotNotificationTwitch.Model
{
    public class Pagination
    {
        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
    }
}