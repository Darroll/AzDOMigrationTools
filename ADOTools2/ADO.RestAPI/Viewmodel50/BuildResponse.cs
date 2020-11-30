using System.Runtime.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADO.RestAPI.Viewmodel50
{
    public class BuildResponse
    {
        // This is just a container class for all REST API responses related to build definitions.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/build/definitions/get?view=azure-devops-rest-5.0
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/build/definitions/get%20definition%20revisions?view=azure-devops-rest-5.0
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/build/definitions/list?view=azure-devops-rest-5.0

        #region - Nested Classes and Enumerations.

        [JsonConverter(typeof(StringEnumConverter))]
        public enum AuditAction
        {
            [EnumMember(Value = "add")]
            Add,
            [EnumMember(Value = "delete")]
            Delete,
            [EnumMember(Value = "update")]
            Update
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BuildAuthorizationScope
        {
            [EnumMember(Value = "project")]
            Project,
            [EnumMember(Value = "projectCollection")]
            ProjectCollection
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BuildReason
        {
            [EnumMember(Value = "all")]
            All,
            [EnumMember(Value = "batchedCI")]
            BatchedCI,
            [EnumMember(Value = "buildCompletion")]
            BuildCompletion,
            [EnumMember(Value = "checkInShelveset")]
            CheckInShelveset,
            [EnumMember(Value = "individualCI")]
            IndividualCI,
            [EnumMember(Value = "manual")]
            Manual,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "pullRequest")]
            PullRequest,
            [EnumMember(Value = "schedule")]
            Schedule,
            [EnumMember(Value = "triggered")]
            Triggered,
            [EnumMember(Value = "userCreated")]
            UserCreated,
            [EnumMember(Value = "validateShelveset")]
            ValidateShelveset
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BuildResult
        {
            [EnumMember(Value = "canceled")]
            Canceled,
            [EnumMember(Value = "failed")]
            Failed,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "partiallySucceeded")]
            PartiallySucceeded,
            [EnumMember(Value = "succeeded")]
            Succeeded
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BuildStatus
        {
            [EnumMember(Value = "all")]
            All,
            [EnumMember(Value = "cancelling")]
            Cancelling,
            [EnumMember(Value = "completed")]
            Completed,
            [EnumMember(Value = "inProgress")]
            InProgress,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "notStarted")]
            NotStarted,
            [EnumMember(Value = "postPoned")]
            PostPoned
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ControllerStatus
        {
            [EnumMember(Value = "available")]
            Available,
            [EnumMember(Value = "offline")]
            Offline,
            [EnumMember(Value = "unavailable")]
            Unavailable
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DefinitionQuality
        {
            [EnumMember(Value = "definition")]
            Definition,
            [EnumMember(Value = "draft")]
            Draft
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DefinitionQueueStatus
        {
            [EnumMember(Value = "disabled")]
            Disabled,
            [EnumMember(Value = "enabled")]
            Enabled,
            [EnumMember(Value = "paused")]
            Paused
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DefinitionTriggerType
        {
            [EnumMember(Value = "all")]
            All,
            [EnumMember(Value = "batchedContinuousIntegration")]
            BatchedContinuousIntegration,
            [EnumMember(Value = "batchedGatedCheckIn")]
            BatchedGatedCheckIn,
            [EnumMember(Value = "buildCompletion")]
            BuildCompletion,
            [EnumMember(Value = "continuousIntegration")]
            ContinuousIntegration,
            [EnumMember(Value = "gatedCheckIn")]
            GatedCheckIn,
            [EnumMember(Value = "manual")]
            Manual,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "pullRequest")]
            PullRequest,
            [EnumMember(Value = "schedule")]
            Schedule
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DefinitionType
        {
            [EnumMember(Value = "build")]
            Build,
            [EnumMember(Value = "xaml")]
            Xaml
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum QueueOptions
        {
            [EnumMember(Value = "doNotRun")]
            DoNotRun,
            [EnumMember(Value = "none")]
            None
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum QueuePriority
        {
            [EnumMember(Value = "aboveNormal")]
            AboveNormal,
            [EnumMember(Value = "belowNormal")]
            BelowNormal,
            [EnumMember(Value = "high")]
            High,
            [EnumMember(Value = "low")]
            Low,
            [EnumMember(Value = "normal")]
            Normal
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ValidationResult
        {
            [EnumMember(Value = "error")]
            Error,
            [EnumMember(Value = "ok")]
            Ok,            
            [EnumMember(Value = "warning")]
            Warning
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
            [JsonProperty(PropertyName = "_links")]
            public BuildDefinitionReferenceLink Links { get; set; }

            [JsonProperty(PropertyName = "authoredBy")]
            public IdentityReference AuthoredBy { get; set; }

            [JsonProperty(PropertyName = "createdDate")]
            public string CreatedDate { get; set; }

            [JsonProperty(PropertyName = "draftOf")]
            public DefinitionReference DraftOf { get; set; }

            [JsonProperty(PropertyName = "drafts")]
            public DefinitionReference Drafts { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "latestBuild")]
            public Build LatestBuild { get; set; }

            [JsonProperty(PropertyName = "latestCompletedBuild")]
            public Build LatestCompletedBuild { get; set; }

            [JsonProperty(PropertyName = "metrics")]
            public IEnumerable<BuildMetric> Metrics { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "path")]
            public string Path { get; set; }

            [JsonProperty(PropertyName = "project")]
            public TeamProjectReference Project { get; set; }

            [JsonProperty(PropertyName = "quality")]
            public DefinitionQuality Quality { get; set; }

            [JsonProperty(PropertyName = "queue")]
            public AgentPoolQueue Queue { get; set; }

            [JsonProperty(PropertyName = "queueStatus")]
            public DefinitionQueueStatus QueueStatus { get; set; }

            [JsonProperty(PropertyName = "revision")]
            public int Revision { get; set; }

            [JsonProperty(PropertyName = "type")]
            public DefinitionType Type { get; set; }

            [JsonProperty(PropertyName = "uri")]
            public string Uri { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }        

        public class AgentPoolQueue
        {
            [JsonProperty(PropertyName = "_links")]
            public SelfReferenceLink Links { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "pool")]
            public TaskAgentPoolReference Pool { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class Build
        {
            [JsonProperty(PropertyName = "_links")]
            public BuildDefinitionReferenceLink Links { get; set; }

            [JsonProperty(PropertyName = "buildNumber")]
            public string BuildNumber { get; set; }

            [JsonProperty(PropertyName = "buildNumberRevision")]
            public int BuildNumberRevision { get; set; }

            [JsonProperty(PropertyName = "controller")]
            public BuildController Controller { get; set; }

            [JsonProperty(PropertyName = "definition")]
            public DefinitionReference Definition { get; set; }

            [JsonProperty(PropertyName = "deleted")]
            public bool Deleted { get; set; }

            [JsonProperty(PropertyName = "deletedBy")]
            public IdentityReference DeletedBy { get; set; }

            [JsonProperty(PropertyName = "deletedDate")]
            public string DeletedDate { get; set; }

            [JsonProperty(PropertyName = "deletedReason")]
            public string DeletedReason { get; set; }

            [JsonProperty(PropertyName = "demands")]
            public IEnumerable<Demand> Demands { get; set; }

            [JsonProperty(PropertyName = "finishTime")]
            public string FinishTime { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "keepForever")]
            public bool KeepForever { get; set; }

            [JsonProperty(PropertyName = "lastChangedBy")]
            public IdentityReference LastChangedBy { get; set; }

            [JsonProperty(PropertyName = "lastChangedDate")]
            public string LastChangedDate { get; set; }

            [JsonProperty(PropertyName = "logs")]
            public BuildLogReference Logs { get; set; }

            [JsonProperty(PropertyName = "plans")]
            public TaskOrchestrationPlanReference Plans { get; set; }

            [JsonProperty(PropertyName = "priority")]
            public QueuePriority Priority { get; set; }

            [JsonProperty(PropertyName = "project")]
            public TeamProjectReference Project { get; set; }

            [JsonProperty(PropertyName = "properties")]
            public IDictionary<string, object> Properties { get; set; }

            [JsonProperty(PropertyName = "quality")]
            public string Quality { get; set; }

            [JsonProperty(PropertyName = "queue")]
            public AgentPoolQueue Queue { get; set; }

            [JsonProperty(PropertyName = "queueOptions")]
            public QueueOptions QueueOptions { get; set; }

            [JsonProperty(PropertyName = "queuePosition")]
            public int QueuePosition { get; set; }

            [JsonProperty(PropertyName = "queueTime")]
            public string QueueTime { get; set; }

            [JsonProperty(PropertyName = "reason")]
            public BuildReason Reason { get; set; }

            [JsonProperty(PropertyName = "repository")]
            public BuildRepository Repository { get; set; }

            [JsonProperty(PropertyName = "requestedBy")]
            public IdentityReference RequestedBy { get; set; }

            [JsonProperty(PropertyName = "requestedFor")]
            public IdentityReference RequestedFor { get; set; }

            [JsonProperty(PropertyName = "result")]
            public BuildResult Result { get; set; }

            [JsonProperty(PropertyName = "retainedByRelease")]
            public bool RetainedByRelease { get; set; }

            [JsonProperty(PropertyName = "sourceBranch")]
            public string SourceBranch { get; set; }

            [JsonProperty(PropertyName = "sourceVersion")]
            public string SourceVersion { get; set; }

            [JsonProperty(PropertyName = "startTime")]
            public string StartTime { get; set; }

            [JsonProperty(PropertyName = "status")]
            public BuildStatus Status { get; set; }

            [JsonProperty(PropertyName = "tags")]
            public IEnumerable<string> Tags { get; set; }

            [JsonProperty(PropertyName = "triggeredInfo")]
            public object TriggeredInfo { get; set; }

            [JsonProperty(PropertyName = "uri")]
            public string Uri { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "validationResults")]
            public IEnumerable<BuildRequestValidationResult> ValidationResults { get; set; }
        }

        public class BuildController
        {
            [JsonProperty(PropertyName = "createdDate")]
            public string CreatedDate { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "enabled")]
            public bool Enabled { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "status")]
            public ControllerStatus Status { get; set; }

            [JsonProperty(PropertyName = "updatedDate")]
            public string UpdatedDate { get; set; }

            [JsonProperty(PropertyName = "uri")]
            public string Uri { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class BuildDefinitionRevisions
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<BuildDefinitionRevision> Value { get; set; }
        }        

        public class BuildDefinitionRevision
        {
            [JsonProperty(PropertyName = "changeType")]
            public AuditAction ChangeType { get; set; }

            [JsonProperty(PropertyName = "changedBy")]
            public IdentityReference ChangedBy { get; set; }

            [JsonProperty(PropertyName = "changedDate")]
            public string ChangedDate { get; set; }

            [JsonProperty(PropertyName = "comment")]
            public string Comment { get; set; }

            [JsonProperty(PropertyName = "definitionUrl")]
            public string DefinitionUrl { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "revision")]
            public int Revision { get; set; }
        }

        public class BuildDefinition
        {
            [JsonProperty(PropertyName = "_links")]
            public BuildDefinitionReferenceLink Links { get; set; }

            [JsonProperty(PropertyName = "authoredBy")]
            public IdentityReference AuthoredBy { get; set; }

            [JsonProperty(PropertyName = "badgeEnabled")]
            public bool BadgeEnabled { get; set; }

            [JsonProperty(PropertyName = "buildNumberFormat")]
            public string BuildNumberFormat { get; set; }

            [JsonProperty(PropertyName = "createdDate")]
            public string CreatedDate { get; set; }

            [JsonProperty(PropertyName = "demands")]
            public IEnumerable<string> Demands { get; set; }

            [JsonProperty(PropertyName = "draftOf")]
            public DefinitionReference DraftOf { get; set; }

            [JsonProperty(PropertyName = "drafts")]
            public IEnumerable<DefinitionReference> Drafts { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "jobAuthorizationScope")]
            public BuildAuthorizationScope JobAuthorizationScope { get; set; }

            [JsonProperty(PropertyName = "jobCancelTimeoutInMinutes")]
            public int JobCancelTimeoutInMinutes { get; set; }

            [JsonProperty(PropertyName = "jobTimeoutInMinutes")]
            public int JobTimeoutInMinutes { get; set; }

            [JsonProperty(PropertyName = "latestBuild")]
            public Build LatestBuild { get; set; }

            [JsonProperty(PropertyName = "latestCompletedBuild")]
            public Build LatestCompletedBuild { get; set; }

            [JsonProperty(PropertyName = "metrics")]
            public IEnumerable<BuildMetric> Metrics { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "options")]
            public IEnumerable<BuildOption> Options { get; set; }

            [JsonProperty(PropertyName = "path")]
            public string Path { get; set; }

            [JsonProperty(PropertyName = "process")]
            public BuildProcess Process { get; set; }

            [JsonProperty(PropertyName = "processParameters")]
            public ProcessParameters ProcessParameters { get; set; }

            [JsonProperty(PropertyName = "project")]
            public TeamProjectReference Project { get; set; }

            [JsonProperty(PropertyName = "properties")]
            public IDictionary<string, object> Properties { get; set; }

            [JsonProperty(PropertyName = "quality")]
            public DefinitionQuality Quality { get; set; }

            [JsonProperty(PropertyName = "queue")]
            public AgentPoolQueue Queue { get; set; }

            [JsonProperty(PropertyName = "queueStatus")]
            public DefinitionQueueStatus QueueStatus { get; set; }

            [JsonProperty(PropertyName = "repository")]
            public BuildRepository Repository { get; set; }

            [JsonProperty(PropertyName = "retentionRules")]
            public IEnumerable<RetentionPolicy> RetentionRules { get; set; }

            [JsonProperty(PropertyName = "revision")]
            public int Revision { get; set; }

            [JsonProperty(PropertyName = "tags")]
            public IEnumerable<string> Tags { get; set; }

            [JsonProperty(PropertyName = "triggers")]
            public IEnumerable<BuildTrigger> Triggers { get; set; }

            [JsonProperty(PropertyName = "type")]
            public DefinitionType Type { get; set; }

            [JsonProperty(PropertyName = "uri")]
            public string Uri { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "variableGroups")]
            public IEnumerable<VariableGroup> VariableGroups { get; set; }

            [JsonProperty(PropertyName = "variables")]
            public Dictionary<string, BuildDefinitionVariable> Variables { get; set; }
        }

        public class BuildDefinitionVariable
        {
            [JsonProperty(PropertyName = "allowOverride")]
            public bool AllowOverride { get; set; }

            [JsonProperty(PropertyName = "isSecret")]
            public bool IsSecret { get; set; }

            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }
        }

        public class BuildLogReference
        {
            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class BuildMetric
        {
            [JsonProperty(PropertyName = "date")]
            public string Date { get; set; }

            [JsonProperty(PropertyName = "intValue")]
            public int IntValue { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "scope")]
            public string Scope { get; set; }
        }

        public class BuildOption
        {
            [JsonProperty(PropertyName = "definition")]
            public BuildOptionDefinitionReference Definition { get; set; }

            [JsonProperty(PropertyName = "enabled")]
            public bool Enabled { get; set; }

            [JsonProperty(PropertyName = "inputs")]
            public object Inputs { get; set; }
        }

        public class BuildOptionDefinitionReference
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
        }

        public class BuildProcess
        {
            [JsonProperty(PropertyName = "type")]
            public int Type { get; set; }
        }

        public class BuildRepository
        {
            [JsonProperty(PropertyName = "checkoutSubmodules")]
            public bool CheckoutSubmodules { get; set; }

            [JsonProperty(PropertyName = "clean")]
            public string Clean { get; set; }

            [JsonProperty(PropertyName = "defaultBranch")]
            public string DefaultBranch { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "properties")]
            public IDictionary<string, object> Properties { get; set; }

            [JsonProperty(PropertyName = "rootFolder")]
            public string RootFolder { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class BuildRequestValidationResult
        {
            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }

            [JsonProperty(PropertyName = "result")]
            public ValidationResult Result { get; set; }
        }

        public class BuildTrigger
        {
            [JsonProperty(PropertyName = "triggerType")]
            public DefinitionTriggerType TriggerType { get; set; }
        }

        public class DefinitionReference
        {
            [JsonProperty(PropertyName = "createdDate")]
            public string CreatedDate { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "path")]
            public string Path { get; set; }

            [JsonProperty(PropertyName = "project")]
            public TeamProjectReference Project { get; set; }

            [JsonProperty(PropertyName = "queueStatus")]
            public DefinitionQueueStatus QueueStatus { get; set; }

            [JsonProperty(PropertyName = "revision")]
            public int Revision { get; set; }

            [JsonProperty(PropertyName = "type")]
            public DefinitionType Type { get; set; }

            [JsonProperty(PropertyName = "uri")]
            public string Uri { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class RetentionPolicy
        {
            [JsonProperty(PropertyName = "artifactTypesToDelete")]
            public IEnumerable<string> ArtifactTypesToDelete { get; set; }

            [JsonProperty(PropertyName = "artifacts")]
            public IEnumerable<string> Artifacts { get; set; }

            [JsonProperty(PropertyName = "branches")]
            public IEnumerable<string> Branches { get; set; }

            [JsonProperty(PropertyName = "daysToKeep")]
            public int DaysToKeep { get; set; }

            [JsonProperty(PropertyName = "deleteBuildRecord")]
            public bool DeleteBuildRecord { get; set; }

            [JsonProperty(PropertyName = "deleteTestResults")]
            public bool DeleteTestResults { get; set; }

            [JsonProperty(PropertyName = "minimumToKeep")]
            public int MinimumToKeep { get; set; }
        }

        public class TaskAgentPoolReference
        {
            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "isHosted")]
            public bool IsHosted { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }
        
        public class TaskOrchestrationPlanReference
        {
            [JsonProperty(PropertyName = "orchestrationType")]
            public int OrchestrationType { get; set; }

            [JsonProperty(PropertyName = "planId")]
            public string PlanId { get; set; }
        }

        public class VariableGroup
        {
            [JsonProperty(PropertyName = "alias")]
            public string Alias { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "variables")]
            public Dictionary<string, BuildDefinitionVariable> Variables { get; set; }
        }

        #endregion
    }
}
