using System;
using System.Reflection;
using Newtonsoft.Json;

namespace ADO.Engine.Configuration
{
    public sealed class ProjectExportBehavior
    {
        #region - Static Declarations

        #region - Public Members

        public static ProjectExportBehavior GetDefault(bool defaultBooleanValue)
        {
            // Define default behaviors.
            ProjectExportBehavior defaultBehaviors = new ProjectExportBehavior
            {
                AddProjectNameToBuildDefinitionPath = defaultBooleanValue,
                AddProjectNameToReleaseDefinitionPath = defaultBooleanValue,
                ExportAgentQueue = false, //always false since not yet implemented
                ExportArea = defaultBooleanValue,
                ExportBuildDefinition = defaultBooleanValue,
                ExportInstalledExtension = defaultBooleanValue,
                ExportIteration = defaultBooleanValue,
                ExportOrganizationProcess = defaultBooleanValue,
                ExportPolicyConfiguration = defaultBooleanValue,
                ExportPullRequest = defaultBooleanValue,
                ExportReleaseDefinition = defaultBooleanValue,
                ExportRepository = defaultBooleanValue,
                ExportServiceEndpoint = defaultBooleanValue,
                ExportTaskGroup = defaultBooleanValue,
                ExportTeamConfiguration = defaultBooleanValue,
                ExportVariableGroup = defaultBooleanValue,
                ExportWiki = defaultBooleanValue,
                LoadSecretsFromEnvironmentVariables = defaultBooleanValue,
                ResetPipelineQueue = defaultBooleanValue
            };

            // Return default behaviors.
            return defaultBehaviors;
        }

        #endregion

        #endregion

        #region - Public Members

        #region - Properties

        /// <summary>
        /// Add the source project name to build definition path.
        /// </summary>
        [JsonProperty(PropertyName = "addProjectNameToBuildDefinitionPath")]
        public bool AddProjectNameToBuildDefinitionPath { get; set; }

        /// <summary>
        /// Add the source project name to release definition path.
        /// </summary>
        [JsonProperty(PropertyName = "addProjectNameToReleaseDefinitionPath")]
        public bool AddProjectNameToReleaseDefinitionPath { get; set; }

        /// <summary>
        /// Export agent queues.
        /// </summary>
        [JsonProperty(PropertyName = "exportAgentQueue")]
        public bool ExportAgentQueue { get; set; }

        /// <summary>
        /// Export project areas.
        /// </summary>
        [JsonProperty(PropertyName = "exportArea")]
        public bool ExportArea { get; set; }

        /// <summary>
        /// Export project build definitions.
        /// </summary>
        [JsonProperty(PropertyName = "exportBuildDefinition")]
        public bool ExportBuildDefinition { get; set; }

        /// <summary>
        /// Export installed extensions from collection or account or organization.
        /// </summary>
        [JsonProperty(PropertyName = "exportInstalledExtension")]
        public bool ExportInstalledExtension { get; set; }

        /// <summary>
        /// Export project iterations.
        /// </summary>
        [JsonProperty(PropertyName = "exportIteration")]
        public bool ExportIteration { get; set; }

        /// <summary>
        /// Export processes from collection or account or organization.
        /// </summary>
        [JsonProperty(PropertyName = "exportOrganizationProcess")]
        public bool ExportOrganizationProcess { get; set; }

        /// <summary>
        /// Export policy configurations.
        /// </summary>
        [JsonProperty(PropertyName = "exportPolicyConfiguration")]
        public bool ExportPolicyConfiguration { get; set; }

        /// <summary>
        /// Export project open pull requests.
        /// </summary>
        [JsonProperty(PropertyName = "exportPullRequest")]
        public bool ExportPullRequest { get; set; }

        /// <summary>
        /// Export project release definitions.
        /// </summary>
        [JsonProperty(PropertyName = "exportReleaseDefinition")]
        public bool ExportReleaseDefinition { get; set; }

        /// <summary>
        /// Export project non-hidden Git repositories.
        /// </summary>
        [JsonProperty(PropertyName = "exportRepository")]
        public bool ExportRepository { get; set; }

        /// <summary>
        /// Export project service endpoints.
        /// </summary>
        [JsonProperty(PropertyName = "exportServiceEndpoint")]
        public bool ExportServiceEndpoint { get; set; }

        /// <summary>
        /// Export project task groups. It excludes deleted ones.
        /// </summary>
        [JsonProperty(PropertyName = "exportTaskGroup")]
        public bool ExportTaskGroup { get; set; }

        /// <summary>
        /// Export a team configuration within a project.
        /// </summary>
        [JsonProperty(PropertyName = "exportTeamConfiguration")]
        public bool ExportTeamConfiguration { get; set; }

        /// <summary>
        /// Export project variable groups. It excludes deleted ones.
        /// </summary>
        [JsonProperty(PropertyName = "exportVariableGroup")]
        public bool ExportVariableGroup { get; set; }

        /// <summary>
        /// Export project wikis.
        /// </summary>
        [JsonProperty(PropertyName = "exportWiki")]
        public bool ExportWiki { get; set; }

        /// <summary>
        /// Load secrets from environment variables.
        /// </summary>
        [JsonProperty(PropertyName = "loadSecretsFromEnvironmentVariables")]
        public bool LoadSecretsFromEnvironmentVariables { get; set; }

        /// <summary>
        /// Reset build and release definition assigned queues for execution.
        /// </summary>
        [JsonProperty(PropertyName = "resetPipelineQueue")]
        public bool ResetPipelineQueue { get; set; }

        #endregion

        #region - Methods

        public void SetDefaultIfUndefined()
        {
            // Initialize.
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
                    // Change value.
                    propertyInfo.SetValue(this, false, null);
                }
                // Nothing to change.
                // else
                // { }
            }
        }

        #endregion

        #endregion
    }

    public sealed class ProjectImportBehavior
    {
        #region - Static Declarations

        #region - Public Members

        public static ProjectImportBehavior GetDefault(bool defaultBooleanValue)
        {
            // Define default behaviors.
            ProjectImportBehavior defaultBehaviors = new ProjectImportBehavior
            {
                CreateDestinationProject = defaultBooleanValue,
                InitializeProject = defaultBooleanValue,
                DeleteDefaultRepository = defaultBooleanValue,
                DeleteSecurityTasksOutputFile = defaultBooleanValue,
                ExecuteSecurityTasks = defaultBooleanValue,
                ImportAgentQueue = false, //not yet implemented
                ImportArea = defaultBooleanValue,
                ImportBuildDefinition = defaultBooleanValue,
                ImportIteration = defaultBooleanValue,
                ImportPolicyConfiguration = defaultBooleanValue,
                ImportPullRequest = defaultBooleanValue,
                ImportReleaseDefinition = defaultBooleanValue,
                ImportServiceEndpoint = defaultBooleanValue,
                ImportRepository = defaultBooleanValue,
                ImportTeam = defaultBooleanValue,
                ImportTeamConfiguration = defaultBooleanValue,
                ImportTaskGroup = defaultBooleanValue,
                ImportVariableGroup = defaultBooleanValue,
                ImportWiki = defaultBooleanValue,
                IncludeTeamSettings = defaultBooleanValue,
                IncludeTeamAreas = defaultBooleanValue,
                IncludeTeamIterations = defaultBooleanValue,
                IncludeBoardColumns = defaultBooleanValue,
                IncludeBoardRows = defaultBooleanValue,
                IncludeCardFieldSettings = defaultBooleanValue,
                IncludeCardStyleSettings = defaultBooleanValue,
                IncludeAreaSecurity = defaultBooleanValue,
                IncludeBuildPipelineSecurity = defaultBooleanValue,
                IncludeEndpointCreatorsSecurity = defaultBooleanValue,
                IncludeQueryFolderSecurity = defaultBooleanValue,
                IncludeReleasePipelineSecurity = defaultBooleanValue,
                IncludeRepositorySecurity = defaultBooleanValue,
                InstallExtensions = defaultBooleanValue,
                LoadSecretsFromEnvironmentVariables = defaultBooleanValue,
                QuerySecurityDescriptors = defaultBooleanValue,
                RenameRepository = defaultBooleanValue,
                RenameTeam = defaultBooleanValue,
                UseSecurityTasksFile = defaultBooleanValue
            };

            // Return default behaviors.
            return defaultBehaviors;
        }

        #endregion

        #endregion

        #region - Public Members

        #region - Properties

        /// <summary>
        /// Create destination project if it does not exist.
        /// </summary>
        [JsonProperty(PropertyName = "createDestinationProject")]
        public bool CreateDestinationProject { get; set; }

        [JsonProperty(PropertyName = "initializeProject")]
        public bool InitializeProject { get; set; }

        /// <summary>
        /// Delete default repository within a project which has.
        /// the same name as the project. This operation will be aborted if
        /// there is only one repository.
        /// </summary>
        [JsonProperty(PropertyName = "deleteDefaultRepository")]
        public bool DeleteDefaultRepository { get; set; }

        [JsonProperty(PropertyName = "deleteSecurityTasksOutputFile")]
        public bool DeleteSecurityTasksOutputFile { get; set; }

        /// <summary>
        /// Apply security described in the tasks file.
        /// </summary>
        [JsonProperty(PropertyName = "executeSecurityTasks")]
        public bool ExecuteSecurityTasks { get; set; }

        /// <summary>
        /// Import agent queues.
        /// </summary>
        [JsonProperty(PropertyName = "importAgentQueue")]
        public bool ImportAgentQueue { get; set; }

        /// <summary>
        /// Import all project areas.
        /// </summary>
        [JsonProperty(PropertyName = "importArea")]
        public bool ImportArea { get; set; }

        /// <summary>
        /// Import build definitions.
        /// </summary>
        [JsonProperty(PropertyName = "importBuildDefinition")]
        public bool ImportBuildDefinition { get; set; }

        /// <summary>
        /// Import all project iterations.
        /// </summary>
        [JsonProperty(PropertyName = "importIteration")]
        public bool ImportIteration { get; set; }

        /// <summary>
        /// Import policy configurations except build and required reviewers one for now.
        /// </summary>
        [JsonProperty(PropertyName = "importPolicyConfiguration")]
        public bool ImportPolicyConfiguration { get; set; }

        /// <summary>
        /// Import open pull requests.
        /// </summary>
        [JsonProperty(PropertyName = "importPullRequest")]
        public bool ImportPullRequest { get; set; }

        /// <summary>
        /// Import release definitions.
        /// </summary>
        [JsonProperty(PropertyName = "importReleaseDefinition")]
        public bool ImportReleaseDefinition { get; set; }

        /// <summary>
        /// Import repository and source code.
        /// </summary>
        [JsonProperty(PropertyName = "importRepository")]
        public bool ImportRepository { get; set; }

        /// <summary>
        /// Import service endpoint.
        /// </summary>
        [JsonProperty(PropertyName = "importServiceEndpoint")]
        public bool ImportServiceEndpoint { get; set; }

        /// <summary>
        /// Import project teams.
        /// </summary>
        [JsonProperty(PropertyName = "importTeam")]
        public bool ImportTeam { get; set; }

        /// <summary>
        /// Import project team configurations.
        /// </summary>
        [JsonProperty(PropertyName = "importTeamConfiguration")]
        public bool ImportTeamConfiguration { get; set; }

        /// <summary>
        /// ...include team settings from team configurations.
        /// </summary>
        [JsonProperty(PropertyName = "includeTeamSettings")]
        public bool IncludeTeamSettings { get; set; }

        /// <summary>
        /// ...include team areas from team configurations.
        /// </summary>
        [JsonProperty(PropertyName = "includeTeamAreas")]
        public bool IncludeTeamAreas { get; set; }

        /// <summary>
        /// ...include team iterations from team configurations.
        /// </summary>
        [JsonProperty(PropertyName = "includeTeamIterations")]
        public bool IncludeTeamIterations { get; set; }

        /// <summary>
        /// ...include board columns from team configurations.
        /// </summary>
        [JsonProperty(PropertyName = "includeBoardColumns")]
        public bool IncludeBoardColumns { get; set; }

        /// <summary>
        /// ...include board rows from team configurations.
        /// </summary>
        [JsonProperty(PropertyName = "includeBoardRows")]
        public bool IncludeBoardRows { get; set; }

        /// <summary>
        /// ...include card field settings from team configurations.
        /// </summary>
        [JsonProperty(PropertyName = "includeCardFieldSettings")]
        public bool IncludeCardFieldSettings { get; set; }

        /// <summary>
        /// ...include card style settings from team configurations.
        /// </summary>
        [JsonProperty(PropertyName = "includeCardStyleSettings")]
        public bool IncludeCardStyleSettings { get; set; }

        /// <summary>
        /// Import task groups.
        /// </summary>
        [JsonProperty(PropertyName = "importTaskGroup")]
        public bool ImportTaskGroup { get; set; }

        /// <summary>
        /// Import variable groups.
        /// </summary>
        [JsonProperty(PropertyName = "importVariableGroup")]
        public bool ImportVariableGroup { get; set; }

        /// <summary>
        /// Import wikis.
        /// </summary>
        [JsonProperty(PropertyName = "importWiki")]
        public bool ImportWiki { get; set; }

        /// <summary>
        /// Install extensions.
        /// </summary>
        [JsonProperty(PropertyName = "installExtensions")]
        public bool InstallExtensions { get; set; }

        /// <summary>
        /// Query security descriptors and generate security tasks.
        /// </summary>
        [JsonProperty(PropertyName = "querySecurityDescriptors")]
        public bool QuerySecurityDescriptors { get; set; }

        /// <summary>
        /// ...include security descriptors from areas.
        /// </summary>
        [JsonProperty(PropertyName = "includeAreaSecurity")]
        public bool IncludeAreaSecurity { get; set; }

        /// <summary>
        /// ...include security descriptors from build pipelines.
        /// </summary>
        [JsonProperty(PropertyName = "includeBuildPipelineSecurity")]
        public bool IncludeBuildPipelineSecurity { get; set; }

        /// <summary>
        /// ...include security descriptors from endpoint creators.
        /// </summary>
        [JsonProperty(PropertyName = "includeEndpointCreatorsSecurity")]
        public bool IncludeEndpointCreatorsSecurity { get; set; }

        /// <summary>
        /// ...include security descriptors from query folders.
        /// </summary>
        [JsonProperty(PropertyName = "includeQueryFolderSecurity")]
        public bool IncludeQueryFolderSecurity { get; set; }

        /// <summary>
        /// ...include security descriptors from release pipelines.
        /// </summary>
        [JsonProperty(PropertyName = "includeReleasePipelineSecurity")]
        public bool IncludeReleasePipelineSecurity { get; set; }

        [JsonProperty(PropertyName = "includeRepositorySecurity")]
        public bool IncludeRepositorySecurity { get; set; }

        /// <summary>
        /// Load secrets from environment variables.
        /// </summary>
        [JsonProperty(PropertyName = "loadSecretsFromEnvironmentVariables")]
        public bool LoadSecretsFromEnvironmentVariables { get; set; }

        /// <summary>
        /// Rename repositories by prefixing their name with a prefix.
        /// </summary>
        [JsonProperty(PropertyName = "renameRepository")]
        public bool RenameRepository { get; set; }

        /// <summary>
        /// Rename teams by prefixing their name with a prefix.
        /// </summary>
        [JsonProperty(PropertyName = "renameTeam")]
        public bool RenameTeam { get; set; }

        /// <summary>
        /// Use a *.json file to persist security tasks.
        /// </summary>
        [JsonProperty(PropertyName = "useSecurityTasksFile")]
        public bool UseSecurityTasksFile { get; set; }        

        #endregion

        #region - Methods

        public void SetDefaultIfUndefined()
        {
            // Initialize.
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
                    // Change value.
                    propertyInfo.SetValue(this, false, null);
                }
                // Nothing to change.
                // else
                // { }
            }
        }

        #endregion

        #endregion
    }
}
