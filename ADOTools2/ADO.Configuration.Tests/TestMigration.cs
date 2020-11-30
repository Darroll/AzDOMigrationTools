using System.Collections.Generic;
using ADO.Engine.BusinessEntities;
using ADO.Engine.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADO.Configuration.Tests
{
    [TestClass]
    public class TestMigration
    {
        private int projectNumber = 9;

        [TestMethod]
        public void Test_do_migration_CCL()
        {
            string configRoot = "CCL";
            string destinationProject = $"OneProject_{projectNumber.ToString().PadLeft(2, '0')}";
            Migration migration = Migration.LoadFromCsv(
                $"..\\..\\Input\\{configRoot}\\businessHierarchyCsv.csv",
                destinationProject,
                true,
                BusinessNodeType.Product
                );

            migration.GenerateBusinessNodeAndDataNodeFromCsv(
                $"..\\..\\Output\\{configRoot}\\businessNodeFromCsv.json",
                $"..\\..\\Output\\{configRoot}\\businessNodeDataNodeFromCsv.json");

            migration.GenerateExportConfig(
                $"..\\..\\Output\\Configuration\\{configRoot}",
                $"..\\..\\..\\..\\ADOTools2\\ADO.Engine.Console\\bin\\Debug\\ADOTools2.exe",
                "Environment.PatTokenCCL",
                "..\\..\\..\\ADO.Engine\\TemplateFiles",
                "C:\\Templates\\V2\\ExtractedTemplate",
                @"C:\Users\dawal\source\repos\MigrationVNext\ADOTools2\ADO.ProcessMapping.Tests\Output\OneMapToRuleThemAll.json",
                null
                );

            //do this before import config
            string universalAreaMaps = $"..\\..\\Output\\{configRoot}\\oneProjectAreaMap.json";
            string universalIterationMaps = $"..\\..\\Output\\{configRoot}\\oneProjectIterationMap.json";

            //map area paths
            migration.GenerateAreaPathMap(true, $"..\\..\\Input\\{configRoot}\\oneProjectAreaHierarchy.json", true);
            migration.SimpleMutableAreaMinimalWithIdNode.SaveToJson($"..\\..\\Output\\{configRoot}\\oneProjectAreaHierarchy.json");
            migration.SerializableAreaMap.SaveToJson(universalAreaMaps);

            //map iterations
            migration.GenerateIterationPathMap(true, $"..\\..\\Input\\{configRoot}\\oneProjectIterationHierarchy.json", true);
            migration.SimpleMutableIterationMinimalWithIdNode.SaveToJson($"..\\..\\Output\\{configRoot}\\oneProjectIterationHierarchy.json");
            migration.SerializableIterationMap.SaveToJson(universalIterationMaps);

            List<string> teamExclusions = new List<string>() { "someExcludedTeam" };
            string areaInitializationFile = null;
            string iterationInitializationFile = null;
            migration.GenerateImportConfig(
                "Environment.PatToken",
                "ccldevopsabcs",
                "CCL-One",
                $"{destinationProject} Team",
                "One Project to Rule Them All",
                "Environment.PatTokenCCL",
                "Environment.GitHubPassword",
                null,
                true,
                false,
                areaInitializationFile,
                iterationInitializationFile,
                null,
                null,
                destinationProject,
                teamExclusions
                );            

            migration.GenerateBulkImportConfig(
                "Agile",
                "ReflectedWorkItemId",
                true,
                90,
                $"..\\..\\..\\..\\ADOTools1\\src\\VstsSyncMigrator.Console\\bin\\Debug\\migration.exe",
                universalAreaMaps,
                universalIterationMaps
                );

            ////Can't save settings due to self referencing trees e.g. root
            ////find a way to serialize trees
            //migration.SaveToJson($"..\\..\\Output\\{configRoot}\\CCL2-migration.json");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_do_migration_CCL2()
        {
            string configRoot = "CCL2";
            string destinationProject = "OneProject";
            Migration migration = Migration.LoadFromCsv(
                $"..\\..\\Input\\{configRoot}\\businessHierarchyCsv.csv",
                //$"..\\..\\Input\\{configRoot}\\businessHierarchyCsvAlt.csv",
                destinationProject,
                true,
                BusinessNodeType.Product
                );

            migration.GenerateBusinessNodeAndDataNodeFromCsv(
                $"..\\..\\Output\\{configRoot}\\businessNodeFromCsv.json",
                $"..\\..\\Output\\{configRoot}\\businessNodeDataNodeFromCsv.json");

            migration.GenerateExportConfig(
                $"..\\..\\Output\\Configuration\\{configRoot}",
                $"..\\..\\..\\..\\ADOTools2\\ADO.Engine.Console\\bin\\Debug\\ADOTools2.exe",
                "Environment.PatTokenCCL",
                "..\\..\\..\\ADO.Engine\\TemplateFiles",
                "C:\\Templates\\V2\\ExtractedTemplate",
                @"C:\Users\dawal\source\repos\MigrationVNext\ADOTools2\ADO.ProcessMapping.Tests\Output\OneMapToRuleThemAll.json",
                null
                );

            //do this before import config
            string universalAreaMaps = $"..\\..\\Output\\{configRoot}\\oneProjectAreaMap.json";
            string universalIterationMaps = $"..\\..\\Output\\{configRoot}\\oneProjectIterationMap.json";

            //map area paths
            migration.GenerateAreaPathMap(true, $"..\\..\\Input\\{configRoot}\\oneProjectAreaHierarchy.json", true);
            migration.SimpleMutableAreaMinimalWithIdNode.SaveToJson($"..\\..\\Output\\{configRoot}\\oneProjectAreaHierarchy.json");
            migration.SerializableAreaMap.SaveToJson(universalAreaMaps);

            //map iterations
            migration.GenerateIterationPathMap(true, $"..\\..\\Input\\{configRoot}\\oneProjectIterationHierarchy.json", true);
            migration.SimpleMutableIterationMinimalWithIdNode.SaveToJson($"..\\..\\Output\\{configRoot}\\oneProjectIterationHierarchy.json");
            migration.SerializableIterationMap.SaveToJson(universalIterationMaps);

            List<string> teamExclusions = new List<string>() { "someExcludedTeam" };
            string areaInitializationFile = null;
            string iterationInitializationFile = null;
            migration.GenerateImportConfig(
                "Environment.PatToken",
                "ccldevopsabcs",
                "CCL-One",
                $"{destinationProject} Team",
                "One Project to Rule Them All",
                "Environment.PatTokenCCL",
                "Environment.GitHubPassword",
                null,
                true,
                false,
                areaInitializationFile,
                iterationInitializationFile,
                null,
                null,
                destinationProject,
                teamExclusions
                );            

            migration.GenerateBulkImportConfig(
                "Agile",
                "ReflectedWorkItemId",
                true,
                90,
                $"..\\..\\..\\..\\ADOTools1\\src\\VstsSyncMigrator.Console\\bin\\Debug\\migration.exe",
                universalAreaMaps,
                universalIterationMaps
                );

            ////Can't save settings due to self referencing trees e.g. root
            ////find a way to serialize trees
            //migration.SaveToJson($"..\\..\\Output\\{configRoot}\\CCL2-migration.json");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_do_migration_CCLOneProject()
        {
            string configRoot = "CCLOneProject";
            //string destinationProject = "OneProject";
            string destinationProject = $"OneProject_{(projectNumber + 1).ToString().PadLeft(2, '0')}";
            Migration migration = Migration.LoadFromCsv(
                $"..\\..\\Input\\{configRoot}\\businessHierarchyCsv.csv",
                //$"..\\..\\Input\\{configRoot}\\businessHierarchyCsvAlt.csv",
                destinationProject,
                true,
                BusinessNodeType.Product
                );

            migration.GenerateBusinessNodeAndDataNodeFromCsv(
                $"..\\..\\Output\\{configRoot}\\businessNodeFromCsv.json",
                $"..\\..\\Output\\{configRoot}\\businessNodeDataNodeFromCsv.json");

            migration.GenerateExportConfig(
                $"..\\..\\Output\\Configuration\\{configRoot}",
                $"..\\..\\..\\..\\ADOTools2\\ADO.Engine.Console\\bin\\Debug\\ADOTools2.exe",
                "Environment.PatTokenCCL",
                "..\\..\\..\\ADO.Engine\\TemplateFiles",
                "C:\\Templates\\V2\\ExtractedTemplate",
                @"C:\Users\dawal\source\repos\MigrationVNext\ADOTools2\ADO.ProcessMapping.Tests\Output\OneMapToRuleThemAll.json",
                null
                );

            //do this before import config
            string universalAreaMaps = $"..\\..\\Output\\{configRoot}\\oneProjectAreaMap.json";
            string universalIterationMaps = $"..\\..\\Output\\{configRoot}\\oneProjectIterationMap.json";

            //map area paths
            migration.GenerateAreaPathMap(true, $"..\\..\\Input\\{configRoot}\\oneProjectAreaHierarchy.json", true);
            migration.SimpleMutableAreaMinimalWithIdNode.SaveToJson($"..\\..\\Output\\{configRoot}\\oneProjectAreaHierarchy.json");
            migration.SerializableAreaMap.SaveToJson(universalAreaMaps);

            //map iterations
            migration.GenerateIterationPathMap(true, $"..\\..\\Input\\{configRoot}\\oneProjectIterationHierarchy.json", true);
            migration.SimpleMutableIterationMinimalWithIdNode.SaveToJson($"..\\..\\Output\\{configRoot}\\oneProjectIterationHierarchy.json");
            migration.SerializableIterationMap.SaveToJson(universalIterationMaps);

            List<string> teamExclusions = new List<string>() { "someExcludedTeam" };
            string areaInitializationFile = $"C:\\Templates\\V2\\ExtractedTemplate\\carnivalcruiselines\\OneProject_01\\Areas.json";
            string iterationInitializationFile = $"C:\\Templates\\V2\\ExtractedTemplate\\carnivalcruiselines\\OneProject_01\\Iterations.json";
            migration.GenerateImportConfig(
                "Environment.PatToken",
                "ccldevopsabcs",
                "CCL-One",
                $"{destinationProject} Team",
                "One Project to Rule Them All",
                "Environment.PatTokenCCL",
                "Environment.GitHubPassword",
                null,
                true,
                true,
                areaInitializationFile,
                iterationInitializationFile,
                $"..\\..\\Output\\{configRoot}\\oneProjectAreaInitialization.json",
                $"..\\..\\Output\\{configRoot}\\oneProjectIterationInitialization.json",
                destinationProject,
                teamExclusions
                );            

            migration.GenerateBulkImportConfig(
                "Agile",
                "ReflectedWorkItemId",
                true,
                90,
                $"..\\..\\..\\..\\ADOTools1\\src\\VstsSyncMigrator.Console\\bin\\Debug\\migration.exe",
                universalAreaMaps,
                universalIterationMaps
                );

            ////Can't save settings due to self referencing trees e.g. root
            ////find a way to serialize trees
            //migration.SaveToJson($"..\\..\\Output\\{configRoot}\\CCL2-migration.json");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_do_migration_OneProject()
        {
            string configRoot = "agileatscale";
            //string destinationProject = "OneProject";
            string destinationProject = "XProject";
            Migration migration = Migration.LoadFromCsv(
                $"..\\..\\Input\\{configRoot}\\businessHierarchyCsv.csv",
                //$"..\\..\\Input\\{configRoot}\\businessHierarchyCsvAlt.csv",
                destinationProject,
                true,
                BusinessNodeType.Product
                );

            migration.GenerateBusinessNodeAndDataNodeFromCsv(
                $"..\\..\\Output\\{configRoot}\\businessNodeFromCsv.json",
                $"..\\..\\Output\\{configRoot}\\businessNodeDataNodeFromCsv.json");

            migration.GenerateExportConfig(
                $"..\\..\\Output\\Configuration\\{configRoot}",
                $"..\\..\\..\\..\\ADOTools2\\ADO.Engine.Console\\bin\\Debug\\ADOTools2.exe",
                "Environment.PatToken",
                "..\\..\\..\\ADO.Engine\\TemplateFiles",
                "C:\\Templates\\V2\\ExtractedTemplate",
                @"C:\Users\dawal\source\repos\MigrationVNext\ADOTools2\ADO.ProcessMapping.Tests\Output\OneMapToRuleThemAll.json",
                null
                );

            //do this before import config
            string universalAreaMaps = $"..\\..\\Output\\{configRoot}\\oneProjectAreaMap.json";
            string universalIterationMaps = $"..\\..\\Output\\{configRoot}\\oneProjectIterationMap.json";

            //map area paths
            migration.GenerateAreaPathMap(true, $"..\\..\\Input\\{configRoot}\\oneProjectAreaHierarchy.json", true);
            migration.SimpleMutableAreaMinimalWithIdNode.SaveToJson($"..\\..\\Output\\{configRoot}\\oneProjectAreaHierarchy.json");
            migration.SerializableAreaMap.SaveToJson(universalAreaMaps);

            //map iterations
            migration.GenerateIterationPathMap(true, $"..\\..\\Input\\{configRoot}\\oneProjectIterationHierarchy.json", true);
            migration.SimpleMutableIterationMinimalWithIdNode.SaveToJson($"..\\..\\Output\\{configRoot}\\oneProjectIterationHierarchy.json");
            migration.SerializableIterationMap.SaveToJson(universalIterationMaps);

            List<string> teamExclusions = new List<string>() { "someExcludedTeam" };
            string areaInitializationFile = $"..\\..\\Input\\{configRoot}\\Areas.json";
            string iterationInitializationFile = $"..\\..\\Input\\{configRoot}\\Iterations.json";
            migration.GenerateImportConfig(
                "Environment.PatToken",
                "agileatscale",
                "MigrateableScrum",
                $"{destinationProject} Team",
                "One Project to Rule Them All",
                "Environment.PatToken",
                "Environment.GitHubPassword",
                null,
                true,
                true,
                areaInitializationFile,
                iterationInitializationFile,
                $"..\\..\\Output\\{configRoot}\\oneProjectAreaInitialization.json",
                $"..\\..\\Output\\{configRoot}\\oneProjectIterationInitialization.json",
                destinationProject,
                teamExclusions
                );            

            migration.GenerateBulkImportConfig(
                "Scrum",
                "ReflectedWorkItemId",
                true,
                90,
                $"..\\..\\..\\..\\ADOTools1\\src\\VstsSyncMigrator.Console\\bin\\Debug\\migration.exe",
                universalAreaMaps,
                universalIterationMaps
                );

            ////Can't save settings due to self referencing trees e.g. root
            ////find a way to serialize trees
            //migration.SaveToJson($"..\\..\\Output\\{configRoot}\\CCL2-migration.json");

            Assert.IsTrue(true);
        }
    }
}
