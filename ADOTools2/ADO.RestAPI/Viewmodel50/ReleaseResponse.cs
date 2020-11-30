using System.Runtime.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADO.RestAPI.Viewmodel50
{
    public class ReleaseResponse
    {
        // This is just a container class for all REST API responses related to release definitions.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/release/definitions?view=azure-devops-rest-5.1

        #region - Nested Classes and Enumerations.

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ApprovalExecutionOrder
        {
            [EnumMember(Value = "afterGatesAlways")]
            AfterGatesAlways,
            [EnumMember(Value = "afterSuccessfulGates")]
            AfterSuccessfulGates,
            [EnumMember(Value = "beforeGates")]
            BeforeGates
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ConditionType
        {
            [EnumMember(Value = "artifact")]
            Artifact,
            [EnumMember(Value = "environmentState")]
            EnvironmentState,
            [EnumMember(Value = "event")]
            Event,
            [EnumMember(Value = "undefined")]
            Undefined
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DeployPhaseTypes
        {
            [EnumMember(Value = "agentBasedDeployment")]
            AgentBasedDeployment,
            [EnumMember(Value = "deploymentGates")]
            DeploymentGates,
            [EnumMember(Value = "machineGroupBasedDeployment")]
            MachineGroupBasedDeployment,
            [EnumMember(Value = "runOnServer")]
            RunOnServer,
            [EnumMember(Value = "undefined")]
            Undefined
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum EnvironmentTriggerType
        {
            [EnumMember(Value = "deploymentGroupRedeploy")]
            DeploymentGroupRedeploy,
            [EnumMember(Value = "rollbackRedeploy")]
            RollbackRedeploy,
            [EnumMember(Value = "undefined")]
            Undefined
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ReleaseReason
        {
            [EnumMember(Value = "continuousIntegration")]
            ContinuousIntegration,
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
        public enum ReleaseTriggerType
        {
            [EnumMember(Value = "artifactSource")]
            ArtifactSource,
            [EnumMember(Value = "containerImage")]
            ContainerImage,
            [EnumMember(Value = "package")]
            Package,
            [EnumMember(Value = "pullRequest")]
            PullRequest,
            [EnumMember(Value = "schedule")]
            Schedule,
            [EnumMember(Value = "sourceRepo")]
            SourceRepo,
            [EnumMember(Value = "undefined")]
            Undefined
        }

        public enum ScheduleDays
        {
            All = 127,
            Friday = 16,
            Monday = 1,
            None = 0,
            Saturday = 32,
            Sunday = 64,
            Thursday = 8,
            Tuesday = 2,
            Wednesday = 4
        }

        public class ApprovalOptions
        {
            [JsonProperty(PropertyName = "autoTriggeredAndPreviousEnvironmentApprovedCanBeSkipped")]
            public bool AutoTriggeredAndPreviousEnvironmentApprovedCanBeSkipped { get; set; }

            [JsonProperty(PropertyName = "enforceIdentityRevalidation")]
            public bool EnforceIdentityRevalidation { get; set; }

            [JsonProperty(PropertyName = "executionOrder")]
            public ApprovalExecutionOrder ExecutionOrder { get; set; }

            [JsonProperty(PropertyName = "releaseCreatorCanBeApprover")]
            public bool ReleaseCreatorCanBeApprover { get; set; }

            // It seams this property is nullable from the api.
            [JsonProperty(PropertyName = "requiredApproverCount")]
            public int? RequiredApproverCount { get; set; }

            [JsonProperty(PropertyName = "timeoutInMinutes")]
            public int TimeoutInMinutes { get; set; }
        }

        public class Artifact
        {
            [JsonProperty(PropertyName = "sourceId")]
            public string SourceId { get; set; }

            [JsonProperty(PropertyName = "alias")]
            public string Alias { get; set; }

            [JsonProperty(PropertyName = "definitionReference")]
            public Dictionary<string, ArtifactSourceReference> DefinitionReference { get; set; }

            [JsonProperty(PropertyName = "isPrimary")]
            public bool IsPrimary { get; set; }

            [JsonProperty(PropertyName = "isRetained")]
            public bool IsRetained { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
        }

        public class ArtifactSourceReference
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        public class Condition
        {
            [JsonProperty(PropertyName = "conditionType")]
            public ConditionType ConditionType { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }
        }

        public class ConfigurationVariableValue
        {
            [JsonProperty(PropertyName = "allowOverride")]
            public bool AllowOverride { get; set; }

            [JsonProperty(PropertyName = "isSecret")]
            public bool IsSecret { get; set; }

            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }
        }

        public class DeployPhase
        {
            // todo: should it be typed instead?
            [JsonProperty(PropertyName = "deploymentInput")]
            public object DeploymentInput { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "phaseType")]
            public DeployPhaseTypes PhaseType { get; set; }

            [JsonProperty(PropertyName = "rank")]
            public int Rank { get; set; }

            [JsonProperty(PropertyName = "refName")]
            public string RefName { get; set; }

            [JsonProperty(PropertyName = "workflowTasks")]
            public IEnumerable<WorkflowTask> WorkflowTasks { get; set; }
        }

        public class EnvironmentExecutionPolicy
        {
            [JsonProperty(PropertyName = "concurrencyCount")]
            public int ConcurrencyCount { get; set; }

            [JsonProperty(PropertyName = "queueDepthCount")]
            public int QueueDepthCount { get; set; }
        }

        public class EnvironmentOptions
        {
            [JsonProperty(PropertyName = "autoLinkWorkItems")]
            public bool AutoLinkWorkItems { get; set; }

            [JsonProperty(PropertyName = "badgeEnabled")]
            public bool BadgeEnabled { get; set; }

            [JsonProperty(PropertyName = "emailNotificationType")]
            public string EmailNotificationType { get; set; }

            [JsonProperty(PropertyName = "emailRecipients")]
            public string EmailRecipients { get; set; }

            [JsonProperty(PropertyName = "enableAccessToken")]
            public bool EnableAccessToken { get; set; }

            [JsonProperty(PropertyName = "publishDeploymentStatus")]
            public bool PublishDeploymentStatus { get; set; }

            [JsonProperty(PropertyName = "pullRequestDeploymentEnabled")]
            public bool PullRequestDeploymentEnabled { get; set; }

            [JsonProperty(PropertyName = "skipArtifactsDownload")]
            public bool SkipArtifactsDownload { get; set; }

            [JsonProperty(PropertyName = "timeoutInMinutes")]
            public int TimeoutInMinutes { get; set; }
        }

        public class EnvironmentRetentionPolicy
        {
            [JsonProperty(PropertyName = "daysToKeep")]
            public int DaysToKeep { get; set; }

            [JsonProperty(PropertyName = "releasesToKeep")]
            public int ReleasesToKeep { get; set; }

            [JsonProperty(PropertyName = "retainBuild")]
            public bool RetainBuild { get; set; }
        }

        public class EnvironmentTrigger
        {
            [JsonProperty(PropertyName = "definitionEnvironmentId")]
            public int DefinitionEnvironmentId { get; set; }

            [JsonProperty(PropertyName = "releaseDefinitionId")]
            public int ReleaseDefinitionId { get; set; }

            [JsonProperty(PropertyName = "triggerContent")]
            public string TriggerContent { get; set; }

            [JsonProperty(PropertyName = "triggerType")]
            public EnvironmentTriggerType TriggerType { get; set; }
        }

        public class ProjectReference
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        public class ReleaseDefinitionApprovals
        {
            [JsonProperty(PropertyName = "approvalOptions")]
            public ApprovalOptions ApprovalOptions { get; set; }

            [JsonProperty(PropertyName = "approvals")]
            public IEnumerable<ReleaseDefinitionApprovalStep> Approvals { get; set; }
        }

        public class ReleaseDefinitionApprovalStep
        {
            [JsonProperty(PropertyName = "approver")]
            public IdentityReference Approver { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "isAutomated")]
            public bool IsAutomated { get; set; }

            [JsonProperty(PropertyName = "isNotificationsOn")]
            public bool IsNotificationsOn { get; set; }

            [JsonProperty(PropertyName = "rank")]
            public int Rank { get; set; }
        }

        public class ReleaseDefinitionDeployStep
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
        }

        public class ReleaseDefinitionEnvironment
        {
            [JsonProperty(PropertyName = "badgeUrl")]
            public string BadgeUrl { get; set; }

            [JsonProperty(PropertyName = "conditions")]
            public IEnumerable<Condition> Conditions { get; set; }

            [JsonProperty(PropertyName = "currentRelease")]
            public ReleaseShallowReference CurrentRelease { get; set; }

            [JsonProperty(PropertyName = "demands")]
            public IEnumerable<Demand> Demands { get; set; }

            [JsonProperty(PropertyName = "deployPhases")]
            public IEnumerable<DeployPhase> DeployPhases { get; set; }

            [JsonProperty(PropertyName = "deployStep")]
            public ReleaseDefinitionDeployStep DeployStep { get; set; }

            [JsonProperty(PropertyName = "environmentOptions")]
            public EnvironmentOptions EnvironmentOptions { get; set; }

            [JsonProperty(PropertyName = "environmentTriggers")]
            public IEnumerable<EnvironmentTrigger> EnvironmentTriggers { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "owner")]
            public IdentityReference Owner { get; set; }

            [JsonProperty(PropertyName = "postDeployApprovals")]
            public ReleaseDefinitionApprovals PostDeployApprovals { get; set; }

            [JsonProperty(PropertyName = "postDeploymentGates")]
            public ReleaseDefinitionGateStep PostDeploymentGates { get; set; }

            [JsonProperty(PropertyName = "preDeployApprovals")]
            public ReleaseDefinitionApprovals PreDeployApprovals { get; set; }

            [JsonProperty(PropertyName = "preDeploymentGates")]
            public ReleaseDefinitionGateStep PreDeploymentGates { get; set; }

            [JsonProperty(PropertyName = "processParameters")]
            public ProcessParameters ProcessParameters { get; set; }

            [JsonProperty(PropertyName = "properties")]
            public IDictionary<string, object> Properties { get; set; }

            [JsonProperty(PropertyName = "rank")]
            public int Rank { get; set; }

            [JsonProperty(PropertyName = "retentionPolicy")]
            public EnvironmentRetentionPolicy RetentionPolicy { get; set; }

            [JsonProperty(PropertyName = "schedules")]
            public IEnumerable<ReleaseSchedule> Schedules { get; set; }

            [JsonProperty(PropertyName = "variableGroups")]
            public IEnumerable<int> VariableGroups { get; set; }

            [JsonProperty(PropertyName = "variables")]
            public Dictionary<string, ConfigurationVariableValue> Variables { get; set; }
        }

        public class ReleaseDefinitionGate
        {
            [JsonProperty(PropertyName = "tasks")]
            public IEnumerable<WorkflowTask> Tasks { get; set; }
        }

        public class ReleaseDefinitionGateOptions
        {
            [JsonProperty(PropertyName = "isEnabled")]
            public bool IsEnabled { get; set; }

            [JsonProperty(PropertyName = "minimumSuccessDuration")]
            public int MinimumSuccessDuration { get; set; }

            [JsonProperty(PropertyName = "samplingInterval")]
            public int SamplingInterval { get; set; }

            [JsonProperty(PropertyName = "stabilizationTime")]
            public int StabilizationTime { get; set; }

            [JsonProperty(PropertyName = "timeout")]
            public int Timeout { get; set; }
        }

        public class ReleaseDefinitionGateStep
        {
            [JsonProperty(PropertyName = "gates")]
            public IEnumerable<ReleaseDefinitionGate> Gates { get; set; }

            [JsonProperty(PropertyName = "gatesOptions")]
            public IEnumerable<ReleaseDefinitionGateOptions> GatesOptions { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }
        }

        public class ReleaseDefinitions
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IEnumerable<ReleaseDefinition> Value { get; set; }
        }

        public class ReleaseDefinition
        {
            [JsonProperty(PropertyName = "_links")]
            public ReleaseDefinitionReferenceLink Links { get; set; }

            [JsonProperty(PropertyName = "artifacts")]
            public IEnumerable<Artifact> Artifacts { get; set; }

            [JsonProperty(PropertyName = "comment")]
            public string Comment { get; set; }

            [JsonProperty(PropertyName = "createdBy")]
            public IdentityReference CreatedBy { get; set; }

            [JsonProperty(PropertyName = "createdOn")]
            public string CreatedOn { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "environments")]
            public IEnumerable<ReleaseDefinitionEnvironment> Environments { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "isDeleted")]
            public bool IsDeleted { get; set; }

            [JsonProperty(PropertyName = "lastRelease")]
            public ReleaseReference LastRelease { get; set; }

            [JsonProperty(PropertyName = "modifiedBy")]
            public IdentityReference ModifiedBy { get; set; }

            [JsonProperty(PropertyName = "modifiedOn")]
            public string ModifiedOn { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "path")]
            public string Path { get; set; }

            [JsonProperty(PropertyName = "projectReference")]
            public ProjectReference ProjectReference { get; set; }

            [JsonProperty(PropertyName = "properties")]
            public IDictionary<string, object> Properties { get; set; }

            [JsonProperty(PropertyName = "releaseNameFormat")]
            public string ReleaseNameFormat { get; set; }

            [JsonProperty(PropertyName = "revision")]
            public int Revision { get; set; }

            [JsonProperty(PropertyName = "source")]
            public string Source { get; set; }

            [JsonProperty(PropertyName = "tags")]
            public IEnumerable<string> Tags { get; set; }

            // todo: a JsonConverter class might be needed to address all kinds of trigger type.
            // right now, the trigger data is not visible.
            [JsonProperty(PropertyName = "triggers")]
            public IEnumerable<ReleaseTriggerBase> Triggers { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "variableGroups")]
            public IEnumerable<int> VariableGroups { get; set; }

            [JsonProperty(PropertyName = "variables")]
            public Dictionary<string, ConfigurationVariableValue> Variables { get; set; }
        }

        public class ReleaseDefinitionShallowReference
        {
            [JsonProperty(PropertyName = "_links")]
            public object Links { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "path")]
            public string Path { get; set; }

            [JsonProperty(PropertyName = "projectReference")]
            public ProjectReference ProjectReference { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class ReleaseReference
        {
            [JsonProperty(PropertyName = "_links")]
            public object Links { get; set; }

            [JsonProperty(PropertyName = "artifacts")]
            public IEnumerable<Artifact> Artifacts { get; set; }

            [JsonProperty(PropertyName = "createdBy")]
            public IdentityReference CreatedBy { get; set; }

            [JsonProperty(PropertyName = "createdOn")]
            public string CreatedOn { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "modifiedBy")]
            public IdentityReference ModifiedBy { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "reason")]
            public ReleaseReason Reason { get; set; }

            [JsonProperty(PropertyName = "releaseDefinition")]
            public ReleaseDefinitionShallowReference ReleaseDefinition { get; set; }
        }

        public class ReleaseSchedule
        {
            [JsonProperty(PropertyName = "daysToRelease")]
            public ScheduleDays DaysToRelease { get; set; }

            [JsonProperty(PropertyName = "jobId")]
            public string JobId { get; set; }

            [JsonProperty(PropertyName = "startHours")]
            public int StartHours { get; set; }

            [JsonProperty(PropertyName = "startMinutes")]
            public int StartMinutes { get; set; }

            [JsonProperty(PropertyName = "timeZoneId")]
            public string TimeZoneId { get; set; }
        }

        public class ReleaseShallowReference
        {
            [JsonProperty(PropertyName = "_links")]
            public object Links { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class ReleaseTriggerBase
        {
            [JsonProperty(PropertyName = "triggerType")]
            public ReleaseTriggerType TriggerType { get; set; }
        }

        public class WorkflowTask
        {
            [JsonProperty(PropertyName = "alwaysRun")]
            public bool AlwaysRun { get; set; }

            [JsonProperty(PropertyName = "condition")]
            public string Condition { get; set; }

            [JsonProperty(PropertyName = "continueOnError")]
            public bool ContinueOnError { get; set; }

            [JsonProperty(PropertyName = "definitionType")]
            public string DefinitionType { get; set; }

            [JsonProperty(PropertyName = "enabled")]
            public bool Enabled { get; set; }

            [JsonProperty(PropertyName = "environment")]
            public object Environment { get; set; }

            [JsonProperty(PropertyName = "inputs")]
            public object Inputs { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "overrideInputs")]
            public object OverrideInputs { get; set; }

            [JsonProperty(PropertyName = "refName")]
            public string RefName { get; set; }

            [JsonProperty(PropertyName = "taskId")]
            public string TaskId { get; set; }

            [JsonProperty(PropertyName = "timeoutInMinutes")]
            public int TimeoutInMinutes { get; set; }

            [JsonProperty(PropertyName = "version")]
            public string Version { get; set; }
        }
        
        #endregion
    }
}
