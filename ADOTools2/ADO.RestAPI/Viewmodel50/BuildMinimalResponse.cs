using System.Runtime.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADO.RestAPI.Viewmodel50
{
    public class BuildMinimalResponse
    {
        // This is just a container class for all REST API responses related to build definitions.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/build/?view=azure-devops-rest-5.0

        #region - Nested Classes and Enumerations.

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DefinitionType
        {
            [EnumMember(Value = "build")]
            Build,
            [EnumMember(Value = "xaml")]
            Xaml
        }

        public class BuildDefinitionReferences
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<BuildDefinitionReference> Value { get; set; }
        }

        public class BuildDefinitionReference
        {
            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "path")]
            public string Path { get; set; }

            [JsonProperty(PropertyName = "type")]
            public DefinitionType Type { get; set; }

            [JsonProperty(PropertyName = "revision")]
            public int Revision { get; set; }
        }

        #endregion
    }
}
