using ADO.Engine.Utilities;
using ADO.Engine.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ADO.Engine.BusinessEntities
{
    public class Migration
    {

        public List<BusinessHierarchyCsv> BusinessHierarchyCsvList { get; set; }
        public string PathToCsv { get; set; }
        public byte FiscalMonthStart { get; set; }
        public List<Cadence> DefaultPortfolioCadences { get; set; }
        public List<Cadence> DefaultProgramOrProductTeamCadences { get; set; }
        public List<Cadence> DefaultTeamProjectTeamCadences { get; set; }
        public BusinessNodeType ProgramOrProductBusinessNodeType { get; set; }
        public string PortfolioTeamSuffix { get; set; }
        public bool SortByNameRecursively { get; set; }
        public string ProgramOrProductTeamSuffix { get; set; }
        public string ProjectTeamSuffix { get; set; }
        public BusinessNode BusinessNode { get; set; }
        public string BusinessNodeSerialized { get; set; }
        public string PathToCsvFolder { get; set; }
        public SimpleMutableBusinessNodeNode Root { get; set; }
        public SimpleMutableClassificationNodeMinimalWithIdNode SimpleMutableAreaMinimalWithIdNode { get; set; }
        public SimpleMutableClassificationNodeMinimalWithIdNode SimpleMutableIterationMinimalWithIdNode { get; set; }
        public SimpleMutableClassificationNodeMinimalWithIdNode SimpleMutableAreaMinimalWithIdNodeFinal { get; set; }
        public SimpleMutableClassificationNodeMinimalWithIdNode SimpleMutableIterationMinimalWithIdNodeFinal { get; set; }
        public SerializableClassificationNodeMap SerializableAreaMap { get; set; } = new SerializableClassificationNodeMap();
        public SerializableClassificationNodeMap SerializableIterationMap { get; set; } = new SerializableClassificationNodeMap();
        public string UniversalAreaMaps { get; set; }
        public string UniversalIterationMaps { get; set; }
        public BusinessNodeDataNode BusinessNodeDataNode { get; set; }
        public string BusinessNodeDataNodeSerialized { get; set; }
        public string ImportExportBinaryProgram { get; set; }
        public string MigrationBinaryProgram { get; set; }
        public string SourcePatToken { get; set; }
        public string TemplateFilesPath { get; set; }
        public string ExportPath { get; set; }
        public string UniversalMapsProcessMaps { get; set; }
        public ProjectExportBehavior DefaultProjectExportBehavior { get; set; }
        public string ConfigurationRootFolder { get; set; }
        public string DestinationPatToken { get; set; }
        public string DestinationCollection { get; set; }
        public string DestinationProject { get; set; }
        public string DestinationProjectProcessName { get; set; }
        public string DefaultTeamName { get; set; }
        public string Description { get; set; }
        public string ImportSourceCodeCredentialsPassword { get; set; }
        public string ImportSourceCodeCredentialsGitPassword { get; set; }
        public ProjectImportBehavior DefaultProjectImportBehavior { get; set; }
        public bool CreateDestinationProjectOnFirstImport { get; set; }
        public bool InitializeProjectOnFirstImport { get; set; }
        public string AreaInitializationFile { get; set; }
        public SimpleMutableClassificationNodeMinimalWithIdNode AreaInitializationTree { get; set; }
        public string AreaInitializationOutputFile { get; set; }
        public string IterationInitializationFile { get; set; }
        public SimpleMutableClassificationNodeMinimalWithIdNode IterationInitializationTree { get; set; }
        public string IterationInitializationOutputFile { get; set; }
        public List<string> TeamExclusions { get; set; }
        public string DestinationProcessTypeName { get; set; }
        public string ReflectedWorkItemId { get; set; }
        public int NumberOfDaysToImport { get; set; }
        public bool UseTimeRestrictionClause { get; set; }
        public ClassificationNodeMinimalMap AreaMap { get; set; }
        public ClassificationNodeMinimalMap IterationMap { get; set; }

        public void GenerateBusinessNodeAndDataNodeFromCsv(
            //string outputFolderName,
            string businessNodeSerialized,
            string businessNodeDataNodeSerialized)
        {
            List<BusinessHierarchyCsv> businessHierarchyCsvList;
            this.BusinessNode
                = BusinessNode.LoadFromCsv(this.PathToCsv,
                    this.DestinationProject,
                    this.SortByNameRecursively,
                    this.ProgramOrProductBusinessNodeType,
                    this.DefaultPortfolioCadences,
                    this.DefaultProgramOrProductTeamCadences,
                    this.DefaultTeamProjectTeamCadences,
                    this.PortfolioTeamSuffix,
                    this.ProgramOrProductTeamSuffix,
                    this.ProjectTeamSuffix,
                    out businessHierarchyCsvList);
            this.BusinessHierarchyCsvList = businessHierarchyCsvList;

            this.BusinessNodeSerialized = businessNodeSerialized;

            DirectoryInfo di = new FileInfo(this.BusinessNodeSerialized).Directory;
            if (!di.Exists)
                Directory.CreateDirectory(di.FullName);
            string json = JsonConvert.SerializeObject(this.BusinessNode, Formatting.Indented);
            File.WriteAllText(Path.Combine(di.FullName, businessNodeSerialized), json);

            //also write to Input folder
            this.PathToCsvFolder = (new FileInfo(this.PathToCsv)).Directory.FullName;
            File.WriteAllText(Path.Combine(this.PathToCsvFolder, businessNodeSerialized), json);

            bool treeAlreadyCreated = File.Exists(Path.Combine(this.PathToCsvFolder, businessNodeSerialized));
            if (treeAlreadyCreated)
            {
                this.Root = SimpleMutableBusinessNodeNode.LoadFromJson(Path.Combine(this.PathToCsvFolder, businessNodeSerialized));
                //modify root Name
                this.Root.Item.Name = this.DestinationProject;
                //back to dataNode
                this.BusinessNodeDataNode = this.Root.ToBusinessNodeDataNodeRecursive();
                //save back
                this.Root.SaveToJson(Path.Combine(this.PathToCsvFolder, businessNodeDataNodeSerialized));
                this.BusinessNodeDataNodeSerialized = businessNodeDataNodeSerialized;
            }
        }

        public void GenerateExportConfig(
            string configurationRootFolder,
            string binaryProgram,
            string sourcePatToken,
            string templateFilesPath,
            string exportPath,
            string universalMapsProcessMaps,
            Engine.Configuration.ProjectExportBehavior defaultProjectExportBehavior)
        {
            System.Collections.Generic.Dictionary<SimpleMutableBusinessNodeNode, Engine.Configuration.ProjectExport.EngineConfiguration> mapTeamProjectToExportConfig;
            this.SourcePatToken = sourcePatToken;
            this.TemplateFilesPath = templateFilesPath;
            this.ExportPath = exportPath;
            this.UniversalMapsProcessMaps = universalMapsProcessMaps;
            if (defaultProjectExportBehavior == null)
            {
                defaultProjectExportBehavior = ProjectExportBehavior.GetDefault(true);
            }
            this.DefaultProjectExportBehavior = defaultProjectExportBehavior;
            this.ConfigurationRootFolder = configurationRootFolder;
            this.Root.GenerateExportConfigs(
                Path.Combine(this.ConfigurationRootFolder, "Export"),
                sourcePatToken,
                templateFilesPath,
                exportPath,
                universalMapsProcessMaps,
                defaultProjectExportBehavior,
                out mapTeamProjectToExportConfig);
            this.ImportExportBinaryProgram = new FileInfo(binaryProgram).FullName;
            this.Root.GenerateAllScript(Path.Combine(this.ConfigurationRootFolder, "Export"),
                Path.Combine(this.ConfigurationRootFolder, "Scripts\\ExportAll.bat"),
                binaryProgram,
                "export --consoleoutput -c");
        }

        public void GenerateImportConfig(
            string destinationPatToken,
            string destinationCollection,
            string destinationProjectProcessName,
            string defaultTeamName,
            string description,
            string importSourceCodeCredentialsPassword,
            string importSourceCodeCredentialsGitPassword,
            Engine.Configuration.ProjectImportBehavior defaultProjectImportBehavior,
            bool createDestinationProjectOnFirstImport,
            bool initializeProjectOnFirstImport,
            string areaInitializationFile,
            string iterationInitializationFile,
            string areaInitializationOutputFile,
            string iterationInitializationOutputFile,
            string newClassificationRootName,
            List<string> teamExclusions)
        {
            System.Collections.Generic.Dictionary<SimpleMutableBusinessNodeNode, Engine.Configuration.ProjectImport.EngineConfiguration> mapTeamProjectToImportConfig;
            SimpleMutableBusinessNodeNode firstProject;
            this.DestinationPatToken = destinationPatToken;
            this.DestinationCollection = destinationCollection;
            this.DestinationProjectProcessName = destinationProjectProcessName;
            this.DefaultTeamName = defaultTeamName;
            this.Description = description;
            this.ImportSourceCodeCredentialsPassword = importSourceCodeCredentialsPassword;
            this.ImportSourceCodeCredentialsGitPassword = importSourceCodeCredentialsGitPassword;
            if (defaultProjectImportBehavior == null)
            {
                defaultProjectImportBehavior = ProjectImportBehavior.GetDefault(true);
                //change some values
                defaultProjectImportBehavior.DeleteDefaultRepository = false;
                defaultProjectImportBehavior.InstallExtensions = false;
                defaultProjectImportBehavior.RenameRepository = false;
                defaultProjectImportBehavior.RenameTeam = false;
                defaultProjectImportBehavior.UseSecurityTasksFile = false;
                //defaultProjectImportBehavior.ImportTeamConfiguration = false;
            }
            this.DefaultProjectImportBehavior = defaultProjectImportBehavior;
            this.CreateDestinationProjectOnFirstImport = createDestinationProjectOnFirstImport;
            this.InitializeProjectOnFirstImport = initializeProjectOnFirstImport;
            this.AreaInitializationFile = areaInitializationFile;
            this.IterationInitializationFile = iterationInitializationFile;
            this.TeamExclusions = teamExclusions;

            //project init
            if (initializeProjectOnFirstImport)
            {
                if (File.Exists(this.AreaInitializationFile))
                {
                    ClassificationNodeMinimal areaClassificationNodeMinimal
                        = ClassificationNodeMinimal.LoadFromJson(this.AreaInitializationFile);
                    areaClassificationNodeMinimal.RenameRoot(newClassificationRootName);
                    Dictionary<int, SimpleMutableClassificationNodeMinimalWithIdNode> mapFromClassNodeIdsToTreeNodes;
                    this.AreaInitializationTree = areaClassificationNodeMinimal
                        .ToSimpleMutableClassificationNodeMinimalWithIdNode(out mapFromClassNodeIdsToTreeNodes);
                    this.AreaInitializationOutputFile = areaInitializationOutputFile;
                    this.AreaInitializationTree.SaveToJson(areaInitializationOutputFile);
                }
                else
                {
                    throw new FileNotFoundException(new FileInfo(this.AreaInitializationFile).FullName);
                }

                if (File.Exists(this.IterationInitializationFile))
                {
                    ClassificationNodeMinimal IterationClassificationNodeMinimal
                        = ClassificationNodeMinimal.LoadFromJson(this.IterationInitializationFile);
                    IterationClassificationNodeMinimal.RenameRoot(newClassificationRootName);
                    Dictionary<int, SimpleMutableClassificationNodeMinimalWithIdNode> mapFromClassNodeIdsToTreeNodes;
                    this.IterationInitializationTree = IterationClassificationNodeMinimal
                        .ToSimpleMutableClassificationNodeMinimalWithIdNode(out mapFromClassNodeIdsToTreeNodes);
                    this.IterationInitializationOutputFile = iterationInitializationOutputFile;
                    this.IterationInitializationTree.SaveToJson(iterationInitializationOutputFile);
                }
                else
                {
                    throw new FileNotFoundException(new FileInfo(this.IterationInitializationFile).FullName);
                }
            }

            this.Root.GenerateImportConfigs(
                Path.Combine(this.ConfigurationRootFolder, "Import"),
                destinationPatToken,
                destinationCollection,
                this.DestinationProject,
                destinationProjectProcessName,
                defaultTeamName,
                description,
                importSourceCodeCredentialsPassword,
                importSourceCodeCredentialsGitPassword,
                defaultProjectImportBehavior,
                this.ExportPath,
                this.UniversalMapsProcessMaps,
                this.CreateDestinationProjectOnFirstImport,
                this.InitializeProjectOnFirstImport,
                this.AreaInitializationOutputFile,
                this.IterationInitializationOutputFile,
                teamExclusions,
                //teamInclusions,
                out firstProject,
                out mapTeamProjectToImportConfig);

            //project init
            if (initializeProjectOnFirstImport)
            {
                string pathProjectInit = Path.Combine(this.ExportPath,
                    firstProject.Item.OrganizationName,
                    firstProject.Item.Name, "ProjectInitialization");
                this.Root.GenerateIterations(Path.Combine(pathProjectInit, "Iterations"));
                this.Root.GenerateAreas(Path.Combine(pathProjectInit, "Areas"));
                this.Root.GenerateTeams(Path.Combine(pathProjectInit, "Teams"));
            }

            this.Root.GenerateAllScript(Path.Combine(this.ConfigurationRootFolder, "Import"),
                Path.Combine(this.ConfigurationRootFolder, "Scripts\\ImportAll.bat"),
                this.ImportExportBinaryProgram,
                "import --consoleoutput -c");
        }

        public void GenerateBulkImportConfig(
            string destinationProcessTypeName,
                string reflectedWorkItemId,
                bool useTimeRestrictionClause,
                int numberOfDaysToImport,
                string migrationBinaryProgram,
                string universalAreaMaps,
            string universalIterationMaps
            )
        {
            System.Collections.Generic.Dictionary<SimpleMutableBusinessNodeNode, VstsSyncMigrator.Engine.Configuration.EngineConfiguration> mapTeamProjectToBulkImportConfig;
            this.DestinationProcessTypeName = destinationProcessTypeName;
            this.ReflectedWorkItemId = reflectedWorkItemId;
            this.NumberOfDaysToImport = numberOfDaysToImport;
            this.UseTimeRestrictionClause = useTimeRestrictionClause;
            this.UniversalAreaMaps = universalAreaMaps;
            this.UniversalIterationMaps = universalIterationMaps;
            this.Root.GenerateBulkImportConfigs(
                Path.Combine(this.ConfigurationRootFolder, "BulkImport"),
                this.SourcePatToken,
                this.DestinationPatToken,
                this.DestinationCollection,
                this.DestinationProject,
                this.DestinationProcessTypeName,
                this.ReflectedWorkItemId,
                this.UseTimeRestrictionClause,
                this.NumberOfDaysToImport,
                this.UniversalMapsProcessMaps,
                this.UniversalAreaMaps,
                this.UniversalIterationMaps,
                out mapTeamProjectToBulkImportConfig);
            this.MigrationBinaryProgram = migrationBinaryProgram;
            this.Root.GenerateAllScript(Path.Combine(this.ConfigurationRootFolder, "BulkImport"),
                Path.Combine(this.ConfigurationRootFolder, "Scripts\\BulkImportAll.bat"),
                this.MigrationBinaryProgram,
                "execute --consoleoutput -c");
        }

        public void GenerateAreaPathMap(
            bool includeAdditionalAreaMap,
            string finalAreaHierarchyPath,
            bool deleteExistingHierarchy
            )
        {
            this.AreaMap = ClassificationNodeMinimalMap.NewClassificationNodeMinimalMap();
            ClassificationNodeMinimalWithIdItem item = new ClassificationNodeMinimalWithIdItem(
                1,
                this.DestinationProject,
                Constants.AreaStructureType.ToLower(),
                false,
                $"\\{this.DestinationProject}\\{Constants.AreaStructureType}",
                null);
            this.SimpleMutableAreaMinimalWithIdNode = new SimpleMutableClassificationNodeMinimalWithIdNode(
                item
                );
            ClassificationNodeMinimal destinationClassificationNodeMinimalInitial = new ClassificationNodeMinimal()
            {
                Attributes = null,
                Children = new List<ClassificationNodeMinimal>(),
                HasChildren = false,
                Name = this.DestinationProject,
                Path = $"\\{this.DestinationProject}\\{Constants.AreaStructureType}",
                StructureType = Constants.AreaStructureType.ToLower()
            };
            foreach (var organizationalProject in this.BusinessHierarchyCsvList)
            {
                string organization = organizationalProject.OrganizationOrCollection;
                string project = organizationalProject.Project;
                string path = Path.Combine(this.ExportPath, organization, project, "Areas.json");
                string json = File.ReadAllText(path);
                //sanity check
                List<string> prefixedNodeNames = new List<string>();
                if (!string.IsNullOrEmpty(organizationalProject.AreaPrefixPath))
                {
                    prefixedNodeNames = organizationalProject.AreaPrefixPath
                        .Split(new string[] { Constants.DefaultPathSeparator }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    string expectedAreaPrefixPath
                        = $"{organizationalProject.Portfolio}\\{organizationalProject.ProgramOrProduct}";
                    if (expectedAreaPrefixPath != organizationalProject.AreaPrefixPath)
                    {
                        throw new ArgumentException($"expecting Area prefix [{expectedAreaPrefixPath}] but got [{organizationalProject.AreaPrefixPath}]");
                    }
                }
                else
                {
                    //auto add project name
                    prefixedNodeNames.Add(organizationalProject.Project);
                }
                ClassificationNodeMinimal sourceClassificationNodeMinimal = JsonConvert.DeserializeObject<ClassificationNodeMinimal>(json);
                Dictionary<int, SimpleMutableClassificationNodeMinimalWithIdNode> mapFromClassNodeIdsToTreeNodes;
                SimpleMutableClassificationNodeMinimalWithIdNode simpleMutableClassificationNodeMinimalWithIdNode
                    = sourceClassificationNodeMinimal.ToSimpleMutableClassificationNodeMinimalWithIdNode(out mapFromClassNodeIdsToTreeNodes);
                this.AreaMap.AddClassificationNodeMinimal(organization, project, simpleMutableClassificationNodeMinimalWithIdNode);
                if (string.IsNullOrEmpty(organizationalProject.AreaBasePathList))
                {

                    var retRoot = this.SimpleMutableAreaMinimalWithIdNode.AddTree(prefixedNodeNames, simpleMutableClassificationNodeMinimalWithIdNode);
                    var retRootMinimal = destinationClassificationNodeMinimalInitial.AddTree(prefixedNodeNames, simpleMutableClassificationNodeMinimalWithIdNode);
                    this.SerializableAreaMap.Map[new TripleKey(organization, project, simpleMutableClassificationNodeMinimalWithIdNode.Name)] = retRoot.Item;

                }
                else
                {
                    //TODO - base path not empty so add subtrees only
                    List<string> subPaths = organizationalProject.AreaBasePathList.GetList(Constants.DefaultDelineatorForListsInCsv);
                    foreach (var subPath in subPaths)
                    {
                        var subTree = simpleMutableClassificationNodeMinimalWithIdNode.GetSubTree(subPath, Constants.AreaStructureType);

                        var retRoot = this.SimpleMutableAreaMinimalWithIdNode.AddTree(prefixedNodeNames, subTree);
                        var retRootMinimal = destinationClassificationNodeMinimalInitial.AddTree(prefixedNodeNames, subTree);
                        this.SerializableAreaMap.Map[new TripleKey(organization, project, subPath)] = retRoot.Item;

                    }
                }
            }

            if (includeAdditionalAreaMap)
            {
                ClassificationNodeMinimal classificationNodeMinimalFinal = null;
                if (!deleteExistingHierarchy && finalAreaHierarchyPath != null && File.Exists(finalAreaHierarchyPath))
                {
                    classificationNodeMinimalFinal = ClassificationNodeMinimal.LoadFromJson(finalAreaHierarchyPath);
                }
                else
                {
                    //load what got generated above
                    classificationNodeMinimalFinal = destinationClassificationNodeMinimalInitial;
                    classificationNodeMinimalFinal.SaveToJson(finalAreaHierarchyPath);
                }
                Dictionary<int, SimpleMutableClassificationNodeMinimalWithIdNode> mapFromClassNodeIdsToTreeNodesFinal;
                SimpleMutableClassificationNodeMinimalWithIdNode simpleMutableClassificationNodeMinimalWithIdNodeFinal
                    = classificationNodeMinimalFinal.ToSimpleMutableClassificationNodeMinimalWithIdNode(out mapFromClassNodeIdsToTreeNodesFinal);
                this.SimpleMutableAreaMinimalWithIdNodeFinal = simpleMutableClassificationNodeMinimalWithIdNodeFinal;
                foreach (var organizationalProject in this.BusinessHierarchyCsvList)
                {
                    string organization = organizationalProject.OrganizationOrCollection;
                    string project = organizationalProject.Project;
                    var additionalAreaMap = organizationalProject.AreaPathMapDictionary.GetDictionary(Constants.DefaultDelineatorForListsInCsv, Constants.DefaultKeyValueSeparator);
                    if (additionalAreaMap != null)
                    {
                        foreach (var map in additionalAreaMap)
                        {
                            if (this.SerializableAreaMap.Map.ContainsKey(new TripleKey(organization, project, map.Key)))
                            {
                                string currentTargetPath = this.SerializableAreaMap.Map[new TripleKey(organization, project, map.Key)].Path;
                                System.Diagnostics.Trace.WriteLine($"Area path {map.Key} currently mapped to {currentTargetPath}");
                                string requestedTargetPath = simpleMutableClassificationNodeMinimalWithIdNodeFinal.GetNormalizedPath($"{this.DestinationProject}\\{map.Value}", Constants.AreaStructureType);
                                if (currentTargetPath != requestedTargetPath)
                                {
                                    //remapping
                                    //check source and target paths exists
                                    var sourceAreas = this.AreaMap.GetClassificationNodeMinimal(organization, project);
                                    bool sourcePathExists = sourceAreas.HasPath(map.Key, Constants.AreaStructureType);
                                    var sourceNodeWithPath = sourceAreas.GetNodeWithPath(map.Key, Constants.AreaStructureType);
                                    if (sourcePathExists)
                                    {
                                        bool targetPathExists = this.SimpleMutableAreaMinimalWithIdNodeFinal.HasPath($"{this.DestinationProject}\\{map.Value}", Constants.AreaStructureType);
                                        var targetNodeWithPath = this.SimpleMutableAreaMinimalWithIdNodeFinal.GetNodeWithPath($"{this.DestinationProject}\\{map.Value}", Constants.AreaStructureType);
                                        //sanity check
                                        if (!targetPathExists)
                                        {
                                            throw new ArgumentException($"Could not find target area path [{ this.DestinationProject }\\{ map.Value}]. Unable to map source area path {map.Key}");
                                        }
                                        if (requestedTargetPath != targetNodeWithPath.Item.Path)
                                        {
                                            throw new ArgumentException("these should always be equal");
                                        }
                                        if (targetPathExists)
                                        {
                                            System.Diagnostics.Trace.WriteLine($"REMAPPING Area path {map.Key} previously mapped to {currentTargetPath} now mapped to {requestedTargetPath}");
                                            this.SerializableAreaMap.Map[new TripleKey(organization, project, map.Key)] = targetNodeWithPath.Item;
                                        }
                                        else
                                        {
                                            throw new ArgumentException($"need Area path {map.Value} to exist in target");
                                        }
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"need Area path {map.Key} to exist in source");
                                    }
                                }
                                else
                                {
                                    //we good
                                }
                            }
                            else
                            {
                                //check source and target paths exists
                                var sourceAreas = this.AreaMap.GetClassificationNodeMinimal(organization, project);
                                bool sourcePathExists = sourceAreas.HasPath(map.Key, Constants.AreaStructureType);
                                var sourceNodeWithPath = sourceAreas.GetNodeWithPath(map.Key, Constants.AreaStructureType);
                                if (sourcePathExists)
                                {
                                    bool targetPathExists = this.SimpleMutableAreaMinimalWithIdNodeFinal.HasPath($"{this.DestinationProject}\\{map.Value}", Constants.AreaStructureType);
                                    var targetNodeWithPath = this.SimpleMutableAreaMinimalWithIdNodeFinal.GetNodeWithPath($"{this.DestinationProject}\\{map.Value}", Constants.AreaStructureType);
                                    if (targetPathExists)
                                    {
                                        this.SerializableAreaMap.Map[new TripleKey(organization, project, map.Key)] = targetNodeWithPath.Item;
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"need Area path {map.Value} to exist in target");
                                    }
                                }
                                else
                                {
                                    throw new ArgumentException($"need Area path {map.Key} to exist in source");
                                }
                            }
                        }
                    }
                }
            }
        }
        public void GenerateIterationPathMap(
            bool includeAdditionalIterationMap,
            string finalIterationHierarchyPath,
            bool deleteExistingHierarchy
            )
        {
            this.IterationMap = ClassificationNodeMinimalMap.NewClassificationNodeMinimalMap();
            ClassificationNodeMinimalWithIdItem item = new ClassificationNodeMinimalWithIdItem(
                1,
                this.DestinationProject,
                Constants.IterationStructureType.ToLower(),
                false,
                $"\\{this.DestinationProject}\\{Constants.IterationStructureType}",
                null);
            this.SimpleMutableIterationMinimalWithIdNode = new SimpleMutableClassificationNodeMinimalWithIdNode(
                item
                );
            ClassificationNodeMinimal destinationClassificationNodeMinimalInitial = new ClassificationNodeMinimal()
            {
                Attributes = null,
                Children = new List<ClassificationNodeMinimal>(),
                HasChildren = false,
                Name = this.DestinationProject,
                Path = $"\\{this.DestinationProject}\\{Constants.IterationStructureType}",
                StructureType = Constants.IterationStructureType.ToLower()
            };
            foreach (var organizationalProject in this.BusinessHierarchyCsvList)
            {
                string organization = organizationalProject.OrganizationOrCollection;
                string project = organizationalProject.Project;
                string path = Path.Combine(this.ExportPath, organization, project, "Iterations.json");
                string json = File.ReadAllText(path);
                //sanity check
                List<string> prefixedNodeNames = new List<string>();
                if (!string.IsNullOrEmpty(organizationalProject.IterationPrefixPath))
                {
                    prefixedNodeNames = organizationalProject.IterationPrefixPath
                        .Split(new string[] { Constants.DefaultPathSeparator }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    string expectedIterationPrefixPath
                        = $"{organizationalProject.Portfolio}\\{organizationalProject.ProgramOrProduct}";
                    if (expectedIterationPrefixPath != organizationalProject.IterationPrefixPath)
                    {
                        throw new ArgumentException($"expecting iteration prefix [{expectedIterationPrefixPath}] but got [{organizationalProject.IterationPrefixPath}]");
                    }
                }
                else
                {
                    //auto add project name
                    prefixedNodeNames.Add(organizationalProject.Project);
                }
                ClassificationNodeMinimal sourceClassificationNodeMinimal = JsonConvert.DeserializeObject<ClassificationNodeMinimal>(json);
                Dictionary<int, SimpleMutableClassificationNodeMinimalWithIdNode> mapFromClassNodeIdsToTreeNodes;
                SimpleMutableClassificationNodeMinimalWithIdNode simpleMutableClassificationNodeMinimalWithIdNode
                    = sourceClassificationNodeMinimal.ToSimpleMutableClassificationNodeMinimalWithIdNode(out mapFromClassNodeIdsToTreeNodes);
                this.IterationMap.AddClassificationNodeMinimal(organization, project, simpleMutableClassificationNodeMinimalWithIdNode);
                if (string.IsNullOrEmpty(organizationalProject.IterationBasePathList))
                {
                    var retRoot = this.SimpleMutableIterationMinimalWithIdNode.AddTree(prefixedNodeNames, simpleMutableClassificationNodeMinimalWithIdNode);
                    var retRootMinimal = destinationClassificationNodeMinimalInitial.AddTree(prefixedNodeNames, simpleMutableClassificationNodeMinimalWithIdNode);
                    this.SerializableIterationMap.Map[new TripleKey(organization, project, simpleMutableClassificationNodeMinimalWithIdNode.Name)] = retRoot.Item;

                }
                else
                {
                    //TODO - base path not empty so add subtrees only
                    List<string> subPaths = organizationalProject.IterationBasePathList.GetList(Constants.DefaultDelineatorForListsInCsv);
                    foreach (var subPath in subPaths)
                    {
                        var subTree = simpleMutableClassificationNodeMinimalWithIdNode.GetSubTree(subPath, Constants.IterationStructureType);

                        var retRoot = this.SimpleMutableIterationMinimalWithIdNode.AddTree(prefixedNodeNames, subTree);
                        var retRootMinimal = destinationClassificationNodeMinimalInitial.AddTree(prefixedNodeNames, subTree);
                        this.SerializableIterationMap.Map[new TripleKey(organization, project, subPath)] = retRoot.Item;

                    }
                }
            }
            if (includeAdditionalIterationMap)
            {
                ClassificationNodeMinimal classificationNodeMinimalFinal = null;
                if (!deleteExistingHierarchy && finalIterationHierarchyPath != null && File.Exists(finalIterationHierarchyPath))
                {
                    classificationNodeMinimalFinal = ClassificationNodeMinimal.LoadFromJson(finalIterationHierarchyPath);
                }
                else
                {
                    //load what got generated above
                    classificationNodeMinimalFinal = destinationClassificationNodeMinimalInitial;
                    classificationNodeMinimalFinal.SaveToJson(finalIterationHierarchyPath);
                }
                Dictionary<int, SimpleMutableClassificationNodeMinimalWithIdNode> mapFromClassNodeIdsToTreeNodesFinal;
                SimpleMutableClassificationNodeMinimalWithIdNode simpleMutableClassificationNodeMinimalWithIdNodeFinal
                    = classificationNodeMinimalFinal.ToSimpleMutableClassificationNodeMinimalWithIdNode(out mapFromClassNodeIdsToTreeNodesFinal);
                this.SimpleMutableIterationMinimalWithIdNodeFinal = simpleMutableClassificationNodeMinimalWithIdNodeFinal;
                foreach (var organizationalProject in this.BusinessHierarchyCsvList)
                {
                    string organization = organizationalProject.OrganizationOrCollection;
                    string project = organizationalProject.Project;
                    var additionalIterationMap = organizationalProject.IterationPathMapDictionary.GetDictionary(Constants.DefaultDelineatorForListsInCsv, Constants.DefaultKeyValueSeparator);
                    if (additionalIterationMap != null)
                    {
                        foreach (var map in additionalIterationMap)
                        {
                            if (this.SerializableIterationMap.Map.ContainsKey(new TripleKey(organization, project, map.Key)))
                            {
                                string currentTargetPath = this.SerializableIterationMap.Map[new TripleKey(organization, project, map.Key)].Path;
                                System.Diagnostics.Trace.WriteLine($"Iteration path {map.Key} currently mapped to {currentTargetPath}");
                                string requestedTargetPath = simpleMutableClassificationNodeMinimalWithIdNodeFinal.GetNormalizedPath($"{this.DestinationProject}\\{map.Value}", Constants.IterationStructureType);
                                if (currentTargetPath != requestedTargetPath)
                                {
                                    //remapping
                                    //check source and target paths exists
                                    var sourceIterations = this.IterationMap.GetClassificationNodeMinimal(organization, project);
                                    bool sourcePathExists = sourceIterations.HasPath(map.Key, Constants.IterationStructureType);
                                    var sourceNodeWithPath = sourceIterations.GetNodeWithPath(map.Key, Constants.IterationStructureType);
                                    if (sourcePathExists)
                                    {
                                        bool targetPathExists = this.SimpleMutableIterationMinimalWithIdNodeFinal.HasPath($"{this.DestinationProject}\\{map.Value}", Constants.IterationStructureType);
                                        var targetNodeWithPath = this.SimpleMutableIterationMinimalWithIdNodeFinal.GetNodeWithPath($"{this.DestinationProject}\\{map.Value}", Constants.IterationStructureType);
                                        //sanity check
                                        if (!targetPathExists)
                                        {
                                            throw new ArgumentException($"Could not find target iteration path [{ this.DestinationProject }\\{ map.Value}]. Unable to map source iteration path {map.Key}");
                                        }
                                        if (requestedTargetPath != targetNodeWithPath.Item.Path)
                                        {
                                            throw new ArgumentException("these should always be equal");
                                        }
                                        if (targetPathExists)
                                        {
                                            System.Diagnostics.Trace.WriteLine($"REMAPPING Iteration path {map.Key} previously mapped to {currentTargetPath} now mapped to {requestedTargetPath}");
                                            this.SerializableIterationMap.Map[new TripleKey(organization, project, map.Key)] = targetNodeWithPath.Item;
                                        }
                                        else
                                        {
                                            throw new ArgumentException($"need Iteration path {map.Value} to exist in target");
                                        }
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"need Iteration path {map.Key} to exist in source");
                                    }
                                }
                                else
                                {
                                    //we good
                                }
                            }
                            else
                            {
                                //check source and target paths exists
                                var sourceIterations = this.IterationMap.GetClassificationNodeMinimal(organization, project);
                                bool sourcePathExists = sourceIterations.HasPath(map.Key, Constants.IterationStructureType);
                                var sourceNodeWithPath = sourceIterations.GetNodeWithPath(map.Key, Constants.IterationStructureType);
                                if (sourcePathExists)
                                {
                                    bool targetPathExists = this.SimpleMutableIterationMinimalWithIdNodeFinal.HasPath($"{this.DestinationProject}\\{map.Value}", Constants.IterationStructureType);
                                    var targetNodeWithPath = this.SimpleMutableIterationMinimalWithIdNodeFinal.GetNodeWithPath($"{this.DestinationProject}\\{map.Value}", Constants.IterationStructureType);
                                    if (targetPathExists)
                                    {
                                        this.SerializableIterationMap.Map[new TripleKey(organization, project, map.Key)] = targetNodeWithPath.Item;
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"need Iteration path {map.Value} to exist in target");
                                    }
                                }
                                else
                                {
                                    throw new ArgumentException($"need Iteration path {map.Key} to exist in source");
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Migration LoadFromCsv(string pathToCsv,
            string destinationProject,
            bool sortByNameRecursively,
            BusinessNodeType programOrProductBusinessNodeType,
            string programOrProductTeamSuffix = null,
            string portfolioTeamSuffix = "Portfolio Team",
            string projectTeamSuffix = "Team")
        {
            Migration migration = new Migration();
            migration.PathToCsv = pathToCsv;
            if (!File.Exists(pathToCsv))
            {
                FileInfo pathToCsvInfo = new FileInfo(pathToCsv);
                throw new FileNotFoundException(pathToCsvInfo.FullName);
            }
            migration.DestinationProject = destinationProject;
            migration.FiscalMonthStart = 1;
            migration.DefaultPortfolioCadences = new List<Cadence>() { new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.FiscalSemester, 0, 0, DayOfWeek.Sunday, migration.FiscalMonthStart) };
            migration.DefaultProgramOrProductTeamCadences = new List<Cadence>() { new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.ProgramIncrement, 2, 5, DayOfWeek.Sunday, migration.FiscalMonthStart) };
            migration.DefaultTeamProjectTeamCadences = new List<Cadence>() { new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.Sprint, 2, 0, DayOfWeek.Sunday, migration.FiscalMonthStart) };

            migration.ProgramOrProductBusinessNodeType = programOrProductBusinessNodeType;

            if (programOrProductBusinessNodeType == BusinessNodeType.Program || programOrProductBusinessNodeType == BusinessNodeType.Product)
            {
                //good
            }
            else
            {
                throw new ArgumentException($"we must have BusinessNodeType either a Program or Product but instead found {programOrProductBusinessNodeType.ToString()}");
            }

            migration.PortfolioTeamSuffix = portfolioTeamSuffix;
            migration.ProgramOrProductTeamSuffix = programOrProductTeamSuffix != null ? programOrProductTeamSuffix : $"{programOrProductBusinessNodeType.ToString()} Team";
            migration.ProjectTeamSuffix = projectTeamSuffix;
            migration.SortByNameRecursively = sortByNameRecursively;

            return migration;
        }
    }
}
