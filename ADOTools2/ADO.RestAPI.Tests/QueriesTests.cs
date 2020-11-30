using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ADO.RestAPI.Core;
using ADO.Engine.Configuration.ProjectImport;
using ADO.Tools;
using ADO.Engine;

namespace ADO.RestAPI.Tests
{
    [TestClass]
    public class QueriesTests
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Tests.Queries"));

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

            string configFilePath = @"..\..\Input\configuration-MergedProject22.json";
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
        public void Get_Queries()
        {
            // Create Azure DevOps REST api service object.
            Queries.Queries adorasQueries = new Queries.Queries(_projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"])
            {
                ProjectId = _projectDef.DestinationProject.Id
            };
            string path = "/Shared Queries/Portfolio A";
            int depth = 2;
            var result = adorasQueries.GetQueries(path, depth);

            Assert.IsTrue(result.HasChildren);

            //ultimately want:
            // "Shared Queries/Portfolio A/Program 2/SmartHotel360"
            string queryFolderPath = "Shared Queries/Portfolio A/Program 2/SmartHotel360";
            string expectedToken = "$/782e99f8-00cd-4a3c-ac71-f1e4b66a4b00/c6900c27-7b7a-4ec6-9123-10f2af01e5b2/d04ccb12-126a-40eb-a017-d51de34d6105/f343b153-2536-443d-b4ad-57003685f88a/5d2c7b8c-88c0-4182-95a6-8cd8fb9e365b";
            string actualToken = adorasQueries.GenerateQueryFolderToken(queryFolderPath);

            Assert.AreEqual(expectedToken, actualToken);
        }
    }
}
