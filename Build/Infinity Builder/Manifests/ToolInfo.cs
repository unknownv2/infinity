using NoDev.Infinity.Build.InfinityBuilder.Solutions;
using Newtonsoft.Json;

namespace NoDev.Infinity.Build.InfinityBuilder.Manifests
{
    internal class ToolInfo
    {
        public CSharpProject Project { get; set; }
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "game")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "access")]
        public int AccessLevel { get; set; }

        [JsonProperty(PropertyName = "ignore")]
        public bool Ignore { get; set; }
    }
}
