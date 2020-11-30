using Newtonsoft.Json;

namespace ADO.Engine.Configuration
{
    public sealed class NamespacePathSecuritySpec
    {
        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "teamName")]
        public string TeamName { get; set; }

        [JsonProperty(PropertyName = "disableInheritance")]
        public bool DisableInheritance { get; set; }
    }
}
