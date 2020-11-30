using System.Runtime.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADO.RestAPI.Viewmodel50
{
    public class TaskAgentResponse
    {
        // This is just a container class for all REST API responses related to task groups, variable groups.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/distributedtask/taskgroups?view=azure-devops-rest-5.0
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/distributedtask/variablegroups?view=azure-devops-rest-5.0
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/distributedtask/queues/get%20agent%20queues?view=azure-devops-rest-5.1#taskagentpoolreference
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/distributedtask/queues/get?view=azure-devops-rest-5.1

        #region - Nested Classes and Enumerations.

        [JsonConverter(typeof(StringEnumConverter))]
        public enum TaskAgentPoolType
        {
            [EnumMember(Value = "automation")]
            Automation,
            [EnumMember(Value = "deployment")]
            Deployment
        }

        public class TaskAgentPoolReference
        {
            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "isHosted")]
            public bool IsHosted { get; set; }

            [JsonProperty(PropertyName = "isLegacy")]
            public bool IsLegacy { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "poolType")]
            public TaskAgentPoolType PoolType { get; set; }

            [JsonProperty(PropertyName = "scope")]
            public string Scope { get; set; }

            [JsonProperty(PropertyName = "size")]
            public int Size { get; set; }
        }

        public class TaskAgentQueues
        {
            // This data class makes use of definitions from these links:
            // https://docs.microsoft.com/en-us/rest/api/azure/devops/distributedtask/queues/get%20agent%20queues?view=azure-devops-rest-5.1#taskagentpoolreference
            // https://docs.microsoft.com/en-us/rest/api/azure/devops/distributedtask/queues/get?view=azure-devops-rest-5.1

            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<TaskAgentQueue> Value { get; set; }
        }

        public class TaskAgentQueue
        {
            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "pool")]
            public TaskAgentPoolReference Pool { get; set; }

            [JsonProperty(PropertyName = "projectId")]
            public string ProjectId { get; set; }
        }

        public class TaskDefinitionReference
        {
            [JsonProperty(PropertyName = "definitionType")]
            public string DefinitionType { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "versionSpec")]
            public string VersionSpec { get; set; }
        }

        public class TaskExecution
        {
            [JsonProperty(PropertyName = "execTask")]
            public TaskReference ExecTask { get; set; }

            [JsonProperty(PropertyName = "platformInstrunctions")]
            public object PlatformInstrunctions { get; set; }
        }

        public class TaskGroupDefinition
        {
            [JsonProperty(PropertyName = "displayName")]
            public string DisplayName { get; set; }

            [JsonProperty(PropertyName = "isExpanded")]
            public bool IsExpanded { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "tags")]
            public IEnumerable<string> Tags { get; set; }

            [JsonProperty(PropertyName = "visibleRule")]
            public string VisibleRule { get; set; }
        }

        public class TaskGroups
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<TaskGroup> Value { get; set; }
        }

        public class TaskGroup
        {
            [JsonProperty(PropertyName = "agentExecution")]
            public TaskExecution AgentExecution { get; set; }

            [JsonProperty(PropertyName = "author")]
            public string Author { get; set; }

            [JsonProperty(PropertyName = "category")]
            public string Category { get; set; }

            [JsonProperty(PropertyName = "comment")]
            public string Comment { get; set; }

            [JsonProperty(PropertyName = "contentsUploaded")]
            public bool ContentsUploaded { get; set; }

            [JsonProperty(PropertyName = "contributionIdentifier")]
            public string ContributionIdentifier { get; set; }

            [JsonProperty(PropertyName = "contributionVersion")]
            public string ContributionVersion { get; set; }

            [JsonProperty(PropertyName = "createdBy")]
            public IdentityReference CreatedBy { get; set; }

            [JsonProperty(PropertyName = "createdOn")]
            public string CreatedOn { get; set; }

            [JsonProperty(PropertyName = "dataSourceBindings")]
            public IEnumerable<DataSourceBinding> DataSourceBindings { get; set; }

            [JsonProperty(PropertyName = "definitionType")]
            public string DefinitionType { get; set; }

            [JsonProperty(PropertyName = "deleted")]
            public bool Deleted { get; set; }

            [JsonProperty(PropertyName = "demands")]
            public IEnumerable<Demand> Demands { get; set; }

            [JsonProperty(PropertyName = "deprecated")]
            public bool Deprecated { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "disabled")]
            public bool Disabled { get; set; }

            [JsonProperty(PropertyName = "execution")]
            public Dictionary<string, AbstractJToken> Execution { get; set; }

            [JsonProperty(PropertyName = "friendlyName")]
            public string FriendlyName { get; set; }

            [JsonProperty(PropertyName = "groups")]
            public IEnumerable<TaskGroupDefinition> Groups { get; set; }

            [JsonProperty(PropertyName = "helpMarkDown")]
            public string HelpMarkDown { get; set; }

            [JsonProperty(PropertyName = "helpUrl")]
            public string HelpUrl { get; set; }

            [JsonProperty(PropertyName = "hostType")]
            public string HostType { get; set; }

            [JsonProperty(PropertyName = "iconUrl")]
            public string IconUrl { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "inputs")]
            public IEnumerable<TaskInputDefinition> Inputs { get; set; }

            [JsonProperty(PropertyName = "instanceNameFormat")]
            public string InstanceNameFormat { get; set; }

            [JsonProperty(PropertyName = "minimumAgentVersion")]
            public string MinimumAgentVersion { get; set; }

            [JsonProperty(PropertyName = "modifiedBy")]
            public IdentityReference ModifiedBy { get; set; }

            [JsonProperty(PropertyName = "modifiedOn")]
            public string ModifiedOn { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "owner")]
            public string Owner { get; set; }

            [JsonProperty(PropertyName = "packageLocation")]
            public string PackageLocation { get; set; }

            [JsonProperty(PropertyName = "packageType")]
            public string PackageType { get; set; }

            [JsonProperty(PropertyName = "parentDefinitionId")]
            public string ParentDefinitionId { get; set; }

            [JsonProperty(PropertyName = "postJobExecution")]
            public Dictionary<string, AbstractJToken> PostJobExecution { get; set; }

            [JsonProperty(PropertyName = "preJobExecution")]
            public Dictionary<string, AbstractJToken> PreJobExecution { get; set; }

            [JsonProperty(PropertyName = "preview")]
            public bool Preview { get; set; }

            [JsonProperty(PropertyName = "releaseNotes")]
            public string ReleaseNotes { get; set; }

            [JsonProperty(PropertyName = "revision")]
            public int Revision { get; set; }

            [JsonProperty(PropertyName = "runsOn")]
            public IEnumerable<string> RunsOn { get; set; }

            [JsonProperty(PropertyName = "satisfies")]
            public IEnumerable<string> Satisfies { get; set; }

            [JsonProperty(PropertyName = "serverOwned")]
            public bool ServerOwned { get; set; }

            [JsonProperty(PropertyName = "showEnvironmentVariables")]
            public bool ShowEnvironmentVariables { get; set; }

            [JsonProperty(PropertyName = "sourceDefinitions")]
            public IEnumerable<TaskSourceDefinition> SourceDefinitions { get; set; }

            [JsonProperty(PropertyName = "sourceLocation")]
            public string SourceLocation { get; set; }

            [JsonProperty(PropertyName = "tasks")]
            public IEnumerable<TaskGroupStep> Tasks { get; set; }

            [JsonProperty(PropertyName = "version")]
            public TaskVersion Version { get; set; }

            [JsonProperty(PropertyName = "visibility")]
            public IEnumerable<string> Visibility { get; set; }
        }

        public class TaskGroupStep
        {
            [JsonProperty(PropertyName = "alwaysRun")]
            public bool AlwaysRun { get; set; }

            [JsonProperty(PropertyName = "condition")]
            public string Condition { get; set; }

            [JsonProperty(PropertyName = "continueOnError")]
            public bool ContinueOnError { get; set; }

            [JsonProperty(PropertyName = "displayName")]
            public string DisplayName { get; set; }

            [JsonProperty(PropertyName = "enabled")]
            public bool Enabled { get; set; }

            [JsonProperty(PropertyName = "inputs")]
            public object Inputs { get; set; }

            [JsonProperty(PropertyName = "task")]
            public TaskDefinitionReference Task { get; set; }

            [JsonProperty(PropertyName = "timeoutInMinutes")]
            public int TimeoutInMinutes { get; set; }
        }

        public class TaskOutputVariable
        {
            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        public class TaskReference
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "inputs")]
            public object Inputs { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "version")]
            public string Version { get; set; }
        }

        public class TaskSourceDefinition
        {
            [JsonProperty(PropertyName = "authKey")]
            public string AuthKey { get; set; }

            [JsonProperty(PropertyName = "endpoint")]
            public string Endpoint { get; set; }

            [JsonProperty(PropertyName = "keySelector")]
            public string KeySelector { get; set; }

            [JsonProperty(PropertyName = "selector")]
            public string Selector { get; set; }

            [JsonProperty(PropertyName = "target")]
            public string Target { get; set; }
        }

        public class TaskVersion
        {
            [JsonProperty(PropertyName = "isTest")]
            public bool IsTest { get; set; }

            [JsonProperty(PropertyName = "major")]
            public int Major { get; set; }

            [JsonProperty(PropertyName = "minor")]
            public int Minor { get; set; }

            [JsonProperty(PropertyName = "patch")]
            public int Patch { get; set; }
        }

        public class VariableGroups
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<VariableGroup> Value { get; set; }
        }

        public class VariableGroup
        {
            [JsonProperty(PropertyName = "createdBy")]
            public IdentityReference CreatedBy { get; set; }

            [JsonProperty(PropertyName = "createdOn")]
            public string CreatedOn { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "isShared")]
            public string IsShared { get; set; }

            [JsonProperty(PropertyName = "modifiedBy")]
            public IdentityReference ModifiedBy { get; set; }

            [JsonProperty(PropertyName = "modifiedOn")]
            public string ModifiedOn { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "providerData")]
            public object ProviderData { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "variables")]
            public Dictionary<string, VariableValue> Variables { get; set; }
        }

        public class VariableValue
        {
            [JsonProperty(PropertyName = "isSecret")]
            public string IsSecret { get; set; }

            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }
        }

        #endregion
    }
}
