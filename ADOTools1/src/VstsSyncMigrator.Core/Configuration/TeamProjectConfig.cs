using System;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration
{
    public class TeamProjectConfig
    {
        [JsonProperty(PropertyName = "collection")]
        public Uri Collection { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}
