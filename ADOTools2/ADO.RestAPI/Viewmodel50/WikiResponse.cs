using System.Collections.Generic;
using Newtonsoft.Json;

namespace ADO.RestAPI.Viewmodel50
{
    public class WikiResponse
    {
        // This is just a container class for all REST API responses related to wikis.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/wikis?view=azure-devops-rest-5.0

        #region - Nested Classes and Enumerations.

        public class Wikis
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<Wiki> Value { get; set; }
        }

        public class Wiki
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "versions")]
            public IList<VersionDescriptor> Versions { get; set; }
            
            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
            
            [JsonProperty(PropertyName = "remoteUrl")]
            public string RemoteUrl { get; set; }
            
            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
            
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
            
            [JsonProperty(PropertyName = "projectId")]
            public string ProjectId { get; set; }
            
            [JsonProperty(PropertyName = "repositoryId")]
            public string RepositoryId { get; set; }
            
            [JsonProperty(PropertyName = "mappedPath")]
            public string MappedPath { get; set; }
        }

        public class VersionDescriptor
        {
            [JsonProperty(PropertyName = "version")]
            public string Version { get; set; }
        }

        #endregion
    }
}
