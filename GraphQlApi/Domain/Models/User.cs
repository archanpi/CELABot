using Newtonsoft.Json;

namespace Domain.Models
{
    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
