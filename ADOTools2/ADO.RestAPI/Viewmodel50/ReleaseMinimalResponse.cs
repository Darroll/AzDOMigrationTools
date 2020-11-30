using System.Collections.Generic;
using Newtonsoft.Json;

namespace ADO.RestAPI.Viewmodel50
{
    public class ReleaseMinimalResponse
    {
        // This is just a container class for all REST API responses related to release definitions.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/release/definitions?view=azure-devops-rest-5.1

        #region - Nested Classes and Enumerations.

        public class ReleaseDefinitions
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IEnumerable<ReleaseDefinition> Value { get; set; }
        }

        public class ReleaseDefinition
        {
            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "path")]
            public string Path { get; set; }

            [JsonProperty(PropertyName = "type")]
            public int Type { get; set; }

            [JsonProperty(PropertyName = "revision")]
            public int Revision { get; set; }
        }

        #endregion
    }
}
