using ADO.Engine.Configuration;
using ADO.Engine.DefinitionFiles;
using ADO.RestAPI;
using ADO.RestAPI.Tasks.Security;
using ADO.RestAPI.Viewmodel50;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ADO.Engine
{
    public sealed class ProjectDefinition
    {
        #region - Private Members.

        #region - Properties.

        /// <summary>
        /// This property is involved only with internal class operations.
        /// </summary>
        private string Path { get; set; }

        /// <summary>
        /// This property is involved only with internal class operations.
        /// </summary>
        private string RootPath { get; set; }

        /// <summary>
        /// This property is involved only with internal class operations.
        /// </summary>
        private string TemplatesPath { get; set; }

        #endregion

        #region - Methods.

        /// <summary>
        /// This method is involved only with internal class operations.
        /// </summary>
        private List<MappingRecord> LoadMappingsFromFile()
        {
            // Initialize.
            string path = GetFileFor("Mappings");
            string jsonContent;
            List<MappingRecord> mappingRecords;

            // Read file to memory
            if (File.Exists(path))
            {
                jsonContent = ReadDefinitionFile(path);
                mappingRecords = JsonConvert.DeserializeObject<List<MappingRecord>>(jsonContent);

                // If the object is null, instantiate a new one.
                if (mappingRecords == null)
                    mappingRecords = new List<MappingRecord>();
            }
            else
            {
                // Instantiate.
                mappingRecords = new List<MappingRecord>();
            }

            // Return mapping records.
            return mappingRecords;
        }

        /// <summary>
        /// This method is involved only with internal class operations.
        /// </summary>
        private void UpdateMappingRecordInternal(string mappingType, string searchMode, string oldEntityKey, string oldEntityName, string newEntityKey, string newEntityName)
        {
            // Initialize.
            List<MappingRecord> mappings = null;
            MappingRecord mappingRecord = null;

            // Load all mappings from file.
            mappings = LoadMappingsFromFile();

            // Retrieve right mapping record based on type and old name.
            if (mappings != null)
            {
                if (searchMode.ToLower() == "key")
                    mappingRecord = mappings.Find(x => (x.OldEntityKey == oldEntityKey) && (x.Type == mappingType));
                else if (searchMode.ToLower() == "name")
                    mappingRecord = mappings.Find(x => (x.OldEntityName == oldEntityName) && (x.Type == mappingType));
            }

            // If no mapping record exists, create one.
            if (mappingRecord == null)
                AddMappingRecord(mappingType, oldEntityKey, oldEntityName, newEntityKey, newEntityName);
            else
            {
                if (!string.IsNullOrEmpty(newEntityKey))
                    mappingRecord.NewEntityKey = newEntityKey;

                if (!string.IsNullOrEmpty(newEntityName))
                    mappingRecord.NewEntityName = newEntityName;

                // Save back to file.
                WriteMappingsToFile(mappings);
            }
        }

        /// <summary>
        /// This method is involved only with internal class operations.
        /// </summary>
        private void WriteMappingsToFile(List<MappingRecord> mappings)
        {
            // Initialize.
            string filename;
            string jsonContent;

            // Save data.
            jsonContent = JsonConvert.SerializeObject(mappings, Formatting.Indented);

            // Define the export file.
            filename = this.GetFileFor("Mappings");

            // Write definition file.
            this.WriteDefinitionFile(filename, jsonContent);
        }

        #endregion

        #endregion

        #region - Public Members.

        #region - Properties.

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public AdoEntityReferenceCollection AdoEntities { get; set; }

        /// <summary>
        /// This property is involved in both export and import operations.
        /// </summary>
        public Dictionary<string, RestAPI.Configuration> AdoRestApiConfigurations { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public CoreResponse.TeamMembers DefaultTeamMembers { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public string DefaultTeamName { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public CoreResponse.TeamMember DefaultUser { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public ProjectProperties DestinationProject { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public SecurityTasks SecurityTasks { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public string SecurityTasksFile { get; private set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public SecurityResponse.SecurityNamespaces SecurityNamespaces { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public Dictionary<string, GraphResponse.GraphGroup> SecurityPrincipalCache { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public Dictionary<string, string> SidCache { get; set; }

        /// <summary>
        /// This property is involved in both export and import operations.
        /// </summary>
        public ProjectProperties SourceProject { get; set; }

        #region - Mappings to definitions files.

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public MultiObjectsDefinitionFile AreaDefinitions { get; set; }

        public List<MultiObjectsDefinitionFile> AreaInitializationDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public List<SingleObjectDefinitionFile> BuildDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public MultiObjectsDefinitionFile ExtensionDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public MultiObjectsDefinitionFile IterationDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public List<MultiObjectsDefinitionFile> IterationInitializationDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public List<PullRequestDefinitionFiles> PullRequestDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public List<SingleObjectDefinitionFile> ReleaseDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public List<SingleObjectDefinitionFile> RepositoryDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public MultiObjectsDefinitionFile ServiceEndpointDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public List<SingleObjectDefinitionFile> ServiceEndpointForCodeImportDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public List<SingleObjectDefinitionFile> TaskGroupDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public List<TeamConfigurationDefinitionFiles> TeamConfigurationDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public List<TeamConfigurationDefinitionFiles> TeamConfigurationInitializationDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public MultiObjectsDefinitionFile TeamDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public MultiObjectsDefinitionFile TeamInitializationDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public List<SingleObjectDefinitionFile> VariableGroupDefinitions { get; set; }

        /// <summary>
        /// This property is involved with import operations.
        /// </summary>
        public MultiObjectsDefinitionFile WikiDefinitions { get; set; }



        #endregion

        #endregion

        #region - Constructors.

        /// <summary>
        /// This constructor is involved with export operations.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "This simplification would make the code more obscur.")]
        public ProjectDefinition(string rootPath, string sourceCollection, string sourceProject, string sourceProjectProcessName, string accessToken, string templatesPath, RestApiServiceConfig restApiServiceConfig)
        {
            // Initialize.
            DirectoryInfo di;

            // Store root path.
            this.RootPath = rootPath;

            // Define path to definition based on source collection and project.
            this.Path = System.IO.Path.Combine(this.RootPath, sourceCollection, sourceProject);

            // Define the security tasks file.
            this.SecurityTasksFile = System.IO.Path.Combine(this.RootPath, sourceCollection, sourceProject, "SecurityTasks.json");

            // Create project definition folder.
            di = new DirectoryInfo(this.Path);
            if (!di.Exists)
                Directory.CreateDirectory(this.Path);

            // Instantiate source project properties.
            this.SourceProject = new ProjectProperties
            {
                // Set source collection and project.
                Collection = sourceCollection,
                Name = sourceProject,
                ProcessTemplateTypeName = sourceProjectProcessName,
                // Set access token.
                AccessToken = accessToken
            };

            // Description, process template type must be found.

            // Define location of templates path.
            this.TemplatesPath = templatesPath;

            // Validate the templates path exists.
            di = new DirectoryInfo(this.TemplatesPath);
            if (!di.Exists)
                throw (new DirectoryNotFoundException($"Please ensure templates path [{di.FullName}] exists"));

            // No security tasks are needed.
            // No security principal cache is needed.
            // No sid cache is needed.

            // Instantiate the dictionary of configurations.
            // but it is not in use yet.
            this.AdoRestApiConfigurations = new Dictionary<string, RestAPI.Configuration>();

            // Generate REST api service configurations.                        
            this.AdoRestApiConfigurations.Add("BuildApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.BuildApi));

            this.AdoRestApiConfigurations.Add("CoreApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.CoreApi));

            this.AdoRestApiConfigurations.Add("DistributedTaskApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.DistributedTaskApi));

            this.AdoRestApiConfigurations.Add("ExtensionMgmtApi",
                    new RestAPI.Configuration(ServiceHost.ExtensionManagementHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.ExtensionMgmtApi));

            this.AdoRestApiConfigurations.Add("GetClassificationNodesApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.GetClassificationNodesApi));

            this.AdoRestApiConfigurations.Add("GetProjectPropertiesApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.GetProjectPropertiesApi));

            this.AdoRestApiConfigurations.Add("GitRepositoriesApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.GitRepositoriesApi));

            this.AdoRestApiConfigurations.Add("GitPullRequestsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.GitPullRequestsApi));

            this.AdoRestApiConfigurations.Add("GraphApi",
                    new RestAPI.Configuration(ServiceHost.GraphHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.GraphApi));

            this.AdoRestApiConfigurations.Add("PolicyConfigurationsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.PolicyConfigurationsApi));

            this.AdoRestApiConfigurations.Add("ProjectsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.ProjectsApi));

            this.AdoRestApiConfigurations.Add("ReleaseApi",
                    new RestAPI.Configuration(ServiceHost.ReleaseHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.ReleaseApi));

            this.AdoRestApiConfigurations.Add("SecurityApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.SecurityApi));

            this.AdoRestApiConfigurations.Add("ServiceEndpointApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.ServiceEndpointApi));

            this.AdoRestApiConfigurations.Add("TaskAgentApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.TaskAgentApi));

            this.AdoRestApiConfigurations.Add("TeamsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.TeamsApi));

            this.AdoRestApiConfigurations.Add("WikiApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.WikiApi));

            this.AdoRestApiConfigurations.Add("WorkBoardsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.WorkBoardsApi));

            this.AdoRestApiConfigurations.Add("WorkItemTrackingApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.WorkItemTrackingApi));

            this.AdoRestApiConfigurations.Add("WorkTeamSettingsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.SourceProject.Collection, this.SourceProject.Name, this.SourceProject.AccessToken, restApiServiceConfig.WorkTeamSettingsApi));

            // Instantiate the Azure DevOps entities.
            this.AdoEntities = new AdoEntityReferenceCollection()
            {
                AgentQueues = new Dictionary<string, int>(),
                Areas = new Dictionary<string, int>(),
                BuildDefinitions = new Dictionary<string, AdoEntityReference.IdBasedReferenceWithPath>(),
                Iterations = new Dictionary<string, int>(),
                PullRequests = new Dictionary<string, string>(),
                Queues = new Dictionary<string, int>(),
                ReleaseDefinitions = new Dictionary<string, AdoEntityReference.IdBasedReferenceWithPath>(),
                Repositories = new Dictionary<string, string>(),
                ServiceEndpoints = new Dictionary<string, string>(),
                TasksById = new Dictionary<string, TaskReference>(),
                TasksByNameVersionSpec = new Dictionary<string, TaskVersionReference>(),
                TaskGroups = new Dictionary<string, string>(),
                Teams = new Dictionary<string, string>(),
                VariableGroups = new Dictionary<string, int>(),
            };
        }

        /// <summary>
        /// This constructor is involved with import operations.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "This simplification would make the code more obscure.")]
        public ProjectDefinition(string rootPath, string sourceCollection, string sourceProject, string destinationCollection, string destinationProject, string projectDescription, string destinationProjectProcessName, string defaultTeamName, string accessToken, RestApiServiceConfig restApiServiceConfig)
        {
            // Initialize.
            DirectoryInfo di;

            // Store root path.
            this.RootPath = rootPath;

            // Validate the root path exists.
            di = new DirectoryInfo(this.RootPath);
            if (!di.Exists)
                throw (new DirectoryNotFoundException($"Please ensure importPath [{di.FullName}] exists"));

            // Define path to definition based on collection and project.
            this.Path = System.IO.Path.Combine(rootPath, sourceCollection, sourceProject);

            // Define the security tasks file.
            this.SecurityTasksFile = System.IO.Path.Combine(this.RootPath, sourceCollection, sourceProject, "SecurityTasks.json");

            // Instantiate destination project properties.
            this.DestinationProject = new ProjectProperties
            {
                // Set source collection and project.
                Collection = destinationCollection,
                Name = destinationProject,
                Description = projectDescription,
                ProcessTemplateTypeName = destinationProjectProcessName,
                // Set access token.
                AccessToken = accessToken
            };

            // Set default team name.
            this.DefaultTeamName = defaultTeamName;

            // Instantiate security tasks.
            this.SecurityTasks = new SecurityTasks();

            // Instantiate security principal cache.
            this.SecurityPrincipalCache = new Dictionary<string, GraphResponse.GraphGroup>();

            // Instantiate sid cache
            this.SidCache = new Dictionary<string, string>();

            // Instantiate the dictionary of configurations.
            this.AdoRestApiConfigurations = new Dictionary<string, RestAPI.Configuration>();

            // Generate REST api service configurations.                        
            this.AdoRestApiConfigurations.Add("BuildApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.BuildApi));

            this.AdoRestApiConfigurations.Add("CoreApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.CoreApi));

            this.AdoRestApiConfigurations.Add("DistributedTaskApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.DistributedTaskApi));

            this.AdoRestApiConfigurations.Add("ExtensionMgmtApi",
                    new RestAPI.Configuration(ServiceHost.ExtensionManagementHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.ExtensionMgmtApi));

            this.AdoRestApiConfigurations.Add("GetClassificationNodesApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.GetClassificationNodesApi));

            this.AdoRestApiConfigurations.Add("GetProjectPropertiesApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.GetProjectPropertiesApi));

            this.AdoRestApiConfigurations.Add("GitImportRequestsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.GitImportRequestsApi));

            this.AdoRestApiConfigurations.Add("GitRepositoriesApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.GitRepositoriesApi));

            this.AdoRestApiConfigurations.Add("GitPullRequestsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.GitPullRequestsApi));

            this.AdoRestApiConfigurations.Add("GraphApi",
                    new RestAPI.Configuration(ServiceHost.GraphHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.GraphApi));

            this.AdoRestApiConfigurations.Add("PolicyConfigurationsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.PolicyConfigurationsApi));

            this.AdoRestApiConfigurations.Add("ProjectsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.ProjectsApi));

            this.AdoRestApiConfigurations.Add("ReleaseApi",
                    new RestAPI.Configuration(ServiceHost.ReleaseHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.ReleaseApi));

            this.AdoRestApiConfigurations.Add("SecurityApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.SecurityApi));

            this.AdoRestApiConfigurations.Add("ServiceEndpointApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.ServiceEndpointApi));

            this.AdoRestApiConfigurations.Add("TaskAgentApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.TaskAgentApi));

            this.AdoRestApiConfigurations.Add("TeamsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.TeamsApi));

            this.AdoRestApiConfigurations.Add("WikiApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.WikiApi));

            this.AdoRestApiConfigurations.Add("WorkApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.WorkApi));

            this.AdoRestApiConfigurations.Add("WorkBoardsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.WorkBoardsApi));

            this.AdoRestApiConfigurations.Add("WorkItemTrackingApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.WorkItemTrackingApi));

            this.AdoRestApiConfigurations.Add("WorkTeamSettingsApi",
                    new RestAPI.Configuration(ServiceHost.DefaultHost, this.DestinationProject.Collection, this.DestinationProject.Name, this.DestinationProject.AccessToken, restApiServiceConfig.WorkTeamSettingsApi));

            // Instantiate the Azure DevOps entities.
            this.AdoEntities = new AdoEntityReferenceCollection()
            {
                AgentQueues = new Dictionary<string, int>(),
                Areas = new Dictionary<string, int>(),
                BuildDefinitions = new Dictionary<string, AdoEntityReference.IdBasedReferenceWithPath>(),
                Iterations = new Dictionary<string, int>(),
                PullRequests = new Dictionary<string, string>(),
                Queues = new Dictionary<string, int>(),
                ReleaseDefinitions = new Dictionary<string, AdoEntityReference.IdBasedReferenceWithPath>(),
                Repositories = new Dictionary<string, string>(),
                ServiceEndpoints = new Dictionary<string, string>(),
                TasksById = new Dictionary<string, TaskReference>(),
                TasksByNameVersionSpec = new Dictionary<string, TaskVersionReference>(),
                TaskGroups = new Dictionary<string, string>(),
                Teams = new Dictionary<string, string>(),
                VariableGroups = new Dictionary<string, int>(),
            };
        }

        #endregion

        #region - Methods.

        /// <summary>
        /// This method is involved in both export and import operations.
        /// </summary>
        public void AddMappingRecord(string mappingType, string oldEntityKey, string oldEntityName)
        {
            AddMappingRecord(mappingType, oldEntityKey, oldEntityName, null, null);
        }

        /// <summary>
        /// This method is involved in both export and import operations.
        /// </summary>
        public void AddMappingRecord(string mappingType, string oldEntityKey, string oldEntityName, string newEntityKey, string newEntityName)
        {
            // Initialize.
            List<MappingRecord> listOfMappings;
            MappingRecord mappingRecord;

            // Load all mappings from file.
            listOfMappings = LoadMappingsFromFile();

            // Validate if it exists already by checking both old and new entity keys.
            mappingRecord = listOfMappings.Find(x => x.Type == mappingType && x.OldEntityKey == oldEntityKey && x.NewEntityKey == newEntityKey);

            // Raise an exception if the record already exists.
            if (mappingRecord != null)
                throw (new Exception($"Record with Type: {mappingType}, OldEntityKey: {oldEntityKey} and NewEntityKey: {newEntityKey} already exists."));

            // Create a new mapping record.
            mappingRecord = new MappingRecord()
            {
                Type = mappingType,
                OldEntityKey = oldEntityKey,
                NewEntityKey = newEntityKey,
                OldEntityName = oldEntityName,
                NewEntityName = newEntityName
            };

            // Add to list.
            listOfMappings.Add(mappingRecord);

            // Save back to file.
            WriteMappingsToFile(listOfMappings);
        }

        /// <summary>
        /// This method is involved in both export and import operations.
        /// </summary>
        public void CleanWorkFolder()
        {
            // Clean work folder.
            DirectoryInfo di = new DirectoryInfo(this.GetFolderFor("Work"));
            if (di.Exists)
                di.Delete(true);
        }

        public MappingRecord FindMappingRecord(Predicate<MappingRecord> match)
        {
            // Initialize.
            ProjectDefinition.MappingRecord mappingRecord;
            List<ProjectDefinition.MappingRecord> mappings;

            // Get mappings list.
            mappings = this.GetMappings();

            // Find with...
            mappingRecord = mappings.Find(match);

            // Return the mapping record.
            return mappingRecord;
        }

        public List<MappingRecord> FindMappingRecords(Predicate<MappingRecord> match)
        {
            // Initialize.
            List<ProjectDefinition.MappingRecord> mappings;

            // Get mappings list.
            mappings = this.GetMappings();

            // Find with...
            // Return the mapping records.
            return mappings.FindAll(match);
        }

        /// <summary>
        /// This method is involved with import operations.
        /// </summary>
        public string ResolveShortPrincipalName(string name)
        {
            // Initialize.
            string memberName;
            string principalName;

            if (name == "Administrators")
                memberName = "Project Administrators";
            else if (name == "Contributors")
                memberName = "Contributors";
            else if (name == "EndpointCreators")
                memberName = "Endpoint Creators";
            else if (name == "ValidUsers")
                memberName = "Project Valid Users";
            else if (name == "BuildAdministrators")
                memberName = "Build Administrators";
            else if (name == "DefaultTeam")
                memberName = DefaultTeamName;
            else if (name == "Readers")
                memberName = "Readers";
            else
                throw (new Exception("Undefined short name"));

            // Form the principal name and return it.            
            principalName = string.Format(@"[{0}]\{1}", DestinationProject.Name, memberName);
            return principalName;
        }

        /// <summary>
        /// This method is involved in both export and import operations.
        /// </summary>
        public string GetFileFor(string name)
        {
            return (this.GetFileFor(name, new object[] { }));
        }

        /// <summary>
        /// This method is involved in both export and import operations.
        /// </summary>
        public string GetFileFor(string name, object[] args)
        {
            // Initialize.
            string filename;
            string subFolderName;
            string name2ndLevel;
            string identifier;
            string identifierHash;
            string fqfn;

            if (name == "Areas")
                fqfn = System.IO.Path.Combine(this.Path, "Areas.json");
            else if (name == "AreasInitialization")
            {
                subFolderName = args[0].ToString();
                fqfn = System.IO.Path.Combine(this.GetFolderFor("ProjectInitializationAreas"), subFolderName, "AreasInitialization.json");
            }
            else if (name == "BoardColumns")
            {
                subFolderName = args[0].ToString();
                fqfn = System.IO.Path.Combine(this.GetFolderFor("TeamConfigurations"), subFolderName, "BoardColumns.json");
            }
            else if (name == "BoardRows")
            {
                subFolderName = args[0].ToString();
                fqfn = System.IO.Path.Combine(this.GetFolderFor("TeamConfigurations"), subFolderName, "BoardRows.json");
            }
            else if (name == "BuildDefinition")
            {
                identifier = args[0].ToString();
                filename = string.Format("BuildDef{0}.json", identifier);
                fqfn = System.IO.Path.Combine(this.GetFolderFor("BuildDefinitions"), filename);
            }
            else if (name == "CardFields")
            {
                subFolderName = args[0].ToString();
                fqfn = System.IO.Path.Combine(this.GetFolderFor("TeamConfigurations"), subFolderName, "CardFields.json");
            }
            else if (name == "CardStyles")
            {
                subFolderName = args[0].ToString();
                fqfn = System.IO.Path.Combine(this.GetFolderFor("TeamConfigurations"), subFolderName, "CardStyles.json");
            }
            else if (name == "CreateDestinationProject")
                fqfn = System.IO.Path.Combine(this.Path, "CreateDestinationProject.json");
            else if (name == "Extensions")
                fqfn = System.IO.Path.Combine(this.Path, "Extensions.json");
            else if (name == "GitEndpoint")
            {
                identifier = args[0].ToString();
                identifierHash = args[1].ToString();
                filename = string.Format("Git-{0}-{1}-EndPoint.json", identifier, identifierHash);
                fqfn = System.IO.Path.Combine(this.Path, filename);
            }
            else if (name == "GitHubEndpoint")
            {
                identifier = args[0].ToString();
                filename = string.Format("GitHub-{0}-EndPoint.json", identifier);
                fqfn = System.IO.Path.Combine(this.Path, filename);
            }
            else if (name == "ServiceEndpointForCodeImport")
            {
                identifier = args[0].ToString();
                filename = string.Format("{0}-code.json", identifier);
                fqfn = System.IO.Path.Combine(this.GetFolderFor("ImportRepositories"), filename);
            }
            else if (name == "InstalledExtensions")
                fqfn = System.IO.Path.Combine(this.Path, "InstalledExtensions.json");
            else if (name == "Iterations")
                fqfn = System.IO.Path.Combine(this.Path, "Iterations.json");
            else if (name == "IterationsInitialization")
            {
                subFolderName = args[0].ToString();
                fqfn = System.IO.Path.Combine(this.GetFolderFor("ProjectInitializationIterations"), subFolderName, "IterationsInitialization.json");
            }
            else if (name == "Mappings")
                fqfn = System.IO.Path.Combine(this.Path, "Mappings.json");
            else if (name == "Policies")
                fqfn = System.IO.Path.Combine(this.Path, "PolicyConfigurations.json");
            else if (name == "Processes")
                fqfn = System.IO.Path.Combine(this.Path, "Processes.json");
            else if (name == "ProjectInfo.Source")
                fqfn = System.IO.Path.Combine(this.Path, "ProjectInfo.Source.json");
            else if (name == "ProjectInfo.Destination")
                fqfn = System.IO.Path.Combine(this.Path, "ProjectInfo.Destination.json");
            else if (name == "PullRequest")
            {
                subFolderName = args[0].ToString();
                identifier = args[1].ToString();
                filename = string.Format("PullRequest{0}.json", identifier);
                fqfn = System.IO.Path.Combine(this.GetFolderFor("PullRequests"), subFolderName, filename);
            }
            else if (name == "PullRequestThreads")
            {
                subFolderName = args[0].ToString();
                identifier = args[1].ToString();
                filename = string.Format("ThreadsOfPullRequest{0}.json", identifier);
                fqfn = System.IO.Path.Combine(this.GetFolderFor("PullRequests"), subFolderName, filename);
            }
            else if (name == "ReleaseDefinition")
            {
                identifier = args[0].ToString();
                filename = string.Format("ReleaseDef{0}.json", identifier);
                fqfn = System.IO.Path.Combine(this.GetFolderFor("ReleaseDefinitions"), filename);
            }
            else if (name == "Repository")
            {
                identifier = args[0].ToString();
                filename = string.Format("{0}-repository.json", identifier);
                fqfn = System.IO.Path.Combine(this.GetFolderFor("ImportRepositories"), filename);
            }
            else if (name == "ServiceEndpoints")
            {
                fqfn = System.IO.Path.Combine(this.Path, "ServiceEndpoints.json");
            }
            else if (name == "TaskGroup")
            {
                identifier = args[0].ToString();
                filename = string.Format("TaskGroup{0}.json", identifier);
                fqfn = System.IO.Path.Combine(this.GetFolderFor("TaskGroups"), filename);
            }
            else if (name == "Teams")
                fqfn = System.IO.Path.Combine(this.Path, "Teams.json");
            else if (name == "TeamsInitialization")
                fqfn = System.IO.Path.Combine(this.GetFolderFor("ProjectInitializationTeams"), "Teams.json");
            else if (name == "TeamAreas")
            {
                subFolderName = args[0].ToString();
                fqfn = System.IO.Path.Combine(this.GetFolderFor("TeamConfigurations"), subFolderName, "TeamAreas.json");
            }
            else if (name == "TeamIterations")
            {
                subFolderName = args[0].ToString();
                fqfn = System.IO.Path.Combine(this.GetFolderFor("TeamConfigurations"), subFolderName, "TeamIterations.json");
            }
            else if (name == "TeamSettings")
            {
                subFolderName = args[0].ToString();
                fqfn = System.IO.Path.Combine(this.GetFolderFor("TeamConfigurations"), subFolderName, "TeamSettings.json");
            }
            else if (name == "VariableGroup")
            {
                identifier = args[0].ToString();
                filename = string.Format("VariableGroup{0}.json", identifier);
                fqfn = System.IO.Path.Combine(this.GetFolderFor("VariableGroups"), filename);
            }
            else if (name == "Wikis")
                fqfn = System.IO.Path.Combine(this.Path, "Wikis.json");
            else
            {
                // It can be working *.json document.
                if (name.StartsWith("Work."))
                {
                    // Extract second level name.
                    name2ndLevel = name.Replace("Work.", null);

                    if (args.Length == 0)
                        filename = string.Format("{0}.json", name2ndLevel);
                    else
                    {
                        identifier = args[0].ToString();
                        filename = string.Format("{0}{1}.json", name2ndLevel, identifier);
                    }
                    fqfn = System.IO.Path.Combine(this.GetFolderFor("Work", true), filename);
                }
                else
                {
                    // Track any misses in the logic of generating the definition path + file.
                    string errorMsg = string.Format("Undefined logic to get definition file defined for {0}", name);
                    throw (new Exception(errorMsg));
                }
            }

            // Return file + path;
            return fqfn;
        }

        /// <summary>
        /// This method is involved in both export and import operations.
        /// </summary>
        public string GetFolderFor(string name)
        {
            return (this.GetFolderFor(name, new object[] { }, false));
        }

        /// <summary>
        /// This method is involved in import operations.
        /// </summary>
        public string GetFolderFor(string name, bool CreateFolder)
        {
            return (this.GetFolderFor(name, new object[] { }, CreateFolder));
        }

        /// <summary>
        /// This method is involved in import operations.
        /// </summary>
        public string GetFolderFor(string name, object[] args)
        {
            return (this.GetFolderFor(name, args, false));
        }

        /// <summary>
        /// This method is involved in import operations.
        /// </summary>
        public string GetFolderFor(string name, object[] args, bool CreateFolder)
        {
            // Initialize.
            string subFolderName;
            string path;

            if (name == "BuildDefinitions")
                path = System.IO.Path.Combine(this.Path, "BuildDefinitions");
            else if (name == "ImportRepositories")
                path = System.IO.Path.Combine(this.Path, "Repositories");
            else if (name == "ProjectInitialization")
                path = System.IO.Path.Combine(this.Path, "ProjectInitialization");
            else if (name == "ProjectInitializationAreas")
                path = System.IO.Path.Combine(this.Path, "ProjectInitialization", "Areas");
            else if (name == "ProjectInitializationIterations")
                path = System.IO.Path.Combine(this.Path, "ProjectInitialization", "Iterations");
            else if (name == "ProjectInitializationTeams")
                path = System.IO.Path.Combine(this.Path, "ProjectInitialization", "Teams");
            else if (name == "PullRequests")
                path = System.IO.Path.Combine(this.Path, "PullRequests");
            else if (name == "RepositoryPullRequests")
            {
                subFolderName = args[0].ToString();
                path = System.IO.Path.Combine(this.Path, "PullRequests", subFolderName);
            }
            else if (name == "ReleaseDefinitions")
                path = System.IO.Path.Combine(this.Path, "ReleaseDefinitions");
            else if (name == "SpecificAreasInitialization")
            {
                subFolderName = args[0].ToString();
                path = System.IO.Path.Combine(this.Path, "ProjectInitialization", "Areas", subFolderName);
            }
            else if (name == "SpecificIterationsInitialization")
            {
                subFolderName = args[0].ToString();
                path = System.IO.Path.Combine(this.Path, "ProjectInitialization", "Iterations", subFolderName);
            }
            else if (name == "SpecificTeamConfiguration")
            {
                subFolderName = args[0].ToString();
                path = System.IO.Path.Combine(this.Path, "Teams", subFolderName);
            }
            else if (name == "SpecificTeamConfigurationInitialization")
            {
                subFolderName = args[0].ToString();
                path = System.IO.Path.Combine(this.Path, "ProjectInitialization", "Teams", subFolderName);
            }
            else if (name == "TeamConfigurations")
                path = System.IO.Path.Combine(this.Path, "Teams");
            else if (name == "TeamConfigurationsInitialization")
                path = System.IO.Path.Combine(this.Path, "ProjectInitialization", "Teams");
            else if (name == "VariableGroups")
                path = System.IO.Path.Combine(this.Path, "VariableGroups");
            else if (name == "TaskGroups")
                path = System.IO.Path.Combine(this.Path, "TaskGroups");
            else if (name == "Work")
                path = System.IO.Path.Combine(this.Path, "Work");
            else
            {
                // Track any misses in the logic of generating the folder path.
                string errorMsg = string.Format("Undefined logic to get a folder defined for {0}", name);
                throw (new Exception(errorMsg));
            }

            // Create directory if it does not exist.
            if (CreateFolder)
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

            // Return path.
            return path;
        }

        /// <summary>
        /// This method is involved in import operations.
        /// </summary>
        public List<MappingRecord> GetMappings()
        {
            // Load all mappings from file.
            List<MappingRecord> mappings = LoadMappingsFromFile();

            // Return mappings.
            return mappings;
        }

        /// <summary>
        /// This method is involved with import operations.
        /// </summary>
        public void LoadDefinition(string name)
        {
            // Initialize.
            bool hasMultipleDefinitionFiles = false;
            bool isMultiObjectsDefinitionFile = false;
            bool useGenericLoader = false;
            string importPath;
            string importFile = null;
            string path;
            string[] files;
            DirectoryInfo di;
            FileInfo fi;
            List<SingleObjectDefinitionFile> listOfSingleObjectDefinitionFiles = new List<SingleObjectDefinitionFile>();
            MultiObjectsDefinitionFile modf = null;

            // Use a custom comparer to sort correctly filenames ending with a number.
            FilenameEndsWithNumberComparer customComparer = new FilenameEndsWithNumberComparer();

            if (name.ToLower() == "areas")
            {
                useGenericLoader = true;
                isMultiObjectsDefinitionFile = true;
                importFile = this.GetFileFor("Areas");
            }
            else if (name.ToLower() == "areasinitialization")
            {
                // Initialize.
                string portfolioName;
                List<string> importFolders;
                MultiObjectsDefinitionFile aid;

                // Initialize list of definitions.
                this.AreaInitializationDefinitions = new List<MultiObjectsDefinitionFile>();

                // What is the import path.
                importPath = this.GetFolderFor("ProjectInitializationAreas");

                // Get all definition files from path.
                if (Directory.Exists(importPath))
                {
                    // Obtain the list of all folders inside this folder. Each one corresponds to a different team.
                    importFolders = Directory.GetDirectories(importPath).ToList();

                    // Retrieve all team configuration folders.
                    foreach (string portfolioFolder in importFolders)
                    {
                        // Get information about the folder.
                        di = new DirectoryInfo(portfolioFolder);

                        // The team name is the same as the folder name.
                        portfolioName = di.Name;

                        // Create new configuration definition files for this team.
                        aid = new MultiObjectsDefinitionFile();

                        importFile = this.GetFileFor("AreasInitialization", new object[] { portfolioName });

                        // What is the path to store specific portfolio Project Initialization Iterations.
                        aid.AddDefinition(importFile);

                        // Add to list of team configuration definition files.
                        this.AreaInitializationDefinitions.Add(aid);
                    }
                }
            }
            else if (name.ToLower() == "builddefinitions")
            {
                useGenericLoader = true;
                hasMultipleDefinitionFiles = true;
            }
            else if (name.ToLower() == "extensions")
            {
                useGenericLoader = true;
                isMultiObjectsDefinitionFile = true;
                importFile = this.GetFileFor("InstalledExtensions");
            }
            else if (name.ToLower() == "iterations")
            {
                useGenericLoader = true;
                isMultiObjectsDefinitionFile = true;
                importFile = this.GetFileFor("Iterations");
            }
            else if (name.ToLower() == "iterationsinitialization")
            {
                // Initialize.
                string portfolioName;
                List<string> importFolders;
                MultiObjectsDefinitionFile iid;

                // Initialize list of definitions.
                this.IterationInitializationDefinitions = new List<MultiObjectsDefinitionFile>();

                // What is the import path.
                importPath = this.GetFolderFor("ProjectInitializationIterations");

                // Get all definition files from path.
                if (Directory.Exists(importPath))
                {
                    // Obtain the list of all folders inside this folder. Each one corresponds to a different team.
                    importFolders = Directory.GetDirectories(importPath).ToList();

                    // Retrieve all team configuration folders.
                    foreach (string portfolioFolder in importFolders)
                    {
                        // Get information about the folder.
                        di = new DirectoryInfo(portfolioFolder);

                        // The team name is the same as the folder name.
                        portfolioName = di.Name;

                        // Create new configuration definition files for this team.
                        iid = new MultiObjectsDefinitionFile();

                        importFile = this.GetFileFor("IterationsInitialization", new object[] { portfolioName });

                        // What is the path to store specific portfolio Project Initialization Iterations.
                        iid.AddDefinition(importFile);

                        // Add to list of team configuration definition files.
                        this.IterationInitializationDefinitions.Add(iid);
                    }
                }
            }
            else if (name.ToLower() == "pullrequests")
            {
                // Initialize.
                string repositoryPullRequestPath;
                PullRequestDefinitionFiles prdf;

                // Initialize list of definitions.
                this.PullRequestDefinitions = new List<PullRequestDefinitionFiles>();

                // Retrieve import part for...
                importPath = this.GetFolderFor(name);

                // Get all definition files from path.
                if (Directory.Exists(importPath))
                {
                    // Get the list of definition files.
                    files = Directory.GetFiles(importPath, "PullRequest*.json", SearchOption.AllDirectories);

                    // Sort files to make sure they are processed in the right order.
                    Array.Sort(files, customComparer);

                    foreach (string file in files)
                    {
                        // Instantiate the pull request definition files.
                        prdf = new PullRequestDefinitionFiles();

                        // Get file details.
                        fi = new FileInfo(file);

                        // Instantiate the pull request definition files object.
                        prdf = new PullRequestDefinitionFiles();

                        // Get the repository pull request path.
                        repositoryPullRequestPath = System.IO.Path.GetDirectoryName(file);

                        // Add the pull request defintion file.
                        prdf.PullRequest = new SingleObjectDefinitionFile();
                        prdf.PullRequest.AddDefinition(file);

                        // Verify if a pull request threads file exists.
                        importFile = System.IO.Path.Combine(repositoryPullRequestPath, $"ThreadsOf{fi.Name}");
                        if (File.Exists(importFile))
                        {
                            // Add the pull request threads defintion file.
                            prdf.PullRequestThreads = new MultiObjectsDefinitionFile();
                            prdf.PullRequestThreads.AddDefinition(importFile);
                        }

                        // Add to list of pull request definitions.
                        this.PullRequestDefinitions.Add(prdf);
                    }
                }
            }
            else if (name.ToLower() == "releasedefinitions")
            {
                useGenericLoader = true;
                hasMultipleDefinitionFiles = true;
            }
            else if (name.ToLower() == "repositories")
            {
                // Initialize list of definitions.
                this.RepositoryDefinitions = new List<SingleObjectDefinitionFile>();

                // Retrieve import part for...
                importPath = this.GetFolderFor("ImportRepositories");

                // Get all definition files from path.
                if (Directory.Exists(importPath))
                {
                    // Get the list of definition files.
                    files = Directory.GetFiles(importPath, "*-repository.json", SearchOption.AllDirectories);

                    // Sort files to make sure they are processed in the right order.
                    Array.Sort(files, customComparer);

                    foreach (string file in files)
                    {
                        SingleObjectDefinitionFile sodf = new SingleObjectDefinitionFile();
                        sodf.AddDefinition(file);
                        this.RepositoryDefinitions.Add(sodf);
                    }
                }
            }
            else if (name.ToLower() == "serviceendpoints")
            {
                useGenericLoader = true;
                isMultiObjectsDefinitionFile = true;
                importFile = this.GetFileFor("ServiceEndpoints");
            }
            else if (name.ToLower() == "serviceendpointsforcodeimport")
            {
                // Initialize list of definitions.
                this.ServiceEndpointForCodeImportDefinitions = new List<SingleObjectDefinitionFile>();

                // Retrieve import part for...
                importPath = this.GetFolderFor("ImportRepositories");

                // Get all definition files from path.
                if (Directory.Exists(importPath))
                {
                    // Get the list of definition files.
                    files = Directory.GetFiles(importPath, "*-code.json", SearchOption.AllDirectories);

                    // Sort files to make sure they are processed in the right order.
                    Array.Sort(files, customComparer);

                    foreach (string file in files)
                    {
                        SingleObjectDefinitionFile sodf = new SingleObjectDefinitionFile();
                        sodf.AddDefinition(file);
                        this.ServiceEndpointForCodeImportDefinitions.Add(sodf);
                    }
                }
            }
            else if (name.ToLower() == "taskgroups")
            {
                useGenericLoader = true;
                hasMultipleDefinitionFiles = true;
            }
            else if (name.ToLower() == "teamconfigurations")
            {
                // Initialize.
                string teamName;
                List<string> importFolders;
                TeamConfigurationDefinitionFiles tcdf;

                // Initialize list of definitions.
                this.TeamConfigurationDefinitions = new List<TeamConfigurationDefinitionFiles>();

                // What is the import path.
                importPath = this.GetFolderFor("TeamConfigurations");

                // Get all definition files from path.
                if (Directory.Exists(importPath))
                {
                    // Obtain the list of all folders inside this folder. Each one corresponds to a different team.
                    importFolders = Directory.GetDirectories(importPath).ToList();

                    // Retrieve all team configuration folders.
                    foreach (string teamFolder in importFolders)
                    {
                        // Get information about the folder.
                        di = new DirectoryInfo(teamFolder);

                        // The team name is the same as the folder name.
                        teamName = di.Name;

                        // What is the path to store specific team configurations.
                        path = this.GetFolderFor("SpecificTeamConfiguration", new object[] { teamName });

                        // Create new configuration definition files for this team.
                        tcdf = new TeamConfigurationDefinitionFiles { TeamName = teamName };

                        // Set board columns.
                        importFile = System.IO.Path.Combine(path, "BoardColumns.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.BoardColumns = new SingleObjectDefinitionFile();
                            tcdf.BoardColumns.AddDefinition(importFile);
                        }

                        // Set board rows.
                        importFile = System.IO.Path.Combine(path, "BoardRows.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.BoardRows = new SingleObjectDefinitionFile();
                            tcdf.BoardRows.AddDefinition(importFile);
                        }

                        // Set card fields.
                        importFile = System.IO.Path.Combine(path, "CardFields.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.CardFields = new SingleObjectDefinitionFile();
                            tcdf.CardFields.AddDefinition(importFile);
                        }

                        // Set card styles.
                        importFile = System.IO.Path.Combine(path, "CardStyles.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.CardStyles = new SingleObjectDefinitionFile();
                            tcdf.CardStyles.AddDefinition(importFile);
                        }

                        // Set team areas.
                        importFile = System.IO.Path.Combine(path, "TeamAreas.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.Areas = new SingleObjectDefinitionFile();
                            tcdf.Areas.AddDefinition(importFile);
                        }

                        // Set team iterations.
                        importFile = System.IO.Path.Combine(path, "TeamIterations.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.Iterations = new SingleObjectDefinitionFile();
                            tcdf.Iterations.AddDefinition(importFile);
                        }

                        // Set team settings.
                        importFile = System.IO.Path.Combine(path, "TeamSettings.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.Settings = new SingleObjectDefinitionFile();
                            tcdf.Settings.AddDefinition(importFile);
                        }

                        // Add to list of team configuration definition files.
                        this.TeamConfigurationDefinitions.Add(tcdf);
                    }
                }
            }
            else if (name.ToLower() == "teamconfigurationsinitialization")
            {
                // Initialize.
                string teamName;
                List<string> importFolders;
                TeamConfigurationDefinitionFiles tcdf;

                // Initialize list of definitions.
                this.TeamConfigurationInitializationDefinitions = new List<TeamConfigurationDefinitionFiles>();

                // What is the import path.
                importPath = this.GetFolderFor("TeamConfigurationsInitialization");

                // Get all definition files from path.
                if (Directory.Exists(importPath))
                {
                    // Obtain the list of all folders inside this folder. Each one corresponds to a different team.
                    importFolders = Directory.GetDirectories(importPath).ToList();

                    // Retrieve all team configuration folders.
                    foreach (string teamFolder in importFolders)
                    {
                        // Get information about the folder.
                        di = new DirectoryInfo(teamFolder);

                        // The team name is the same as the folder name.
                        teamName = di.Name;

                        // What is the path to store specific team configurations.
                        path = this.GetFolderFor("SpecificTeamConfigurationInitialization", new object[] { teamName });

                        // Create new configuration definition files for this team.
                        tcdf = new TeamConfigurationDefinitionFiles { TeamName = teamName };

                        // Set board columns.
                        importFile = System.IO.Path.Combine(path, "BoardColumns.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.BoardColumns = new SingleObjectDefinitionFile();
                            tcdf.BoardColumns.AddDefinition(importFile);
                        }

                        // Set board rows.
                        importFile = System.IO.Path.Combine(path, "BoardRows.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.BoardRows = new SingleObjectDefinitionFile();
                            tcdf.BoardRows.AddDefinition(importFile);
                        }

                        // Set card fields.
                        importFile = System.IO.Path.Combine(path, "CardFields.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.CardFields = new SingleObjectDefinitionFile();
                            tcdf.CardFields.AddDefinition(importFile);
                        }

                        // Set card styles.
                        importFile = System.IO.Path.Combine(path, "CardStyles.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.CardStyles = new SingleObjectDefinitionFile();
                            tcdf.CardStyles.AddDefinition(importFile);
                        }

                        // Set team areas.
                        importFile = System.IO.Path.Combine(path, "TeamAreas.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.Areas = new SingleObjectDefinitionFile();
                            tcdf.Areas.AddDefinition(importFile);
                        }

                        // Set team iterations.
                        importFile = System.IO.Path.Combine(path, "TeamIterations.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.Iterations = new SingleObjectDefinitionFile();
                            tcdf.Iterations.AddDefinition(importFile);
                        }

                        // Set team settings.
                        importFile = System.IO.Path.Combine(path, "TeamSettings.json");
                        if (File.Exists(importFile))
                        {
                            tcdf.Settings = new SingleObjectDefinitionFile();
                            tcdf.Settings.AddDefinition(importFile);
                        }

                        // Add to list of team configuration definition files.
                        this.TeamConfigurationInitializationDefinitions.Add(tcdf);
                    }
                }
            }
            else if (name.ToLower() == "teams")
            {
                useGenericLoader = true;
                isMultiObjectsDefinitionFile = true;
                importFile = this.GetFileFor("Teams");
            }
            else if (name.ToLower() == "teamsinitialization")
            {
                useGenericLoader = true;
                isMultiObjectsDefinitionFile = true;
                importFile = this.GetFileFor("TeamsInitialization");
            }
            else if (name.ToLower() == "variablegroups")
            {
                useGenericLoader = true;
                hasMultipleDefinitionFiles = true;
            }
            else if (name.ToLower() == "wikis")
            {
                useGenericLoader = true;
                isMultiObjectsDefinitionFile = true;
                importFile = this.GetFileFor("Wikis");
            }

            #region - Generic loader.

            if (useGenericLoader)
            {
                if (hasMultipleDefinitionFiles)
                {
                    // Retrieve import part for...
                    importPath = this.GetFolderFor(name);

                    // Has it been found?
                    if (importPath == null)
                        throw (new Exception($"Cannot retrieve folder for {name}"));

                    // Get all definition files from path.
                    if (Directory.Exists(importPath))
                    {
                        // Get the list of definition files.
                        files = Directory.GetFiles(importPath, "*.json", SearchOption.AllDirectories);

                        // Sort files to make sure they are processed in the right order.
                        Array.Sort(files, customComparer);

                        foreach (string file in files)
                        {
                            SingleObjectDefinitionFile sodf = new SingleObjectDefinitionFile();
                            sodf.AddDefinition(file);
                            listOfSingleObjectDefinitionFiles.Add(sodf);
                        }
                    }
                }
                else
                {
                    if (isMultiObjectsDefinitionFile)
                    {
                        // Instantiate.
                        modf = new MultiObjectsDefinitionFile();

                        if (File.Exists(importFile))
                            modf.AddDefinition(importFile);
                        else
                            throw (new Exception($"{importFile} does not exist and cannot be loaded"));
                    }
                    else
                    {
                        if (File.Exists(importFile))
                        {
                            SingleObjectDefinitionFile sodf = new SingleObjectDefinitionFile();
                            sodf.AddDefinition(importFile);
                        }
                        else
                            throw (new Exception($"{importFile} does not exist and cannot be loaded"));
                    }
                }

                // Store definitions.
                if (name.ToLower() == "areas")
                    this.AreaDefinitions = modf;
                else if (name.ToLower() == "builddefinitions")
                    this.BuildDefinitions = listOfSingleObjectDefinitionFiles;
                else if (name.ToLower() == "extensions")
                    this.ExtensionDefinitions = modf;
                else if (name.ToLower() == "iterations")
                    this.IterationDefinitions = modf;
                else if (name.ToLower() == "releasedefinitions")
                    this.ReleaseDefinitions = listOfSingleObjectDefinitionFiles;
                else if (name.ToLower() == "serviceendpoints")
                    this.ServiceEndpointDefinitions = modf;
                else if (name.ToLower() == "taskgroups")
                    this.TaskGroupDefinitions = listOfSingleObjectDefinitionFiles;
                else if (name.ToLower() == "teams")
                    this.TeamDefinitions = modf;
                else if (name.ToLower() == "teamsinitialization")
                    this.TeamInitializationDefinitions = modf;
                else if (name.ToLower() == "variablegroups")
                    this.VariableGroupDefinitions = listOfSingleObjectDefinitionFiles;
                else if (name.ToLower() == "wikis")
                    this.WikiDefinitions = modf;
            }

            #endregion
        }

        /// <summary>
        /// This method is involved with export operations.
        /// </summary>
        public string ReadTemplate(string name)
        {
            // Initialize an empty file content.
            string fileContent = null;
            string definitionFQFN;

            // Which one is it?
            if (name == "CreateProject")
                definitionFQFN = System.IO.Path.Combine(TemplatesPath, "CreateProject.json");
            else if (name == "GitServiceEndpoint")
                definitionFQFN = System.IO.Path.Combine(TemplatesPath, "GitServiceEndpoint.json");
            else if (name == "ImportSourceCode")
                definitionFQFN = System.IO.Path.Combine(TemplatesPath, "ImportSourceCode.json");
            else if (name == "RedirectedTaskGroup")
                definitionFQFN = System.IO.Path.Combine(TemplatesPath, "RedirectedTaskGroup.json");
            else
            {
                // Track any misses in the logic of reading a template file.
                string errorMsg = string.Format("Undefined logic to read template for {0}", name);
                throw (new Exception(errorMsg));
            }

            using (FileStream fs = new FileStream(definitionFQFN, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader sr = new StreamReader(fs))
                fileContent = sr.ReadToEnd();

            // Return the file content read.
            return fileContent;
        }

        /// <summary>
        /// This method is involved in import operations.
        /// </summary>
        public string ReadDefinitionFile(string path)
        {
            // Initialize an empty file content.
            string fileContent = null;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader sr = new StreamReader(fs))
                fileContent = sr.ReadToEnd();

            // Return the file content read.
            return fileContent;
        }

        /// <summary>
        /// This method is involved in export operations.
        /// </summary>
        public void ResetMappings()
        {
            // Initialize.
            List<MappingRecord> mappings = null;

            // Write resetted mappings.
            WriteMappingsToFile(mappings);
        }

        /// <summary>
        /// This method is involved with import operations.
        /// </summary>
        public void UpdateMappingRecordByKey(string mappingType, string oldEntityKey, string newEntityKey, string newEntityName = null)
        {
            // If the name does not change, reuse it.
            if (string.IsNullOrEmpty(newEntityName))
                UpdateMappingRecordInternal(mappingType, "Key", oldEntityKey, null, newEntityKey, null);
            else
                UpdateMappingRecordInternal(mappingType, "Key", oldEntityKey, null, newEntityKey, newEntityName);
        }

        /// <summary>
        /// This method is involved with import operations.
        /// </summary>
        public void UpdateMappingRecordByName(string mappingType, string oldEntityName, string newEntityKey, string newEntityName = null)
        {
            // If the name does not change, reuse it.
            if (string.IsNullOrEmpty(newEntityName))
                UpdateMappingRecordInternal(mappingType, "Name", null, oldEntityName, newEntityKey, oldEntityName);
            else
                UpdateMappingRecordInternal(mappingType, "Name", null, oldEntityName, newEntityKey, newEntityName);
        }

        /// <summary>
        /// This method is involved with export operations.
        /// </summary>
        public void WriteDefinitionFile(string path, string jsonContent)
        {
            // If not empty, write the content into the file.
            if (!string.IsNullOrEmpty(jsonContent))
            {
                // Write JSON file.
                File.WriteAllText(path, jsonContent);
            }
        }

        /// <summary>
        /// This method is involved with export operations.
        /// </summary>
        public void WriteDefinitionFile(string path, object value)
        {
            this.WriteDefinitionFile(path, value, false);
        }

        /// <summary>
        /// This method is involved with export operations.
        /// </summary>
        public void WriteDefinitionFile(string path, object value, bool ignoreNullValue)
        {
            // Initialize.
            string jsonContent;

            // Serialize object into a JSON string.
            if (ignoreNullValue)
            {
                JsonSerializerSettings jsonSerializerNullSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                jsonContent = JsonConvert.SerializeObject(value, Formatting.Indented, jsonSerializerNullSetting);
            }
            else
                jsonContent = JsonConvert.SerializeObject(value, Formatting.Indented);

            // If not empty write definition file.
            if (!string.IsNullOrEmpty(jsonContent))
                File.WriteAllText(path, jsonContent);
        }

        #endregion

        #endregion

        #region - Nested Classes.        

        public class AdoEntityReferenceCollection
        {
            public Dictionary<string, int> AgentQueues { get; set; }

            public Dictionary<string, int> Areas { get; set; }

            public Dictionary<string, AdoEntityReference.IdBasedReferenceWithPath> BuildDefinitions { get; set; }

            public Dictionary<string, int> Iterations { get; set; }

            public Dictionary<string, string> PullRequests { get; set; }

            public Dictionary<string, int> Queues { get; set; }

            public Dictionary<string, AdoEntityReference.IdBasedReferenceWithPath> ReleaseDefinitions { get; set; }

            public Dictionary<string, string> Repositories { get; set; }

            public Dictionary<string, string> ServiceEndpoints { get; set; }

            public Dictionary<string, string> TaskGroups { get; set; }

            /// <summary>
            /// Dictionary to store all task identifiers and all semantic versions found.
            /// </summary>
            public Dictionary<string, TaskReference> TasksById { get; set; }

            /// <summary>
            /// Dictionary to store all tasks and indexed by task name and version spec.
            /// </summary>
            public Dictionary<string, TaskVersionReference> TasksByNameVersionSpec { get; set; }

            public Dictionary<string, string> Teams { get; set; }

            public Dictionary<string, int> VariableGroups { get; set; }
        }

        public class MappingRecord
        {
            [JsonProperty(PropertyName = "newEntityKey")]
            public string NewEntityKey { get; set; }

            [JsonProperty(PropertyName = "newEntityName")]
            public string NewEntityName { get; set; }

            [JsonProperty(PropertyName = "oldEntityKey")]
            public string OldEntityKey { get; set; }

            [JsonProperty(PropertyName = "oldEntityName")]
            public string OldEntityName { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
        }

        public class ProjectProperties
        {
            public string Collection { get; set; }

            public string Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public ProjectVisibility Visibility { get; set; }

            public string SourceControlType { get; set; }

            public string ProcessTemplateTypeName { get; set; }

            public string ProcessTemplateTypeId { get; set; }

            public bool Validated { get; set; }

            public string AccessToken { get; set; }

            public ProjectState State { get; set; }
        }

        public class TaskVersionReference
        {
            public string TaskId { get; set; }

            public string TaskName { get; set; }

            public string VersionSpec { get; set; }
        }

        public class TaskReference
        {
            public string TaskId { get; set; }

            public string TaskName { get; set; }

            public List<string> Versions { get; set; }

            public List<string> VersionSpecs { get; set; }
        }

        public class AdoEntityReference
        {
            public class GuidBasedReference
            {
                public string Identifier { get; set; }

                public string Name { get; set; }
            }

            public class IdBasedReference
            {
                public int Identifier { get; set; }

                public string Name { get; set; }
            }

            public class IdBasedReferenceWithPath
            {
                public int Identifier { get; set; }

                public string Name { get; set; }

                public string Path { get; set; }

                public string RelativePath
                {
                    get
                    {
                        return (Utility.CombineRelativePath(Path, Name));
                    }
                }
            }
        }

        #endregion
    }
}
