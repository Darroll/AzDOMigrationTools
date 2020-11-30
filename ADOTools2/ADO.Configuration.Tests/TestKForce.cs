using System;
using System.Collections.Generic;
using System.IO;
using ADO.Engine.BusinessEntities;
using ADO.Engine.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ADO.Configuration.Tests
{
    [TestClass]
    public class TestKForce
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
        private static string configRoot;

        private static byte fiscalMonthStart;
        private static List<Cadence> defaultPortfolioCadences;
        private static List<Cadence> defaultProgramOrProductTeamCadences;
        private static List<Cadence> defaultTeamProjectTeamCadences;
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
            fiscalMonthStart = 1;
            defaultPortfolioCadences = new List<Cadence>() { new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.FiscalSemester, 0, 0, DayOfWeek.Sunday, fiscalMonthStart) };
            defaultProgramOrProductTeamCadences = new List<Cadence>() { new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.ProgramIncrement, 2, 5, DayOfWeek.Sunday, fiscalMonthStart) };
            defaultTeamProjectTeamCadences = new List<Cadence>()
            { new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.Sprint, 2, 0, DayOfWeek.Sunday, fiscalMonthStart) };


            configRoot = "KForce";
            sourcePatToken = "Environment.PatToken";
            destinationPatToken = "Environment.PatToken";
            templateFilesPath = "..\\..\\..\\ADO.Engine\\TemplateFiles";
            exportPath = "C:\\Templates\\V2\\ExtractedTemplate";
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
            destinationCollection = "kforce";
            int projectNumber = 1;
            destinationProject = $"OneProjectPOC_{projectNumber.ToString().PadLeft(2, '0')}";
            destinationProjectProcessName = "Agile001";
            //used for Bulk Import
            destinationProcessTypeName = "Agile001";
            defaultTeamName = $"{destinationProject} Team";
            description = "One Project to Rule Them All";
            reflectedWorkItemId = "ReflectedWorkItemId";
            numberOfDaysToImport = 365;
            universalMapsProcessMaps = @"C:\Users\dawal\source\repos\MigrationVNext\ADOTools2\ADO.ProcessMapping.Tests\Output\OneMapToRuleThemAll.json";

            teamExclusions = new List<string>() { "someExcludedTeam" };
            //teamInclusions = new List<string>() { "someIncludedTeam" }; //actually grab this from business hierarchy csv

            root = SimpleMutableBusinessNodeNode.LoadFromJson(@"..\..\Input\KForce\businessNodeFromCsv2.json");
            //modify root Name
            root.Item.Name = destinationProject;
            //back to dataNode
            BusinessNodeDataNode businessNodeDataNode = root.ToBusinessNodeDataNodeRecursive();
            //save back
            root.SaveToJson(@"..\..\Input\KForce\businessNodeFromCsvNEW2.json");
        }

        [TestMethod]
        public void TestGenerateBusinessNodeFromCsv()
        {
            List<BusinessHierarchyCsv> businessHierarchyCsvList;
            BusinessNode businessNode
                = BusinessNode.LoadFromCsv(@"..\..\Input\KForce\businessHierarchyCsv2.csv",
                    destinationProject, true, BusinessNodeType.Program, defaultPortfolioCadences,
                    defaultProgramOrProductTeamCadences,
                    defaultTeamProjectTeamCadences,
                    "Portfolio Team",
                    "Program Team",
                    "Team",
                    out businessHierarchyCsvList);

            string json = JsonConvert.SerializeObject(businessNode, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\KForce\businessNodeFromCsv2.json", json);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Generate_Export_Config()
        {
            System.Collections.Generic.Dictionary<SimpleMutableBusinessNodeNode, Engine.Configuration.ProjectExport.EngineConfiguration> mapTeamProjectToExportConfig;
            root.GenerateExportConfigs($"..\\..\\Output\\Configuration\\{configRoot}\\Export",
                sourcePatToken,
                templateFilesPath,
                exportPath,
                universalMapsProcessMaps,
                defaultProjectExportBehavior,
                out mapTeamProjectToExportConfig);
            root.GenerateAllScript($"..\\..\\Output\\Configuration\\{configRoot}\\Export",
                $"..\\..\\Output\\Configuration\\{configRoot}\\Scripts\\ExportAll.bat",
                $"..\\..\\..\\..\\ADOTools2\\ADO.Engine.Console\\bin\\Debug\\ADOTools2.exe",
                "export --consoleoutput -c");
            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void Test_Generate_Import_Config()
        {
            System.Collections.Generic.Dictionary<SimpleMutableBusinessNodeNode, Engine.Configuration.ProjectImport.EngineConfiguration> mapTeamProjectToImportConfig;
            SimpleMutableBusinessNodeNode firstProject;
            root.GenerateImportConfigs($"..\\..\\Output\\Configuration\\{configRoot}\\Import",
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
                false,
                false,
                null,
                null,
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

            root.GenerateAllScript($"..\\..\\Output\\Configuration\\{configRoot}\\Import",
                $"..\\..\\Output\\Configuration\\{configRoot}\\Scripts\\ImportAll.bat",
                $"..\\..\\..\\..\\ADOTools2\\ADO.Engine.Console\\bin\\Debug\\ADOTools2.exe",
                "import --consoleoutput -c");
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Generate_Bulk_Import_Config()
        {
            System.Collections.Generic.Dictionary<SimpleMutableBusinessNodeNode, VstsSyncMigrator.Engine.Configuration.EngineConfiguration> mapTeamProjectToBulkImportConfig;
            root.GenerateBulkImportConfigs($"..\\..\\Output\\Configuration\\{configRoot}\\BulkImport",
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
            root.GenerateAllScript($"..\\..\\Output\\Configuration\\{configRoot}\\BulkImport",
                $"..\\..\\Output\\Configuration\\{configRoot}\\Scripts\\BulkImportAll.bat",
                $"..\\..\\..\\..\\ADOTools1\\src\\VstsSyncMigrator.Console\\bin\\Debug\\migration.exe",
                "execute --consoleoutput -c");
            Assert.IsTrue(true);
        }
    }
}
