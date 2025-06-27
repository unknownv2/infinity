
using Newtonsoft.Json;

namespace NoDev.Infinity.Build.InfinityBuilder.Manifests
{
    internal class GameInfo
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "thumbnail")]
        public byte[] Thumbnail { get; set; }
    }
}
