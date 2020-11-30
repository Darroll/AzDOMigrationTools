using ADO.Engine.BusinessEntities;
using ADO.Engine.Utilities;
using ADO.Engine.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace ADO.Configuration.Tests
{
    [TestClass]
    public class TestGenerateConfigs
    {
        private static string sourcePatToken;
        private static string templateFilesPath;
        private static string exportPath;
        private static ProjectExportBehavior defaultProjectExportBehavior;
        private static ProjectImportBehavior defaultProjectImportBehavior;
        private static string importSourceCodeCredentialsPassword;
        private static string importSourceCodeCredentialsGitPassword;
        private static string destinationCollection;
        private static string destinationProject;
        private static string destinationProjectProcessName;
        private static string defaultTeamName;
        private static string description;
        private static SimpleMutableBusinessNodeNode root;
        private static string destinationProcessTypeName;
        private static string reflectedWorkItemId;
        private static int numberOfDaysToImport;
        private static string universalMapsProcessMaps;
        private static List<string> teamExclusions;
        //private static List<string> teamInclusions;
        private static string destinationPatToken;

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            sourcePatToken = "Environment.PatToken";
            destinationPatToken = "Environment.PatToken";
            templateFilesPath = "..\\..\\..\\ADO.Engine\\TemplateFiles";
            exportPath = "..\\..\\..\\Templates\\V2\\ExtractedTemplate";
            defaultProjectExportBehavior = ProjectExportBehavior.GetDefault(true);
            defaultProjectImportBehavior = ProjectImportBehavior.GetDefault(true);
            //change some values
            defaultProjectImportBehavior.DeleteDefaultRepository = false;
            defaultProjectImportBehavior.InstallExtensions = false;
            defaultProjectImportBehavior.RenameRepository = false;
            defaultProjectImportBehavior.RenameTeam = false;
            defaultProjectImportBehavior.UseSecurityTasksFile = false;
            importSourceCodeCredentialsPassword = "Environment.PatToken";
            importSourceCodeCredentialsGitPassword = "Environment.GitHubPassword";
            destinationCollection = "agileatscale";
            int projectNumber = 6;
            destinationProject = $"OneProjectV2_{projectNumber.ToString().PadLeft(2,'0')}";
            destinationProjectProcessName = "MigrateableScrum";
            //used for Bulk Import
            destinationProcessTypeName = "Scrum";// "MigrateableScrum";
            defaultTeamName = $"{destinationProject} Team";
            description = "One Project to Rule Them All V2";
            reflectedWorkItemId = "ReflectedWorkItemId";
            numberOfDaysToImport = 365*2;
            universalMapsProcessMaps = @"C:\Users\dawal\source\repos\MigrationVNext\ADOTools2\ADO.ProcessMapping.Tests\Output\OneMapToRuleThemAll.json";

            teamExclusions = new List<string>() { "someExcludedTeam" };
            //teamInclusions = new List<string>() { "someIncludedTeam" }; //actually grab this from business hierarchy csv

            root = SimpleMutableBusinessNodeNode.LoadFromJson(@"..\..\Input\businessNodeFromCsv.json");
            //modify root Name
            root.Item.Name = destinationProject;
            //back to dataNode
            BusinessNodeDataNode businessNodeDataNode = root.ToBusinessNodeDataNodeRecursive();
            //save back
            root.SaveToJson(@"..\..\Input\businessNodeFromCsvNEW.json");
        }

        [TestMethod]
        public void Test_Generate_Export_Config()
        {  
            System.Collections.Generic.Dictionary<SimpleMutableBusinessNodeNode, Engine.Configuration.ProjectExport.EngineConfiguration> mapTeamProjectToExportConfig;
            root.GenerateExportConfigs($"..\\..\\Output\\Configuration\\Phase1\\Export",
                sourcePatToken,
                templateFilesPath,
                exportPath,
                universalMapsProcessMaps,
                defaultProjectExportBehavior,
                out mapTeamProjectToExportConfig);
            root.GenerateAllScript($"..\\..\\Output\\Configuration\\Phase1\\Export", 
                $"..\\..\\Output\\Configuration\\Phase1\\Scripts\\ExportAll.bat",
                $"..\\..\\..\\..\\ADOTools2\\ADO.Engine.Console\\bin\\Debug\\ADOTools2.exe",
                "export --consoleoutput -c");
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Generate_Import_Config()
        {
            System.Collections.Generic.Dictionary<SimpleMutableBusinessNodeNode, Engine.Configuration.ProjectImport.EngineConfiguration> mapTeamProjectToImportConfig;
            SimpleMutableBusinessNodeNode firstProject;
            string areaInitializationTreePath = $"..\\..\\Input\\oneProjectAreaInitialization.json";
            string iterationInitializationTreePath = $"..\\..\\Input\\oneProjectIterationInitialization.json";
            //rewrite project root name
            SimpleMutableClassificationNodeMinimalWithIdNode areaInitializationTree
                = SimpleMutableClassificationNodeMinimalWithIdNode.LoadFromJson(areaInitializationTreePath);
            ClassificationNodeMinimalWithId areaClassificationNodeMinimalWithId
                        = ClassificationNodeMinimalWithId.LoadFromJson(areaInitializationTreePath);
            areaClassificationNodeMinimalWithId.RenameRoot(destinationProject);
            areaClassificationNodeMinimalWithId.SaveToJson(areaInitializationTreePath);
            SimpleMutableClassificationNodeMinimalWithIdNode iterationInitializationTree
                = SimpleMutableClassificationNodeMinimalWithIdNode.LoadFromJson(iterationInitializationTreePath);
            ClassificationNodeMinimalWithId IterationClassificationNodeMinimalWithId
                        = ClassificationNodeMinimalWithId.LoadFromJson(iterationInitializationTreePath);
            IterationClassificationNodeMinimalWithId.RenameRoot(destinationProject);
            IterationClassificationNodeMinimalWithId.SaveToJson(iterationInitializationTreePath);
            root.GenerateImportConfigs($"..\\..\\Output\\Configuration\\Phase1\\Import",
                sourcePatToken,
                destinationCollection,
                destinationProject,
                destinationProjectProcessName,
                defaultTeamName,
                description,
                importSourceCodeCredentialsPassword,
                importSourceCodeCredentialsGitPassword,
                defaultProjectImportBehavior,
                exportPath,
                universalMapsProcessMaps,
                true,
                true, 
                areaInitializationTreePath,
                iterationInitializationTreePath,                
                teamExclusions,
                //teamInclusions,
                out firstProject,
                out mapTeamProjectToImportConfig);

            //project init
            string pathProjectInit = Path.Combine(exportPath, firstProject.Item.OrganizationName,
                firstProject.Item.Name, "ProjectInitialization");
            root.GenerateIterations(Path.Combine(pathProjectInit, "Iterations"));
            root.GenerateAreas(Path.Combine(pathProjectInit, "Areas"));
            root.GenerateTeams(Path.Combine(pathProjectInit, "Teams"));

            root.GenerateAllScript($"..\\..\\Output\\Configuration\\Phase1\\Import",
                $"..\\..\\Output\\Configuration\\Phase1\\Scripts\\ImportAll.bat",
                $"..\\..\\..\\..\\ADOTools2\\ADO.Engine.Console\\bin\\Debug\\ADOTools2.exe",
                "import --consoleoutput -c");
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Generate_Bulk_Import_Config()
        {
            System.Collections.Generic.Dictionary<SimpleMutableBusinessNodeNode, VstsSyncMigrator.Engine.Configuration.EngineConfiguration> mapTeamProjectToBulkImportConfig;
            root.GenerateBulkImportConfigs($"..\\..\\Output\\Configuration\\Phase1\\BulkImport",
                sourcePatToken,
                destinationPatToken,
                destinationCollection,
                destinationProject,
                destinationProcessTypeName,
                reflectedWorkItemId,
                true,
                numberOfDaysToImport,
                universalMapsProcessMaps,
                null,
                null,
                out mapTeamProjectToBulkImportConfig);
            root.GenerateAllScript($"..\\..\\Output\\Configuration\\Phase1\\BulkImport",
                $"..\\..\\Output\\Configuration\\Phase1\\Scripts\\BulkImportAll.bat",
                $"..\\..\\..\\..\\ADOTools1\\src\\VstsSyncMigrator.Console\\bin\\Debug\\migration.exe",
                "execute --consoleoutput -c");
            Assert.IsTrue(true);
        }
    }
}
