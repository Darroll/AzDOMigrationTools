using System;
using System.Reflection;
using Newtonsoft.Json;

namespace ADO.Engine.Configuration
{
    public sealed class RestApiServiceConfig
    {
        #region - Static Declarations

        #region - Public Members

        public static RestApiServiceConfig GetDefault()
        {
            // Define default configuration.
            RestApiServiceConfig defaultConfig = new RestApiServiceConfig
            {
                BuildApi = "5.0",
                CoreApi = "5.0",
                DistributedTaskApi = "2.0-preview.1",
                ExtensionMgmtApi = "5.0-preview.1",
                GetClassificationNodesApi = "5.0",
                GetProjectPropertiesApi = "5.0-preview.1",
                GitApi = "5.0",
                GitImportRequestsApi = "5.0-preview.1",
                GitPullRequestsApi = "5.0",
                GitRepositoriesApi = "5.0",
                GraphApi = "5.0-preview.1",
                PolicyConfigurationsApi = "5.0",
                ProcessesApi = "5.0",
                ProjectsApi = "5.0",
                ReleaseApi = "5.0",
                SecurityApi = "5.0-preview.1",
                ServiceEndpointApi = "5.0-preview.1",
                TaskAgentApi = "5.0-preview.1",
                TeamsApi = "5.0",
                WikiApi = "5.0",
                WIQLApi = "5.0",
                WorkApi = "5.0",
                WorkBoardsApi = "5.0",
                WorkItemTrackingApi = "5.0",
                WorkTeamSettingsApi = "5.0"
            };

            // Return default configuration.
            return defaultConfig;
        }

        #endregion

        #endregion

        #region - Public Members

        [JsonProperty(PropertyName = "build")]
        public string BuildApi { get; set; }

        [JsonProperty(PropertyName = "coreApi")]
        public string CoreApi { get; set; }

        [JsonProperty(PropertyName = "distributedTask")]
        public string DistributedTaskApi { get; set; }

        [JsonProperty(PropertyName = "extensionMgmtApi")]
        public string ExtensionMgmtApi { get; set; }

        [JsonProperty(PropertyName = "taskAgentApi")]
        public string TaskAgentApi { get; set; }

        [JsonProperty(PropertyName = "git")]
        public string GitApi { get; set; }

        [JsonProperty(PropertyName = "git|PullRequests")]
        public string GitPullRequestsApi { get; set; }

        [JsonProperty(PropertyName = "git|Repositories")]
        public string GitRepositoriesApi { get; set; }

        [JsonProperty(PropertyName = "git|ImportRequests")]
        public string GitImportRequestsApi { get; set; }

        [JsonProperty(PropertyName = "graph")]
        public string GraphApi { get; set; }

        [JsonProperty(PropertyName = "policy")]
        public string PolicyConfigurationsApi { get; set; }

        [JsonProperty(PropertyName = "processes")]
        public string ProcessesApi { get; set; }

        [JsonProperty(PropertyName = "projects")]
        public string ProjectsApi { get; set; }

        [JsonProperty(PropertyName = "projects|Get Project Properties")]
        public string GetProjectPropertiesApi { get; set; }

        [JsonProperty(PropertyName = "teams")]
        public string TeamsApi { get; set; }

        [JsonProperty(PropertyName = "release")]
        public string ReleaseApi { get; set; }

        [JsonProperty(PropertyName = "security")]
        public string SecurityApi { get; set; }

        [JsonProperty(PropertyName = "serviceEndpoint")]
        public string ServiceEndpointApi { get; set; }

        [JsonProperty(PropertyName = "wiki")]
        public string WikiApi { get; set; }

        [JsonProperty(PropertyName = "wiql")]
        public string WIQLApi { get; set; }

        [JsonProperty(PropertyName = "work")]
        public string WorkApi { get; set; }

        [JsonProperty(PropertyName = "work|Boards")]
        public string WorkBoardsApi { get; set; }

        [JsonProperty(PropertyName = "work|TeamSettings")]
        public string WorkTeamSettingsApi { get; set; }

        [JsonProperty(PropertyName = "workItemTracking")]
        public string WorkItemTrackingApi { get; set; }

        [JsonProperty(PropertyName = "workItemTracking|Get Classification Nodes")]
        public string GetClassificationNodesApi { get; set; }

        public void SetDefaultIfUndefined()
        {
            // Initialize.
            string defaultApiVersion = "5.0";
            object newValue = null;
            PropertyInfo[] propertyInfos;

            // Get all public properties using reflection.
            Type myClassType = this.GetType();
            propertyInfos = myClassType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // Sort properties by name.
            Array.Sort(propertyInfos,
                    delegate (PropertyInfo propertyInfo1, PropertyInfo propertyInfo2)
                    { return propertyInfo1.Name.CompareTo(propertyInfo2.Name); });

            // If not defined, set default value for each property.
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                // Extract property's value.
                object value = propertyInfo.GetValue(this, null);

                // If value is not defined, set default.
                if (value == null)
                {
                    // Address the exceptions.
                    if (propertyInfo.Name == "DistributedTaskApi")
                        newValue = "2.0-preview.1";
                    if (propertyInfo.Name == "ExtensionMgmtApi")
                        newValue = "5.0-preview.1";
                    else if (propertyInfo.Name == "GetProjectPropertiesApi")
                        newValue = "5.0-preview.1";
                    else if (propertyInfo.Name == "GitImportRequestsApi")
                        newValue = "5.0-preview.1";
                    else if (propertyInfo.Name == "GraphApi")
                        newValue = "5.0-preview.1";
                    else if (propertyInfo.Name == "SecurityApi")
                        newValue = "5.0-preview.1";
                    else if (propertyInfo.Name == "ServiceEndpointApi")
                        newValue = "5.0-preview.1";
                    else if (propertyInfo.Name == "TaskAgentApi")
                        newValue = "5.0-preview.1";
                    // Set default api version to use.
                    else
                        newValue = defaultApiVersion;

                    // Change value.
                    propertyInfo.SetValue(this, newValue, null);
                }
                // Nothing to change.
                // else
                // { }
            }
        }

        #endregion
    }
}
