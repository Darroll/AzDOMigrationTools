using System;
using System.Collections.Generic;
using System.IO;
using ADO.RestAPI.ProcessMapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADO.ProcessMapping.Tests
{
    [TestClass]
    public class LoadMapsTests
    {
        [TestMethod]
        public void TestLoadAllMaps()
        {
            var maps = new Maps(@"..\..\Input\InheritedProcess");

            //create simple maps
            maps.ProcessMaps.Add(new ProcessMap()
            {
                SourceProcess = "Agile",
                TargetProcess="CMMI",
                WorkItemTypeMap=new System.Collections.Generic.Dictionary<string, string>()
                {
                    { "Microsoft.VSTS.WorkItemTypes.Bug", "Microsoft.VSTS.WorkItemTypes.Bug" },
                    { "Microsoft.VSTS.WorkItemTypes.Epic", "Microsoft.VSTS.WorkItemTypes.Epic" }
                },
            }
                );

            maps.Validate();

            maps.SaveToJson(@"..\..\Output\myProcessMaps.json");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_Agile_To_CMMI()
        {
            string filename = @"..\..\Output\ValidMaps\myBestPossibleProcessMapsAgileToCmmi.json";
            var maps = Maps.LoadFromJson(filename);
            
            //create simple maps
            var bestMap =maps.PopulateBestPossibleProcessMap("Agile","CMMI", true, true, true);
            
            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestLoadAllMaps_BestPossible_CMMI_to_agile()
        {
            string filename = @"..\..\Output\myBestPossibleProcessMapsCmmiToAgile.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("CMMI", "Agile", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestLoadAllMaps_BestPossible_Scrum_to_agile()
        {
            string filename = @"..\..\Output\ValidMaps\scrumToAgile.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Scrum", "Agile", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestLoadAllMaps_BestPossible_agile_to_Scrum()
        {
            string filename = @"..\..\Output\ValidMaps\agileToScrum.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Agile", "Scrum", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestLoadAllMaps_BestPossible_agile001_to_agile()
        {
            Maps maps = null;
            string filename = @"..\..\Output\ValidMaps\agile001ToAgile.json";
            if (File.Exists(filename))
            {
                maps = Maps.LoadFromJson(filename);
            }
            else
            {
                maps = new Maps(@"..\..\Input\InheritedProcess");
            }

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Agile001", "Agile", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeFields());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestLoadAllMaps_BestPossible_agile_to_agile001()
        {
            Maps maps = null;
            string filename = @"..\..\Output\ValidMaps\agileToAgile001.json";
            if (File.Exists(filename))
            {
                maps = Maps.LoadFromJson(filename);
            }
            else
            {
                maps = new Maps(@"..\..\Input\InheritedProcess");
            }

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Agile", "Agile001", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeFields());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestLoadAllMaps_BestPossible_Scrum_to_cmmi()
        {
            string filename = @"..\..\Output\ValidMaps\scrumToCmmi.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Scrum", "CMMI", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_Scrum_to_MigrateableScrum()
        {
            string filename = @"..\..\Output\ValidMaps\scrumToMigrateableScrum.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Scrum", "MigrateableScrum", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_MigrateableScrum_to_Scrum()
        {
            string filename = @"..\..\Output\ValidMaps\MigrateableScrumToScrum.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("MigrateableScrum", "Scrum", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            Assert.AreEqual(9, bestMap.TotalNumberOfUnmappedItems()); //due to 9 times "Custom.ReflectedWorkItemId"
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems); //due to 9 times "Custom.ReflectedWorkItemId"

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_Agile_to_MigrateableAgile()
        {
            string filename = @"..\..\Output\ValidMaps\AgileToMigrateableAgile.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Agile", "MigrateableAgile", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_Agile001_to_MigrateableAgile()
        {
            string filename = @"..\..\Output\ValidMaps\Agile001ToMigrateableAgile.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Agile001", "MigrateableAgile", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            //var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_CclOne_to_Agile()
        {
            string filename = @"..\..\Output\ValidMaps\CCL-OneToAgile.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("CCL-One", "Agile", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            //var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_AgileCarnival_to_Agile()
        {
            string filename = @"..\..\Output\ValidMaps\Agile - CarnivalToAgile.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Agile - Carnival", "Agile", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            //var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_ScrumCarnival_to_Scrum()
        {
            string filename = @"..\..\Output\ValidMaps\Scrum - CarnivalToScrum.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Scrum - Carnival", "Scrum", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            //var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_ScrumCarnival_to_Agile()
        {
            string filename = @"..\..\Output\ValidMaps\Scrum - CarnivalToAgile.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Scrum - Carnival", "Agile", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            //var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_ScrumCarnival_to_CCLOne()
        {
            string filename = @"..\..\Output\ValidMaps\Scrum - CarnivalToCCL-One.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Scrum - Carnival", "CCL-One", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            //var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_AgileCarnival_to_CCLOne()
        {
            string filename = @"..\..\Output\ValidMaps\Agile - CarnivalToCCL-One.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Agile - Carnival", "CCL-One", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            //var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_Agile_to_CCLOne()
        {
            string filename = @"..\..\Output\ValidMaps\AgileToCCL-One.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Agile", "CCL-One", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            //var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_TDAgile2_to_TDAgile()
        {
            string filename = @"..\..\Output\ValidMaps\TDAgile2ToTDAgile.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("TD Agile 2", "TDAgile", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            //var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_CMMI_to_MigrateableUniversal()
        {
            string filename = @"..\..\Output\ValidMaps\CMMIToMigrateableUniversal.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("CMMI", "MigrateableUniversal", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_Agile_to_MigrateableUniversal()
        {
            string filename = @"..\..\Output\ValidMaps\AgileToMigrateableUniversal.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Agile", "MigrateableUniversal", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            var hasUnmappedItems = bestMap.HasUnmappedItems();

            Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_BestPossible_Scrum_to_MigrateableUniversal()
        {
            string filename = @"..\..\Output\ValidMaps\ScrumToMigrateableUniversal.json";
            var maps = Maps.LoadFromJson(filename);

            //create simple maps
            var bestMap = maps.PopulateBestPossibleProcessMap("Scrum", "MigrateableUniversal", true, true, true);

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
            //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());

            //var hasUnmappedItems = bestMap.HasUnmappedItems();

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestLoadAllMaps_OneMap_ToRuleThemAll()
        {
            Maps maps = null;
            string filename = @"..\..\Output\OneMapToRuleThemAll.json";
            if (File.Exists(filename))
            {
                maps = Maps.LoadFromJson(filename);
            }
            else
            {
                maps = new Maps(@"..\..\Input\InheritedProcess");
            }
            var allMaps = Directory.EnumerateFiles(@"..\..\Output\ValidMaps");
            foreach (var inputFile in allMaps)
            {
                foreach (var bestMap in Maps.LoadFromJson(inputFile).ProcessMaps)
                {
                    maps.AddOrUpdateProcessMap(bestMap);
                }                
            }

            var isValid = maps.Validate();

            Assert.IsTrue(isValid);

            foreach (var bestMap in maps.ProcessMaps)
            {
                //Assert.AreEqual(0, bestMap.TotalNumberOfUnmappedItems());
                //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypes());
                //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeStates());
                //Assert.AreEqual(0, bestMap.NumberOfUnmappedWorkItemTypeFields());

                var hasUnmappedItems = bestMap.HasUnmappedItems();
            }

            //Assert.IsFalse(hasUnmappedItems);

            maps.SaveToJson(filename);

            Assert.IsTrue(true);
        }
    }
}
