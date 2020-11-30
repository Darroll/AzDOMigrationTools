using System.Collections.Generic;
using System.IO;
using System.Linq;
using ADO.RestAPI.ProcessMapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using static ADO.RestAPI.Viewmodel50.WorkResponse;

namespace AzureDevOpsEngine.Tests
{
    [TestClass]
    public class MappingTests
    {
        private static Maps maps;

        // private static string AppDir => Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            string mapFilesPath = @"..\..\..\ADO.ProcessMapping.Tests\Output\OneMapToRuleThemAll.json";
            maps = Maps.LoadFromJson(mapFilesPath);
        }

        [TestMethod]
        public void TestLoadBoards()
        {
            string json = File.ReadAllText(@"..\..\Input\AgileBoards.json");
            var agileBoards = JsonConvert.DeserializeObject<List<AgileBoardColumns>>(json);
            Assert.IsNotNull(agileBoards);

            json = JsonConvert.SerializeObject(agileBoards, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\AgileBoards.json", json);

            json = File.ReadAllText(@"..\..\Input\ScrumBoards.json");
            var scrumBoards = JsonConvert.DeserializeObject<List<ScrumBoardColumns>>(json);
            Assert.IsNotNull(scrumBoards);

            json = JsonConvert.SerializeObject(scrumBoards, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\ScrumBoards.json", json);

            json = File.ReadAllText(@"..\..\Input\CMMIBoards.json");
            var cmmiBoards = JsonConvert.DeserializeObject<List<CmmiBoardColumns>>(json);
            Assert.IsNotNull(cmmiBoards);

            json = JsonConvert.SerializeObject(cmmiBoards, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\CMMIBoards.json", json);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestMapToCmmiBoards()
        {
            string json = File.ReadAllText(@"..\..\Input\AgileBoards.json");
            var agileBoards = JsonConvert.DeserializeObject<List<AgileBoardColumns>>(json);
            Assert.IsNotNull(agileBoards);

            json = JsonConvert.SerializeObject(agileBoards, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\AgileBoards.json", json);

            //List<ADO.RestAPI.Viewmodel50.WorkResponse.CmmiBoardColumns> cmmiBoards = agileBoards.Select(
            //                            ab => ab.ToCmmiBoardColumns(maps)).ToList();

            //json = JsonConvert.SerializeObject(cmmiBoards, Formatting.Indented);
            //File.WriteAllText(@"..\..\Output\AgileAsCmmiBoards.json", json);

            json = File.ReadAllText(@"..\..\Input\ScrumBoards.json");
            var scrumBoards = JsonConvert.DeserializeObject<List<ScrumBoardColumns>>(json);
            Assert.IsNotNull(scrumBoards);

            json = JsonConvert.SerializeObject(scrumBoards, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\ScrumBoards.json", json);

            //cmmiBoards = scrumBoards.Select(
            //                            ab => ab.ToCmmiBoardColumns(maps).InsertMissingCmmiBoardColumns()).ToList();

            //json = JsonConvert.SerializeObject(cmmiBoards, Formatting.Indented);
            //File.WriteAllText(@"..\..\Output\ScrumAsCmmiBoards.json", json);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestMapToCmmiCards()
        {
            string json = File.ReadAllText(@"..\..\Input\AgileCardFields.json");
            var agileCards = JsonConvert.DeserializeObject<List<AgileCards>>(json);
            Assert.IsNotNull(agileCards);

            json = JsonConvert.SerializeObject(agileCards, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\AgileCardFields.json", json);

            //List<ADO.RestAPI.Viewmodel50.WorkResponse.CmmiCards> cmmiCards = agileCards.Select(
            //                            ab => ab.ToCmmiCards(maps)).ToList();

            //json = JsonConvert.SerializeObject(cmmiCards, Formatting.Indented);
            //File.WriteAllText(@"..\..\Output\AgileAsCmmiCards.json", json);


            json = File.ReadAllText(@"..\..\Input\ScrumCardFields.json");
            var scrumCards = JsonConvert.DeserializeObject<List<ScrumCards>>(json);
            Assert.IsNotNull(scrumCards);

            json = JsonConvert.SerializeObject(scrumCards, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\ScrumCardFields.json", json);

            //cmmiCards = scrumCards.Select(
            //                            ab => ab.ToCmmiCards(maps)).ToList();

            //json = JsonConvert.SerializeObject(cmmiCards, Formatting.Indented);
            //File.WriteAllText(@"..\..\Output\ScrumAsCmmiCards.json", json);

            Assert.IsTrue(true);

        }
    }
}
