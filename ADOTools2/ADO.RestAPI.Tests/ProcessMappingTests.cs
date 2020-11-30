using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ADO.Engine;
using ADO.Engine.Configuration.ProjectImport;
using ADO.RestAPI.Core;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace AzureDevOpsRestAPI.Tests
{
    [TestClass]
    public class ProcessMappingTests
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Tests.ProcessMappingTests"));

        #endregion

        #endregion

        private static ProjectDefinition _projectDef;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            // Send some traces.
            _mySource.Value.TraceInformation("Load Config");
            _mySource.Value.Flush();

            string configFilePath = @"..\..\Input\configuration-processdevopsabcs.json";
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
        public void Test_Load_Inherited_Process()
        {
            string json = File.ReadAllText(@"..\..\Input\InheritedProcess\Agile001.json");
            InheritedProcess agile001
                = JsonConvert.DeserializeObject<InheritedProcess>(json);

            json = JsonConvert.SerializeObject(agile001, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\InheritedProcess\Agile001.json", json);

            json = File.ReadAllText(@"..\..\Input\InheritedProcess\Agile.json");
            InheritedProcess agileCopy
                = JsonConvert.DeserializeObject<InheritedProcess>(json);

            json = JsonConvert.SerializeObject(agileCopy, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\InheritedProcess\Agile.json", json);

            json = File.ReadAllText(@"..\..\Input\InheritedProcess\Scrum.json");
            InheritedProcess scrumCopy
                = JsonConvert.DeserializeObject<InheritedProcess>(json);

            json = JsonConvert.SerializeObject(scrumCopy, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\InheritedProcess\Scrum.json", json);

            json = File.ReadAllText(@"..\..\Input\InheritedProcess\CMMI.json");
            InheritedProcess cmmiCopy
                = JsonConvert.DeserializeObject<InheritedProcess>(json);

            json = JsonConvert.SerializeObject(cmmiCopy, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\InheritedProcess\CMMI.json", json);


            json = File.ReadAllText(@"..\..\Input\InheritedProcess\MigrateableScrum.json");
            InheritedProcess migrateableScrum
                = JsonConvert.DeserializeObject<InheritedProcess>(json);

            json = JsonConvert.SerializeObject(migrateableScrum, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\InheritedProcess\MigrateableScrum.json", json);


            json = File.ReadAllText(@"..\..\Input\InheritedProcess\MigrateableAgile.json");
            InheritedProcess migrateableAgile
                = JsonConvert.DeserializeObject<InheritedProcess>(json);

            json = JsonConvert.SerializeObject(migrateableAgile, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\InheritedProcess\MigrateableAgile.json", json);

            Assert.IsNotNull(agile001);
        }


        [TestMethod]
        [Ignore()] //takes time and only needs to be done when process changes
        public void Test_Load_System_Process_Agile()
        {
            // Create Azure DevOps REST api service object.
            _projectDef.AdoRestApiConfigurations["ProjectsApi"].Version = "5.1-preview";
            Projects adorasProjects = new Projects(_projectDef.AdoRestApiConfigurations["ProjectsApi"]);
            string systemProcessId = "adcc42ab-9882-485e-a3ed-7678f01f66bc"; //for agile

            string json = File.ReadAllText(@"..\..\Input\InheritedProcess\Agile.json");
            InheritedProcess agileCopy
                = JsonConvert.DeserializeObject<InheritedProcess>(json);

            var workitemtypes = adorasProjects.GetSystemProcessworkitemtypes(systemProcessId);

            agileCopy.States = new List<State>();
            foreach (var item in agileCopy.WorkItemTypes)
            {
                item.Id = $"Microsoft.VSTS.WorkItemTypes.{item.Name.Replace(" ", "")}";
                agileCopy.States.Add(new State()
                {
                    WorkItemTypeRefName = item.Id,
                    States = adorasProjects.GetSystemProcessworkitemtypeStates(systemProcessId, item.Id).value
                });
            }

            //do layouts NO CAN DO
            //do workItemTypeFields
            agileCopy.WorkItemTypeFields = new List<WorkItemTypeField>();
            foreach (var item in agileCopy.WorkItemTypes)
            {
                item.Id = $"Microsoft.VSTS.WorkItemTypes.{item.Name.Replace(" ", "")}";
                agileCopy.WorkItemTypeFields.Add(new WorkItemTypeField()
                {
                    WorkItemTypeRefName = item.Id,
                    Fields = adorasProjects.GetSystemProcessworkitemtypeworkItemTypeFields(systemProcessId, item.Id).value
                });
            }

            //witFieldPicklists
            var picklists = adorasProjects.GetSystemProcessPicklists();

            foreach (var item in picklists.value)
            {
                var pick = adorasProjects.GetSystemProcessPicklists(item.id);
            }

            var actualFieldsWithPicklists = agileCopy.WorkItemTypeFields
                .SelectMany(witf => witf.Fields).Where(f => f.PickList != null);

            foreach (var item in agileCopy.WorkItemTypeFields)
            {
                var newItemFields = new List<Field1>();
                foreach (var witf in item.Fields)
                {
                    var newField1 = adorasProjects.GetSystemProcessworkitemtypeFields(systemProcessId, item.WorkItemTypeRefName,
                        witf.ReferenceName);
                    newItemFields.Add(newField1);
                }
                //assign new list
                item.Fields = newItemFields;
            }

            //rules
            agileCopy.Rules = new List<Rule>();
            foreach (var item in agileCopy.WorkItemTypes)
            {
                item.Id = $"Microsoft.VSTS.WorkItemTypes.{item.Name.Replace(" ", "")}";
                agileCopy.Rules.Add(new Rule()
                {
                    WorkItemTypeRefName = item.Id,
                    Rules = adorasProjects.GetSystemProcessworkitemtyperules(systemProcessId, item.Id).value
                });
            }

            json = JsonConvert.SerializeObject(agileCopy, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\InheritedProcess\Agile.json", json);

            Assert.IsTrue(true);
        }

        [TestMethod]
        [Ignore()] //takes time and only needs to be done when process changes
        public void Test_Load_System_Process_Scrum()
        {
            // Create Azure DevOps REST api service object.
            _projectDef.AdoRestApiConfigurations["ProjectsApi"].Version = "5.1-preview";
            Projects adorasProjects = new Projects(_projectDef.AdoRestApiConfigurations["ProjectsApi"]);
            string systemProcessId = "6b724908-ef14-45cf-84f8-768b5384da45"; //for scrum

            string json = File.ReadAllText(@"..\..\Input\InheritedProcess\Scrum.json");
            InheritedProcess scrumCopy
                = JsonConvert.DeserializeObject<InheritedProcess>(json);

            var workitemtypes = adorasProjects.GetSystemProcessworkitemtypes(systemProcessId);
            scrumCopy.WorkItemTypes = workitemtypes.value;

            scrumCopy.States = new List<State>();
            foreach (var item in scrumCopy.WorkItemTypes)
            {
                item.Id = $"Microsoft.VSTS.WorkItemTypes.{item.Name.Replace(" ", "")}";
                scrumCopy.States.Add(new State()
                {
                    WorkItemTypeRefName = item.Id,
                    States = adorasProjects.GetSystemProcessworkitemtypeStates(systemProcessId, item.Id).value
                });
            }

            //do layouts NO CAN DO
            //do workItemTypeFields
            scrumCopy.WorkItemTypeFields = new List<WorkItemTypeField>();
            foreach (var item in scrumCopy.WorkItemTypes)
            {
                item.Id = $"Microsoft.VSTS.WorkItemTypes.{item.Name.Replace(" ", "")}";
                scrumCopy.WorkItemTypeFields.Add(new WorkItemTypeField()
                {
                    WorkItemTypeRefName = item.Id,
                    Fields = adorasProjects.GetSystemProcessworkitemtypeworkItemTypeFields(systemProcessId, item.Id).value
                });
            }

            //witFieldPicklists
            var picklists = adorasProjects.GetSystemProcessPicklists();

            foreach (var item in picklists.value)
            {
                var pick = adorasProjects.GetSystemProcessPicklists(item.id);
            }

            var actualFieldsWithPicklists = scrumCopy.WorkItemTypeFields
                .SelectMany(witf => witf.Fields).Where(f => f.PickList != null);

            foreach (var item in scrumCopy.WorkItemTypeFields)
            {
                var newItemFields = new List<Field1>();
                foreach (var witf in item.Fields)
                {
                    var newField1 = adorasProjects.GetSystemProcessworkitemtypeFields(systemProcessId, item.WorkItemTypeRefName,
                        witf.ReferenceName);
                    newItemFields.Add(newField1);
                }
                //assign new list
                item.Fields = newItemFields;
            }

            //rules
            scrumCopy.Rules = new List<Rule>();
            foreach (var item in scrumCopy.WorkItemTypes)
            {
                item.Id = $"Microsoft.VSTS.WorkItemTypes.{item.Name.Replace(" ", "")}";
                scrumCopy.Rules.Add(new Rule()
                {
                    WorkItemTypeRefName = item.Id,
                    Rules = adorasProjects.GetSystemProcessworkitemtyperules(systemProcessId, item.Id).value
                });
            }

            json = JsonConvert.SerializeObject(scrumCopy, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\InheritedProcess\Scrum.json", json);

            Assert.IsTrue(true);
        }

        [TestMethod]
        [Ignore()] //takes time and only needs to be done when process changes
        public void Test_Load_System_Process_CMMI()
        {
            // Create Azure DevOps REST api service object.
            _projectDef.AdoRestApiConfigurations["ProjectsApi"].Version = "5.1-preview";
            Projects adorasProjects = new Projects(_projectDef.AdoRestApiConfigurations["ProjectsApi"]);
            string systemProcessId = "27450541-8e31-4150-9947-dc59f998fc01"; //for cmmi

            string json = File.ReadAllText(@"..\..\Input\InheritedProcess\CMMI.json");
            InheritedProcess cmmiCopy
                = JsonConvert.DeserializeObject<InheritedProcess>(json);

            var workitemtypes = adorasProjects.GetSystemProcessworkitemtypes(systemProcessId);
            cmmiCopy.WorkItemTypes = workitemtypes.value;

            cmmiCopy.States = new List<State>();
            foreach (var item in cmmiCopy.WorkItemTypes)
            {
                item.Id = $"Microsoft.VSTS.WorkItemTypes.{item.Name.Replace(" ", "")}";
                cmmiCopy.States.Add(new State()
                {
                    WorkItemTypeRefName = item.Id,
                    States = adorasProjects.GetSystemProcessworkitemtypeStates(systemProcessId, item.Id).value
                });
            }

            //do layouts NO CAN DO
            //do workItemTypeFields
            cmmiCopy.WorkItemTypeFields = new List<WorkItemTypeField>();
            foreach (var item in cmmiCopy.WorkItemTypes)
            {
                item.Id = $"Microsoft.VSTS.WorkItemTypes.{item.Name.Replace(" ", "")}";
                cmmiCopy.WorkItemTypeFields.Add(new WorkItemTypeField()
                {
                    WorkItemTypeRefName = item.Id,
                    Fields = adorasProjects.GetSystemProcessworkitemtypeworkItemTypeFields(systemProcessId, item.Id).value
                });
            }

            //witFieldPicklists
            var picklists = adorasProjects.GetSystemProcessPicklists();

            foreach (var item in picklists.value)
            {
                var pick = adorasProjects.GetSystemProcessPicklists(item.id);
            }

            var actualFieldsWithPicklists = cmmiCopy.WorkItemTypeFields
                .SelectMany(witf => witf.Fields).Where(f => f.PickList != null);

            foreach (var item in cmmiCopy.WorkItemTypeFields)
            {
                var newItemFields = new List<Field1>();
                foreach (var witf in item.Fields)
                {
                    var newField1 = adorasProjects.GetSystemProcessworkitemtypeFields(systemProcessId, item.WorkItemTypeRefName,
                        witf.ReferenceName);
                    newItemFields.Add(newField1);
                }
                //assign new list
                item.Fields = newItemFields;
            }

            //rules
            cmmiCopy.Rules = new List<Rule>();
            foreach (var item in cmmiCopy.WorkItemTypes)
            {
                item.Id = $"Microsoft.VSTS.WorkItemTypes.{item.Name.Replace(" ", "")}";
                cmmiCopy.Rules.Add(new Rule()
                {
                    WorkItemTypeRefName = item.Id,
                    Rules = adorasProjects.GetSystemProcessworkitemtyperules(systemProcessId, item.Id).value
                });
            }

            json = JsonConvert.SerializeObject(cmmiCopy, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\InheritedProcess\CMMI.json", json);

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(Newtonsoft.Json.JsonReaderException))]
        public void Test_Load_and_Save_Inherited_Process_MigrateableAgile()
        {
            // Create Azure DevOps REST api service object.
            _projectDef.AdoRestApiConfigurations["ProjectsApi"].Version = "5.1-preview";
            Projects adorasProjects = new Projects(_projectDef.AdoRestApiConfigurations["ProjectsApi"]);

            // Create an Azure DevOps REST api service object.
            Processes adorasProcesses = new Processes(_projectDef.AdoRestApiConfigurations["CoreApi"]);

            // Get processes.
            var processes = adorasProcesses.GetProcesses();

            //https://dev.azure.com/processdevopsabcs/_apis/process/processes?api-version=5.1

            var migrateableAgile = processes.Value.Single(p => p.Name == "MigrateableAgile");

            //"https://dev.azure.com/processdevopsabcs/_apis/work/processes/b6e47900-d287-4f56-84af-3183369b2108"

            //need to use process migration tools
            InheritedProcess migrateableAgileIP
                = adorasProjects.GetInheritedProcess(migrateableAgile.Id);

            //NEED TO USE it for XML
            //otherwise for inherited json use pipeline task
        }
    }
}
