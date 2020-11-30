using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ADO.RestAPI.Security;
using ADO.Engine.Configuration.ProjectImport;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using ADO.Engine;
using ADO.RestAPI.Core;

namespace ADO.RestAPI.Tests
{
    [TestClass]
    public class SecurityTests
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Tests.Security"));

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

            string configFilePath = @"..\..\Input\configuration-SmartHotel360.json";
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

        //// Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup() { }

        //// Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize() { }

        //// Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup() { }

        [TestMethod]
        public void Test_Get_Security_Namespaces()
        {

            // Create an Azure DevOps REST api service object.
            SecurityNamespaces adorasSecurityNamespace = new SecurityNamespaces(_projectDef.AdoRestApiConfigurations["SecurityApi"]);

            var result = adorasSecurityNamespace.GetSecurityNamespaces();

            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void Test_Get_Security_Namespace_by_ID()
        {

            // Create an Azure DevOps REST api service object.
            SecurityNamespaces adorasSecurityNamespace = new SecurityNamespaces(_projectDef.AdoRestApiConfigurations["SecurityApi"]);
            var allSecurityNamespaces = adorasSecurityNamespace.GetSecurityNamespaces();

            var releaseManagements =
                allSecurityNamespaces.Value.Where(x => x.Name == "ReleaseManagement");

            string releaseManagementId = releaseManagements.OrderByDescending(x => x.Actions.Count()).First().NamespaceId;
            var result = adorasSecurityNamespace.GetSecurityNamespaceById(releaseManagementId);

            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void Test_Get_ACL_by_ID()
        {

            // Create an Azure DevOps REST api service object.
            SecurityNamespaces adorasSecurityNamespace = new SecurityNamespaces(_projectDef.AdoRestApiConfigurations["SecurityApi"]);
            var allSecurityNamespaces = adorasSecurityNamespace.GetSecurityNamespaces();

            var releaseManagements =
                allSecurityNamespaces.Value.Where(x => x.Name == "ReleaseManagement");

            string releaseManagementId = releaseManagements.OrderByDescending(x => x.Actions.Count()).First().NamespaceId;

            // Create an Azure DevOps REST api service object.
            AccessControlLists adorasACLs = new AccessControlLists(_projectDef.AdoRestApiConfigurations["SecurityApi"]);
            var acls = adorasACLs.GetAccessControlList(releaseManagementId);

            Assert.IsTrue(acls.Count > 0);
        }

        [TestMethod]
        public void Test_Set_ACL_by_ID()
        {

            // Create an Azure DevOps REST api service object.
            SecurityNamespaces adorasSecurityNamespace = new SecurityNamespaces(_projectDef.AdoRestApiConfigurations["SecurityApi"]);
            var allSecurityNamespaces = adorasSecurityNamespace.GetSecurityNamespaces();

            var WorkItemQueryFolders = allSecurityNamespaces.Value.Where(x => x.Name == "WorkItemQueryFolders");

            string WorkItemQueryFoldersId = WorkItemQueryFolders.OrderByDescending(x => x.Actions.Count()).First().NamespaceId;

            //query path for MergedProject22 of org: devopsmigration
            // Shared Queries/Portfolio A/Program 2/SmartHotel360
            string tokenStr = "$/782e99f8-00cd-4a3c-ac71-f1e4b66a4b00/c6900c27-7b7a-4ec6-9123-10f2af01e5b2/d04ccb12-126a-40eb-a017-d51de34d6105/f343b153-2536-443d-b4ad-57003685f88a/5d2c7b8c-88c0-4182-95a6-8cd8fb9e365b";
            bool mergeBool = false;
            List<SecurityResponse.AccessControlEntry> listOfAces = new List<SecurityResponse.AccessControlEntry>()
            {
                new SecurityResponse.AccessControlEntry()
                {
                    //test team
                    Descriptor="Microsoft.TeamFoundation.Identity;S-1-9-1551374245-4170788472-3439344714-2893148644-3060419328-1-1269247236-1779663680-2620129462-846055378",
                    Allow=15, //allow all first four bits 15 = 1+2+4+8
                    Deny=0,
                    ExtendedInfo=null
                },
                //also add SmartHotel360 team  
                new SecurityResponse.AccessControlEntry()
                {
                    Descriptor="Microsoft.TeamFoundation.Identity;S-1-9-1551374245-4170788472-3439344714-2893148644-3060419328-1-103231501-2676988997-2987904274-1825082100",
                    //descriptor="vssgp.Uy0xLTktMTU1MTM3NDI0NS00MTcwNzg4NDcyLTM0MzkzNDQ3MTQtMjg5MzE0ODY0NC0zMDYwNDE5MzI4LTEtMTI2OTI0NzIzNi0xNzc5NjYzNjgwLTI2MjAxMjk0NjItODQ2MDU1Mzc4",
                    Allow=15, //allow all first four bits 15 = 1+2+4+8
                    Deny=0,
                    ExtendedInfo=null
                }
            };
            object node = new
            {
                Token = tokenStr,
                Merge = mergeBool,
                AccessControlEntries = listOfAces
            };

            var json = JsonConvert.SerializeObject(node, Formatting.Indented);
            string path = @"..\..\Input\aceInputEx.json";
            File.WriteAllText(path, json);

            // Create an Azure DevOps REST api service object.
            AccessControlEntries adorasAccessControlEntries = new AccessControlEntries(_projectDef.AdoRestApiConfigurations["SecurityApi"]);
            var aces = adorasAccessControlEntries.SetAccessControlEntries(WorkItemQueryFoldersId, tokenStr, mergeBool, listOfAces);

            Assert.IsTrue(aces.Count > 0);
        }
    }
}
