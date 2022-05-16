using Newtonsoft.Json;

namespace Domain.Models
{
    public class Message
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string UserId { get; set; }

        public string ReplyToId { get; set; }

        public string Type { get; set; }

        public string Text { get; set; }

        public string ResponseText { get; set; }

    }
}
