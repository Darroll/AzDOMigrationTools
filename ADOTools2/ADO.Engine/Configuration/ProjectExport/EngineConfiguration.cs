using System.Collections.Generic;
using Newtonsoft.Json;

namespace ADO.Engine.Configuration.ProjectExport
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
                SourceProjectProcessName = "<Enter your source project process name>",
                PAT = "<Enter your personal access token to access the source collection>",
                TemplatesPath = "<Enter path to template files>",
                ExportPath = "<Enter your path for output files>",
                BuildReleasePrefixPath= "<Enter a prefix path for builds and releases>",
                Behaviors = ProjectExportBehavior.GetDefault(false),
                ExcludeRepositories = null,
                IncludeRepositories = null,
                CustomInputParameterNamesForServiceEndpoint = null,
                RestApiService = RestApiServiceConfig.GetDefault()
            };

            // Return default engine configuration.
            return engineConfig;
        }

        #endregion

        #endregion

        #region - Public Members

        #region - Properties.

        // Azure DevOps Organization = Azure DevOps account = Azure DevOps collection
        [JsonProperty(PropertyName = "sourceCollection", Required = Required.Always)]
        public string SourceCollection { get; set; }

        [JsonProperty(PropertyName = "sourceProject", Required = Required.Always)]
        public string SourceProject { get; set; }
        
        [JsonProperty(PropertyName = "sourceProjectProcessName", Required = Required.Always)]
        public string SourceProjectProcessName { get; set; }

        [JsonProperty(PropertyName = "pat", Required = Required.Always)]
        public string PAT { get; set; }

        [JsonProperty(PropertyName = "templatesPath", Required = Required.Always)]
        public string TemplatesPath { get; set; }
        
        [JsonProperty(PropertyName = "buildReleasePrefixPath", Required = Required.Default)]
        public string BuildReleasePrefixPath { get; set; }

        [JsonProperty(PropertyName = "exportPath", Required = Required.Always)]
        public string ExportPath { get; set; }

        [JsonProperty(PropertyName = "behaviors", Required = Required.Default)]
        public ProjectExportBehavior Behaviors { get; set; }

        [JsonProperty(PropertyName = "excludeRepositories", Required = Required.Default)]
        public List<string> ExcludeRepositories { get; set; }
        
        [JsonProperty(PropertyName = "includeRepositories", Required = Required.Default)]
        public List<string> IncludeRepositories { get; set; }

        /// <summary>
        /// It permits to specify custom input parameter names utilized for service endpoint. It is case insensitive.
        /// For example, if you have created task group with inputs necessiting service endpoints, these input names
        /// must be put in that array of names.
        /// </summary>
        [JsonProperty(PropertyName = "customInputParameterNamesForServiceEndpoint", Required = Required.Default)]
        public List<string> CustomInputParameterNamesForServiceEndpoint { get; set; }

        [JsonProperty(PropertyName = "restApiService", Required = Required.Default)]
        public RestApiServiceConfig RestApiService { get; set; }

        [JsonProperty(PropertyName = "processMapLocation", Required = Required.Always)]
        public string ProcessMapLocation { get; set; }

        [JsonProperty(PropertyName = "isOnPremiseMigration", Required = Required.Default)]
        public bool IsOnPremiseMigration { get; set; } = true;

        #endregion

        #region - Constructors.

        public EngineConfiguration()
        {

        }

        #endregion

        #region - Methods.

        public void Validate()
        {
            // Nothing to do for now.

            // load secrets from environment variables
            if (Behaviors.LoadSecretsFromEnvironmentVariables)
            {
                PAT = Utility.LoadFromEnvironmentVariables(PAT);
            }
        }

        #endregion

        #endregion
    }
}
