using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ADO.Engine.Configuration.ProjectImport;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using ADO.Engine;
using ADO.RestAPI.Core;

namespace ADO.RestAPI.Tests
{
    [TestClass]
    public class GraphTests
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Tests.Graph"));

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
        [TestMethod]
        public void Test_Get_Groups()
        {
            Graph.Graph adorasGraph = new Graph.Graph(_projectDef.AdoRestApiConfigurations["GraphApi"]);
            var result = adorasGraph.GetAllGroups();

            Assert.IsTrue(result.Count > 0);

            GraphResponse.GraphGroup testTeam = result.Value.Single(x => x.PrincipalName == "[MergedProject22]\\testTeam");
            var testTeamDescriptor = testTeam.Descriptor;
            string sid = ADO.Tools.Utility.GetSidFromDescriptor(testTeamDescriptor);
            var expectedSid = "S-1-9-1551374245-4170788472-3439344714-2893148644-3060419328-1-1269247236-1779663680-2620129462-846055378";
            Assert.AreEqual(expectedSid, sid);
            //note missing Microsoft.TeamFoundation.Identity
        }
        [TestMethod]
        public void Test_Add_Group_Membership()
        {
            Graph.Graph adorasGraph = new Graph.Graph(_projectDef.AdoRestApiConfigurations["GraphApi"]);
            var allGroups = adorasGraph.GetAllGroups();

            Assert.IsTrue(allGroups.Count > 0);

            GraphResponse.GraphGroup testTeam = allGroups.Value.Single(x => x.PrincipalName == "[MergedProject22]\\Endpoint Creators");
            var testTeamDescriptor = testTeam.Descriptor;
            string sid = ADO.Tools.Utility.GetSidFromDescriptor(testTeamDescriptor);
            var expectedSid = "S-1-9-1551374245-750889501-4111815489-3133983551-3036711662-0-0-0-8-9";
            Assert.AreEqual(expectedSid, sid);
            //note missing Microsoft.TeamFoundation.Identity

            //first get group members
            var groupMembers = adorasGraph.GetGroupMembers(testTeam.Descriptor, "down");
            foreach (var member in groupMembers.Value)
            {
                var membergGroup = allGroups.Value.Single(x => x.Descriptor == member.MemberDescriptor);

                // Send some traces.
                _mySource.Value.TraceInformation($"{membergGroup.PrincipalName}:{member.MemberDescriptor}:{ADO.Tools.Utility.GetSidFromDescriptor(member.MemberDescriptor)}");
                _mySource.Value.Flush();
            }
            //parts unlimited team
            string subjectDescriptor = "vssgp.Uy0xLTktMTU1MTM3NDI0NS00MTcwNzg4NDcyLTM0MzkzNDQ3MTQtMjg5MzE0ODY0NC0zMDYwNDE5MzI4LTEtMzg2OTI2MDk5OC05MjI2MDg0NTAtMzA2MDk1MDM3Ni0xMjc1NjA0NDU4";
            //endpoint creators
            string containerDescriptor = "vssgp.Uy0xLTktMTU1MTM3NDI0NS03NTA4ODk1MDEtNDExMTgxNTQ4OS0zMTMzOTgzNTUxLTMwMzY3MTE2NjItMC0wLTAtOC05";
            GraphResponse.GroupMember res = adorasGraph.AddGroupMembership(subjectDescriptor, containerDescriptor);

            //what about contributor aces?
        }

        [TestMethod]
        public void Test_Get_Group_by_descriptor()
        {
            Graph.Graph adorasGraph = new Graph.Graph(_projectDef.AdoRestApiConfigurations["GraphApi"]);
            string groupDescriptor = "vssgp.Uy0xLTktMTU1MTM3NDI0NS00MTcwNzg4NDcyLTM0MzkzNDQ3MTQtMjg5MzE0ODY0NC0zMDYwNDE5MzI4LTEtMTI2OTI0NzIzNi0xNzc5NjYzNjgwLTI2MjAxMjk0NjItODQ2MDU1Mzc4";
            var result = adorasGraph.GetGroupByDescriptor(groupDescriptor);

            Assert.IsNotNull(result);
            var expectedSid = "S-1-9-1551374245-4170788472-3439344714-2893148644-3060419328-1-1269247236-1779663680-2620129462-846055378";
            string sid = ADO.Tools.Utility.GetSidFromDescriptor(result.Descriptor);
            Assert.AreEqual(expectedSid, sid);
            //note missing Microsoft.TeamFoundation.Identity
        }
        
        [TestMethod]
        public void Test_Get_Descriptor_by_id()
        {
            Graph.Graph adorasGraph = new Graph.Graph(_projectDef.AdoRestApiConfigurations["GraphApi"]);
            string teamId = "b490bc69-a42e-48df-9536-10b21ae88da9";
            var result = adorasGraph.GetDescriptor(teamId);

            Assert.IsNotNull(result);
            string expectedGroupDescriptor = "vssgp.Uy0xLTktMTU1MTM3NDI0NS00MTcwNzg4NDcyLTM0MzkzNDQ3MTQtMjg5MzE0ODY0NC0zMDYwNDE5MzI4LTEtMTI2OTI0NzIzNi0xNzc5NjYzNjgwLTI2MjAxMjk0NjItODQ2MDU1Mzc4";
            Assert.AreEqual(expectedGroupDescriptor, result.Value);

            var expectedSid = "S-1-9-1551374245-4170788472-3439344714-2893148644-3060419328-1-1269247236-1779663680-2620129462-846055378";
            string sid = ADO.Tools.Utility.GetSidFromDescriptor(result.Value);
            Assert.AreEqual(expectedSid, sid);
            //note missing Microsoft.TeamFoundation.Identity
        }
    }
}
