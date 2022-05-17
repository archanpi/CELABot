using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GraphQlTypes.InputDtos
{
    public sealed class MessageInputDto
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
