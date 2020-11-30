using System.Collections.Generic;
using Newtonsoft.Json;

namespace ADO.RestAPI.Viewmodel50
{
    public class ServiceEndpointResponse
    {
        // This is just a container class for all REST API responses related to service endpoints.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/serviceendpoint/endpoints?view=azure-devops-rest-5.0

        // To know which service endpoint types.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/serviceendpoint/types/list?view=azure-devops-server-rest-5.0

        public class EndpointAuthorization
        {
            [JsonProperty(PropertyName = "parameters")]
            public object Parameters { get; set; }

            [JsonProperty(PropertyName = "scheme")]
            public string Scheme { get; set; }
        }

        public class ServiceEndpoints
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<ServiceEndpoint> Value { get; set; }
        }

        public class ServiceEndpoint
        {
            [JsonProperty(PropertyName = "administrationsGroup")]
            public IdentityReference AdministrationsGroup { get; set; }

            [JsonProperty(PropertyName = "authorization")]
            public EndpointAuthorization Authorization { get; set; }

            [JsonProperty(PropertyName = "createdBy")]
            public IdentityReference CreatedBy { get; set; }

            [JsonProperty(PropertyName = "data")]
            public object Data { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "groupScopeId")]
            public string GroupScopeId { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "isReady")]
            public bool IsReady { get; set; }

            [JsonProperty(PropertyName = "isShared")]
            public bool IsShared { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "operationStatus")]
            public AbstractJObject OperationStatus { get; set; }

            [JsonProperty(PropertyName = "owner")]
            public string Owner { get; set; }

            [JsonProperty(PropertyName = "readersGroup")]
            public IdentityReference ReadersGroup { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }
    }
}
