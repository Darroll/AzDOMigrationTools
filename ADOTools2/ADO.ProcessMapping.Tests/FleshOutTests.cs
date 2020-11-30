using ADO.RestAPI.Viewmodel50;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADO.ProcessMapping.Tests
{
    [TestClass]
    public class FleshOutTests
    {
        [TestMethod]
        public void Test_Inject_Parent_Into_Child_Agile_Carnival()
        {
            string childPath = @"..\..\Input\InheritedProcess\Agile - Carnival.json";
            var child = InheritedProcess.LoadFromJson(childPath);
            var parent = InheritedProcess.LoadFromJson(@"..\..\Input\InheritedProcess\Agile.json");

            InheritedProcess.MergeParentIntoChild(parent, child);
            child.SaveToJson(childPath);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Inject_Parent_Into_Child_Scrum_Carnival()
        {
            string childPath = @"..\..\Input\InheritedProcess\Scrum - Carnival.json";
            var child = InheritedProcess.LoadFromJson(childPath);
            var parent = InheritedProcess.LoadFromJson(@"..\..\Input\InheritedProcess\Scrum.json");

            InheritedProcess.MergeParentIntoChild(parent, child);
            child.SaveToJson(childPath);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Inject_Parent_Into_Child_MigrateableUniversal()
        {
            string childPath = @"..\..\Input\InheritedProcess\MigrateableUniversal.json";
            var child = InheritedProcess.LoadFromJson(childPath);
            var parent = InheritedProcess.LoadFromJson(@"..\..\Input\InheritedProcess\CMMI.json");

            InheritedProcess.MergeParentIntoChild(parent, child);
            child.SaveToJson(childPath);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Inject_Parent_Into_Child_MigrateableAgile()
        {
            string childPath = @"..\..\Input\InheritedProcess\MigrateableAgile.json";
            var child = InheritedProcess.LoadFromJson(childPath);
            var parent = InheritedProcess.LoadFromJson(@"..\..\Input\InheritedProcess\Agile.json");

            InheritedProcess.MergeParentIntoChild(parent, child);
            child.SaveToJson(childPath);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Inject_Parent_Into_Child_MigrateableScrum()
        {
            string childPath = @"..\..\Input\InheritedProcess\MigrateableScrum.json";
            var child = InheritedProcess.LoadFromJson(childPath);
            var parent = InheritedProcess.LoadFromJson(@"..\..\Input\InheritedProcess\Scrum.json");

            InheritedProcess.MergeParentIntoChild(parent, child);
            child.SaveToJson(childPath);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Inject_Parent_Into_Child_CCL_One()
        {
            string childPath = @"..\..\Input\InheritedProcess\CCL-One.json";
            var child = InheritedProcess.LoadFromJson(childPath);
            var parent = InheritedProcess.LoadFromJson(@"..\..\Input\InheritedProcess\Agile.json");

            InheritedProcess.MergeParentIntoChild(parent, child);
            child.SaveToJson(childPath);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Inject_Parent_Into_TDAgile()
        {
            string childPath = @"..\..\Input\InheritedProcess\TDAgile.json";
            var child = InheritedProcess.LoadFromJson(childPath);
            var parent = InheritedProcess.LoadFromJson(@"..\..\Input\InheritedProcess\Agile.json");

            InheritedProcess.MergeParentIntoChild(parent, child);
            child.SaveToJson(childPath);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Inject_Parent_Into_TDAgile2()
        {
            string childPath = @"..\..\Input\InheritedProcess\TD Agile 2.json";
            var child = InheritedProcess.LoadFromJson(childPath);
            var parent = InheritedProcess.LoadFromJson(@"..\..\Input\InheritedProcess\Agile.json");

            InheritedProcess.MergeParentIntoChild(parent, child);
            child.SaveToJson(childPath);

            Assert.IsTrue(true);
        }
    }
}
