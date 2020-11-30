using ADO.Engine;
using ADO.Engine.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TreeCollections;
using VstsSyncMigrator.Core.BusinessEntities;

namespace ADO.Engine.BusinessEntities
{
    public partial class SimpleMutableBusinessNodeNode
    {
        public static SimpleMutableBusinessNodeNode LoadFromJson(string path)
        {
            string json = File.ReadAllText(path);
            BusinessNodeDataNode dataRoot
                = JsonConvert.DeserializeObject<BusinessNodeDataNode>(json);

            var root = new SimpleMutableBusinessNodeNode(
                new BusinessNodeItem(
                    dataRoot.Id,
                    dataRoot.Name,
                    dataRoot.BusinessNodeType,
                    dataRoot.Team,
                    dataRoot.Disabled,
                    dataRoot.IsClone,
                    dataRoot.IsOnPremiseProject,
                    dataRoot.AzureDevOpsServerFQDN,
                    dataRoot.OrganizationName,
                    dataRoot.TeamPrefix,
                    dataRoot.Process,
                    dataRoot.TeamInclusions,
                    dataRoot.AreaBasePaths,
                    dataRoot.AreaPathMap,
                    dataRoot.IterationBasePaths,
                    dataRoot.IterationPathMap
                    ));
            root.Build(dataRoot, n =>
            new BusinessNodeItem(
                n.Id,
                n.Name,
                n.BusinessNodeType,
                n.Team,
                n.Disabled,
                n.IsClone,
                    n.IsOnPremiseProject,
                    n.AzureDevOpsServerFQDN,
                n.OrganizationName,
                n.TeamPrefix,
                n.Process,
                n.TeamInclusions,
                n.AreaBasePaths,
                n.AreaPathMap,
                n.IterationBasePaths,
                n.IterationPathMap
                ));
            return root;
        }

        public void GenerateExportConfigs(string outputPath,
            string sourcePatToken,
            string templateFilesPath,
            string exportPath,
            string universalMapsProcessMaps,
            Engine.Configuration.ProjectExportBehavior defaultProjectExportBehavior,
            out Dictionary<SimpleMutableBusinessNodeNode,
                Engine.Configuration.ProjectExport.EngineConfiguration> mapTeamProjectToExportConfig
            )
        {
            DirectoryInfo di = new DirectoryInfo(outputPath);
            if (!di.Exists)
                Directory.CreateDirectory(di.FullName);

            mapTeamProjectToExportConfig
                = new Dictionary<SimpleMutableBusinessNodeNode, Engine.Configuration.ProjectExport.EngineConfiguration>();

            foreach (var treeNode in this.Where(tn => tn.Item.BusinessNodeType == BusinessNodeType.TeamProject))
            {
                Engine.Configuration.ProjectExport.EngineConfiguration newEngineConfig
                    = Engine.Configuration.ProjectExport.EngineConfiguration.GetDefault();

                BusinessNodeItem businessNodeItem = treeNode.Item;

                //some mods
                newEngineConfig.SourceCollection = businessNodeItem.OrganizationName;
                newEngineConfig.SourceProject = businessNodeItem.Name;
                newEngineConfig.SourceProjectProcessName = businessNodeItem.Process;
                newEngineConfig.PAT = sourcePatToken;
                newEngineConfig.TemplatesPath = templateFilesPath;
                newEngineConfig.ExportPath = exportPath;
                string rootNode = $"\\{this.Root.Item.Name}\\";
                if (!businessNodeItem.IsClone)
                {
                    newEngineConfig.BuildReleasePrefixPath = treeNode.GetAreaOrIterationPath(Constants.AreaStructureType, false, true)
                        .Substring(rootNode.Length);
                }
                else
                {
                    newEngineConfig.BuildReleasePrefixPath = Constants.DefaultEmptyBuildReleasePrefixPathForClone;
                }
                newEngineConfig.Behaviors = defaultProjectExportBehavior;

                newEngineConfig.ProcessMapLocation = universalMapsProcessMaps;

                //newEngineConfig.Validate(); //this will also load secrets

                mapTeamProjectToExportConfig.Add(treeNode, newEngineConfig);
                string json = JsonConvert.SerializeObject(newEngineConfig, Formatting.Indented);
                string exportConfigFileName = $"{businessNodeItem.Id.ToString().PadLeft(3, '0')}--{businessNodeItem.Name}--ProjectExportConfig.json";
                File.WriteAllText(Path.Combine(outputPath, exportConfigFileName), json);
            }
        }

        public void SaveToJson(string path)
        {
            BusinessNodeDataNode dataRoot = ToBusinessNodeDataNodeRecursive();
            string json = JsonConvert.SerializeObject(dataRoot, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public BusinessNodeDataNode ToBusinessNodeDataNodeRecursive()
        {
            if (this.Children.Count == 0)
            {
                BusinessNodeDataNode newBusinessNodeDataNodeRoot =
                    new BusinessNodeDataNode()
                    {
                        Id = this.Item.Id,
                        BusinessNodeType = this.Item.BusinessNodeType,
                        Name = this.Item.Name,
                        OrganizationName = this.Item.OrganizationName,
                        Team = this.Item.Team,
                        TeamPrefix = this.Item.TeamPrefix,
                        Process = this.Item.Process,
                        //don't forget
                        Disabled = this.Item.Disabled,
                        TeamInclusions = this.Item.TeamInclusions,
                        AreaBasePaths = this.Item.AreaBasePaths,
                        AreaPathMap = this.Item.AreaPathMap,
                        IterationBasePaths = this.Item.IterationBasePaths,
                        IterationPathMap = this.Item.IterationPathMap,
                        //always FORGET!!!
                        IsClone = this.Item.IsClone,
                        IsOnPremiseProject = this.Item.IsOnPremiseProject,
                        AzureDevOpsServerFQDN = this.Item.AzureDevOpsServerFQDN,
                    };
                return newBusinessNodeDataNodeRoot;
            }
            else
            {
                BusinessNodeDataNode newBusinessNodeDataNodeRoot = new BusinessNodeDataNode()
                {
                    Id = this.Item.Id,
                    BusinessNodeType = this.Item.BusinessNodeType,
                    Name = this.Item.Name,
                    OrganizationName = this.Item.OrganizationName,
                    Team = this.Item.Team,
                    TeamPrefix = this.Item.TeamPrefix,
                    Process = this.Item.Process,
                    //don't forget
                    Disabled = this.Item.Disabled,
                    TeamInclusions = this.Item.TeamInclusions,
                    AreaBasePaths = this.Item.AreaBasePaths,
                    AreaPathMap = this.Item.AreaPathMap,
                    IterationBasePaths = this.Item.IterationBasePaths,
                    IterationPathMap = this.Item.IterationPathMap,
                    //always FORGET!!!
                    IsClone = this.Item.IsClone,
                    IsOnPremiseProject = this.Item.IsOnPremiseProject,
                    AzureDevOpsServerFQDN = this.Item.AzureDevOpsServerFQDN,
                    Children = new BusinessNodeDataNode[this.Children.Count]
                };
                int childCount = 0;
                foreach (var child in this.Children)
                {
                    newBusinessNodeDataNodeRoot.Children[childCount++] = child.ToBusinessNodeDataNodeRecursive();
                }
                return newBusinessNodeDataNodeRoot;
            }
        }

        public void GenerateAllScript(
            string inputPath,
            string outputPath,
            string binaryProgram,
            string arguments)
        {
            FileInfo outputfi = new FileInfo(binaryProgram);
            DirectoryInfo di = new DirectoryInfo(outputfi.Directory.FullName);
            if (!di.Exists)
                Directory.CreateDirectory(di.FullName);

            FileInfo fi = new FileInfo(binaryProgram);
            if (!fi.Exists)
                throw new FileNotFoundException(fi.FullName);

            var inputFiles = Directory.EnumerateFiles(inputPath);
            List<string> commandsToExecute = new List<string>();
            commandsToExecute.Add($"cd \"{fi.Directory.FullName}\"");
            foreach (var inputFile in inputFiles)
            {
                FileInfo inputFileInfo = new FileInfo(inputFile);
                var commandToExecute = $"{Path.GetFileNameWithoutExtension(fi.FullName)} {arguments} \"{inputFileInfo.FullName}\"";
                commandsToExecute.Add(commandToExecute);
            }
            FileInfo scriptFile = new FileInfo(outputPath);
            commandsToExecute.Add($"cd \"{scriptFile.Directory.FullName}\"");
            DirectoryInfo di2 = new DirectoryInfo(scriptFile.Directory.FullName);
            if (!di2.Exists)
                Directory.CreateDirectory(di2.FullName);
            File.WriteAllLines(outputPath, commandsToExecute);
        }

        public void GenerateImportConfigs(string outputPath,
            string destinationPatToken,
            string destinationCollection,
            string destinationProject,
            string destinationProjectProcessName,
            string defaultTeamName,
            string description,
            string importSourceCodeCredentialsPassword,
            string importSourceCodeCredentialsGitPassword,
            Engine.Configuration.ProjectImportBehavior defaultProjectImportBehavior,
            string importPath,
            string universalMapsProcessMaps,
            bool createDestinationProjectOnFirstImport,
            bool initializeProjectOnFirstImport,
            string areaInitializationTreePath,
            string iterationInitializationTreePath,
            List<string> teamExclusions,
            //List<string> teamInclusions,
            out SimpleMutableBusinessNodeNode firstProject,
            out Dictionary<SimpleMutableBusinessNodeNode, Engine.Configuration.ProjectImport.EngineConfiguration> mapTeamProjectToImportConfig
            )
        {
            //assign null
            firstProject = null;

            DirectoryInfo di = new DirectoryInfo(outputPath);
            if (!di.Exists)
                Directory.CreateDirectory(di.FullName);

            mapTeamProjectToImportConfig
                = new Dictionary<SimpleMutableBusinessNodeNode, Engine.Configuration.ProjectImport.EngineConfiguration>();

            int firstProjectId = this.Where(tn => tn.Item.BusinessNodeType == BusinessNodeType.TeamProject)
                .Min(prj => prj.Item.Id);

            foreach (var treeNode in this
                .Where(tn => tn.Item.BusinessNodeType == BusinessNodeType.TeamProject)
                .OrderBy(tn => tn.Item.Id))
            {

                Engine.Configuration.ProjectImport.EngineConfiguration newEngineConfig
                    = Engine.Configuration.ProjectImport.EngineConfiguration.GetDefault();

                BusinessNodeItem businessNodeItem = treeNode.Item;

                //some mods
                newEngineConfig.SourceCollection = businessNodeItem.OrganizationName;
                newEngineConfig.SourceProject = businessNodeItem.Name;
                newEngineConfig.PAT = destinationPatToken;
                newEngineConfig.PrefixName = businessNodeItem.TeamPrefix;
                newEngineConfig.PrefixSeparator = Constants.DefaultPrefixSeparator;

                newEngineConfig.TeamExclusions
                    = teamExclusions;
                newEngineConfig.TeamInclusions
                    = businessNodeItem.TeamInclusions;
                newEngineConfig.AreaInclusions
                    = businessNodeItem.AreaBasePaths;
                newEngineConfig.IterationInclusions
                    = businessNodeItem.IterationBasePaths;

                string rootNode = $"{Constants.DefaultPathSeparator}{destinationProject}{Constants.DefaultPathSeparator}";
                //area
                string fullGenericAreaPath = treeNode.GetAreaOrIterationPath(Constants.AreaStructureType, false, true);
                string genericAreaPath = null;
                string fullGenericParentAreaPath = null;
                string genericParentAreaPath = null;
                if (!businessNodeItem.IsClone)
                {
                    genericAreaPath = fullGenericAreaPath.Substring(rootNode.Length);
                    fullGenericParentAreaPath = treeNode.Parent.GetAreaOrIterationPath(Constants.AreaStructureType, false, true);
                    genericParentAreaPath = fullGenericParentAreaPath.Substring(rootNode.Length);
                }
                //iteration
                string fullGenericIterationPath = treeNode.GetAreaOrIterationPath(Constants.IterationStructureType, false, true);
                string genericIterationPath = null;
                string fullGenericParentIterationPath = null;
                string genericParentIterationPath = null;
                if (!businessNodeItem.IsClone)
                {
                    genericIterationPath = fullGenericIterationPath.Substring(rootNode.Length);
                    fullGenericParentIterationPath = treeNode.Parent.GetAreaOrIterationPath(Constants.IterationStructureType, false, true);
                    genericParentIterationPath = fullGenericParentIterationPath.Substring(rootNode.Length);
                }

                newEngineConfig.Behaviors = defaultProjectImportBehavior;
                //if not first project then do not initialize or create project
                if (businessNodeItem.Id != firstProjectId)
                {
                    newEngineConfig.Behaviors.CreateDestinationProject = false;
                    newEngineConfig.Behaviors.InitializeProject = false;
                }
                else
                {
                    firstProject = treeNode;
                    //if first project then MAY OR MAY NOT initialize or create project
                    newEngineConfig.Behaviors.CreateDestinationProject = createDestinationProjectOnFirstImport;
                    newEngineConfig.Behaviors.InitializeProject = initializeProjectOnFirstImport;
                    if (initializeProjectOnFirstImport)
                    {
                        if (areaInitializationTreePath != null && File.Exists(areaInitializationTreePath))
                        {
                            newEngineConfig.AreaInitializationTreePath = new FileInfo(areaInitializationTreePath).FullName;
                        }
                        if (iterationInitializationTreePath != null && File.Exists(iterationInitializationTreePath))
                        {
                            newEngineConfig.IterationInitializationTreePath = new FileInfo(iterationInitializationTreePath).FullName;
                        }
                    }
                }

                newEngineConfig.ImportSourceCodeCredentials.Password = importSourceCodeCredentialsPassword;
                newEngineConfig.ImportSourceCodeCredentials.GitPassword = importSourceCodeCredentialsGitPassword;

                newEngineConfig.DestinationCollection = destinationCollection;
                newEngineConfig.DestinationProject = destinationProject;
                newEngineConfig.DestinationProjectProcessName = destinationProjectProcessName;
                newEngineConfig.DefaultTeamName = defaultTeamName;
                newEngineConfig.Description = description;
                newEngineConfig.AreaPrefixPath = treeNode.Item.Disabled ? string.Empty : genericParentAreaPath;
                newEngineConfig.IterationPrefixPath = treeNode.Item.Disabled ? string.Empty : genericParentIterationPath;

                newEngineConfig.ImportPath = importPath;
                newEngineConfig.SecurityTasksFile
                    = Path.Combine(importPath, businessNodeItem.OrganizationName,
                    businessNodeItem.Name, Constants.SecurityTasksJson);

                newEngineConfig.ProcessMapLocation = universalMapsProcessMaps;

                string queryFolderPath = null;
                if (!businessNodeItem.IsClone)
                {
                    queryFolderPath = $"{Constants.SharedQueries}/{genericAreaPath.Replace(Constants.DefaultPathSeparator, Constants.DefaultPathSeparatorForward)}";
                }
                else
                {
                    queryFolderPath = $"{Constants.SharedQueries}";
                }
                newEngineConfig.QueryFolderPaths = new List<NamespacePathSecuritySpec>()
                {
                    new NamespacePathSecuritySpec()
                    {
                        Path=queryFolderPath,
                        TeamName=businessNodeItem.Team.Name,
                        DisableInheritance=false
                    }
                };

                string buildPipelineFolderPath = null;
                if (!businessNodeItem.IsClone)
                {
                    buildPipelineFolderPath = $"{genericAreaPath.Replace(Constants.DefaultPathSeparator, Constants.DefaultPathSeparatorForward)}";
                }
                else
                {
                    buildPipelineFolderPath = ""; //may need to put root instead
                }
                newEngineConfig.BuildPipelineFolderPaths = new List<NamespacePathSecuritySpec>()
                {
                    new NamespacePathSecuritySpec()
                    {
                        Path=buildPipelineFolderPath,
                        TeamName=businessNodeItem.Team.Name,
                        DisableInheritance=false
                    }
                };
                string releasePipelineFolderPath = null;
                if (!businessNodeItem.IsClone)
                {
                    releasePipelineFolderPath = $"{genericAreaPath.Replace(Constants.DefaultPathSeparator, Constants.DefaultPathSeparatorForward)}";
                }
                else
                {
                    releasePipelineFolderPath = ""; //may need to put root instead
                }
                newEngineConfig.ReleasePipelineFolderPaths = new List<NamespacePathSecuritySpec>()
                {
                    new NamespacePathSecuritySpec()
                    {
                        Path=releasePipelineFolderPath,
                        TeamName=businessNodeItem.Team.Name,
                        DisableInheritance=false
                    }
                };

                // Define the project to import.
                var _projectDef = new ProjectDefinition(
                    newEngineConfig.ImportPath,
                    newEngineConfig.SourceCollection,
                    newEngineConfig.SourceProject,
                    newEngineConfig.DestinationCollection,
                    newEngineConfig.DestinationProject,
                    newEngineConfig.Description,
                    newEngineConfig.DestinationProjectProcessName,
                    newEngineConfig.DefaultTeamName,
                    newEngineConfig.PAT,
                    newEngineConfig.RestApiService);

                //now to load dynamic area paths and repos
                //areas
                var teamFolder = _projectDef.GetFolderFor("TeamConfigurations");
                IEnumerable<string> teamFolders = null;
                if (Directory.Exists(teamFolder))
                {
                    teamFolders = Directory.EnumerateDirectories(teamFolder);
                }
                else
                {
                    DirectoryInfo teamFolderInfo = new DirectoryInfo(teamFolder);
                    throw new DirectoryNotFoundException($"PLEASE ENSURE YOU HAVE EXECUTED EXPORT as the following folder DNE: {teamFolderInfo.FullName}");
                }
                newEngineConfig.AreaPaths = new List<NamespacePathSecuritySpec>();
                foreach (var teamFolderF in teamFolders)
                {
                    var filename = _projectDef.GetFileFor("TeamAreas", new object[] { teamFolderF });
                    var rawjson = File.ReadAllText(filename);
                    TeamAreasMinimal teamAreas = JsonConvert.DeserializeObject<TeamAreasMinimal>(rawjson);
                    string defaultTeamAreaPath = null;
                    if (treeNode.Item.IsClone)
                    {
                        defaultTeamAreaPath = $"{teamAreas.DefaultValue}";
                    }
                    else if (treeNode.Item.Disabled)
                    {
                        defaultTeamAreaPath = $"{destinationProject}{Constants.DefaultPathSeparator}{teamAreas.DefaultValue}";
                    }
                    else
                    {
                        defaultTeamAreaPath = $"{fullGenericParentAreaPath.Substring(Constants.DefaultPathSeparator.Length)}{Constants.DefaultPathSeparator}{teamAreas.DefaultValue}";
                    }
                    newEngineConfig.AreaPaths.Add(new NamespacePathSecuritySpec()
                    {
                        Path = defaultTeamAreaPath,
                        TeamName = Path.GetFileName(teamFolderF),
                        DisableInheritance = false
                    });
                }

                //repos
                List<ProjectDefinition.MappingRecord> repositories
                    = _projectDef.FindMappingRecords(x =>
                   x.Type == "Repository");

                newEngineConfig.RepositorySecurity = new List<NamespacePathSecuritySpec>();
                foreach (var repo in repositories)
                {
                    newEngineConfig.RepositorySecurity.Add(new NamespacePathSecuritySpec()
                    {
                        Path = $"{repo.OldEntityName}",
                        TeamName = businessNodeItem.Team.Name,
                        DisableInheritance = false
                    });
                }

                //newEngineConfig.Validate(); //this will also load secrets



                mapTeamProjectToImportConfig.Add(treeNode, newEngineConfig);
                string json = JsonConvert.SerializeObject(newEngineConfig, Formatting.Indented);
                string importConfigFileName = $"{businessNodeItem.Id.ToString().PadLeft(3, '0')}--{businessNodeItem.Name}--ProjectImportConfig.json";
                File.WriteAllText(Path.Combine(outputPath, importConfigFileName), json);
            }
        }

        public string SkipFirstNodesFromPath(string currentPath, int numberOfNodesToSkip, string pathSeparator)
        {
            if (numberOfNodesToSkip < 0)
                throw new ArgumentException($"number of nodes to skip {numberOfNodesToSkip} can't be negative!");
            if (numberOfNodesToSkip == 0)
                return currentPath;

            var pathParts = currentPath.Split(new string[] { pathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            if (numberOfNodesToSkip > pathParts.Length)
                throw new ArgumentException($"can't skip {numberOfNodesToSkip} nodes as there are only {pathParts.Length}");

            return String.Join(pathSeparator, pathParts.Skip(numberOfNodesToSkip));
        }

        public void GenerateBulkImportConfigs(string outputPath,
            string sourcePatToken,
            string destinationPatToken,
            string destinationCollection,
            string destinationProject,
            string destinationProcessTypeName,
            string reflectedWorkItemId,
            bool useTimeRestrictionClause,
            int numberOfDaysToImport,
            string universalMapsProcessMaps,
            string universalAreaMaps,
            string universalIterationMaps,
            out Dictionary<SimpleMutableBusinessNodeNode, VstsSyncMigrator.Engine.Configuration.EngineConfiguration> mapTeamProjectToBulkImportConfig)
        {
            DirectoryInfo di = new DirectoryInfo(outputPath);
            if (!di.Exists)
                Directory.CreateDirectory(di.FullName);

            mapTeamProjectToBulkImportConfig
                = new Dictionary<SimpleMutableBusinessNodeNode, VstsSyncMigrator.Engine.Configuration.EngineConfiguration>();

            foreach (var treeNode in this.Where(tn => tn.Item.BusinessNodeType == BusinessNodeType.TeamProject))
            {
                //VstsSyncMigrator.Engine.Configuration.EngineConfiguration newEngineConfig
                //    = VstsSyncMigrator.Engine.Configuration.EngineConfiguration.GetDefault();

                BusinessNodeItem businessNodeItem = treeNode.Item;

                string rootNode = $"\\{destinationProject}\\";
                string genericPath = null;
                string genericParentPath = null;
                if (!businessNodeItem.IsClone)
                {
                    genericPath = treeNode.GetAreaOrIterationPath(Constants.AreaStructureType, false, true)
                    .Substring(rootNode.Length);
                    genericParentPath
                        = treeNode.Parent.GetAreaOrIterationPath(Constants.AreaStructureType, false, true)
                        .Substring(rootNode.Length);
                }

                Uri sourceCollectionUri = null;
                if (!businessNodeItem.IsOnPremiseProject)
                {
                    sourceCollectionUri = new Uri($"https://dev.azure.com/{businessNodeItem.OrganizationName}");
                }
                else
                {
                    sourceCollectionUri = new Uri($"https://{businessNodeItem.AzureDevOpsServerFQDN}/tfs/{businessNodeItem.OrganizationName}");
                }
                Uri destinationCollectionUri = null;
                if (!businessNodeItem.IsOnPremiseProject)
                {
                    destinationCollectionUri = new Uri($"https://dev.azure.com/{destinationCollection}");
                }
                else
                {
                    destinationCollectionUri = new Uri($"https://{businessNodeItem.AzureDevOpsServerFQDN}/tfs/{destinationCollection}");
                }
                VstsSyncMigrator.Engine.Configuration.EngineConfiguration newEngineConfig
                    = new VstsSyncMigrator.Engine.Configuration.EngineConfiguration()
                    {
                        TelemetryEnableTrace = true,
                        LoadSecretsFromEnvironmentVariables = true,
                        Source = new VstsSyncMigrator.Engine.Configuration.TeamProjectConfig()
                        {
                            Name = businessNodeItem.Name,
                            Collection = sourceCollectionUri,
                            Token = sourcePatToken
                        },
                        Target = new VstsSyncMigrator.Engine.Configuration.TeamProjectConfig()
                        {
                            Name = destinationProject,
                            Collection = destinationCollectionUri,
                            Token = destinationPatToken
                        },
                        ReflectedWorkItemIDFieldName = reflectedWorkItemId,
                        FieldMaps = new List<VstsSyncMigrator.Engine.Configuration.FieldMap.IFieldMapConfig>
                        {
                            //new VstsSyncMigrator.Engine.Configuration.FieldMap.FieldtoFieldMapConfig()
                            //{
                            //    WorkItemTypeName = "Bug",
                            //    SourceField = "System.Description",
                            //    TargetField = "devopsabcs.ScrumIntegratedToServiceNow.ServiceNowDescription"
                            //},
                        },
                        WorkItemTypeDefinition = new Dictionary<string, string>
                        {
                            { "Bug", "Bug" },
                            { "Epic", "Epic"},
                            { "Feature", "Feature" },
                            { "Issue", "Issue" },
                            { "Task", "Task" },

                            { "Test Case", "Test Case" },
                            { "Test Plan", "Test Plan" },
                            { "Test Suite", "Test Suite" },

                            { "User Story", "User Story" },
                            { "Requirement", "Requirement" },
                            { "Product Backlog Item", "Product Backlog Item" },

                            { "Shared Steps", "Shared Steps" },
                            { "Shared Parameter", "Shared Parameter" },

                            { "Impediment", "Impediment" },

                            { "Change Request", "Change Request" },
                            { "Review", "Review" },
                            { "Risk", "Risk" },
                        },

                        // Create an empty list of processors.
                        Processors = new List<VstsSyncMigrator.Engine.Configuration.Processing.ITfsProcessingConfig>()
                    };

                RestAPI.ProcessMapping.Maps maps =
                    RestAPI.ProcessMapping.Maps.LoadFromJson(universalMapsProcessMaps);
                //apply maps for process
                ADO.ProcessMapping.ProcessMapUtility1
                    .ApplyProcessMapping(newEngineConfig.WorkItemTypeDefinition,
                    treeNode.Item.Process,
                    destinationProcessTypeName, maps);
                ADO.ProcessMapping.ProcessMapUtility1
                    .ApplyProcessMappingFieldMap(newEngineConfig.FieldMaps,
                    treeNode.Item.Process,
                    destinationProcessTypeName, maps);

                bool useAreaPathMap = treeNode.Item.AreaPathMap != null && treeNode.Item.AreaPathMap.Count > 0;
                Dictionary<string, string> areaPathMap = treeNode.Item.AreaPathMap;
                bool useIterationPathMap = treeNode.Item.IterationPathMap != null && treeNode.Item.IterationPathMap.Count > 0;
                Dictionary<string, string> iterationPathMap = treeNode.Item.IterationPathMap;

                SerializableClassificationNodeMap serializableAreaMap = null;
                SerializableClassificationNodeMap serializableIterationMap = null;
                SerializableClassificationNodeMapWithCache fullAreaPathMap = null;
                SerializableClassificationNodeMapWithCache fullIterationPathMap = null;

                if (!string.IsNullOrEmpty(universalAreaMaps) && File.Exists(universalAreaMaps))
                {
                    serializableAreaMap = SerializableClassificationNodeMap.LoadFromJson(universalAreaMaps);

                    bool hasMappedAreaForOrganizationalProject = serializableAreaMap.HasMappedClassificationNodeForOrganizationalProject(businessNodeItem.OrganizationName, businessNodeItem.Name);
                    if (!businessNodeItem.IsClone)
                    {
                        fullAreaPathMap = serializableAreaMap.GetMappedClassificationNodeListForOrganizationalProject(businessNodeItem.OrganizationName, businessNodeItem.Name);
                    }
                    if (fullAreaPathMap != null && fullAreaPathMap.Map.Count > 0 && !useAreaPathMap)
                    {
                        System.Diagnostics.Trace.WriteLine($"Setting useAreaPathMap to true since we have at least one entry in fullAreaPathMap");
                        useAreaPathMap = true;
                    }

                }
                else if (useAreaPathMap)
                {
                    //show stopper
                    if (string.IsNullOrEmpty(universalAreaMaps))
                    {
                        throw new ArgumentNullException($"Please specify the filename for an existing SerializableClassificationNodeMap for Areas");
                    }
                    else
                    {
                        FileInfo fileInfoArea = new FileInfo(universalAreaMaps);
                        throw new ArgumentNullException($"The specified filename for an existing SerializableClassificationNodeMap for Areas {fileInfoArea.FullName} does not exist");
                    }
                }

                if (!string.IsNullOrEmpty(universalIterationMaps) && File.Exists(universalIterationMaps))
                {
                    serializableIterationMap = SerializableClassificationNodeMap.LoadFromJson(universalIterationMaps);
                    bool hasMappedIterationForOrganizationalProject = serializableIterationMap.HasMappedClassificationNodeForOrganizationalProject(businessNodeItem.OrganizationName, businessNodeItem.Name);
                    if (!businessNodeItem.IsClone)
                    {
                        fullIterationPathMap = serializableIterationMap.GetMappedClassificationNodeListForOrganizationalProject(businessNodeItem.OrganizationName, businessNodeItem.Name);
                    }
                    if (fullIterationPathMap != null && fullIterationPathMap.Map.Count > 0 && !useIterationPathMap)
                    {
                        System.Diagnostics.Trace.WriteLine($"Setting useIterationPathMap to true since we have at least one entry in fullIterationPathMap");
                        useIterationPathMap = true;
                    }
                }
                else if (useIterationPathMap)
                {
                    //show stopper
                    if (string.IsNullOrEmpty(universalIterationMaps))
                    {
                        throw new ArgumentNullException($"Please specify the filename for an existing SerializableClassificationNodeMap for Iterations");
                    }
                    else
                    {
                        FileInfo fileInfoIteration = new FileInfo(universalIterationMaps);
                        throw new ArgumentNullException($"The specified filename for an existing SerializableClassificationNodeMap for Iterations {fileInfoIteration.FullName} does not exist");
                    }
                }


                // Define processors.
                // AND [System.AreaPath] UNDER 'HR Finance Accounting\Tokenization'
                string extraAreaPathRestrictionClause = string.Empty;
                if (treeNode.Item.AreaBasePaths != null
                    && treeNode.Item.AreaBasePaths.Count > 0)
                {
                    //extra area path restriction clause
                    var extraAreaInnerClause = String.Join(" OR ", treeNode.Item.AreaBasePaths.Select(path => $"[System.AreaPath] UNDER '{path}'"));
                    extraAreaPathRestrictionClause = $" AND ({extraAreaInnerClause}) ";
                }

                string timeRestrictionClause
                    = useTimeRestrictionClause ? $" AND [System.CreatedDate] > @Today - {numberOfDaysToImport.ToString()} " : string.Empty;


                newEngineConfig.Processors.Add(new VstsSyncMigrator.Engine.Configuration.Processing.WorkItemRevisionReplayMigrationConfig()
                {
                    AreaPath = "",
                    IterationPath = "",
                    DegreeOfParallelism = 16,
                    Enabled = true,
                    PrefixProjectToNodes = false,
                    PrefixPath = treeNode.Item.Disabled || treeNode.Item.IsClone ? string.Empty : genericParentPath,
                    UseAreaPathMap = useAreaPathMap,
                    AreaPathMap = areaPathMap,
                    FullAreaPathMap = fullAreaPathMap,
                    UseIterationPathMap = useIterationPathMap,
                    IterationPathMap = iterationPathMap,
                    FullIterationPathMap = fullIterationPathMap,
                    QueryBit = timeRestrictionClause + $"AND [System.WorkItemType] IN ('Requirement', 'Task', 'User Story', 'Bug', 'Epic', 'Feature', 'Product Backlog Item', 'Impediment', 'Issue', 'Change Request', 'Review', 'Risk') AND [System.State] <> 'Removed'"
                    + extraAreaPathRestrictionClause,
                });

                newEngineConfig.Processors.Add(new VstsSyncMigrator.Engine.Configuration.Processing.WorkItemRevisionReplayMigrationConfig()
                {
                    AreaPath = "",
                    IterationPath = "",
                    DegreeOfParallelism = 16,
                    Enabled = true,
                    PrefixProjectToNodes = false,
                    PrefixPath = treeNode.Item.Disabled || treeNode.Item.IsClone ? string.Empty : genericParentPath,
                    UseAreaPathMap = useAreaPathMap,
                    AreaPathMap = areaPathMap,
                    FullAreaPathMap = fullAreaPathMap,
                    UseIterationPathMap = useIterationPathMap,
                    IterationPathMap = iterationPathMap,
                    FullIterationPathMap = fullIterationPathMap,
                    QueryBit = $"AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case') AND [System.State] <> 'Removed'"// AND {timeRestrictionClause}"
                    + extraAreaPathRestrictionClause,
                });
                newEngineConfig.Processors.Add(new VstsSyncMigrator.Engine.Configuration.Processing.LinkMigrationConfig()
                {
                    DegreeOfParallelism = 16,
                    Enabled = true,
                    QueryBit = $"AND [System.RelatedLinkCount] > 0" + timeRestrictionClause
                    + extraAreaPathRestrictionClause,
                });
                //resolve VS402335: The timeout period (30 seconds) elapsed prior to completion of the query or the server is not responding
                //for some projects which have many attachments
                newEngineConfig.Processors.Add(new VstsSyncMigrator.Engine.Configuration.Processing.AttachementExportMigrationConfig()
                {
                    Enabled = true,
                    QueryBit = @"AND [System.AttachedFileCount] > 0" + timeRestrictionClause
                });
                newEngineConfig.Processors.Add(new VstsSyncMigrator.Engine.Configuration.Processing.AttachementImportMigrationConfig()
                {
                    Enabled = true
                });
                newEngineConfig.Processors.Add(new VstsSyncMigrator.Engine.Configuration.Processing.WorkItemQueryMigrationConfig()
                {
                    Enabled = true,
                    PrefixProjectToNodes = false,
                    PrefixPath = treeNode.Item.IsClone ? string.Empty : genericParentPath,
                    UseAreaPathMap = useAreaPathMap,
                    AreaPathMap = areaPathMap,
                    FullAreaPathMap = fullAreaPathMap,
                    UseIterationPathMap = useIterationPathMap,
                    IterationPathMap = iterationPathMap,
                    FullIterationPathMap = fullIterationPathMap,
                });
                newEngineConfig.Processors.Add(new VstsSyncMigrator.Engine.Configuration.Processing.TestVariablesMigrationConfig()
                {
                    Enabled = true
                });
                newEngineConfig.Processors.Add(new VstsSyncMigrator.Engine.Configuration.Processing.TestConfigurationsMigrationConfig()
                {
                    Enabled = true
                });
                newEngineConfig.Processors.Add(new VstsSyncMigrator.Engine.Configuration.Processing.TestPlansAndSuitesMigrationConfig()
                {
                    Enabled = true,
                    PrefixProjectToNodes = false,
                    PrefixPath = treeNode.Item.IsClone ? string.Empty : genericParentPath,
                    UseAreaPathMap = useAreaPathMap,
                    AreaPathMap = areaPathMap,
                    FullAreaPathMap = fullAreaPathMap,
                    UseIterationPathMap = useIterationPathMap,
                    IterationPathMap = iterationPathMap,
                    FullIterationPathMap = fullIterationPathMap,
                    AreaPath = "",
                    IterationPath = "",
                });

                mapTeamProjectToBulkImportConfig.Add(treeNode, newEngineConfig);
                string json = JsonConvert.SerializeObject(newEngineConfig, Formatting.Indented,
                    new VstsSyncMigrator.Engine.Configuration.FieldMap.FieldMapConfigJsonConverter(),
                    new VstsSyncMigrator.Engine.Configuration.FieldMap.ProcessorConfigJsonConverter(),
                    new EngineConfigurationJsonConverter());
                string bulkImportConfigFileName = $"{businessNodeItem.Id.ToString().PadLeft(3, '0')}--{businessNodeItem.Name}--ProjectBulkImportConfig.json";
                File.WriteAllText(Path.Combine(outputPath, bulkImportConfigFileName), json);

            }
        }

        public string GetAreaOrIterationPath(string structureType, bool includeStructureTypeInPath, bool excludeDisabledNodes)
        {
            // \OneProjectV2_02\Area\Research\DevOps\KanbanCustomization
            var allNodesExceptLeaf = this.SelectAncestorsDownward().ToList();
            StringBuilder areaOrIterationPath = null;
            if (allNodesExceptLeaf.Count > 0)
            {
                areaOrIterationPath = new StringBuilder($"\\{allNodesExceptLeaf.First().Name}{(includeStructureTypeInPath ? $"\\{structureType}" : "")}");
                var allOtherNodes = allNodesExceptLeaf.Skip(1);
                foreach (var item in allOtherNodes)
                {
                    if (excludeDisabledNodes && item.Item.Disabled)
                    {
                        //exclude disabled node
                    }
                    else
                    {
                        areaOrIterationPath.Append($"\\{item.Name}");
                    }
                }
                if (excludeDisabledNodes && this.Item.Disabled)
                {
                    //exclude disabled node
                }
                else
                {
                    areaOrIterationPath.Append($"\\{this.Name}");
                }
            }
            else
            {
                areaOrIterationPath = new StringBuilder($"\\{this.Name}{(includeStructureTypeInPath ? $"\\{structureType}" : "")}");
            }
            return areaOrIterationPath == null ? null : areaOrIterationPath.ToString();
        }

        public ClassificationNodeMinimal GenerateAreaHierarchy(
            bool includeStructureTypeInPath
            )
        {
            //do preorder traversal
            ClassificationNodeMinimal currentRootClassificationNodeMinimal
                = new ClassificationNodeMinimal()
                {
                    Name = this.Name,
                    StructureType = Constants.AreaStructureType.ToLower(),
                    Path = this.GetAreaOrIterationPath(Constants.AreaStructureType, includeStructureTypeInPath, true),
                    HasChildren = this.Children.Count > 0,
                    Attributes = null,
                    Children = new List<ClassificationNodeMinimal>()
                };

            Dictionary<SimpleMutableBusinessNodeNode, ClassificationNodeMinimal> mapFromTreeToIterationHierarchy
                = new Dictionary<SimpleMutableBusinessNodeNode, ClassificationNodeMinimal>()
                {
                    { this, currentRootClassificationNodeMinimal }
                };

            //preorder traversal INCLUDING root!!! like SelectAll: would have been root.Select(tn=>tn)
            //https://www.geeksforgeeks.org/tree-traversals-inorder-preorder-and-postorder/
            foreach (var treeNode in this.SelectDescendants())
            {
                Trace.WriteLine($"at node: {treeNode.GetAreaOrIterationPath(Constants.AreaStructureType, includeStructureTypeInPath, true)} of business type {treeNode.Item.BusinessNodeType.ToString()}");
                switch (treeNode.Item.BusinessNodeType)
                {
                    case BusinessNodeType.None:
                    case BusinessNodeType.Portfolio:
                    case BusinessNodeType.Program:
                    case BusinessNodeType.Product:
                    case BusinessNodeType.TeamProject:
                        {
                            ClassificationNodeMinimal newGenericClassificationNodeMinimal
                                = new ClassificationNodeMinimal()
                                {
                                    Name = treeNode.Name,
                                    StructureType = Constants.AreaStructureType.ToLower(),
                                    Path = treeNode.GetAreaOrIterationPath(Constants.AreaStructureType, includeStructureTypeInPath, true),
                                    HasChildren = treeNode.Children.Count > 0,
                                    Attributes = null,
                                    Children = new List<ClassificationNodeMinimal>()
                                };
                            mapFromTreeToIterationHierarchy.Add(treeNode, newGenericClassificationNodeMinimal);
                            //map to parent
                            mapFromTreeToIterationHierarchy[treeNode.Parent].Children.Add(newGenericClassificationNodeMinimal);
                            break;
                        }

                    default:
                        {
                            throw new NotImplementedException($"can't handle business type {treeNode.Item.BusinessNodeType.ToString()}");
                        }
                }
            }

            return currentRootClassificationNodeMinimal;
        }

        public ClassificationNodeEnhanced GenerateIterationHierarchy(
            bool includeStructureTypeInPath,
            out Dictionary<SimpleMutableBusinessNodeNode, ClassificationNodeEnhanced> mapFromTreeToIterationHierarchy
            )
        {
            int idCounter = 0;
            //do preorder traversal
            ClassificationNodeEnhanced currentRootClassificationNodeMinimal
                = new ClassificationNodeEnhanced()
                {
                    Id = ++idCounter,
                    Name = this.Name,
                    StructureType = Constants.IterationStructureType.ToLower(),
                    Path = this.GetAreaOrIterationPath(Constants.IterationStructureType, includeStructureTypeInPath, true),
                    HasChildren = this.Children.Count > 0,
                    TeamLevel = TeamLevel.None,
                    Attributes = null,
                    Children = new List<ClassificationNodeEnhanced>()
                };

            mapFromTreeToIterationHierarchy
                = new Dictionary<SimpleMutableBusinessNodeNode, ClassificationNodeEnhanced>()
                {
                    { this, currentRootClassificationNodeMinimal }
                };

            //preorder traversal INCLUDING root!!! like SelectAll: would have been root.Select(tn=>tn)
            //https://www.geeksforgeeks.org/tree-traversals-inorder-preorder-and-postorder/
            foreach (var treeNode in this.SelectDescendants())
            {
                Trace.WriteLine($"at node: {treeNode.GetAreaOrIterationPath(Constants.IterationStructureType, includeStructureTypeInPath, true)} of business type {treeNode.Item.BusinessNodeType.ToString()}");
                switch (treeNode.Item.BusinessNodeType)
                {
                    case BusinessNodeType.None:
                    case BusinessNodeType.Portfolio:
                    case BusinessNodeType.Program:
                    case BusinessNodeType.Product:
                        {
                            ClassificationNodeEnhanced newGenericClassificationNodeMinimal
                                = new ClassificationNodeEnhanced()
                                {
                                    Id = ++idCounter,
                                    Name = treeNode.Name,
                                    StructureType = Constants.IterationStructureType.ToLower(),
                                    Path = treeNode.GetAreaOrIterationPath(Constants.IterationStructureType, includeStructureTypeInPath, true),
                                    HasChildren = treeNode.Children.Count > 0,
                                    TeamLevel = TeamLevel.None,
                                    Attributes = null,
                                    Children = new List<ClassificationNodeEnhanced>()
                                };
                            mapFromTreeToIterationHierarchy.Add(treeNode, newGenericClassificationNodeMinimal);
                            //map to parent
                            mapFromTreeToIterationHierarchy[treeNode.Parent].Children.Add(newGenericClassificationNodeMinimal);
                            break;
                        }
                    case BusinessNodeType.TeamProject:
                        {
                            ClassificationNodeEnhanced newTeamProjectClassificationNodeMinimal
                                = new ClassificationNodeEnhanced()
                                {
                                    Id = ++idCounter,
                                    Name = treeNode.Name,
                                    StructureType = Constants.IterationStructureType.ToLower(),
                                    Path = treeNode.GetAreaOrIterationPath(Constants.IterationStructureType, includeStructureTypeInPath, true),
                                    HasChildren = treeNode.Children.Count > 0,
                                    TeamLevel = TeamLevel.None,
                                    Attributes = null,
                                    Children = new List<ClassificationNodeEnhanced>()
                                };

                            //fetch portfolio and ProgramOrProduct ancestors
                            SimpleMutableBusinessNodeNode ancestorPortfolio = treeNode.SelectAncestorsUpward()
                                .First(anc => anc.Item.BusinessNodeType == BusinessNodeType.Portfolio);
                            SimpleMutableBusinessNodeNode ancestorProgramOrProduct = treeNode.SelectAncestorsUpward()
                                .First(anc => anc.Item.BusinessNodeType == BusinessNodeType.Program
                                || anc.Item.BusinessNodeType == BusinessNodeType.Product);

                            var iterationTree = GetIterationTree(newTeamProjectClassificationNodeMinimal,
                                ancestorPortfolio.Item.Team.Cadences,
                                ancestorProgramOrProduct.Item.Team.Cadences,
                                treeNode.Item.Team.Cadences,
                                ref idCounter
                                );
                            mapFromTreeToIterationHierarchy.Add(treeNode, iterationTree);
                            //map to parent
                            mapFromTreeToIterationHierarchy[treeNode.Parent].Children.Add(newTeamProjectClassificationNodeMinimal);
                            break;
                        }

                    default:
                        {
                            throw new NotImplementedException($"can't handle business type {treeNode.Item.BusinessNodeType.ToString()}");
                        }
                }
            }

            return currentRootClassificationNodeMinimal;
        }

        public ClassificationNodeMinimal GenerateIterationHierarchy(
            bool includeStructureTypeInPath,
            out Dictionary<SimpleMutableBusinessNodeNode, ClassificationNodeMinimal> mapFromTreeToIterationHierarchy
            )
        {
            //do preorder traversal
            ClassificationNodeMinimal currentRootClassificationNodeMinimal
                = new ClassificationNodeMinimal()
                {
                    Name = this.Name,
                    StructureType = Constants.IterationStructureType.ToLower(),
                    Path = this.GetAreaOrIterationPath(Constants.IterationStructureType, includeStructureTypeInPath, true),
                    HasChildren = this.Children.Count > 0,
                    Attributes = null,
                    Children = new List<ClassificationNodeMinimal>()
                };

            mapFromTreeToIterationHierarchy
                = new Dictionary<SimpleMutableBusinessNodeNode, ClassificationNodeMinimal>()
                {
                    { this, currentRootClassificationNodeMinimal }
                };

            //preorder traversal INCLUDING root!!! like SelectAll: would have been root.Select(tn=>tn)
            //https://www.geeksforgeeks.org/tree-traversals-inorder-preorder-and-postorder/
            foreach (var treeNode in this.SelectDescendants())
            {
                Trace.WriteLine($"at node: {treeNode.GetAreaOrIterationPath(Constants.IterationStructureType, includeStructureTypeInPath, true)} of business type {treeNode.Item.BusinessNodeType.ToString()}");
                switch (treeNode.Item.BusinessNodeType)
                {
                    case BusinessNodeType.None:
                    case BusinessNodeType.Portfolio:
                    case BusinessNodeType.Program:
                    case BusinessNodeType.Product:
                        {
                            ClassificationNodeMinimal newGenericClassificationNodeMinimal
                                = new ClassificationNodeMinimal()
                                {
                                    Name = treeNode.Name,
                                    StructureType = Constants.IterationStructureType.ToLower(),
                                    Path = treeNode.GetAreaOrIterationPath(Constants.IterationStructureType, includeStructureTypeInPath, true),
                                    HasChildren = treeNode.Children.Count > 0,
                                    Attributes = null,
                                    Children = new List<ClassificationNodeMinimal>()
                                };
                            mapFromTreeToIterationHierarchy.Add(treeNode, newGenericClassificationNodeMinimal);
                            //map to parent
                            mapFromTreeToIterationHierarchy[treeNode.Parent].Children.Add(newGenericClassificationNodeMinimal);
                            break;
                        }
                    case BusinessNodeType.TeamProject:
                        {
                            ClassificationNodeMinimal newTeamProjectClassificationNodeMinimal
                                = new ClassificationNodeMinimal()
                                {
                                    Name = treeNode.Name,
                                    StructureType = Constants.IterationStructureType.ToLower(),
                                    Path = treeNode.GetAreaOrIterationPath(Constants.IterationStructureType, includeStructureTypeInPath, true),
                                    HasChildren = treeNode.Children.Count > 0,
                                    Attributes = null,
                                    Children = new List<ClassificationNodeMinimal>()
                                };

                            //fetch portfolio and ProgramOrProduct ancestors
                            SimpleMutableBusinessNodeNode ancestorPortfolio = treeNode.SelectAncestorsUpward()
                                .First(anc => anc.Item.BusinessNodeType == BusinessNodeType.Portfolio);
                            SimpleMutableBusinessNodeNode ancestorProgramOrProduct = treeNode.SelectAncestorsUpward()
                                .First(anc => anc.Item.BusinessNodeType == BusinessNodeType.Program
                                || anc.Item.BusinessNodeType == BusinessNodeType.Product);

                            var iterationTree = GetIterationTree(newTeamProjectClassificationNodeMinimal,
                                ancestorPortfolio.Item.Team.Cadences,
                                ancestorProgramOrProduct.Item.Team.Cadences,
                                treeNode.Item.Team.Cadences
                                );
                            mapFromTreeToIterationHierarchy.Add(treeNode, iterationTree);
                            //map to parent
                            mapFromTreeToIterationHierarchy[treeNode.Parent].Children.Add(newTeamProjectClassificationNodeMinimal);
                            break;
                        }

                    default:
                        {
                            throw new NotImplementedException($"can't handle business type {treeNode.Item.BusinessNodeType.ToString()}");
                        }
                }
            }

            return currentRootClassificationNodeMinimal;
        }

        public void GenerateIterations(string projectInitializationIterationsPath)
        {
            string json;
            foreach (var portfolio in this.Children)
            {
                SimpleMutableBusinessNodeNode detachedPortfolioClone = new SimpleMutableBusinessNodeNode(portfolio.Item);
                portfolio.CopyTo(detachedPortfolioClone);

                Dictionary<SimpleMutableBusinessNodeNode, ClassificationNodeMinimal> mapFromTreeToIterationHierarchy;
                var iterationsFromBuildNode = detachedPortfolioClone.GenerateIterationHierarchy(true, out mapFromTreeToIterationHierarchy);

                json = JsonConvert.SerializeObject(iterationsFromBuildNode, Formatting.Indented);

                var portfolioPath = Path.Combine(projectInitializationIterationsPath, portfolio.Name);
                DirectoryInfo di = new DirectoryInfo(portfolioPath);
                if (!di.Exists)
                    Directory.CreateDirectory(di.FullName);

                File.WriteAllText(Path.Combine(projectInitializationIterationsPath, portfolio.Name, "IterationsInitialization.json"), json);
            }
        }

        public void GenerateAreas(string projectInitializationAreasPath)
        {
            string json;
            foreach (var portfolio in this.Children)
            {
                SimpleMutableBusinessNodeNode detachedPortfolioClone = new SimpleMutableBusinessNodeNode(portfolio.Item);
                portfolio.CopyTo(detachedPortfolioClone);

                var areasFromBuildNode = detachedPortfolioClone.GenerateAreaHierarchy(true);

                json = JsonConvert.SerializeObject(areasFromBuildNode, Formatting.Indented);

                var portfolioPath = Path.Combine(projectInitializationAreasPath, portfolio.Name);
                DirectoryInfo di = new DirectoryInfo(portfolioPath);
                if (!di.Exists)
                    Directory.CreateDirectory(di.FullName);

                File.WriteAllText(Path.Combine(projectInitializationAreasPath, portfolio.Name, "AreasInitialization.json"), json);
            }
        }

        public void GenerateTeams(string projectInitializationTeamsPath)
        {
            Teams teams = new Teams();
            string json;
            bool includeStructureTypeInPath = false;

            Dictionary<Team, SimpleMutableBusinessNodeNode> mapTeamToTreeNode
                = new Dictionary<Team, SimpleMutableBusinessNodeNode>();

            foreach (var treeNode in this.Select(tn => tn))
            {
                if (treeNode.Item.Team != null)
                {
                    Team newTeam = new Team()
                    {
                        Name = treeNode.Item.Team.Name,
                        ProjectName = this.Name,
                        Description = treeNode.Item.Team.Description
                    };

                    teams.TeamList.Add(newTeam);
                    mapTeamToTreeNode.Add(newTeam, treeNode);
                }
            }
            json = JsonConvert.SerializeObject(teams.TeamList, Formatting.Indented);
            DirectoryInfo di = new DirectoryInfo(projectInitializationTeamsPath);
            if (!di.Exists)
                Directory.CreateDirectory(di.FullName);
            File.WriteAllText(Path.Combine(projectInitializationTeamsPath, "Teams.json"), json);

            Dictionary<SimpleMutableBusinessNodeNode, ClassificationNodeEnhanced> mapFromTreeToIterationHierarchy;
            this.GenerateIterationHierarchy(includeStructureTypeInPath, out mapFromTreeToIterationHierarchy);

            foreach (var team in mapTeamToTreeNode.Keys)
            {

                SimpleMutableBusinessNodeNode treeNode = mapTeamToTreeNode[team];

                ClassificationNodeEnhanced mappedIterationHierarchy = mapFromTreeToIterationHierarchy[treeNode];

                var currentAreaPath = treeNode.GetAreaOrIterationPath(Constants.AreaStructureType, includeStructureTypeInPath, true);

                Dictionary<int, SimpleMutableClassificationNodeEnhancedNode> mapFromClassNodeIdsToTreeNodesSub;
                var iterations = mappedIterationHierarchy.ToSimpleMutableClassificationNodeEnhancedNode(out mapFromClassNodeIdsToTreeNodesSub);

                var businessNodeType = treeNode.Item.BusinessNodeType;

                List<SimpleMutableClassificationNodeEnhancedNode> relevantIterations;

                switch (businessNodeType)
                {
                    case BusinessNodeType.Portfolio:
                        {
                            relevantIterations = iterations.Where(tn => tn.Item.TeamLevel == TeamLevel.Epic).ToList();
                            break;
                        }
                    case BusinessNodeType.Program:
                    case BusinessNodeType.Product:
                        {
                            relevantIterations = iterations.Where(tn => tn.Item.TeamLevel == TeamLevel.Feature).ToList();
                            break;
                        }
                    case BusinessNodeType.TeamProject:
                        {
                            relevantIterations = iterations.Where(tn => tn.Item.TeamLevel == TeamLevel.Requirement).ToList();
                            break;
                        }

                    default:
                        throw new NotImplementedException($"can't handle {businessNodeType.ToString()}");
                }

                var currentIteration = relevantIterations.FirstOrDefault(it => it.ContainsDate(DateTime.Now));
                if (currentIteration == null)
                {
                    throw new ArgumentException($"you should include an iteration for today's date: {DateTime.Now.Date}");
                }

                var currentIterationParent = currentIteration.Parent;

                di = new DirectoryInfo(Path.Combine(projectInitializationTeamsPath, team.Name));
                if (!di.Exists)
                    Directory.CreateDirectory(di.FullName);

                var teamSettings = new TeamSettingsMinimal()
                {
                    BugsBehavior = "asRequirements",
                    WorkingDays = new string[] {
                            "monday",
                            "tuesday",
                            "wednesday",
                            "thursday",
                            "friday"},
                    BacklogVisibilities = new BacklogVisibilities()
                    {
                        MicrosoftEpicCategory = true,
                        MicrosoftFeatureCategory = true,
                        MicrosoftRequirementCategory = true
                    },
                    DefaultIterationMacro = "@currentIteration",
                    DefaultIteration = new DefaultIteration()
                    {
                        Name = currentIteration.Item.Name,
                        Path = currentIteration.Item.Path
                    },
                    BacklogIteration = new BacklogIteration()
                    {
                        Name = currentIterationParent.Item.Name,
                        Path = currentIterationParent.Item.Path
                    }
                };

                json = JsonConvert.SerializeObject(teamSettings, Formatting.Indented);
                File.WriteAllText(Path.Combine(projectInitializationTeamsPath, team.Name, "TeamSettings.json"), json);

                var teamAreas = new TeamAreasMinimal()
                {
                    DefaultValue = currentAreaPath,
                    Field = new Field() { ReferenceName = "System.AreaPath" },
                    Values = new System.Collections.Generic.List<AreaValue>() {
                        new AreaValue()
                        {
                            Value=currentAreaPath,
                            IncludeChildren=true
                        }
                    }
                };

                json = JsonConvert.SerializeObject(teamAreas, Formatting.Indented);
                File.WriteAllText(Path.Combine(projectInitializationTeamsPath, team.Name, "TeamAreas.json"), json);

                var teamIterations = new TeamIterationsMinimal()
                {
                    Value = relevantIterations.Select(ri =>
                    new TeamIteration()
                    {
                        Name = ri.Item.Name,
                        Path = ri.Item.Path,
                        Attributes = new AttributesWithTimeFrame()
                        {
                            StartDate = ri.Item.Attributes.StartDate,
                            FinishDate = ri.Item.Attributes.FinishDate,
                            TimeFrame = GetTimeFrame(ri.Item.Attributes.StartDate, ri.Item.Attributes.FinishDate)
                        }
                    }
                    ).ToList()
                };

                teamIterations.Count = teamIterations.Value.Count;

                json = JsonConvert.SerializeObject(teamIterations, Formatting.Indented);
                File.WriteAllText(Path.Combine(projectInitializationTeamsPath, team.Name, "TeamIterations.json"), json);

            }
        }


        #region Private Helpers
        private static ClassificationNodeEnhanced GetIterationTree(ClassificationNodeEnhanced root,
            List<Cadence> portfolioCadences, List<Cadence> programOrProductCadences, List<Cadence> teamProjectCadences,
            ref int idCounter)
        {
            //check no Cadence collisions
            bool portfolioHasOverlap = portfolioCadences.CheckHasNoCadenceOverlap();
            bool programOrProductHasOverlap = programOrProductCadences.CheckHasNoCadenceOverlap();
            bool teamProjectHasOverlap = teamProjectCadences.CheckHasNoCadenceOverlap();
            if (portfolioHasOverlap || programOrProductHasOverlap || teamProjectHasOverlap)
                throw new ArgumentException("you have cadence overlap - please fix this");

            string pathPrefix = null;
            List<Iteration> allPortfolioIterations = portfolioCadences.SelectMany(cad => cad.GetIterations(pathPrefix, TeamLevel.Epic)).ToList();
            List<Iteration> allProgramOrProductIterations = programOrProductCadences.SelectMany(cad => cad.GetIterations(pathPrefix, TeamLevel.Feature)).ToList();
            List<Iteration> allTeamProjectIterations = teamProjectCadences.SelectMany(cad => cad.GetIterations(pathPrefix, TeamLevel.Requirement)).ToList();


            foreach (var portfolioIteration in allPortfolioIterations)
            {
                ClassificationNodeEnhanced newPortfolioIteration = new ClassificationNodeEnhanced()
                {
                    Id = ++idCounter,
                    Name = portfolioIteration.Name,
                    StructureType = Constants.IterationStructureType.ToLower(),
                    Children = new List<ClassificationNodeEnhanced>(),
                    Path = $"{root.Path}\\{portfolioIteration.Name}",
                    TeamLevel = TeamLevel.Epic,
                    Attributes = new Attributes() { StartDate = portfolioIteration.StartDate, FinishDate = portfolioIteration.FinishDate }
                };
                root.Children.Add(newPortfolioIteration);
                //fix HasChildren for parent
                root.HasChildren = true;

                foreach (var programOrProductIteration in allProgramOrProductIterations)
                {
                    //find right portfolio iteration parent
                    if (portfolioIteration.ContainsIteration(programOrProductIteration, ContainsType.AllowDupesForSplittingPurposes))
                    {
                        ClassificationNodeEnhanced newProgramIteration = new ClassificationNodeEnhanced()
                        {
                            Id = ++idCounter,
                            Name = programOrProductIteration.Name,
                            StructureType = Constants.IterationStructureType.ToLower(),
                            Children = new List<ClassificationNodeEnhanced>(),
                            Path = $"{newPortfolioIteration.Path}\\{programOrProductIteration.Name}",
                            TeamLevel = TeamLevel.Feature,
                            Attributes = new Attributes() { StartDate = programOrProductIteration.StartDate, FinishDate = programOrProductIteration.FinishDate }
                        };
                        newPortfolioIteration.Children.Add(newProgramIteration);
                        //fix HasChildren for parent
                        newPortfolioIteration.HasChildren = true;

                        //find the right sprint children (not the most efficient)
                        //due to reiteration over and over again
                        //would be quicker from ground up
                        foreach (var teamProjectIteration in allTeamProjectIterations)
                        {
                            if (
                                programOrProductIteration.ContainsIteration(teamProjectIteration, ContainsType.AllowDupesForSplittingPurposes)
                                && portfolioIteration.ContainsIteration(teamProjectIteration, ContainsType.MustStartWithinButMayFinishAfter) //in order to carry out the splitting of sprints
                                                                                                                                             //TODO: may want to specify Split in Name e.g. PI3.1 and PI3.2 or PI3 (1 of 2) and PI3 (2 of 2)
                                ) //make explicit choice for documentation purposes
                            {
                                ClassificationNodeEnhanced newTeamProjectIteration = new ClassificationNodeEnhanced()
                                {
                                    Id = ++idCounter,
                                    Name = teamProjectIteration.Name,
                                    StructureType = Constants.IterationStructureType.ToLower(),
                                    Children = new List<ClassificationNodeEnhanced>(),
                                    Path = $"{newProgramIteration.Path}\\{teamProjectIteration.Name}",
                                    TeamLevel = TeamLevel.Requirement,
                                    Attributes = new Attributes() { StartDate = teamProjectIteration.StartDate, FinishDate = teamProjectIteration.FinishDate }
                                };
                                newProgramIteration.Children.Add(newTeamProjectIteration);
                                //fix HasChildren for parent
                                newProgramIteration.HasChildren = true;
                            }
                        }
                    }
                }

            }
            return root;
        }

        private static ClassificationNodeMinimal GetIterationTree(ClassificationNodeMinimal root,
            List<Cadence> portfolioCadences, List<Cadence> programOrProductCadences, List<Cadence> teamProjectCadences)
        {
            //check no Cadence collisions
            bool portfolioHasOverlap = portfolioCadences.CheckHasNoCadenceOverlap();
            bool programOrProductHasOverlap = programOrProductCadences.CheckHasNoCadenceOverlap();
            bool teamProjectHasOverlap = teamProjectCadences.CheckHasNoCadenceOverlap();
            if (portfolioHasOverlap || programOrProductHasOverlap || teamProjectHasOverlap)
                throw new ArgumentException("you have cadence overlap - please fix this");

            string pathPrefix = null;
            List<Iteration> allPortfolioIterations = portfolioCadences.SelectMany(cad => cad.GetIterations(pathPrefix, TeamLevel.Epic)).ToList();
            List<Iteration> allProgramOrProductIterations = programOrProductCadences.SelectMany(cad => cad.GetIterations(pathPrefix, TeamLevel.Feature)).ToList();
            List<Iteration> allTeamProjectIterations = teamProjectCadences.SelectMany(cad => cad.GetIterations(pathPrefix, TeamLevel.Requirement)).ToList();


            foreach (var portfolioIteration in allPortfolioIterations)
            {
                ClassificationNodeMinimal newPortfolioIteration
                    = new ClassificationNodeMinimal()
                    {
                        Name = portfolioIteration.Name,
                        StructureType = Constants.IterationStructureType.ToLower(),
                        Children = new List<ClassificationNodeMinimal>(),
                        Path = $"{root.Path}\\{portfolioIteration.Name}",
                        Attributes = new Attributes() { StartDate = portfolioIteration.StartDate, FinishDate = portfolioIteration.FinishDate }
                    };
                root.Children.Add(newPortfolioIteration);
                //fix HasChildren for parent
                root.HasChildren = true;

                foreach (var programOrProductIteration in allProgramOrProductIterations)
                {
                    //find right portfolio iteration parent
                    if (portfolioIteration.ContainsIteration(programOrProductIteration, ContainsType.AllowDupesForSplittingPurposes))
                    {
                        ClassificationNodeMinimal newProgramIteration
                            = new ClassificationNodeMinimal()
                            {
                                Name = programOrProductIteration.Name,
                                StructureType = Constants.IterationStructureType.ToLower(),
                                Children = new List<ClassificationNodeMinimal>(),
                                Path = $"{newPortfolioIteration.Path}\\{programOrProductIteration.Name}",
                                Attributes = new Attributes() { StartDate = programOrProductIteration.StartDate, FinishDate = programOrProductIteration.FinishDate }
                            };
                        newPortfolioIteration.Children.Add(newProgramIteration);
                        //fix HasChildren for parent
                        newPortfolioIteration.HasChildren = true;

                        //find the right sprint children (not the most efficient)
                        //due to reiteration over and over again
                        //would be quicker from ground up
                        foreach (var teamProjectIteration in allTeamProjectIterations)
                        {
                            if (
                                programOrProductIteration.ContainsIteration(teamProjectIteration, ContainsType.AllowDupesForSplittingPurposes)
                                && portfolioIteration.ContainsIteration(teamProjectIteration, ContainsType.MustStartWithinButMayFinishAfter) //in order to carry out the splitting of sprints
                                                                                                                                             //TODO: may want to specify Split in Name e.g. PI3.1 and PI3.2 or PI3 (1 of 2) and PI3 (2 of 2)
                                ) //make explicit choice for documentation purposes
                            {
                                ClassificationNodeMinimal newTeamProjectIteration
                                    = new ClassificationNodeMinimal()
                                    {
                                        Name = teamProjectIteration.Name,
                                        StructureType = Constants.IterationStructureType.ToLower(),
                                        Children = new List<ClassificationNodeMinimal>(),
                                        Path = $"{newProgramIteration.Path}\\{teamProjectIteration.Name}",
                                        Attributes = new Attributes() { StartDate = teamProjectIteration.StartDate, FinishDate = teamProjectIteration.FinishDate }
                                    };
                                newProgramIteration.Children.Add(newTeamProjectIteration);
                                //fix HasChildren for parent
                                newProgramIteration.HasChildren = true;
                            }
                        }
                    }
                }

            }
            return root;
        }

        private static string GetTimeFrame(DateTime startDate, DateTime finishDate)
        {
            if (finishDate <= startDate)
                throw new ArgumentException($"please ensure finish date {finishDate.Date.ToLongDateString()} is greater than start date {startDate.Date.ToLongDateString()}");
            if (finishDate < DateTime.Now.Date)
            {
                return "past";
            }
            else if (startDate > DateTime.Now.Date)
            {
                return "future";
            }
            else
                return "current";
        }
        #endregion Private Helpers
    }
}
