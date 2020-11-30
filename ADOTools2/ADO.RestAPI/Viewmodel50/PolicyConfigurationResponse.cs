using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ADO.RestAPI.Viewmodel50
{
    public class PolicyConfigurationResponse
    {
        // This is just a container class for all REST API responses related to policy configurations.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/policy/configurations?view=azure-devops-rest-5.0

        #region - Nested Classes and Enumerations.

        public class PolicyConfigurations
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<PolicyConfiguration> Value { get; set; }
        }

        public class PolicyConfiguration
        {
            [JsonProperty(PropertyName = "createdBy")]
            public IdentityReference CreatedBy { get; set; }

            [JsonProperty(PropertyName = "createdDate")]
            public string CreatedDate { get; set; }

            [JsonProperty(PropertyName = "isEnabled")]
            public string IsEnabled { get; set; }

            [JsonProperty(PropertyName = "isBlocking")]
            public string IsBlocking { get; set; }

            [JsonProperty(PropertyName = "isDeleted")]
            public string IsDeleted { get; set; }

            [JsonProperty(PropertyName = "settings")]
            public JObject Settings { get; set; }

            [JsonProperty(PropertyName = "revision")]
            public int Revision { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "type")]
            public JObject Type { get; set; }

        }

        #endregion
    }
}