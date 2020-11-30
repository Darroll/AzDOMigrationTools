using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ADO.Engine.Configuration.ProjectImport
{
    public sealed class EngineConfiguration
    {
        #region - Static Declarations

        #region - Public Members

        public static EngineConfiguration GetDefault()
        {
            EngineConfiguration engineConfig = new EngineConfiguration
            {
                SourceCollection = "<Enter your collection/organization name>",
                SourceProject = "<Enter your source project name>",
                DestinationCollection = "<Enter your destination project name>",
                DestinationProject = "<Enter your destination project name>",
                DestinationProjectProcessName = "<Enter your destination project process name>",
                PAT = "<Enter your personal access token to access the source collection>",
                DefaultTeamName = "<Enter your default team>",
                Description = "<Enter description>",
                SecurityTasksFile = "<Enter path to security tasks file>",
                ImportPath = "<Enter your path for input files>",
                CustomInputParameterNamesForServiceEndpoint = null,
                ImportSourceCodeCredentials = ProjectImportSourceCodeCredentials.GetDefault(),
                Behaviors = ProjectImportBehavior.GetDefault(false),
                RestApiService = RestApiServiceConfig.GetDefault(),
                QueryFolderPaths = null,
                AreaPaths = null,
                AreaPrefixPath = string.Empty,
                IterationPrefixPath = string.Empty,
                BuildPipelineFolderPaths = null,
                ReleasePipelineFolderPaths = null
            };

            // Return default engine configuration.
            return engineConfig;
        }

        #endregion

        #endregion

        #region - Public Members

        #region - Properties.

        [JsonProperty(PropertyName = "sourceCollection", Required = Required.Always)]
        public string SourceCollection { get; set; }

        [JsonProperty(PropertyName = "sourceProject", Required = Required.Always)]
        public string SourceProject { get; set; }

        [JsonProperty(PropertyName = "destinationCollection", Required = Required.Always)]
        public string DestinationCollection { get; set; }

        [JsonProperty(PropertyName = "destinationProject", Required = Required.Always)]
        public string DestinationProject { get; set; }

        [JsonProperty(PropertyName = "destinationProjectProcessName", Required = Required.Always)]
        public string DestinationProjectProcessName { get; set; }

        [JsonProperty(PropertyName = "pat", Required = Required.Always)]
        public string PAT { get; set; }

        [JsonProperty(PropertyName = "defaultTeamName", Required = Required.Default)]
        public string DefaultTeamName { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Default)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "securityTasksFile", Required = Required.Default)]
        public string SecurityTasksFile { get; set; }

        [JsonProperty(PropertyName = "importPath", Required = Required.Always)]
        public string ImportPath { get; set; }

        [JsonProperty(PropertyName = "restApiService", Required = Required.Default)]
        public RestApiServiceConfig RestApiService { get; set; }

        /// <summary>
        /// It permits to specify custom input parameter names utilized for service endpoint. It is case insensitive.
        /// For example, if you have created task group with inputs necessiting service endpoints, these input names
        /// must be put in that array of names.
        /// </summary>
        [JsonProperty(PropertyName = "customInputParameterNamesForServiceEndpoint", Required = Required.Default)]
        public List<string> CustomInputParameterNamesForServiceEndpoint { get; set; }

        [JsonProperty(PropertyName = "importSourceCodeCredentials", Required = Required.Default)]
        public ProjectImportSourceCodeCredentials ImportSourceCodeCredentials { get; set; }

        [JsonProperty(PropertyName = "behaviors", Required = Required.Default)]
        public ProjectImportBehavior Behaviors { get; set; }

        [JsonProperty(PropertyName = "queryFolderPaths", Required = Required.Default)]
        public List<NamespacePathSecuritySpec> QueryFolderPaths { get; set; }

        [JsonProperty(PropertyName = "areaPaths", Required = Required.Default)]
        public List<NamespacePathSecuritySpec> AreaPaths { get; set; }

        [JsonProperty(PropertyName = "areaPrefixPath", Required = Required.Default)]
        public string AreaPrefixPath { get; set; }

        [JsonProperty(PropertyName = "iterationPrefixPath", Required = Required.Default)]
        public string IterationPrefixPath { get; set; }

        [JsonProperty(PropertyName = "buildPipelineFolderPaths", Required = Required.Default)]
        public List<NamespacePathSecuritySpec> BuildPipelineFolderPaths { get; set; }

        [JsonProperty(PropertyName = "releasePipelineFolderPaths", Required = Required.Default)]
        public List<NamespacePathSecuritySpec> ReleasePipelineFolderPaths { get; set; }

        [JsonProperty(PropertyName = "repositorySecurity", Required = Required.Default)]
        public List<NamespacePathSecuritySpec> RepositorySecurity { get; set; }

        [JsonProperty(PropertyName = "teamMappings", Required = Required.Default)]
        public List<TeamMapping> TeamMappings { get; set; }

        [JsonProperty(PropertyName = "teamExclusions", Required = Required.Default)]
        public List<string> TeamExclusions { get; set; }

        [JsonProperty(PropertyName = "teamInclusions", Required = Required.Default)]
        public List<string> TeamInclusions { get; set; }

        [JsonProperty(PropertyName = "areaInclusions", Required = Required.Default)]
        public List<string> AreaInclusions { get; set; }

        [JsonProperty(PropertyName = "iterationInclusions", Required = Required.Default)]
        public List<string> IterationInclusions { get; set; }

        [JsonProperty(PropertyName = "prefixName", Required = Required.Default)]
        public string PrefixName { get; set; }

        [JsonProperty(PropertyName = "prefixSeparator", Required = Required.Default)]
        public string PrefixSeparator { get; set; }

        [JsonProperty(PropertyName = "processMapLocation", Required = Required.Always)]
        public string ProcessMapLocation { get; set; }

        [JsonProperty(PropertyName = "areaInitializationTreePath", Required = Required.Default)]
        public string AreaInitializationTreePath { get; set; }

        [JsonProperty(PropertyName = "iterationInitializationTreePath", Required = Required.Default)]
        public string IterationInitializationTreePath { get; set; }

        [JsonProperty(PropertyName = "isOnPremiseMigration", Required = Required.Default)]
        public bool IsOnPremiseMigration { get; set; } = true;

        [JsonProperty(PropertyName = "isClone", Required = Required.Default)]
        public bool IsClone { get; set; } = true;


        #endregion

        #region - Constructors.

        public EngineConfiguration()
        {
            QueryFolderPaths = null;
            AreaPaths = null;
            BuildPipelineFolderPaths = null;
            PrefixName = string.Empty;
            PrefixSeparator = string.Empty;
            ReleasePipelineFolderPaths = null;
            RepositorySecurity = null;
            TeamMappings = null;
            TeamExclusions = null;
            TeamInclusions = null;
            AreaInclusions = null;
        }

        #endregion

        #region - Methods.

        #region - Public Members
        public void Validate()
        {
            // Set to null this property when this behavior is false.
            if (!Behaviors.CreateDestinationProject)
                Description = null;

            // Set to null this property when this behavior is false.
            if (!Behaviors.ImportRepository)
            {
                // Instantiate the credentials but set them to null.
                ImportSourceCodeCredentials = new ProjectImportSourceCodeCredentials
                {
                    UserID = null,
                    Password = null,
                    GitUsername = null,
                    GitPassword = null
                };
            }

            // Set to null this property when these behaviors are false.
            if (!(Behaviors.ImportRepository || Behaviors.ImportBuildDefinition || Behaviors.ImportReleaseDefinition || Behaviors.QuerySecurityDescriptors))
                DefaultTeamName = null;

            // Set to null this property when this behavior is false.
            if (Behaviors.QuerySecurityDescriptors)
            {
                // Set to null this property when this behavior is false.
                if (!Behaviors.IncludeQueryFolderSecurity)
                    QueryFolderPaths = null;
                // Set to null this property when this behavior is false.
                if (!Behaviors.IncludeAreaSecurity)
                    AreaPaths = null;
                // Set to null this property when this behavior is false.
                if (!Behaviors.IncludeBuildPipelineSecurity)
                    BuildPipelineFolderPaths = null;
                // Set to null this property when this behavior is false.
                if (!Behaviors.IncludeReleasePipelineSecurity)
                    ReleasePipelineFolderPaths = null;
            }
            // Set to null this property when this behavior is false.
            else
            {
                QueryFolderPaths = null;
                AreaPaths = null;
                BuildPipelineFolderPaths = null;
                ReleasePipelineFolderPaths = null;
            }

            // Set to null this property when these behaviors are false.
            if (!(Behaviors.QuerySecurityDescriptors || Behaviors.ExecuteSecurityTasks))
                SecurityTasksFile = null;

            // load secrets from environment variables
            if (Behaviors.LoadSecretsFromEnvironmentVariables)
            {
                PAT = Utility.LoadFromEnvironmentVariables(PAT);
                ImportSourceCodeCredentials.Password = Utility.LoadFromEnvironmentVariables(ImportSourceCodeCredentials.Password);
                ImportSourceCodeCredentials.GitPassword = Utility.LoadFromEnvironmentVariables(ImportSourceCodeCredentials.GitPassword);
            }
        }
        #endregion - Public Members        

        #endregion

        #region - Nested Classes and Enumerations.

        public class TeamMapping
        {
            [JsonProperty(PropertyName = "sourceTeam")]
            public string SourceTeam { get; set; }

            [JsonProperty(PropertyName = "destinationTeam")]
            public string DestinationTeam { get; set; }
        }

        #endregion

        #endregion
    }
}