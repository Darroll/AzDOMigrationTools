using System;
using System.Collections.Generic;
using System.IO;
using ADO.Engine.BusinessEntities;
using LINQtoCSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ADO.Configuration.Tests
{
    [TestClass]
    public class TestOneHierarchy
    {
        private static byte fiscalMonthStart;
        private static List<Cadence> defaultPortfolioCadences;
        private static List<Cadence> defaultProgramOrProductTeamCadences;
        private static List<Cadence> defaultTeamProjectTeamCadences;

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            fiscalMonthStart = 1;
            defaultPortfolioCadences = new List<Cadence>() { new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.FiscalSemester, 0, 0, DayOfWeek.Sunday, fiscalMonthStart) };
            defaultProgramOrProductTeamCadences = new List<Cadence>() { new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.ProgramIncrement, 2, 5, DayOfWeek.Sunday, fiscalMonthStart) };
            defaultTeamProjectTeamCadences = new List<Cadence>()
            { new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.Sprint, 2, 0, DayOfWeek.Sunday, fiscalMonthStart) };
        }

        [TestMethod]
        public void TestWriteHierarchyToCsv()
        {
            string json = File.ReadAllText(@"..\..\Input\businessHierarchyFromAreas.json");

            var businessHierarchy = JsonConvert.DeserializeObject<BusinessHierarchy>(json);

            List<BusinessHierarchyCsv> businessHierarchyCsvList
                = businessHierarchy.ToBusinessHierarchyCsvList("devopsabcs");

            CsvFileDescription outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',', // tab delimited
                FirstLineHasColumnNames = true, // no column names in first record
                //FileCultureName = "nl-NL" // use formats used in The Netherlands
            };

            CsvContext cc = new CsvContext();

            cc.Write(
                businessHierarchyCsvList,
                @"..\..\Output\businessHierarchyCsv.csv",
                outputFileDescription);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestGenerateBusinessHierarchyFromCsv()
        {
            List<BusinessHierarchyCsv> businessHierarchyCsvList;
            BusinessHierarchy businessHierarchy
                = BusinessHierarchy.LoadFromCsv(@"..\..\Input\businessHierarchyCsv.csv",
                "OneProjectV2_02", "Portfolio Team", "Program Team", "Team", BusinessNodeType.Program, true, out businessHierarchyCsvList);

            string json = JsonConvert.SerializeObject(businessHierarchy, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\businessHierarchyFromCsv.json", json);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestGenerateBusinessNodeFromBusinessHierarchy()
        {
            string json = File.ReadAllText(@"..\..\Input\businessHierarchyFromCsv.json");
            BusinessHierarchy businessHierarchy
                = JsonConvert.DeserializeObject<BusinessHierarchy>(json);

            BusinessNode businessNode = BusinessNode.FromBusinessHierarchy(
                businessHierarchy, defaultPortfolioCadences,
                BusinessNodeType.Program,
                defaultProgramOrProductTeamCadences,
                defaultTeamProjectTeamCadences, "Team");

            json = JsonConvert.SerializeObject(businessNode, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\businessNodeFromBusinessHierarchy.json", json);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestGenerateBusinessNodeFromCsv()
        {
            List<BusinessHierarchyCsv> businessHierarchyCsvList;
            BusinessNode businessNode
                = BusinessNode.LoadFromCsv(@"..\..\Input\businessHierarchyCsv.csv",
                    "OneProjectV2_02", true, BusinessNodeType.Program, defaultPortfolioCadences,
                    defaultProgramOrProductTeamCadences,
                    defaultTeamProjectTeamCadences,
                    "Portfolio Team",
                    "Program Team",
                    "Team",
                    out businessHierarchyCsvList);

            string json = JsonConvert.SerializeObject(businessNode, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\businessNodeFromCsv.json", json);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestOneHierarchy_Generate_Iterations()
        {
            SimpleMutableBusinessNodeNode root = SimpleMutableBusinessNodeNode.LoadFromJson(@"..\..\Input\businessNodeFromCsv.json");

            root.GenerateIterations($"..\\..\\Output\\ProjectInitialization\\Iterations");
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestOneHierarchy_Generate_Areas()
        {
            SimpleMutableBusinessNodeNode root = SimpleMutableBusinessNodeNode.LoadFromJson(@"..\..\Input\businessNodeFromCsv.json");

            root.GenerateAreas($"..\\..\\Output\\ProjectInitialization\\Areas");
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestOneHierarchy_Generate_Teams()
        {
            SimpleMutableBusinessNodeNode root = SimpleMutableBusinessNodeNode.LoadFromJson(@"..\..\Input\businessNodeFromCsv.json");

            root.GenerateTeams($"..\\..\\Output\\ProjectInitialization\\Teams");
            Assert.IsTrue(true);
        }
    }
}
