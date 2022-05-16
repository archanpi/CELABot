using Newtonsoft.Json;

namespace Domain.GraphQlTypes.InputDtos
{
    public sealed class UserInputDto
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
