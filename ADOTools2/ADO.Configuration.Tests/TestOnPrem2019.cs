using System;
using System.Collections.Generic;
using ADO.Engine.BusinessEntities;
using ADO.Engine.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADO.Configuration.Tests
{
    [TestClass]
    public class TestOnPrem2019
    {
        //private readonly int projectNumber = 1;

        [TestMethod]
        public void Test_do_migration_OnPrem2019()
        {
            string configRoot = "OnPrem2019";
            string destinationProject = $"Core";
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
                "Environment.PatTokenTfs2019",
                "..\\..\\..\\ADO.Engine\\TemplateFiles",
                "..\\..\\..\\Templates\\V2\\ExtractedTemplate",
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
                "Environment.PatTokenTfs2019Target",
                "PEOC3T",
                "MigrateableScrum",
                $"{destinationProject} Team",
                "One Project to Rule Them All",
                "Environment.PatTokenTfs2019",
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
                "Scrum",
                "ReflectedWorkItemId",
                false,
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
