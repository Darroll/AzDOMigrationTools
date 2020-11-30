using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ADO.RestAPI.Core;
using ADO.RestAPI.WorkItemTracking.ClassificationNodes;
using ADO.Engine.Configuration.ProjectImport;
using ADO.Tools;
using ADO.Engine;

namespace ADO.RestAPI.Tests
{
    [TestClass]
    public class WorkItemTrackingTests
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Tests.WorkItemTrackingTests"));

        #endregion

        #endregion

        private static ProjectDefinition _projectDef;

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            // Send some traces.
            _mySource.Value.TraceInformation("Load Config");
            _mySource.Value.Flush();

            string configFilePath = @"..\..\Input\configuration-MergedProject27.json";
            StreamReader sr = new StreamReader(configFilePath);
            string configurationjson = sr.ReadToEnd();
            sr.Close();
            EngineConfiguration _engineConfig = JsonConvert.DeserializeObject<EngineConfiguration>(configurationjson);
            _engineConfig.Validate(); //this will also load secrets

            // Define the project to import.
            _projectDef = new ProjectDefinition(_engineConfig.ImportPath, _engineConfig.SourceCollection, _engineConfig.SourceProject, _engineConfig.DestinationCollection, _engineConfig.DestinationProject, _engineConfig.Description, _engineConfig.DestinationProjectProcessName, _engineConfig.DefaultTeamName, _engineConfig.PAT, _engineConfig.RestApiService);

            // Retrieve the project.
            Projects adorasProjects = new Projects(_projectDef.AdoRestApiConfigurations["ProjectsApi"]);
            var project = adorasProjects.GetProjectByName(_projectDef.DestinationProject.Name);
            Assert.IsNotNull(project, adorasProjects.LastApiErrorMessage);
            _projectDef.DestinationProject.Id = project.Id;
        }

        [TestMethod]
        public void Test_Get_Classification_nodes()
        {
            ClassificationNodes adorasCN = new ClassificationNodes(_projectDef.AdoRestApiConfigurations["GetClassificationNodesApi"]);
            string classificationNodePath = "MergedProject27/Portfolio A/Program 1/PartsUnlimited";
            int depth = 2;
            var result = adorasCN.GetClassificationNodes(depth);

            Assert.IsTrue(result.Count>0);

            //pick area node:
            var rootAreaNode = result.Value.Where(x => x.StructureType == "area").Single();

            Assert.IsTrue(rootAreaNode.HasChildren);

            //looking for:
            string expectedToken = "vstfs:///Classification/Node/24b72d77-f873-417e-baae-e88c3fffaf5a:vstfs:///Classification/Node/c3b21dce-5136-4549-b498-6e04f2143cc3:vstfs:///Classification/Node/9c1a77b8-1740-4b0a-8434-dc15d1577851:vstfs:///Classification/Node/6eacd0ed-c977-43c4-af4a-efe1f1e3b02d";

            //classification node = 4-- the chain
            var actualToken = adorasCN.GetClassificationNodeToken(classificationNodePath);
            Assert.AreEqual(expectedToken, actualToken);
        }
    }
}
