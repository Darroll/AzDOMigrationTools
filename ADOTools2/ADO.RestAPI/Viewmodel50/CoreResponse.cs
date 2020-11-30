using System.Collections.Generic;
using Newtonsoft.Json;

namespace ADO.RestAPI.Viewmodel50
{
    public class CoreResponse
    {
        // This is just a container class for all REST API responses related to teams.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/core/teams?view=azure-devops-rest-5.0

        #region - Nested Classes and Enumerations.

        public class ProjectProperties
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<ProjectProperty> Value { get; set; }
        }

        public class ProjectProperty
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }
        }

        public class Processes
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<Process> Value { get; set; }
        }

        public class Process
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "isDefault")]
            public bool IsDefault { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        public class TeamMembers
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<TeamMember> Value { get; set; }
        }

        public class TeamMember
        {
            [JsonProperty(PropertyName = "identity")]
            public IdentityReference Identity { get; set; }

            [JsonProperty(PropertyName = "isTeamAdmin")]
            public bool IsTeamAdmin { get; set; }
        }

        public class TeamProject
        {
            [JsonProperty(PropertyName = "_links")]
            public ProjectReferenceLinks Links { get; set; }
            
            [JsonProperty(PropertyName = "capabilities")]
            public object Capabilities { get; set; }

            [JsonProperty(PropertyName = "defaultTeam")]
            public WebApiTeamReference DefaultTeam { get; set; }
            
            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "lastUpdateTime")]
            public string LastUpdateTime { get; set; }

            [JsonProperty(PropertyName = "revision")]
            public int Revision { get; set; }

            [JsonProperty(PropertyName = "state")]
            public ProjectState State { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "visibility")]
            public ProjectVisibility Visibility { get; set; }
        }

        public class WebApiTeams
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<WebApiTeam> Value { get; set; }
        }

        public class WebApiTeam
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "identityUrl")]
            public string IdentityUrl { get; set; }

            [JsonProperty(PropertyName = "projectName")]
            public string ProjectName { get; set; }

            [JsonProperty(PropertyName = "projectId")]
            public string ProjectId { get; set; }
        }

        #endregion
    }
}
