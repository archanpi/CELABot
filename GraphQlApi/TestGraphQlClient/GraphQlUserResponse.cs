using Newtonsoft.Json;

namespace TestGraphQlClient
{
    public class GraphQlUserResponse
    {
        public List<User> Users { get; set; }
    }

    public class MutationResponse
    {
        public bool SaveMessage { get; set; }
    }

    public class Message
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }

        public string? UserId { get; set; }

        public string? ReplyToId { get; set; }

        public string? Type { get; set; }

        public string? Text { get; set; }

        public string? ResponseText { get; set; }

    }

    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }

        public string? Name { get; set; }

        public List<Message>? Messages { get; set; }
    }
}
