using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ADO.Engine.Configuration;
using ADO.Engine.Configuration.ProjectImport;
using ADO.Engine.DefinitionFiles;
using ADO.Extensions;
using ADO.RestAPI;
using ADO.RestAPI.Build;
using ADO.RestAPI.Core;
using ADO.RestAPI.DistributedTasks;
using ADO.RestAPI.ExtensionManagement;
using ADO.RestAPI.Git;
using ADO.RestAPI.Graph;
using ADO.RestAPI.Policy;
using ADO.RestAPI.Queries;
using ADO.RestAPI.Release;
using ADO.RestAPI.Security;
using ADO.RestAPI.Service;
using ADO.RestAPI.Tasks.Security;
using ADO.RestAPI.Viewmodel50;
using ADO.RestAPI.Wiki;
using ADO.RestAPI.Work;
using ADO.RestAPI.WorkItemTracking.ClassificationNodes;
using ADO.Tools;
using ADO.RestAPI.ProcessMapping;

namespace ADO.Engine
{
    /// <summary>
    /// Class to manage Azure DevOps import processes.
    /// </summary>
    public sealed class ProjectImportEngine
    {
        #region - Static Declarations

        #region - Private Members

        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.Engine.ProjectImport"));
        private static Regex _validServiceEndpointInputPropertyNamesRegex;

        #endregion

        #endregion

        #region - Private Members

        private readonly EngineConfiguration _engineConfig;

        private void CreateDestinationProject(ProjectDefinition projectDef)
        {
            // Initialize.
            bool isADOProjectDeleted;
            string jsonContent;
            string operationId;
            string projectId;
            string tokenName;
            string tokenValue;
            List<KeyValuePair<string, string>> tokensList;
            string destinationProjectProcessName;
            CoreResponse.Processes processes;

            // Create Azure DevOps REST api service object.
            Projects adorasProjects = new Projects(projectDef.AdoRestApiConfigurations["ProjectsApi"]);

            // Delete it if it exists already.
            if (adorasProjects.TestIfProjectExistByName(projectDef.DestinationProject.Name))
            {
                // Find the project identifier based on the project name.
                projectId = adorasProjects.GetProjectIdByName(projectDef.DestinationProject.Name);

                // Send some traces.
                _mySource.Value.TraceInformation("Destination project {0} ({1}) already exists. It will be deleted", projectDef.DestinationProject.Name, projectId);
                _mySource.Value.Flush();

                // Delete existing project.
                isADOProjectDeleted = adorasProjects.DeleteProject(projectId);

                // The call to delete a project is in fact a request queued to delete one and it takes a short moment to complete,
                // so give a 15 seconds break to complete.
                // todo: this logic could be improved.
                Thread.Sleep(15000);

                // Is it really deleted?
                if (isADOProjectDeleted)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("Destination project {0} ({1}) has been deleted", projectDef.DestinationProject.Name, projectId);
                    _mySource.Value.Flush();
                }
                else
                {
                    string errorMsg = string.Format("Cannot delete project: {0}.", projectDef.DestinationProject.Name);
                    throw (new Exception(errorMsg));
                }
            }

            // Read the create project *.json file and replace tokens with the right values.
            var filename = projectDef.GetFileFor("CreateDestinationProject");
            jsonContent = projectDef.ReadDefinitionFile(filename);

            #region - Replace all generalization tokens.

            // Define all tokens to replace from the definition file.
            // Tokens must be created in order of resolution, so this is why
            // a list of key/value pairs is used instead of a dictionary.
            tokensList = new List<KeyValuePair<string, string>>();

            // Add token and replacement value to list.
            tokenName = Engine.Utility.CreateTokenName("Project");
            tokenValue = projectDef.DestinationProject.Name;
            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

            tokenName = Engine.Utility.CreateTokenName("Description");
            tokenValue = $"{projectDef.DestinationProject.Description}{Environment.NewLine}Created on {DateTime.Now.ToLongDateString()} at {DateTime.Now.ToLongTimeString()}";
            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

            tokenName = Engine.Utility.CreateTokenName("Visibility");
            tokenValue = "private";
            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

            tokenName = Engine.Utility.CreateTokenName("SourceControlType");
            tokenValue = "Git";
            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

            tokenName = Engine.Utility.CreateTokenName("ProcessTemplateType", ReplacementTokenValueType.Id);
            destinationProjectProcessName = projectDef.DestinationProject.ProcessTemplateTypeName;

            // Create an Azure DevOps REST api service object.
            Processes adorasProcesses = new Processes(projectDef.AdoRestApiConfigurations["CoreApi"]);

            // Get processes.
            processes = adorasProcesses.GetProcesses();
            tokenValue = processes.Value.SingleOrDefault(proc => proc.Name == destinationProjectProcessName).Id;

            //tokenValue = ADO.RestAPI.Constants.DestinationProcessTemplate;
            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

            // Replace tokens.
            jsonContent = Engine.Utility.ReplaceTokensInJsonContent(jsonContent, tokensList);

            #endregion

            // Send some traces.
            _mySource.Value.TraceInformation("Destination project {0} will be created", projectDef.DestinationProject.Name);
            _mySource.Value.Flush();

            // Create project against the destination collection/account.
            operationId = adorasProjects.CreateProject(jsonContent);

            // The call to create a project is in fact a request queued to create one and it takes a short moment to complete, so give a 15 seconds break to complete.
            Thread.Sleep(15000);

            // Send some traces.
            _mySource.Value.TraceInformation("Destination project {0} has been created (Operation Id: {1})", projectDef.DestinationProject.Name, operationId);
            _mySource.Value.Flush();
        }

        private WorkItemTrackingResponse.QueryHierarchyItem CreateQueryFolders(
            ProjectDefinition projectDef,
            string queryFolderPath)
        {
            // Initialize.
            int depth = 0;
            string path = string.Empty;
            string queryFolderToken;
            JObject requestBodyAsJObject;
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            string filename;
            string jsonContent;
            string[] tokens;
            string pathSoFar = string.Empty;

            WorkItemTrackingResponse.QueryHierarchyItem lastFolderCreated = null;

            Queries adorasQueries = new Queries(projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"])
            {
                // Set project identifier.
                ProjectId = projectDef.DestinationProject.Id
            };

            // Generate the query folder token.
            queryFolderToken = string.Format(@"$/{0}", adorasQueries.ProjectId);

            // Extract tokens.
            tokens = queryFolderPath.Split(new char[] { '/', '\\' });

            foreach (string token in tokens)
            {
                pathSoFar = path;

                // Add to path.
                path += string.Format(@"/{0}", token);

                // Get queries for that path.
                var qhi = adorasQueries.GetQueries(path, depth);

                // Add to token.
                //queryFolderToken += string.Format(@"/{0}", qhi.Id);

                //create it if it doesn't exist                
                if (qhi == null)
                {

                    // Generate the raw request message to create an iteration.
                    requestBodyAsJObject = new JObject
                    {
                        { "name", token },
                        { "isFolder", true }
                    };

                    // Write temporarily the content to create a new iteration.
                    jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);
                    filename = projectDef.GetFileFor("Work.CreateQueryFolder", new object[] { ++entitiesToImportCounter });
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Serialize the raw request message.
                    jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);

                    // Send some traces.
                    _mySource.Value.TraceInformation("Create query folder: {0}.", token);
                    _mySource.Value.Flush();

                    // Import one iteration at a specific path.                    
                    lastFolderCreated = adorasQueries.CreateQueryFolder(jsonContent, pathSoFar);
                }
                else if (qhi.Name == token)
                {
                    //already exists nothing to do
                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, "The query folder: {0} with path {1} already exists.", token, qhi.Path);
                    _mySource.Value.Flush();
                }
                else
                {
                    throw new NotImplementedException("too doo");
                }

            }

            return lastFolderCreated;
        }

        private void DeleteDefaultRepository(ProjectDefinition projectDef)
        {
            // Initialize.
            bool defaultRepositoryIsOnlyOne = false;
            bool operationStatus;
            string repositoryId;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Delete default repository.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                Repositories adorasRepositories = new Repositories(projectDef.AdoRestApiConfigurations["GitRepositoriesApi"]);

                // Get default repository identifier which has the same name as the project.
                repositoryId = adorasRepositories.GetRepositoryIdByName(projectDef.DestinationProject.Name);

                // Delete default repository if needed.
                if (repositoryId != null)
                {
                    if (projectDef.AdoEntities.Repositories.Count == 1)
                        foreach (var item in projectDef.AdoEntities.Repositories)
                            if (item.Value == repositoryId)
                                defaultRepositoryIsOnlyOne = true;

                    if (defaultRepositoryIsOnlyOne)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Default repository deletion operation was aborted as at least one repository must exist per project");
                        _mySource.Value.Flush();
                    }
                    else
                    {
                        // Delete repository.
                        operationStatus = adorasRepositories.DeleteRepository(repositoryId);

                        if (operationStatus)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("Default repository deleted.");
                            _mySource.Value.Flush();
                        }
                        else
                        {
                            string errorMsg = string.Format("Error while deleting default Git repository. Azure DevOps REST api error message: {0}", adorasRepositories.LastApiErrorMessage);

                            // Send some traces.
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                            _mySource.Value.Flush();
                        }
                    }
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while deleting default repository: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }
        }

        private string GetEntityNameWithoutPrefix(string name, string prefixName, string prefixSeparator)
        {
            // Initialize.
            string nameWOprefix;
            string expr = $"{prefixName}{prefixSeparator}";

            if (name.StartsWith(expr))
            {
                // Use a .Substring to replace instead of .Replace
                // this will prevent removing more than wanted if their are
                // repetitions of the prefix value inside the name.
                nameWOprefix = name.Substring(expr.Length);
            }
            else
                nameWOprefix = name;

            // Return name without prefix.
            return nameWOprefix;
        }

        private ProjectDefinition.MappingRecord GetMappingRecordForTaskGroupInitVersion(ProjectDefinition projectDef, string[] possibleTaskgroupNames)
        {
            // Initialize.
            string entityKey;
            string taskGroupId;
            string versionSpec;
            string[] subkeys;
            ProjectDefinition.MappingRecord mappingRecord = null;
            List<ProjectDefinition.MappingRecord> mappingRecords = new List<ProjectDefinition.MappingRecord>();

            // Try to retrieve the mapping record which will contain the original entity key.
            foreach (string taskgroupName in possibleTaskgroupNames)
            {
                // Find all possible records.
                List<ProjectDefinition.MappingRecord> mrs = projectDef.FindMappingRecords(x => x.OldEntityName == taskgroupName && x.Type == "TaskGroup");

                // Add only if results were found.
                if (mrs.Count > 0)
                    mappingRecords.AddRange(mrs);
            }

            if (mappingRecords.Count > 0)
            {
                // Extract the version spec. from key which is a composition key with
                // task group identifier + version spec.
                subkeys = SplitEntityKey(mappingRecords[0].OldEntityKey);
                taskGroupId = subkeys[0];
                versionSpec = subkeys[1];

                // Generate the entity key for version 1.*
                entityKey = NewEntityKeyForTaskGroupMapping(taskGroupId, "1.*");

                // Find record differently.
                mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityKey == entityKey && x.Type == "TaskGroup");
            }

            // Return mapping record found.
            return mappingRecord;
        }

        private void HandleSecurity(ProjectDefinition projectDef, string prefixName, string prefixSeparator, string securityTasksFile, ProjectImportBehavior behaviors,
            List<string> teamInclusions,
            List<string> teamExclusions
            )
        {
            // Initialize.
            bool haveACEsDefined = false;
            string identityDescriptor = null;
            string securityNamespaceId = null;
            string defaultSecurityToken = null;
            string jsonContent = null;
            string securityToken = null;
            string sid = null;
            FileInfo fi = null;
            SecurityResponse.AccessControlList acl = null;
            SecurityResponse.AccessControlList defaultAcl = null;
            SecurityResponse.AccessControlEntries aces = null;
            SecurityResponse.AccessControlEntry ace = null;
            string validUsersDescriptor = Utility.GenerateIdentityDescriptor(projectDef.SidCache["ValidUsers"]);

            string teamName = null;
            string oldTeamName = null;
            ProjectDefinition.MappingRecord mappingRecord = null;

            // Create Azure DevOps REST api service objects.
            Teams adorasTeams = new Teams(projectDef.AdoRestApiConfigurations["TeamsApi"]);
            Graph adorasGraph = new Graph(projectDef.AdoRestApiConfigurations["GraphApi"]);
            ClassificationNodes adorasCN = new ClassificationNodes(projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"]);
            AccessControlLists adorasACLs = new AccessControlLists(projectDef.AdoRestApiConfigurations["SecurityApi"]);

            if (behaviors.UseSecurityTasksFile)
            {
                // Get security tasks from file.
                fi = new FileInfo(securityTasksFile);
                if (!fi.Exists)
                    throw new FileNotFoundException($"Please ensure file [{fi.FullName}] exists");

                // Get content and deserialize.
                jsonContent = projectDef.ReadDefinitionFile(fi.FullName);
                SecurityTasks newlyAddedSecurityTasks = JsonConvert.DeserializeObject<SecurityTasks>(jsonContent);

                // Add tasks stored in file into living list.
                foreach (SecurityTasks.SecurityTask task in newlyAddedSecurityTasks.SecurityTaskList)
                    projectDef.SecurityTasks.SecurityTaskList.Add(task);
            }

            if (behaviors.QuerySecurityDescriptors)
            {
                // Retrieve all default security principals, some might miss at this step.
                Engine.Utility.RetrieveDefaultSecurityPrincipal(projectDef);

                #region - Include query folders security.

                if (behaviors.IncludeQueryFolderSecurity)
                {
                    // Initialize.
                    acl = null;
                    CoreResponse.WebApiTeam team = null;

                    try
                    {
                        // Get security namespace identifier.
                        securityNamespaceId = Constants.QueryFolderNamespaceId;

                        // Create an Azure DevOps REST api service object.
                        Queries adorasQueries = new Queries(projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"])
                        {
                            ProjectId = projectDef.DestinationProject.Id
                        };
                        // Retrieve security token for query folder called Shared Queries.
                        defaultSecurityToken = adorasQueries.GenerateQueryFolderToken("Shared Queries");

                        // add folder path query tasks.
                        foreach (var queryFolderPath in _engineConfig.QueryFolderPaths)
                        {
                            // Create Folder If It Doesn't Exist
                            var lastQueryFolderCreated = CreateQueryFolders(projectDef, queryFolderPath.Path);

                            // Retrieve security token for query folder.
                            securityToken = adorasQueries.GenerateQueryFolderToken(queryFolderPath.Path);

                            // Try to find team defined.
                            // todo: act upon problem when finding the team.



                            // Does it have a prefix?
                            // Tokenization occurred before knowing a prefix is required, so
                            // the token name has no prefix.
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                teamName = queryFolderPath.TeamName;
                            }
                            else
                            {
                                // Retrieve mapping record for team.
                                mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityName == queryFolderPath.TeamName && x.Type == "Team");
                                teamName = mappingRecord.NewEntityName;
                            }



                            //was team included???
                            string warningMsg = teamName == null ? $"Team {queryFolderPath.TeamName} is NULL" : null;
                            if (
                                teamName != null &&
                                IsTeamIncluded(queryFolderPath.TeamName, teamInclusions, teamExclusions,
                                out warningMsg)
                               )
                            {

                                team = adorasTeams.GetTeamByName(teamName);

                                // Get team SID under its resolved form.
                                sid = adorasGraph.GetTeamSid(team.Name, projectDef.AdoRestApiConfigurations["TeamsApi"]);

                                // Get acl.
                                acl = adorasACLs.GetAccessControlList(securityNamespaceId, defaultSecurityToken);

                                // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                                if (acl.AcesDictionary == null)
                                    haveACEsDefined = false;
                                else
                                {
                                    haveACEsDefined = true;

                                    // Extract ACE defined for project contributors.
                                    ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                                }

                                // Add a security task only if specific ACLs are defined.
                                if (haveACEsDefined)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"Add security task for query path: {queryFolderPath.Path}");
                                    _mySource.Value.Flush();

                                    // Add a security task to apply same ACE as project contributors to default team.
                                    identityDescriptor = Engine.Utility.GenerateIdentityDescriptor(sid);
                                    projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                                }
                            }
                            else
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, warningMsg);
                                _mySource.Value.Flush();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = string.Format("Error while handling query folder security, Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);

                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        _mySource.Value.Flush();
                    }
                }

                #endregion

                #region - Include endpoint security.

                if (behaviors.IncludeEndpointCreatorsSecurity)
                {
                    // Initialize.
                    CoreResponse.WebApiTeam team = null;
                    GraphResponse.GraphGroups allGroups = null;
                    GraphResponse.GraphGroup member = null;
                    GraphResponse.GroupMembers endpointCreatorsMembers = null;

                    // Get security namespace identifier.
                    securityNamespaceId = Constants.ProjectNamespaceId;

                    // Retrieve only creators.
                    try
                    {
                        // Get all members of endpoint creators group.
                        endpointCreatorsMembers = adorasGraph.GetGroupMembers(projectDef.SecurityPrincipalCache["EndpointCreators"].Descriptor, "down");
                        if (endpointCreatorsMembers.Value != null)
                        {
                            // Get all possible groups.
                            allGroups = adorasGraph.GetAllGroups();

                            // Browse each member.
                            foreach (var endpointCreatorsMember in endpointCreatorsMembers.Value)
                            {
                                // Retrieve member.
                                try
                                {
                                    // Try to retrieve member information.
                                    member = allGroups.Value.Single(x => x.Descriptor == endpointCreatorsMember.MemberDescriptor);

                                    // Extract sid from descriptor.
                                    sid = ADO.Tools.Utility.GetSidFromDescriptor(member.Descriptor);

                                    // Send some traces.
                                    _mySource.Value.TraceInformation("Found member: {0}", member.PrincipalName);
                                    _mySource.Value.TraceInformation("Descriptor: {0}", member.Descriptor);
                                    _mySource.Value.TraceInformation("SID: {0}", sid);
                                    _mySource.Value.Flush();
                                }
                                catch (Exception ex)
                                {
                                    string errorMsg = string.Format("Error while resolving descriptor: {0}, Error: {1}, Stack Trace: {2}", "groups", member.Descriptor, ex.Message, ex.StackTrace);

                                    // Send some traces.
                                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                                    _mySource.Value.Flush();
                                }
                            }

                            // Create membership between default team and endpoint creators.
                            GraphResponse.GroupMember gm1 = adorasGraph.AddGroupMembership(projectDef.SecurityPrincipalCache["DefaultTeam"].Descriptor, projectDef.SecurityPrincipalCache["EndpointCreators"].Descriptor);
                        }
                        else
                        {
                            _mySource.Value.TraceEvent(TraceEventType.Warning, 0, "Endpoint Creators group is empty");
                            _mySource.Value.Flush();
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("The endpoint creators group was not found");
                        _mySource.Value.Flush();
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = string.Format("Error while handling endpoint security: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);

                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                    }

                    // Create membership between default team and project valid users.
                    GraphResponse.GroupMember gm2 = adorasGraph.AddGroupMembership(projectDef.SecurityPrincipalCache["DefaultTeam"].Descriptor, projectDef.SecurityPrincipalCache["ValidUsers"].Descriptor);

                    // Retrieve security token for project.
                    securityToken = Engine.Utility.GenerateProjectToken(projectDef.DestinationProject.Id);

                    // Try to find team defined.
                    // todo: act upon problem when finding the team.
                    team = adorasTeams.GetTeamByName(projectDef.DefaultTeamName);

                    // Get team SID under its resolved form.
                    sid = adorasGraph.GetTeamSid(projectDef.DefaultTeamName, projectDef.AdoRestApiConfigurations["TeamsApi"]);

                    // Get acl.
                    acl = adorasACLs.GetAccessControlList(securityNamespaceId, securityToken);

                    // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                    if (acl.AcesDictionary == null)
                        haveACEsDefined = false;
                    else
                    {
                        haveACEsDefined = true;

                        // Extract ACE defined for project contributors.
                        ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                    }

                    // Add a security task only if specific ACLs are defined.
                    if (haveACEsDefined)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Add security task for endpoints");
                        _mySource.Value.Flush();

                        // Add a security task to apply same ACE as project contributors to default team.
                        identityDescriptor = Engine.Utility.GenerateIdentityDescriptor(sid);
                        projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                    }
                }

                #endregion

                #region - Include area security.

                if (behaviors.IncludeAreaSecurity)
                {
                    // Initialize.
                    acl = null;
                    CoreResponse.WebApiTeam team = null;

                    try
                    {
                        // Get security namespace identifier.
                        securityNamespaceId = Constants.CSSNamespaceId;
                        defaultSecurityToken = adorasCN.GetClassificationNodeToken(projectDef.DestinationProject.Name);

                        //Add a security task to apply reader permissions on the root
                        acl = adorasACLs.GetAccessControlList(securityNamespaceId, defaultSecurityToken);
                        ace = acl.AcesDictionary[Utility.GenerateIdentityDescriptor(projectDef.SidCache["Readers"])];
                        projectDef.SecurityTasks.AddTask(securityNamespaceId, defaultSecurityToken, false, validUsersDescriptor, ace);

                        // CSS
                        // add area paths tasks
                        foreach (var areaPath in _engineConfig.AreaPaths)
                        {
                            // Get security token for a specific classification node.
                            securityToken = adorasCN.GetClassificationNodeToken(areaPath.Path);

                            // Try to find team defined.
                            // todo: act upon problem when finding the team.


                            // Does it have a prefix?
                            // Tokenization occurred before knowing a prefix is required, so
                            // the token name has no prefix.
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                teamName = areaPath.TeamName;
                            }
                            else
                            {
                                // Retrieve mapping record for team.
                                mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityName == areaPath.TeamName && x.Type == "Team");
                                teamName = mappingRecord.NewEntityName;
                            }

                            //was team included???
                            string warningMsg = teamName == null ? $"Team {areaPath.TeamName} is NULL" : null;
                            if (
                                teamName != null &&
                                IsTeamIncluded(areaPath.TeamName, teamInclusions, teamExclusions,
                                out warningMsg)
                               )
                            {

                                team = adorasTeams.GetTeamByName(teamName);

                                // Get team SID under its resolved form.
                                sid = adorasGraph.GetTeamSid(team.Name, projectDef.AdoRestApiConfigurations["TeamsApi"]);

                                // Get acl.
                                acl = adorasACLs.GetAccessControlList(securityNamespaceId, securityToken);

                                // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                                if (acl.AcesDictionary == null)
                                    haveACEsDefined = false;
                                else
                                {
                                    haveACEsDefined = true;

                                    // Extract ACE defined for project contributors.
                                    ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                                }

                                // Add a security task only if specific ACLs are defined.
                                if (haveACEsDefined)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"Add security task for area path: {areaPath.Path}");
                                    _mySource.Value.Flush();

                                    // Add a security task to apply same ACE as project contributors to default team.
                                    identityDescriptor = Engine.Utility.GenerateIdentityDescriptor(sid);
                                    projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                                }
                            }
                            else
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, warningMsg);
                                _mySource.Value.Flush();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = string.Format("Error while handling area security, Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);

                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        _mySource.Value.Flush();
                    }
                }

                #endregion


                #region - Include repository security.

                if (behaviors.IncludeRepositorySecurity)
                {
                    // Initialize.
                    acl = null;
                    defaultAcl = null;
                    CoreResponse.WebApiTeam team = null;

                    try
                    {
                        // Get security namespace identifier.
                        securityNamespaceId = Constants.RepoSecNamespaceId;
                        defaultSecurityToken = $"repoV2/{projectDef.DestinationProject.Id}";

                        //Add a security task to apply reader permissions on the root
                        defaultAcl = adorasACLs.GetAccessControlList(securityNamespaceId, defaultSecurityToken);
                        ace = defaultAcl.AcesDictionary[Utility.GenerateIdentityDescriptor(projectDef.SidCache["Readers"])];
                        projectDef.SecurityTasks.AddTask(securityNamespaceId, defaultSecurityToken, false, validUsersDescriptor, ace);

                        // CSS
                        // add area paths tasks
                        foreach (var repo in _engineConfig.RepositorySecurity)
                        {
                            // Get security token for a specific classification node.
                            var mappings = projectDef.GetMappings();
                            var repoMapping = mappings.Find(x => x.OldEntityName == repo.Path);
                            securityToken = Utility.GenerateRepositoryV2Token(projectDef.DestinationProject.Id, repoMapping.NewEntityKey, null);

                            // Does it have a prefix?
                            // Tokenization occurred before knowing a prefix is required, so
                            // the token name has no prefix.
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                teamName = repo.TeamName;
                            }
                            else
                            {
                                // Retrieve mapping record for team.
                                mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityName == repo.TeamName && x.Type == "Team");
                                teamName = mappingRecord.NewEntityName;
                            }

                            // Try to find team defined.
                            // todo: act upon problem when finding the team.
                            team = adorasTeams.GetTeamByName(teamName);

                            // Get team SID under its resolved form.
                            sid = adorasGraph.GetTeamSid(team.Name, projectDef.AdoRestApiConfigurations["TeamsApi"]);

                            // Get acl.
                            acl = adorasACLs.GetAccessControlList(securityNamespaceId, securityToken);

                            if (repo.DisableInheritance)
                            {
                                // Set Inherit to false
                                acl.InheritPermissions = false;

                                // Remove extra users from ace
                                var acesToRemoveList = new string[]{
                                    Utility.GenerateIdentityDescriptor(projectDef.SidCache["Readers"]),
                                    Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"]),
                                    Utility.GenerateIdentityDescriptor(projectDef.SidCache["BuildAdministrators"])
                                };
                                foreach (string aceToRemove in acesToRemoveList)
                                {
                                    if (acl.AcesDictionary.ContainsKey(aceToRemove))
                                        acl.AcesDictionary.Remove(aceToRemove);
                                }
                                JObject setAclInput = new JObject(new JProperty("value", new JArray() { JObject.FromObject(acl) }));
                                jsonContent = JsonConvert.SerializeObject(setAclInput);
                                adorasACLs.UpdateAccessControlList(securityNamespaceId, jsonContent);
                            }

                            // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                            if (defaultAcl.AcesDictionary == null)
                                haveACEsDefined = false;
                            else
                            {
                                haveACEsDefined = true;

                                // Extract ACE defined for project contributors.
                                ace = defaultAcl.AcesDictionary[Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                            }

                            // Add a security task only if specific ACLs are defined.
                            if (haveACEsDefined)
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation($"Add security task for repo: {repo.Path}");
                                _mySource.Value.Flush();

                                ace.Allow = ace.Allow | 2048;  // Edit Branch Policies
                                ace.ExtendedInfo.EffectiveAllow = ace.ExtendedInfo.EffectiveAllow | 2048;

                                // Add a security task to apply same ACE as project contributors to default team.
                                identityDescriptor = Utility.GenerateIdentityDescriptor(sid);
                                projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = string.Format("Error while handling repository security, Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);

                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        _mySource.Value.Flush();
                    }
                }

                #endregion

                #region - Include build pipeline security.

                if (behaviors.IncludeBuildPipelineSecurity)
                {
                    // Initialize.
                    acl = null;
                    CoreResponse.WebApiTeam team = null;

                    try
                    {
                        // Get security namespace identifier.
                        securityNamespaceId = Constants.BuildNamespaceId;
                        defaultSecurityToken = Utility.GeneratePipelineFolderToken(projectDef.DestinationProject.Id);

                        //Add a security task to apply reader permissions on the root
                        acl = adorasACLs.GetAccessControlList(securityNamespaceId, defaultSecurityToken);
                        ace = acl.AcesDictionary[Utility.GenerateIdentityDescriptor(projectDef.SidCache["Readers"])];
                        projectDef.SecurityTasks.AddTask(securityNamespaceId, defaultSecurityToken, false, validUsersDescriptor, ace);

                        foreach (var buildFolder in _engineConfig.BuildPipelineFolderPaths)
                        {
                            // Retrieve security token for build pipeline folder.
                            securityToken = Engine.Utility.GeneratePipelineFolderToken(projectDef.DestinationProject.Id, buildFolder.Path);

                            // Does it have a prefix?
                            // Tokenization occurred before knowing a prefix is required, so
                            // the token name has no prefix.
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                teamName = buildFolder.TeamName;
                            }
                            else
                            {
                                // Retrieve mapping record for team.
                                mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityName == buildFolder.TeamName && x.Type == "Team");
                                teamName = mappingRecord.NewEntityName;
                            }

                            //was team included???
                            string warningMsg = teamName == null ? $"Team {buildFolder.TeamName} is NULL" : null;
                            if (
                                teamName != null &&
                                IsTeamIncluded(buildFolder.TeamName, teamInclusions, teamExclusions,
                                out warningMsg)
                               )
                            {

                                // Try to find team defined.
                                // todo: act upon problem when finding the team.
                                team = adorasTeams.GetTeamByName(teamName);

                                // Get team SID under its resolved form.
                                sid = adorasGraph.GetTeamSid(team.Name, projectDef.AdoRestApiConfigurations["TeamsApi"]);

                                // Get acl.
                                acl = adorasACLs.GetAccessControlList(securityNamespaceId, securityToken);

                                // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                                if (acl.AcesDictionary == null)
                                {
                                    // Get the default contributor permissions on the root build folder
                                    acl = adorasACLs.GetAccessControlList(securityNamespaceId, defaultSecurityToken);

                                    if (acl.AcesDictionary == null)
                                        haveACEsDefined = false;
                                    else
                                    {
                                        haveACEsDefined = true;

                                        // Extract ACE defined for project contributors.
                                        ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                                    }
                                }
                                else
                                {
                                    haveACEsDefined = true;

                                    // Extract ACE defined for project contributors.
                                    ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                                }

                                // Add a security task only if specific ACLs are defined.
                                if (haveACEsDefined)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"Add security task for build pipeline path: {buildFolder.Path}");
                                    _mySource.Value.Flush();

                                    // In addition to contributors permission, users are requesting the following two permissions
                                    ace.Allow = ace.Allow | 16;  // Manage build qualities
                                    ace.ExtendedInfo.EffectiveAllow = ace.ExtendedInfo.EffectiveAllow | 16;
                                    ace.Allow = ace.Allow | 256; // Manage build queue
                                    ace.ExtendedInfo.EffectiveAllow = ace.ExtendedInfo.EffectiveAllow | 256;

                                    // Add a security task to apply same ACE as project contributors to default team.
                                    identityDescriptor = Engine.Utility.GenerateIdentityDescriptor(sid);
                                    projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                                }
                            }
                            else
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, warningMsg);
                                _mySource.Value.Flush();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = string.Format("Error while handling build pipeline security, Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);

                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        _mySource.Value.Flush();
                    }
                }

                #endregion

                #region - Include release pipeline security.

                if (behaviors.IncludeReleasePipelineSecurity)
                {
                    // Initialize.
                    acl = null;
                    CoreResponse.WebApiTeam team = null;

                    try
                    {
                        // Get security namespace identifier.
                        securityNamespaceId = Constants.ReleaseNamespaceId;
                        defaultSecurityToken = Utility.GeneratePipelineFolderToken(projectDef.DestinationProject.Id);

                        //Add a security task to apply reader permissions on the root
                        acl = adorasACLs.GetAccessControlList(securityNamespaceId, defaultSecurityToken);
                        //if this is null - be sure to create a release and security associated to it
                        ace = acl.AcesDictionary[Utility.GenerateIdentityDescriptor(projectDef.SidCache["Readers"])];
                        projectDef.SecurityTasks.AddTask(securityNamespaceId, defaultSecurityToken, false, validUsersDescriptor, ace);

                        foreach (var releaseFolder in _engineConfig.ReleasePipelineFolderPaths)
                        {
                            // Retrieve security token for release pipeline folder.
                            securityToken = Engine.Utility.GeneratePipelineFolderToken(projectDef.DestinationProject.Id, releaseFolder.Path);

                            // Does it have a prefix?
                            // Tokenization occurred before knowing a prefix is required, so
                            // the token name has no prefix.
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                teamName = releaseFolder.TeamName;
                            }
                            else
                            {
                                // Retrieve mapping record for team.
                                mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityName == releaseFolder.TeamName && x.Type == "Team");
                                teamName = mappingRecord.NewEntityName;
                            }

                            //was team included???
                            string warningMsg = teamName == null ? $"Team {releaseFolder.TeamName} is NULL" : null;
                            if (
                                teamName != null &&
                                IsTeamIncluded(releaseFolder.TeamName, teamInclusions, teamExclusions,
                                out warningMsg)
                               )
                            {

                                // Try to find team defined.
                                // todo: act upon problem when finding the team.
                                team = adorasTeams.GetTeamByName(teamName);

                                // Get team SID under its resolved form.
                                sid = adorasGraph.GetTeamSid(team.Name, projectDef.AdoRestApiConfigurations["TeamsApi"]);

                                // Get acl.
                                acl = adorasACLs.GetAccessControlList(securityNamespaceId, securityToken);

                                // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                                if (acl.AcesDictionary == null)
                                {
                                    haveACEsDefined = false;

                                    // Use the default contributor permissions on the root build folder
                                    securityToken = defaultSecurityToken;

                                    // Get acl.
                                    acl = adorasACLs.GetAccessControlList(securityNamespaceId, securityToken);

                                    if (acl.AcesDictionary == null)
                                        haveACEsDefined = false;
                                    else
                                    {
                                        haveACEsDefined = true;

                                        // Extract ACE defined for project contributors.
                                        ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                                    }
                                }
                                else
                                {
                                    haveACEsDefined = true;

                                    // Extract ACE defined for project contributors.
                                    ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                                }

                                // Add a security task only if specific ACLs are defined.
                                if (haveACEsDefined)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"Add security task for release pipeline path: {releaseFolder.Path}");
                                    _mySource.Value.Flush();

                                    // Add a security task to apply same ACE as project contributors to default team.
                                    identityDescriptor = Engine.Utility.GenerateIdentityDescriptor(sid);
                                    projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                                }
                            }
                            else
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, warningMsg);
                                _mySource.Value.Flush();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = string.Format("Error while handling release pipeline security, Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);

                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        _mySource.Value.Flush();
                    }
                }

                #endregion
            }

            // Export tasks only if security was queried.
            if (behaviors.QuerySecurityDescriptors)
            {
                // Get file information.
                fi = new FileInfo(securityTasksFile);

                // Delete the tasks file if requested.
                if (behaviors.DeleteSecurityTasksOutputFile && File.Exists(fi.FullName))
                    File.Delete(fi.FullName);

                if (!Directory.Exists(fi.Directory.FullName))
                    Directory.CreateDirectory(fi.Directory.FullName);

                // Send some traces.
                _mySource.Value.TraceInformation("Save security tasks to {0}", fi.FullName);
                _mySource.Value.Flush();

                // Serialize and save to file.
                jsonContent = JsonConvert.SerializeObject(projectDef.SecurityTasks, Formatting.Indented);
                projectDef.WriteDefinitionFile(fi.FullName, jsonContent);
            }

            if ((behaviors.UseSecurityTasksFile || behaviors.QuerySecurityDescriptors) && behaviors.ExecuteSecurityTasks)
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Execute security tasks.");
                _mySource.Value.Flush();

                // Initialize.
                string allegedSid = null;
                Dictionary<string, string> sidDictionary = new Dictionary<string, string>();

                // Create an Azure DevOps REST api service object.
                AccessControlEntries adorasAccessControlEntries = new AccessControlEntries(projectDef.AdoRestApiConfigurations["SecurityApi"]);

                foreach (SecurityTasks.SecurityTask task in projectDef.SecurityTasks.SecurityTaskList)
                {
                    // Reset counter.
                    int unresolvedSid = 0;

                    // Resolve descriptors according to dictionary
                    foreach (SecurityResponse.AccessControlEntry item in task.AccessControlEntries)
                    {
                        // Try go get SID from descriptor. This SID is not confirmed at this point.
                        allegedSid = Engine.Utility.ExtractSidFromIdentityDescriptor(item.Descriptor);

                        // If a sid is not found within the descriptor, resolve it.
                        if (!(Engine.Utility.IsSid(allegedSid)))
                        {
                            // Resolve SID and cache it..
                            if (!(sidDictionary.ContainsKey(allegedSid)))
                            {
                                sid = adorasGraph.GetTeamSid(allegedSid, projectDef.AdoRestApiConfigurations["TeamsApi"]);
                                sidDictionary[allegedSid] = sid;
                            }

                            // If unresolvable, increment the counter, otherwise replace with...
                            if (sidDictionary[allegedSid] == null)
                            {
                                unresolvedSid++;

                                // Send some traces.
                                _mySource.Value.TraceInformation("Cannot resolve this sid: {0}.", allegedSid);
                                _mySource.Value.Flush();
                            }
                            else
                                item.Descriptor = item.Descriptor.Replace(allegedSid, sidDictionary[allegedSid]);
                        }
                    }

                    // Confirm task is valid.
                    if (unresolvedSid == 0)
                        projectDef.SecurityTasks.ConfirmTask(task);
                }

                // Set ACES.
                foreach (SecurityTasks.SecurityTask task in projectDef.SecurityTasks.ConfirmedSecurityTaskList)
                    aces = adorasAccessControlEntries.SetAccessControlEntries(task.SecurityNamespaceId, task.Token, task.Merge, task.AccessControlEntries);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It will use this parameter but this feature is not implemented yet.")]
        private void ImportAgentQueue(ProjectDefinition projectDef)
        {
            string errorMsg = string.Format("{0} behavior has not been implemented yet.", "ImportAgentQueue");
            throw (new NotImplementedException(errorMsg));
        }

        private void ImportArea(ProjectDefinition projectDef, bool isForProjectInitialization,
            List<string> areaInclusions, bool isClone)
        {
            // Initialize.
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            string areaName;
            string areaPath;
            string filename;
            string jsonContent;
            JObject requestBodyAsJObject;
            WorkItemTrackingResponse.WorkItemClassificationNode ncn;
            List<string> jsonContentsToProcess = new List<string>();

            string definition;
            bool useAreaPrefixPath;
            string workFile;

            //regular or project Initialization?
            if (isForProjectInitialization)
            {
                definition = "AreasInitialization";
                useAreaPrefixPath = false;
                workFile = "Work.ImportAreaInitialization";
            }
            else
            {
                definition = "Areas";
                useAreaPrefixPath = true;
                workFile = "Work.ImportArea";
            }

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import areas.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition(definition);

                // Create an Azure DevOps REST api service object.
                ClassificationNodes adorasCN = new ClassificationNodes(projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"]);

                // Read definition of teams.
                // It may contain one or many teams.
                if (definition == "Areas")
                {
                    jsonContent = projectDef.ReadDefinitionFile(projectDef.AreaDefinitions.FilePath);
                    jsonContentsToProcess.Add(jsonContent);
                }
                else if (definition == "AreasInitialization")
                {
                    foreach (var aid in projectDef.AreaInitializationDefinitions)
                    {
                        jsonContent = projectDef.ReadDefinitionFile(aid.FilePath);
                        jsonContentsToProcess.Add(jsonContent);
                    }
                }
                else
                    throw new NotImplementedException($"can't handle def {definition}");

                foreach (var jsonContentToProcess in jsonContentsToProcess)
                {

                    dynamic test = JObject.Parse(jsonContentToProcess);

                    // Get all classification nodes as queue.
                    Queue<JObject> areaNodeQueue = ClassificationNodes.GetClassificationNodesAsQueue(jsonContentToProcess,
                        areaInclusions,
                        isClone,
                        useAreaPrefixPath ? _engineConfig.AreaPrefixPath : string.Empty);

                    JObject skippedRootAreaNode = null;
                    if (isClone)
                    {
                        //dequeue first node
                        skippedRootAreaNode = areaNodeQueue.Dequeue();
                    }

                    // Set how many entities or objects must be imported.
                    //keep adding the totals to get a final grand total
                    entitiesCounter += areaNodeQueue.Count;

                    // Create iterations in logical order.
                    while (areaNodeQueue.Count > 0)
                    {
                        // Get current iteration node.
                        JObject currentAreaNode = areaNodeQueue.Dequeue();

                        // Set iteration name.
                        areaName = currentAreaNode["name"].ToString();



                        // Generate the raw request message to create an iteration.
                        requestBodyAsJObject = new JObject
                            { { "name", areaName } };

                        // Add attributes if needed.
                        if (currentAreaNode.ContainsKey("attributes"))
                            requestBodyAsJObject.Add("attributes", currentAreaNode["attributes"]);

                        // Write temporarily the content to create a new iteration.
                        jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);
                        filename = projectDef.GetFileFor(workFile, new object[] { ++entitiesToImportCounter });
                        projectDef.WriteDefinitionFile(filename, jsonContent);

                        // Serialize the raw request message.
                        jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);

                        // Send some traces.
                        _mySource.Value.TraceInformation("Import area: {0}.", areaName);
                        _mySource.Value.Flush();

                        // Import one iteration at a specific path.
                        areaPath = Engine.Utility.ConvertToWebPath(currentAreaNode["processedParentPath"].ToString());
                        if (isClone)
                        {
                            string rootNodeName = skippedRootAreaNode["name"].ToString();
                            areaPath = areaPath.Substring(rootNodeName.Length);
                            if (!string.IsNullOrEmpty(areaPath))
                            {
                                areaPath = areaPath.Substring(1);
                            }
                        }
                        ncn = adorasCN.CreateArea(jsonContent, areaPath);

                        if (adorasCN.CRUDOperationSuccess)
                        {
                            // Increment the imported entities.
                            importedEntities++;

                            // Store area information.
                            if (ncn.Path != null)
                            {
                                projectDef.AdoEntities.Areas.Add(ncn.Path, ncn.Id);
                            }
                            else
                            {
                                //on premise does NOT have path attribute. Compare:
                                //ADO services 5.1:
                                //https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/classification%20nodes/get%20classification%20nodes?view=azure-devops-rest-5.1#get-classification-nodes-from-list-of-ids                             
                                //ADO Server 2019 (on prem)
                                //https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/classification%20nodes/get%20classification%20nodes?view=azure-devops-server-rest-5.0#get-classification-nodes-from-list-of-ids
                                string ncnPath = currentAreaNode["processedFullPath"].ToString();// "\\MigrationToolVNext\\Area";
                                projectDef.AdoEntities.Areas.Add(ncnPath, ncn.Id);
                            }
                        }
                        else
                        {
                            string errorMsg = string.Format("Error while importing {0}: {1}. Azure DevOps REST api error message: {2}", "area", areaName, adorasCN.LastApiErrorMessage);

                            // Send some traces.
                            if (errorMsg.Contains(Constants.ErrorVS402371))
                            {
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, errorMsg);
                            }
                            else
                            {
                                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                            }
                            _mySource.Value.Flush();
                        }

                    }

                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating areas: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} areas were imported.", importedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private void InitializeArea(ProjectDefinition projectDef, string areaInitializationTreePath)
        {
            // Initialize.
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            string areaName;
            string areaPath;
            string filename;
            string jsonContent;
            JObject requestBodyAsJObject;
            WorkItemTrackingResponse.WorkItemClassificationNode ncn;

            string workFile;

            //load tree
            ADO.Engine.BusinessEntities.SimpleMutableClassificationNodeMinimalWithIdNode areaInitializationTree
                = ADO.Engine.BusinessEntities.SimpleMutableClassificationNodeMinimalWithIdNode.LoadFromJson(areaInitializationTreePath);

            workFile = "Work.ImportAreaInitialization";

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Initialize areas.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                ClassificationNodes adorasCN = new ClassificationNodes(projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"]);

                // Set how many entities or objects must be imported.
                //keep adding the totals to get a final grand total
                entitiesCounter += areaInitializationTree.Count() - 1; //subtract one for root being skipped

                // Create iterations in logical order. -- pre traversal order!!!
                foreach (var currentAreaNode in areaInitializationTree.Skip(1)) //skip root!
                {
                    //// Get current iteration node.

                    // Set iteration name.
                    areaName = currentAreaNode.Item.Name;

                    // Generate the raw request message to create an iteration.
                    requestBodyAsJObject = new JObject
                            { { "name", areaName } };

                    // Add attributes if needed.
                    if (currentAreaNode.Item.Attributes != null)
                        requestBodyAsJObject.Add("attributes", currentAreaNode.Item.Attributes.ToString());

                    // Write temporarily the content to create a new iteration.
                    jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);
                    filename = projectDef.GetFileFor(workFile, new object[] { ++entitiesToImportCounter });
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Serialize the raw request message.
                    jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);

                    // Send some traces.
                    _mySource.Value.TraceInformation("Import area (initialization): {0}.", areaName);
                    _mySource.Value.Flush();

                    // Import one iteration at a specific path.
                    areaPath = Engine.Utility.ConvertToWebPath(
                        currentAreaNode.Parent.Item.Path
                        );

                    //hack for now:
                    areaPath = currentAreaNode.Parent.Item
                        .GetClassificationNodePathWithoutRootAndStructureNodes(Constants.DefaultPathSeparatorForward);

                    ncn = adorasCN.CreateArea(jsonContent, areaPath);

                    if (adorasCN.CRUDOperationSuccess)
                    {
                        // Increment the imported entities.
                        importedEntities++;

                        // Store area information.
                        projectDef.AdoEntities.Areas.Add(ncn.Path, ncn.Id);
                    }
                    else
                    {
                        string errorMsg = string.Format("Error while importing {0}: {1}. Azure DevOps REST api error message: {2}", "area", areaName, adorasCN.LastApiErrorMessage);

                        // Send some traces.
                        if (errorMsg.Contains(Constants.ErrorVS402371))
                        {
                            _mySource.Value.TraceEvent(TraceEventType.Warning, 0, errorMsg);
                        }
                        else
                        {
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        }
                        _mySource.Value.Flush();
                    }

                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating areas: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} areas were imported.", importedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private void ImportBuildDefinition(ProjectDefinition projectDef, string prefixName, string prefixSeparator, ProjectImportBehavior behaviors,
            bool isOnPremiseMigration)
        {
            // Initialize.
            bool haveACEsDefined;
            bool skipDefinition;
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            int skippedEntities = 0;
            string buildDefinitionName;
            string defaultSecurityToken;
            string filename;
            string identityDescriptor;
            string jsonContent;
            string securityNamespaceId;
            string securityToken;
            string sid;
            string tokenName;
            string tokenValue;
            JToken buildDefinitionAsJToken;
            List<KeyValuePair<string, string>> tokensList;
            SecurityResponse.AccessControlList acl;
            SecurityResponse.AccessControlEntry ace = null;
            BuildMinimalResponse.BuildDefinitionReference nbd;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import build definitions.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition("BuildDefinitions");

                // Set how many entities or objects must be imported.
                entitiesCounter = projectDef.BuildDefinitions.Count;

                // Create an Azure DevOps REST api service object.
                BuildDefinition adorasBuild = new BuildDefinition(projectDef.AdoRestApiConfigurations["BuildApi"]);
                AccessControlLists adorasACLs = new AccessControlLists(projectDef.AdoRestApiConfigurations["SecurityApi"]);

                foreach (SingleObjectDefinitionFile df in projectDef.BuildDefinitions)
                {
                    try
                    {
                        // Reset skip flag.
                        skipDefinition = false;

                        // Read the content of definition file.
                        jsonContent = projectDef.ReadDefinitionFile(df.FilePath);

                        // Send some traces.
                        _mySource.Value.TraceInformation($"Process build definition stored in {df.FileName}.");
                        _mySource.Value.Flush();

                        #region - Replace all generalization tokens.

                        // Send some traces.
                        _mySource.Value.TraceInformation("Replace tokens with values.");
                        _mySource.Value.Flush();

                        // Define all tokens to replace from the preset file.
                        // Tokens must be created in order of resolution, so this is why
                        // a list of key/value pairs is used instead of a dictionary.
                        tokensList = new List<KeyValuePair<string, string>>();

                        // Add tokens and replacement values related to repositories to list.
                        foreach (string repositoryName in projectDef.AdoEntities.Repositories.Keys)
                        {
                            // Does it have a prefix?
                            // Tokenization occurred before knowing a prefix is required, so
                            // the token name has no prefix.
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                tokenName = Engine.Utility.CreateTokenName(repositoryName, "Repository", ReplacementTokenValueType.Id);
                                tokenValue = projectDef.AdoEntities.Repositories[repositoryName];
                            }
                            else
                            {
                                string repositoryNameWOPrefix = GetEntityNameWithoutPrefix(repositoryName, prefixName, prefixSeparator);
                                tokenName = Engine.Utility.CreateTokenName(repositoryNameWOPrefix, "Repository", ReplacementTokenValueType.Id);
                                tokenValue = projectDef.AdoEntities.Repositories[repositoryName];
                            }
                            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                        }

                        // Add tokens and replacement values related to variable groups to list.
                        foreach (string variableGroupName in projectDef.AdoEntities.VariableGroups.Keys)
                        {
                            // Does it have a prefix?
                            // Tokenization occurred before knowing a prefix is required, so
                            // the token name has no prefix.
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                tokenName = Engine.Utility.CreateTokenName(variableGroupName, "VariableGroup", ReplacementTokenValueType.Id);
                                tokenValue = projectDef.AdoEntities.VariableGroups[variableGroupName].ToString();
                            }
                            else
                            {
                                string variableGroupNameWOPrefix = GetEntityNameWithoutPrefix(variableGroupName, prefixName, prefixSeparator);
                                tokenName = Engine.Utility.CreateTokenName(variableGroupNameWOPrefix, "VariableGroup", ReplacementTokenValueType.Id);
                                tokenValue = projectDef.AdoEntities.VariableGroups[variableGroupName].ToString();
                            }
                            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                        }

                        // Add tokens and replacement values related to global service endpoints to list.
                        foreach (string serviceEndpointName in projectDef.AdoEntities.ServiceEndpoints.Keys)
                        {
                            // Does it have a prefix?
                            // Tokenization occurred before knowing a prefix is required, so
                            // the token name has no prefix.
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                tokenName = Engine.Utility.CreateTokenName(serviceEndpointName, "Endpoint", ReplacementTokenValueType.Id);
                                tokenValue = projectDef.AdoEntities.ServiceEndpoints[serviceEndpointName].ToString();
                            }
                            else
                            {
                                string serviceEnpointNameWOPrefix = GetEntityNameWithoutPrefix(serviceEndpointName, prefixName, prefixSeparator);
                                tokenName = Engine.Utility.CreateTokenName(serviceEnpointNameWOPrefix, "Endpoint", ReplacementTokenValueType.Id);
                                tokenValue = projectDef.AdoEntities.ServiceEndpoints[serviceEndpointName].ToString();
                            }
                            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                        }

                        // Replace tokens.
                        jsonContent = Engine.Utility.ReplaceTokensInJsonContent(jsonContent, tokensList);

                        #endregion

                        #region - Remap identifiers for task, task groups and service endpoints.

                        // Send some traces.
                        _mySource.Value.TraceInformation("Remap task, task group and service endpoint identifiers.");
                        _mySource.Value.Flush();

                        // Read build definition.
                        buildDefinitionAsJToken = JToken.Parse(jsonContent);
                        buildDefinitionName = buildDefinitionAsJToken["name"].ToString();

                        // Validate task identifiers inside process\phases\steps\task object.
                        // Remap metatask (task group) identifiers inside process\phases\steps\task object.
                        // Remap service enpdoint identifiers inside process\phases\steps\inputs object.
                        foreach (JToken phaseAsJToken in (JArray)buildDefinitionAsJToken["process"]["phases"])
                        {
                            foreach (JToken stepAsJToken in (JArray)phaseAsJToken["steps"])
                            {
                                // Process only if the step is enabled. It may be disabled for
                                // long enough time to be able to reconcile the information.
                                // This is safer to discard the data in the step.
                                if (stepAsJToken["enabled"].ToObject<bool>())
                                {
                                    if (stepAsJToken["task"] != null)
                                    {
                                        try
                                        {
                                            // Try to remap task.
                                            RemapTask(projectDef, stepAsJToken, "Build");

                                            // Try to remap task group (also known as metatask).
                                            RemapMetatask(projectDef, stepAsJToken["task"], "Build");
                                        }
                                        catch (RecoverableException ex)
                                        {
                                            // Send some traces.
                                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                            _mySource.Value.Flush();

                                            // Will skip this definition.
                                            // Leave nested processing loop.
                                            skipDefinition = true;
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            // Send some traces.
                                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                            _mySource.Value.Flush();

                                            skipDefinition = true;
                                            break;
                                        }
                                    }

                                    if (stepAsJToken["inputs"] != null)
                                    {
                                        try
                                        {
                                            // Try to remap service endpoint.
                                            RemapServiceEndpoint(projectDef, stepAsJToken);
                                        }
                                        catch (RecoverableException ex)
                                        {
                                            // Send some traces.
                                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                            _mySource.Value.Flush();

                                            // Will skip this definition.
                                            // Leave nested processing loop.
                                            skipDefinition = true;
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            // Send some traces.
                                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                            _mySource.Value.Flush();

                                            skipDefinition = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            // Will skip this definition.
                            // Leave nested processing loop.
                            if (skipDefinition)
                                break;
                        }

                        // Skip definition.
                        if (skipDefinition)
                        {
                            // Increment counter of skipped entities.
                            skippedEntities++;

                            continue;
                        }

                        #endregion

                        #region - Create build definition.

                        // Send some traces.
                        _mySource.Value.TraceInformation("Create build definition: {0}.", buildDefinitionName);
                        _mySource.Value.Flush();

                        // Write temporarily the content to create a new build definition.
                        jsonContent = JsonConvert.SerializeObject(buildDefinitionAsJToken, Formatting.Indented);
                        filename = projectDef.GetFileFor("Work.ImportBuildDefinition", new object[] { ++entitiesToImportCounter });
                        projectDef.WriteDefinitionFile(filename, jsonContent);

                        // Serialize the raw request message.
                        jsonContent = JsonConvert.SerializeObject(buildDefinitionAsJToken);

                        // Create build definitions against destination collection/project.
                        nbd = adorasBuild.CreateBuildDefinition(jsonContent);

                        if (adorasBuild.CRUDOperationSuccess)
                        {
                            // Increment the imported entities.
                            importedEntities++;

                            // Instantiate a reference
                            ProjectDefinition.AdoEntityReference.IdBasedReferenceWithPath adoer = new ProjectDefinition.AdoEntityReference.IdBasedReferenceWithPath
                            {
                                Identifier = nbd.Id,
                                Name = nbd.Name,
                                Path = nbd.Path
                            };

                            // Store the ADO entity reference.
                            projectDef.AdoEntities.BuildDefinitions.Add(adoer.RelativePath, adoer);
                        }
                        else
                        {
                            string errorMsg = string.Format("Error while creating {0}: {1}. Azure DevOps REST api error message: {2}", "build definition", buildDefinitionName, adorasBuild.LastApiErrorMessage);

                            // Send some traces.
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                            _mySource.Value.Flush();
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, $"Error while processing build definition: {ex.Message}, {ex.StackTrace}");
                        _mySource.Value.Flush();
                    }
                }

                // Query security descriptors and generate tasks.
                if (behaviors.QuerySecurityDescriptors && !isOnPremiseMigration)
                {
                    // Wait a little more time to make sure the build definitions are created or
                    // the call to grab the ACLs will fail.
                    Thread.Sleep(10000);

                    // Get security namespace identifier.
                    securityNamespaceId = Constants.BuildNamespaceId;

                    // Define sid to associate to build definition.
                    // sid is just the team name here.
                    sid = projectDef.DefaultTeamName;

                    // Browse each build definitions from ADO entities created.
                    foreach (var item in projectDef.AdoEntities.BuildDefinitions)
                    {
                        // .Key property stores the definition name with its path.
                        // .Value property stores ADO entity reference object.
                        // Extract the reference object.
                        ProjectDefinition.AdoEntityReference.IdBasedReferenceWithPath adoer = item.Value;

                        // Set security token for build.
                        securityToken = Engine.Utility.GenerateBuildToken(projectDef.DestinationProject.Id, adoer.Identifier);
                        defaultSecurityToken = Engine.Utility.GenerateBuildToken(projectDef.DestinationProject.Id, null);

                        // Get acl.
                        acl = adorasACLs.GetAccessControlList(securityNamespaceId, defaultSecurityToken);

                        // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                        if (acl.AcesDictionary == null)
                            haveACEsDefined = false;
                        else
                        {
                            haveACEsDefined = true;

                            // Extract ACE defined for project contributors.
                            ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                        }

                        // Add a security task only if specific ACLs are defined.
                        if (haveACEsDefined)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Add security task for build definition: {adoer.Name}");
                            _mySource.Value.Flush();

                            // Add a security task to apply same ACE as project contributors to default team.
                            identityDescriptor = Engine.Utility.GenerateIdentityDescriptor(sid);
                            projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                        }
                    }
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating build definitions: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} build definitions were imported and {2} were skipped.", importedEntities, entitiesCounter, skippedEntities);
            _mySource.Value.Flush();
        }

        private void ImportIteration(ProjectDefinition projectDef, bool isForProjectInitialization,
            List<string> iterationInclusions, bool isClone)
        {
            // Initialize.
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            string filename;
            string iterationName;
            string iterationPath;
            string jsonContent;
            JObject requestBodyAsJObject;
            WorkItemTrackingResponse.WorkItemClassificationNode ncn;
            List<string> jsonContentsToProcess = new List<string>();

            string definition;
            bool useIterationPrefixPath;
            string workFile;

            //regular or project Initialization?
            if (isForProjectInitialization)
            {
                definition = "IterationsInitialization";
                useIterationPrefixPath = false;
                workFile = "Work.ImportIterationInitialization";
            }
            else
            {
                definition = "Iterations";
                useIterationPrefixPath = true;
                workFile = "Work.ImportIteration";
            }

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import iterations.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition(definition);

                // Create an Azure DevOps REST api service object.
                ClassificationNodes adorasCN = new ClassificationNodes(projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"]);

                // Read definition of teams.
                // It may contain one or many teams.
                if (definition == "Iterations")
                {
                    jsonContent = projectDef.ReadDefinitionFile(projectDef.IterationDefinitions.FilePath);
                    jsonContentsToProcess.Add(jsonContent);
                }
                else if (definition == "IterationsInitialization")
                {
                    foreach (var iid in projectDef.IterationInitializationDefinitions)
                    {
                        jsonContent = projectDef.ReadDefinitionFile(iid.FilePath);
                        jsonContentsToProcess.Add(jsonContent);
                    }
                }
                else
                    throw new NotImplementedException($"can't handle def {definition}");

                foreach (var jsonContentToProcess in jsonContentsToProcess)
                {
                    // Get all classification nodes as queue.
                    Queue<JObject> iterationNodeQueue = ClassificationNodes.GetClassificationNodesAsQueue(jsonContentToProcess,
                        iterationInclusions,
                        isClone,
                        useIterationPrefixPath ? _engineConfig.IterationPrefixPath : string.Empty);

                    JObject skippedRootIterationNode = null;
                    if (isClone)
                    {
                        //dequeue first node
                        skippedRootIterationNode = iterationNodeQueue.Dequeue();
                    }

                    // Iterations found.
                    //keep adding the totals to get a final grand total
                    entitiesCounter += iterationNodeQueue.Count;

                    // Create iterations in logical order.
                    while (iterationNodeQueue.Count > 0)
                    {
                        // Get current iteration node.
                        JObject currentIterationNode = iterationNodeQueue.Dequeue();

                        // Set iteration name.
                        iterationName = currentIterationNode["name"].ToString();

                        // Generate the raw request message to create an iteration.
                        requestBodyAsJObject = new JObject
                        { { "name", iterationName } };

                        // Add attributes if needed.
                        if (currentIterationNode.ContainsKey("attributes"))
                            requestBodyAsJObject.Add("attributes", currentIterationNode["attributes"]);

                        // Write temporarily the content to create a new iteration.
                        jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);
                        filename = projectDef.GetFileFor(workFile, new object[] { ++entitiesToImportCounter });
                        projectDef.WriteDefinitionFile(filename, jsonContent);

                        // Serialize the raw request message.
                        jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);

                        // Send some traces.
                        _mySource.Value.TraceInformation("Import iteration: {0}.", iterationName);
                        _mySource.Value.Flush();

                        // Import one iteration at a specific path.

                        iterationPath = Engine.Utility.ConvertToWebPath(currentIterationNode["processedParentPath"].ToString());
                        if (isClone)
                        {
                            string rootNodeName = skippedRootIterationNode["name"].ToString();
                            iterationPath = iterationPath.Substring(rootNodeName.Length);
                            if (!string.IsNullOrEmpty(iterationPath))
                            {
                                iterationPath = iterationPath.Substring(1);
                            }
                        }
                        ncn = adorasCN.CreateIteration(jsonContent, iterationPath);

                        if (adorasCN.CRUDOperationSuccess)
                        {
                            // Increment the imported entities.
                            importedEntities++;

                            // Store iteration information.                            
                            if (ncn.Path != null)
                            {
                                projectDef.AdoEntities.Iterations.Add(ncn.Path, ncn.Id);
                            }
                            else
                            {
                                //on premise does NOT have path attribute. Compare:
                                //ADO services 5.1:
                                //https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/classification%20nodes/get%20classification%20nodes?view=azure-devops-rest-5.1#get-classification-nodes-from-list-of-ids
                                //ADO Server 2019 (on prem)
                                //https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/classification%20nodes/get%20classification%20nodes?view=azure-devops-server-rest-5.0#get-classification-nodes-from-list-of-ids
                                string ncnPath = currentIterationNode["processedFullPath"].ToString();// "\\MigrationToolVNext\\Iteration";
                                projectDef.AdoEntities.Iterations.Add(ncnPath, ncn.Id);
                            }
                        }
                        else
                        {
                            string errorMsg = string.Format("Error while importing {0}: {1}. Azure DevOps REST api error message: {2}", "iteration", iterationName, adorasCN.LastApiErrorMessage);

                            // Send some traces.
                            if (errorMsg.Contains(Constants.ErrorVS402371))
                            {
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, errorMsg);
                            }
                            else
                            {
                                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                            }
                            _mySource.Value.Flush();
                        }
                    }
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating iterations: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} iterations were imported.", importedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private void InitializeIteration(ProjectDefinition projectDef, string iterationInitializationTreePath)
        {
            // Initialize.
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            string iterationName;
            string iterationPath;
            string filename;
            string jsonContent;
            JObject requestBodyAsJObject;
            WorkItemTrackingResponse.WorkItemClassificationNode ncn;

            string workFile;

            //load tree
            ADO.Engine.BusinessEntities.SimpleMutableClassificationNodeMinimalWithIdNode iterationInitializationTree
                = ADO.Engine.BusinessEntities.SimpleMutableClassificationNodeMinimalWithIdNode.LoadFromJson(iterationInitializationTreePath);

            workFile = "Work.ImportIterationInitialization";

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Initialize iterations.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                ClassificationNodes adorasCN = new ClassificationNodes(projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"]);

                // Set how many entities or objects must be imported.
                //keep adding the totals to get a final grand total
                entitiesCounter += iterationInitializationTree.Count() - 1; //subtract one for root being skipped

                // Create iterations in logical order. -- pre traversal order!!!
                foreach (var currentIterationNode in iterationInitializationTree.Skip(1)) //skip root!
                {
                    //// Get current iteration node.

                    // Set iteration name.
                    iterationName = currentIterationNode.Item.Name;

                    // Generate the raw request message to create an iteration.
                    requestBodyAsJObject = new JObject
                            { { "name", iterationName } };

                    // Add attributes if needed.
                    if (currentIterationNode.Item.Attributes != null)
                    {
                        var jToken = new JObject
                            {
                                { "startDate", currentIterationNode.Item.Attributes.StartDate },
                                { "finishDate", currentIterationNode.Item.Attributes.FinishDate }
                            };
                        requestBodyAsJObject.Add("attributes", jToken);
                    }

                    // Write temporarily the content to create a new iteration.
                    jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);
                    filename = projectDef.GetFileFor(workFile, new object[] { ++entitiesToImportCounter });
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Serialize the raw request message.
                    jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);

                    // Send some traces.
                    _mySource.Value.TraceInformation("Import iteration (initialization): {0}.", iterationName);
                    _mySource.Value.Flush();

                    // Import one iteration at a specific path.
                    iterationPath = Engine.Utility.ConvertToWebPath(
                        currentIterationNode.Parent.Item.Path
                        );

                    //hack for now:
                    iterationPath = currentIterationNode.Parent.Item
                        .GetClassificationNodePathWithoutRootAndStructureNodes(Constants.DefaultPathSeparatorForward);

                    ncn = adorasCN.CreateIteration(jsonContent, iterationPath);

                    if (adorasCN.CRUDOperationSuccess)
                    {
                        // Increment the imported entities.
                        importedEntities++;

                        // Store iteration information.
                        projectDef.AdoEntities.Iterations.Add(ncn.Path, ncn.Id);
                    }
                    else
                    {
                        string errorMsg = string.Format("Error while importing {0}: {1}. Azure DevOps REST api error message: {2}", "iteration", iterationName, adorasCN.LastApiErrorMessage);

                        // Send some traces.
                        if (errorMsg.Contains(Constants.ErrorVS402371))
                        {
                            _mySource.Value.TraceEvent(TraceEventType.Warning, 0, errorMsg);
                        }
                        else
                        {
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        }
                        _mySource.Value.Flush();
                    }

                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating iterations: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} iterations were imported.", importedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private void ImportPolicyConfiguration(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            int skippedEntities = 0;
            bool skipPolicy = false;
            string filename;
            string jsonContent = null;
            string[] pathsToSelectForRemoval = null;
            JToken policiesAsJToken = null;
            JToken policyConfigurationAsJToken;
            JToken curatedJToken = null;
            ProjectDefinition.MappingRecord mappingRecord = null;
            List<ProjectDefinition.MappingRecord> mappings = null;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import policy configurations.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                PolicyConfigurations adorasPolicy = new PolicyConfigurations(projectDef.AdoRestApiConfigurations["PolicyConfigurationsApi"]);

                // Load policy configurations definition.
                jsonContent = projectDef.ReadDefinitionFile(projectDef.GetFileFor("Policies"));
                policiesAsJToken = JsonConvert.DeserializeObject<JToken>(jsonContent);

                // Get mappings.
                mappings = projectDef.GetMappings();

                // Some properties must be removed as they are not needed during creation.
                pathsToSelectForRemoval = new string[]
                {
                    "$.createdBy",
                    "$.createdDate",
                    "$.isDeleted",
                    "$._links",
                    "$.revision",
                    "$.id",
                    "$.url",
                    "$.type.url",
                    "$.type.displayName"
                };

                // Set how many entities or objects must be installed.
                entitiesCounter = policiesAsJToken["count"].ToObject<int>();

                foreach (JToken policyAsJToken in (JArray)policiesAsJToken["value"])
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("Import policy configuration: {0} of type: {1}", policyAsJToken["id"], policyAsJToken["type"]["displayName"]);
                    _mySource.Value.Flush();
                    skipPolicy = false;

                    if (policyAsJToken["type"]["displayName"].ToString() == "Build")
                    {
                        _mySource.Value.TraceInformation("{0} policies currently not supported, it will be skipped.", "Build");
                        _mySource.Value.Flush();

                        // Increment counter of skipped entities.
                        skippedEntities++;

                        continue;
                    }

                    if (policyAsJToken["type"]["displayName"].ToString() == "Required reviewers")
                    {
                        _mySource.Value.TraceInformation("{0} policies currently not supported, it will be skipped.", "Required reviewers");
                        _mySource.Value.Flush();

                        // Increment counter of skipped entities.
                        skippedEntities++;

                        continue;
                    }

                    if (policyAsJToken["type"]["displayName"].ToString() == "File size restriction")
                    {
                        _mySource.Value.TraceInformation("{0} policies currently not supported, it will be skipped.", "File size restriction");
                        _mySource.Value.Flush();

                        // Increment counter of skipped entities.
                        skippedEntities++;

                        continue;
                    }

                    // Curate the JToken.
                    curatedJToken = policyAsJToken.RemoveSelectProperties(pathsToSelectForRemoval);

                    // Serialize again.
                    jsonContent = JsonConvert.SerializeObject(curatedJToken);

                    // Extract scopes.
                    foreach (JToken scopeAsJToken in (JArray)policyAsJToken["settings"]["scope"])
                    {
                        mappingRecord = null;
                        // Retrieve mapping record.
                        mappingRecord = mappings.Find(x => (x.OldEntityKey == scopeAsJToken["repositoryId"].ToString()) && (x.Type == "Repository"));

                        // todo: what happened if it is not found?
                        if (mappingRecord == null)
                        {
                            _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Unable to find mappingRecord for {scopeAsJToken["repositoryId"]}. Skipping...");
                            skipPolicy = true;
                            break;
                        }
                        // Replace identifiers..
                        jsonContent = jsonContent.Replace(mappingRecord.OldEntityKey, mappingRecord.NewEntityKey);

                        // Send some traces.
                        _mySource.Value.TraceInformation("Replacing {0} with {1}", mappingRecord.OldEntityKey, mappingRecord.NewEntityKey);
                    }
                    if (skipPolicy)
                    {
                        skippedEntities++;
                        continue;
                    }

                    // Serialize the raw request message.
                    policyConfigurationAsJToken = JsonConvert.DeserializeObject<JToken>(jsonContent);

                    // Write temporarily the content to create a new policy configuration group.
                    jsonContent = JsonConvert.SerializeObject(policyConfigurationAsJToken, Formatting.Indented);
                    filename = projectDef.GetFileFor("Work.ImportPolicyConfiguration", new object[] { ++entitiesToImportCounter });
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Create policy configuration.
                    // todo: add validation around this.
                    adorasPolicy.CreatePolicyConfigurations(jsonContent);

                    // Increment counter of imported entities.
                    importedEntities++;
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating policy configurations: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} policy configurations were imported and {2} were skipped.", importedEntities, entitiesCounter, skippedEntities);
            _mySource.Value.Flush();
        }

        private void ImportPullRequest(ProjectDefinition projectDef, string prefixName, string prefixSeparator)
        {
            // Initialize.
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            string filename;
            string pullRequestTitle;
            string repositoryId;
            string repositoryName;
            string jsonContent;
            string tokenName;
            string tokenValue;
            List<KeyValuePair<string, string>> tokensList;
            FileInfo fi;
            JToken pullrequestAsJToken;
            JToken threadsAsJToken;
            JArray commentsAsJArray;
            GitResponse.PullRequest pullRequest;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import pull requests.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition("PullRequests");

                // Set how many entities or objects must be imported.
                entitiesCounter = projectDef.PullRequestDefinitions.Count;

                foreach (PullRequestDefinitionFiles prdf in projectDef.PullRequestDefinitions)
                {
                    // Retrieve file information.
                    fi = new FileInfo(prdf.PullRequest.FilePath);

                    // Extract the repository name from the folder where the file is located.
                    repositoryName = fi.Directory.FullName.Split(new string[] { "\\", "//" }, StringSplitOptions.RemoveEmptyEntries).Last();

                    // Does it require a prefix?
                    if (!string.IsNullOrEmpty(prefixName))
                    {
                        // Retain task group name from definition file.
                        string repositoryNameFromDefinitionFile = repositoryName;

                        // Send some traces.
                        _mySource.Value.TraceInformation($"Add prefix: {prefixName}{prefixSeparator} in front of the repository name: {repositoryNameFromDefinitionFile}.");
                        _mySource.Value.Flush();

                        // Add the prefix and a dot.
                        repositoryName = $"{prefixName}{prefixSeparator}{repositoryNameFromDefinitionFile}";
                    }

                    // Extract repository identifier.
                    repositoryId = projectDef.AdoEntities.Repositories[repositoryName];

                    // Read the content of definition file.
                    jsonContent = projectDef.ReadDefinitionFile(prdf.PullRequest.FilePath);

                    // Send some traces.
                    _mySource.Value.TraceInformation($"Process pull request definition stored in {prdf.PullRequest.FileName}.");
                    _mySource.Value.Flush();

                    // Read pull request definition.
                    pullrequestAsJToken = JToken.Parse(jsonContent);
                    pullRequestTitle = pullrequestAsJToken["title"].ToString();

                    #region - Replace all generalization tokens.

                    // Define all tokens to replace from the definition file.
                    // Tokens must be created in order of resolution, so this is why
                    // a list of key/value pairs is used instead of a dictionary.
                    tokensList = new List<KeyValuePair<string, string>>();

                    // Add token and replacement value to list.
                    tokenName = Engine.Utility.CreateTokenName("Reviewer", ReplacementTokenValueType.Id);
                    tokenValue = projectDef.DefaultUser.Identity.Id;
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    // Replace tokens.
                    jsonContent = Engine.Utility.ReplaceTokensInJsonContent(jsonContent, tokensList);

                    #endregion

                    // Write temporarily the content to create a new pull request.
                    filename = projectDef.GetFileFor("Work.ImportPullRequest", new object[] { ++entitiesToImportCounter });
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Create an Azure DevOps REST api service object.
                    PullRequests adorasPullRequests = new PullRequests(projectDef.AdoRestApiConfigurations["GitPullRequestsApi"]);

                    // Create the pull request.
                    pullRequest = adorasPullRequests.CreatePullRequest(jsonContent, repositoryId);

                    if (adorasPullRequests.CRUDOperationSuccess)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Pull request: {0} with title: {1} was created", pullRequest.PullRequestId, pullRequest.Title);
                        _mySource.Value.Flush();

                        // Increment the imported entities.
                        importedEntities++;

                        // Store pull request information.
                        projectDef.AdoEntities.PullRequests.Add(pullRequest.Title, pullRequest.PullRequestId.ToString());

                        // Validate if threads exist?
                        if (prdf.PullRequestThreads != null)
                        {
                            // Read file.
                            jsonContent = projectDef.ReadDefinitionFile(prdf.PullRequestThreads.FilePath);

                            // Send some traces.
                            _mySource.Value.TraceInformation($"Process pull request threads definition stored in {prdf.PullRequestThreads.FileName}.");
                            _mySource.Value.Flush();

                            // Get threads.
                            threadsAsJToken = JsonConvert.DeserializeObject<JToken>(jsonContent);

                            if (threadsAsJToken["count"].ToObject<int>() > 0)
                            {
                                // Browse each thread of comments.
                                foreach (JToken threadAsJToken in threadsAsJToken["value"])
                                {
                                    // Generate the request body message from thread object.
                                    jsonContent = JsonConvert.SerializeObject(threadAsJToken);

                                    // Create the pull request thread and get its identifier.
                                    GitResponse.PRCommentThread thread = adorasPullRequests.CreatePullRequestThread(repositoryId, pullRequest.PullRequestId, jsonContent);

                                    if (thread != null)
                                    {
                                        // Send some traces.
                                        _mySource.Value.TraceInformation($"Pull request thread {thread.Id} was created for pull request {pullRequest.PullRequestId}");
                                        _mySource.Value.Flush();

                                        if (threadAsJToken["comments"] != null)
                                        {
                                            // Extract the list of comments.
                                            commentsAsJArray = (JArray)threadAsJToken["comments"];

                                            // Browse each comment in the thread.
                                            foreach (JToken commentAsJToken in commentsAsJArray)
                                            {
                                                // Generate the request body message from comment object.
                                                jsonContent = JsonConvert.SerializeObject(commentAsJToken);

                                                // Create the comment.
                                                adorasPullRequests.CreateComment(repositoryId, pullRequest.PullRequestId, thread.Id, jsonContent);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string errorMsg = string.Format("Error while creating {0}: {1}. Azure DevOps REST api error message: {2}", "pull request", pullRequestTitle, adorasPullRequests.LastApiErrorMessage);

                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        _mySource.Value.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating pull requests: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} pull requests were imported.", importedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private void ImportReleaseDefinition(ProjectDefinition projectDef, string prefixName, string prefixSeparator, ProjectImportBehavior behaviors,
            bool isOnPremiseMigration)
        {
            // Initialize.
            bool haveACEsDefined;
            bool skipDefinition;
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            int skippedEntities = 0;
            string defaultSecurityToken;
            string filename;
            string identityDescriptor;
            string jsonContent;
            string releaseDefinitionName;
            string securityNamespaceId;
            string securityToken;
            string sid;
            string tokenName;
            string tokenValue;
            List<KeyValuePair<string, string>> tokensList;
            List<KeyValuePair<string, string>> tokensBaseList;
            JToken releaseDefinitionAsJToken;
            SecurityResponse.AccessControlList acl;
            SecurityResponse.AccessControlEntry ace = null;
            ReleaseMinimalResponse.ReleaseDefinition nrd;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import release definitions.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition("ReleaseDefinitions");

                // Set how many entities or objects must be imported.
                entitiesCounter = projectDef.ReleaseDefinitions.Count;

                // Create Azure DevOps REST api service objects.
                ReleaseDefinition adorasRelease = new ReleaseDefinition(projectDef.AdoRestApiConfigurations["ReleaseApi"]);
                AccessControlLists adorasACLs = new AccessControlLists(projectDef.AdoRestApiConfigurations["SecurityApi"]);

                // Get first team member.
                CoreResponse.TeamMember teamMember = projectDef.DefaultTeamMembers.Value.FirstOrDefault();

                #region - Define shared generalization replacement tokens.

                // Define all shared tokens to replace from the definition file.
                // Tokens must be created in order of resolution, so this is why
                // a list of key/value pairs is used instead of a dictionary.
                tokensBaseList = new List<KeyValuePair<string, string>>();

                // Add token and replacement value to list.
                tokenName = Engine.Utility.CreateTokenName("Collection");
                tokenValue = projectDef.DestinationProject.Collection;
                tokensBaseList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                tokenName = Engine.Utility.CreateTokenName("Project");
                tokenValue = projectDef.DestinationProject.Name;
                tokensBaseList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                tokenName = Engine.Utility.CreateTokenName("Project", ReplacementTokenValueType.Id);
                tokenValue = projectDef.DestinationProject.Id;
                tokensBaseList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                tokenName = Engine.Utility.CreateTokenName("Owner", ReplacementTokenValueType.UniqueName);
                tokenValue = EscapeOutBackslashForJson(teamMember.Identity.UniqueName);
                tokensBaseList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                tokenName = Engine.Utility.CreateTokenName("Owner", ReplacementTokenValueType.Id);
                tokenValue = teamMember.Identity.Id;
                tokensBaseList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                tokenName = Engine.Utility.CreateTokenName("Owner", ReplacementTokenValueType.DisplayName);
                tokenValue = teamMember.Identity.DisplayName;
                tokensBaseList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                #endregion

                foreach (SingleObjectDefinitionFile df in projectDef.ReleaseDefinitions)
                {
                    try
                    {
                        // Reset skip flag.
                        skipDefinition = false;

                        // Read the content of definition file.
                        jsonContent = projectDef.ReadDefinitionFile(df.FilePath);

                        // Send some traces.
                        _mySource.Value.TraceInformation($"Process release definition stored in {df.FileName}.");
                        _mySource.Value.Flush();

                        #region - Replace all generalization tokens.

                        // Send some traces.
                        _mySource.Value.TraceInformation("Replace tokens with values.");
                        _mySource.Value.Flush();

                        // Clear dictionary and copy base dictionary.
                        tokensList = new List<KeyValuePair<string, string>>();
                        foreach (var item in tokensBaseList)
                            tokensList.Add(item);

                        // Add tokens and replacement values related to agent queue to list.
                        foreach (string agentQueueName in projectDef.AdoEntities.AgentQueues.Keys)
                        {
                            tokenName = Engine.Utility.CreateTokenName(agentQueueName, "AgentQueue");
                            tokenValue = agentQueueName;
                            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                            tokenName = Engine.Utility.CreateTokenName(agentQueueName, "AgentQueue", ReplacementTokenValueType.Id);
                            tokenValue = projectDef.AdoEntities.AgentQueues[agentQueueName].ToString();
                            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                        }

                        // Add tokens and replacement values related to repositories to list.
                        foreach (string repositoryName in projectDef.AdoEntities.Repositories.Keys)
                        {
                            // Does it have a prefix?
                            // Tokenization occurred before knowing a prefix is required, so
                            // the token name has no prefix.
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                tokenName = Engine.Utility.CreateTokenName(repositoryName, "Repository", ReplacementTokenValueType.Id);
                                tokenValue = projectDef.AdoEntities.Repositories[repositoryName];
                            }
                            else
                            {
                                string repositoryNameWOPrefix = GetEntityNameWithoutPrefix(repositoryName, prefixName, prefixSeparator);
                                tokenName = Engine.Utility.CreateTokenName(repositoryNameWOPrefix, "Repository", ReplacementTokenValueType.Id);
                                tokenValue = projectDef.AdoEntities.Repositories[repositoryName];
                            }
                            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                        }

                        // Add tokens and replacement values related to variable groups to list.
                        foreach (string variableGroupName in projectDef.AdoEntities.VariableGroups.Keys)
                        {
                            // Does it have a prefix?
                            // Tokenization occurred before knowing a prefix is required, so
                            // the token name has no prefix.
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                tokenName = Engine.Utility.CreateTokenName(variableGroupName, "VariableGroup", ReplacementTokenValueType.Id);
                                tokenValue = projectDef.AdoEntities.VariableGroups[variableGroupName].ToString();
                            }
                            else
                            {
                                string variableGroupNameWOPrefix = GetEntityNameWithoutPrefix(variableGroupName, prefixName, prefixSeparator);
                                tokenName = Engine.Utility.CreateTokenName(variableGroupNameWOPrefix, "VariableGroup", ReplacementTokenValueType.Id);
                                tokenValue = projectDef.AdoEntities.VariableGroups[variableGroupName].ToString();
                            }
                            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                        }

                        // Add tokens and replacement values related to build definition to list.
                        foreach (var item in projectDef.AdoEntities.BuildDefinitions)
                        {
                            // Extract the reference object.
                            ProjectDefinition.AdoEntityReference.IdBasedReferenceWithPath adoer = item.Value;

                            tokenName = Engine.Utility.CreateTokenName(adoer.RelativePath, "BuildDefinition", ReplacementTokenValueType.Id);
                            tokenValue = adoer.Identifier.ToString();
                            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                        }                        

                        // Replace tokens.
                        jsonContent = Engine.Utility.ReplaceTokensInJsonContent(jsonContent, tokensList);

                        #endregion

                        #region - Remap identifiers for task groups and service endpoints.

                        // Read release definition.
                        releaseDefinitionAsJToken = JToken.Parse(jsonContent);
                        releaseDefinitionName = releaseDefinitionAsJToken["name"].ToString();

                        // Send some traces.
                        _mySource.Value.TraceInformation("Remap task group and service endpoint identifiers.");
                        _mySource.Value.Flush();

                        // Validate task identifiers inside environments\deployPhases\workflowTasks\task object.
                        // Remap metatask identifiers inside environments\deployPhases\workflowTasks\task object.
                        // Remap enpdoint identifiers inside environments\deployPhases\workflowTasks\task\inputs object.
                        foreach (JToken environmentAsJToken in (JArray)releaseDefinitionAsJToken["environments"])
                        {
                            foreach (JToken phaseAsJToken in (JArray)environmentAsJToken["deployPhases"])
                            {
                                foreach (JToken taskAsJToken in (JArray)phaseAsJToken["workflowTasks"])
                                {
                                    // Process only if the step is enabled. It may be disabled for
                                    // long enough time to be able to reconcile the information.
                                    // This is safer to discard the data in the step.
                                    if (taskAsJToken["enabled"].ToObject<bool>())
                                    {
                                        if (taskAsJToken != null)
                                        {
                                            try
                                            {
                                                // Try to remap task.
                                                RemapTask(projectDef, taskAsJToken, "Release");

                                                // Try to remap task group (also known as metatask).
                                                RemapMetatask(projectDef, taskAsJToken, "Release");
                                            }
                                            catch (RecoverableException ex)
                                            {
                                                // Send some traces.
                                                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                                _mySource.Value.Flush();

                                                // Will skip this definition.
                                                // Leave nested processing loop.
                                                skipDefinition = true;
                                                break;
                                            }
                                            catch (Exception ex)
                                            {
                                                // Send some traces.
                                                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                                _mySource.Value.Flush();
                                                skipDefinition = true;
                                                break;
                                            }

                                            if (taskAsJToken["inputs"] != null)
                                            {
                                                try
                                                {
                                                    // Try to remap service endpoint.
                                                    RemapServiceEndpoint(projectDef, taskAsJToken, "Release");
                                                }
                                                catch (RecoverableException ex)
                                                {
                                                    // Send some traces.
                                                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                                    _mySource.Value.Flush();

                                                    // Will skip this definition.
                                                    // Leave nested processing loop.
                                                    skipDefinition = true;
                                                    break;
                                                }
                                                catch (Exception ex)
                                                {
                                                    // Send some traces.
                                                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                                    _mySource.Value.Flush();

                                                    skipDefinition = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                // Will skip this definition.
                                // Leave nested processing loop.
                                if (skipDefinition)
                                    break;
                            }

                            // Will skip this definition.
                            // Leave nested processing loop.
                            if (skipDefinition)
                                break;
                        }

                        // Skip definition.
                        if (skipDefinition)
                        {
                            // Increment counter of skipped entities.
                            skippedEntities++;

                            continue;
                        }

                        #endregion

                        // Write temporarily the content to create a new release definition.
                        jsonContent = JsonConvert.SerializeObject(releaseDefinitionAsJToken, Formatting.Indented);
                        filename = projectDef.GetFileFor("Work.ImportReleaseDefinition", new object[] { ++entitiesToImportCounter });
                        projectDef.WriteDefinitionFile(filename, jsonContent);

                        // Send some traces.
                        _mySource.Value.TraceInformation("Import release definition: {0}.", releaseDefinitionName);
                        _mySource.Value.Flush();

                        // Serialize the raw request message.
                        jsonContent = JsonConvert.SerializeObject(releaseDefinitionAsJToken);

                        // Create release definition against destination collection / project.
                        nrd = adorasRelease.CreateReleaseDefinition(jsonContent);

                        // It may fail if the ARM outputs are using an older version.
                        if (adorasRelease.CRUDOperationSuccess)
                        {
                            // Verify if the problem is related to ARM templates being used.
                            if (adorasRelease.LastApiErrorMessage.TrimEnd() == "Tasks with versions 'ARM Outputs:3.*' are not valid for deploy job 'Function' in stage Azure-Dev.")
                            {
                                // Replace any version 3.* with 4.*.
                                jsonContent = jsonContent.Replace(@"3.*", @"4.*");

                                // Create release definition against destination collection / project.
                                nrd = adorasRelease.CreateReleaseDefinition(jsonContent);
                            }
                        }

                        if (adorasRelease.CRUDOperationSuccess)
                        {
                            // Increment the imported entities.
                            importedEntities++;

                            // Instantiate a reference
                            ProjectDefinition.AdoEntityReference.IdBasedReferenceWithPath adoer = new ProjectDefinition.AdoEntityReference.IdBasedReferenceWithPath
                            {
                                Identifier = nrd.Id,
                                Name = nrd.Name,
                                Path = nrd.Path
                            };

                            // Store the ADO entity reference.
                            projectDef.AdoEntities.ReleaseDefinitions.Add(adoer.RelativePath, adoer);
                        }
                        else
                        {
                            string errorMsg = string.Format("Error while creating {0}: {1}. Azure DevOps REST api error message: {2}", "release definition", releaseDefinitionName, adorasRelease.LastApiErrorMessage);

                            // Send some traces.
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                            _mySource.Value.Flush();
                        }
                    }
                    catch (Exception ex)
                    {
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, $"Error while processing release definition: {ex.Message}, {ex.StackTrace}");
                        _mySource.Value.Flush();
                    }
                }

                // Query security descriptors and generate tasks.
                if (behaviors.QuerySecurityDescriptors && !isOnPremiseMigration)
                {
                    // Wait a little more time to make sure the release definitions are created or
                    // the call to grab the ACLs will fail.
                    Thread.Sleep(10000);

                    // Get security namespace identifier.
                    securityNamespaceId = Constants.ReleaseNamespaceId;

                    // Define sid to associate to release definition.
                    // sid is just the team name here.
                    sid = projectDef.DefaultTeamName;

                    foreach (var item in projectDef.AdoEntities.ReleaseDefinitions)
                    {
                        // .Key property stores the definition name with its path.
                        // .Value property stores ADO entity reference object.
                        // Extract the reference object.
                        ProjectDefinition.AdoEntityReference.IdBasedReferenceWithPath adoer = item.Value;

                        // Retrieve security token for release.
                        // .Key property stores the variable group identifier needed to create the security token.
                        // .Value property stores the variable group name.
                        securityToken = Engine.Utility.GenerateReleaseToken(projectDef.DestinationProject.Id, adoer.Identifier);
                        defaultSecurityToken = Engine.Utility.GenerateReleaseToken(projectDef.DestinationProject.Id, null);

                        // Get acl.
                        acl = adorasACLs.GetAccessControlList(securityNamespaceId, defaultSecurityToken);

                        // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                        if (acl.AcesDictionary == null)
                            haveACEsDefined = false;
                        else
                        {
                            haveACEsDefined = true;

                            // Extract ACE defined for project contributors.
                            ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                        }

                        // Add a security task only if specific ACLs are defined.
                        if (haveACEsDefined)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Add security task for release definition: {adoer.Name}");
                            _mySource.Value.Flush();

                            // Add a security task to apply same ACE as project contributors to default team.
                            identityDescriptor = Engine.Utility.GenerateIdentityDescriptor(sid);
                            projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                        }
                    }
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating release definitions: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} release definitions were imported and {2} were skipped.", importedEntities, entitiesCounter, skippedEntities);
            _mySource.Value.Flush();
        }

        private string EscapeOutBackslashForJson(string originalValue)
        {
            var newValue = originalValue;
            newValue = newValue.Replace("\\", "\\\\");
            if (newValue != originalValue)
            {
                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Escaping out backslash for Json changed value from: '{originalValue}' to '{newValue}'");
                _mySource.Value.Flush();
            }
            return newValue;
        }

        private void ImportRepository(ProjectDefinition projectDef, string prefixName, string prefixSeparator, ProjectImportBehavior behaviors,
            bool isOnPremiseMigration)
        {
            // Initialize.
            bool haveACEsDefined;
            int entitiesCounter = 0;
            int importedEntities = 0;
            string endpointName;
            string identityDescriptor;
            string jsonContent;
            string repositoryName;
            string repositoryId;
            string securityNamespaceId;
            string securityToken;
            string sid;
            string tokenName;
            string tokenValue;
            List<KeyValuePair<string, string>> tokensList;
            JToken newEndpointAsJToken;
            SecurityResponse.AccessControlList acl;
            SecurityResponse.AccessControlEntry ace = null;
            ServiceEndpointResponse.ServiceEndpoint ep;
            GitMinimalResponse.GitRepository repository;

            try
            {
                #region - Create Git service endpoint for ImportRequest REST api.

                // Send some traces.
                _mySource.Value.TraceInformation("Import service endpoints to import code.");
                _mySource.Value.Flush();

                // Load definitions first to create Git service endpoints.
                projectDef.LoadDefinition("ServiceEndpointsForCodeImport");

                // Create an Azure DevOps REST api service object.
                ServiceEndPoints adorasService = new ServiceEndPoints(projectDef.AdoRestApiConfigurations["ServiceEndpointApi"]);

                foreach (SingleObjectDefinitionFile df in projectDef.ServiceEndpointForCodeImportDefinitions)
                {
                    // Read the content of definition file.
                    jsonContent = projectDef.ReadDefinitionFile(df.FilePath);

                    // Send some traces.
                    _mySource.Value.TraceInformation($"Process service endpoint for code import definition stored in {df.FileName}.");
                    _mySource.Value.Flush();

                    // Read endpoint definition.
                    newEndpointAsJToken = JToken.Parse(jsonContent);
                    endpointName = newEndpointAsJToken["name"].ToString();

                    // Send some traces.
                    _mySource.Value.TraceInformation("Import service endpoint: {0}.", endpointName);
                    _mySource.Value.Flush();

                    #region - Replace all generalization tokens.

                    // Define all tokens to replace from the definition file.
                    // Tokens must be created in order of resolution, so this is why
                    // a list of key/value pairs is used instead of a dictionary.
                    tokensList = new List<KeyValuePair<string, string>>();

                    // Add token and replacement value to list.
                    tokenName = Engine.Utility.CreateTokenName("Username");
                    tokenValue = _engineConfig.ImportSourceCodeCredentials.UserID;
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    tokenName = Engine.Utility.CreateTokenName("Password");
                    tokenValue = _engineConfig.ImportSourceCodeCredentials.Password;
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    tokenName = Engine.Utility.CreateTokenName("GitUsername");
                    tokenValue = _engineConfig.ImportSourceCodeCredentials.GitUsername;
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    tokenName = Engine.Utility.CreateTokenName("GitPassword");
                    tokenValue = _engineConfig.ImportSourceCodeCredentials.GitPassword;
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    // Replace tokens.
                    jsonContent = Engine.Utility.ReplaceTokensInJsonContent(jsonContent, tokensList);

                    #endregion

                    // Check if service endpoint to create already exists
                    ep = adorasService.GetServiceEndpointByName(endpointName);
                    if (ep == null)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Import endpoint: {0}.", endpointName);
                        _mySource.Value.Flush();

                        // Create service endpoint.
                        ep = adorasService.CreateServiceEndPoint(jsonContent);

                        if (adorasService.CRUDOperationSuccess)
                        {
                            // Store service endpoint information.
                            projectDef.AdoEntities.ServiceEndpoints.Add(ep.Name, ep.Id);
                        }
                        else
                        {
                            string errorMsg = string.Format("Error while creating {0}: {1}. Azure DevOps REST api error message: {2}", "service endpoint", endpointName, adorasService.LastApiErrorMessage);

                            // Send some traces.
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                            _mySource.Value.Flush();
                        }
                    }
                    else
                    {
                        // Store service endpoint information.
                        projectDef.AdoEntities.ServiceEndpoints.Add(ep.Name, ep.Id);
                    }
                }

                #endregion

                #region - Import source code using the ImportRequest REST api.

                // Send some traces.
                _mySource.Value.TraceInformation("Import source codes.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition("Repositories");

                // Set how many entities or objects must be imported.
                entitiesCounter = projectDef.RepositoryDefinitions.Count;

                // Create an Azure DevOps REST api service objects.
                Repositories adorasRepositories = new Repositories(projectDef.AdoRestApiConfigurations["GitRepositoriesApi"]);
                Repositories adorasImportRepositories = new Repositories(projectDef.AdoRestApiConfigurations["GitImportRequestsApi"]);
                AccessControlLists adorasACLs = new AccessControlLists(projectDef.AdoRestApiConfigurations["SecurityApi"]);

                #region - Set all generalization tokens to replace

                // Define all tokens to replace from the definition file.
                // Tokens must be created in order of resolution, so this is why
                // a list of key/value pairs is used instead of a dictionary.
                tokensList = new List<KeyValuePair<string, string>>();

                // Generate the token dictionary to import correctly source code via a
                // service endpoint configured for that purpose.
                foreach (var item in projectDef.AdoEntities.ServiceEndpoints)
                {
                    tokenName = Engine.Utility.CreateTokenName(item.Key, "Endpoint");
                    tokenValue = item.Value;
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                }

                #endregion

                foreach (SingleObjectDefinitionFile df in projectDef.RepositoryDefinitions)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Process repository definition stored in {df.FileName}.");
                    _mySource.Value.Flush();

                    // Extract the repository name from filename.
                    // Remove the suffix '-repository'.
                    string repositoryNameFromDefinitionFile = Path.GetFileNameWithoutExtension(df.FileName).Replace("-repository", null);

                    // Does it require a prefix?
                    if (string.IsNullOrEmpty(prefixName))
                        repositoryName = repositoryNameFromDefinitionFile;
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Add prefix: {prefixName}{prefixSeparator} in front of the repository name: {repositoryNameFromDefinitionFile}.");
                        _mySource.Value.Flush();

                        // Add the prefix and a dot.
                        repositoryName = $"{prefixName}{prefixSeparator}{repositoryNameFromDefinitionFile}";
                    }

                    // Try to retrieve the repository identifier if it exists.
                    repositoryId = adorasRepositories.GetRepositoryIdByName(repositoryName);

                    if (repositoryId == null)
                    {
                        // Is the repository named the same as the project?
                        if (projectDef.DestinationProject.Name.ToLower() == repositoryName.ToLower())
                            repository = adorasRepositories.GetDefaultRepository();
                        else
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Create repository: {repositoryName}.");
                            _mySource.Value.Flush();

                            // Set project identifier.
                            adorasRepositories.ProjectId = projectDef.DestinationProject.Id;

                            // Create Git repository.
                            repository = adorasRepositories.CreateRepository(repositoryName);

                            if (adorasRepositories.CRUDOperationSuccess)
                            {
                                // Store repository identifiers.
                                projectDef.AdoEntities.Repositories.Add(repository.Name, repository.Id);

                                // Update mapping record for this repository.
                                projectDef.UpdateMappingRecordByName("Repository", repositoryNameFromDefinitionFile, repository.Id, repository.Name);
                            }
                        }

                        // Store name and identifier of repository.
                        repositoryId = repository.Id;
                        repositoryName = repository.Name;

                        // Read the content of definition file.
                        jsonContent = projectDef.ReadDefinitionFile(df.FilePath);

                        // Replace tokens.
                        jsonContent = Engine.Utility.ReplaceTokensInJsonContent(jsonContent, tokensList);

                        // Send some traces.
                        _mySource.Value.TraceInformation("Import code from source location to: {0}", repositoryName);
                        _mySource.Value.Flush();

                        // Get source code.
                        adorasImportRepositories.GetSourceCodeFromGit(jsonContent, repositoryId);

                        // todo: this will create an async. operation to import the source code but it can take time.
                        // add a validation of the operation before returning.

                        // Increment the imported entities.
                        if (adorasImportRepositories.CRUDOperationSuccess)
                            importedEntities++;
                        else
                        {
                            string errorMsg = string.Format("Error while importing {0}: {1}. Azure DevOps REST api error message: {2}", "source code", repositoryName, adorasImportRepositories.LastApiErrorMessage);

                            // Send some traces.
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                            _mySource.Value.Flush();
                        }
                    }
                    else
                    {
                        _mySource.Value.TraceInformation("Repository already exists: {0}. There is nothing to do.", repositoryName);
                        _mySource.Value.Flush();
                    }
                }

                // Query security descriptors and generate tasks.
                if (behaviors.QuerySecurityDescriptors && !isOnPremiseMigration)
                {
                    // Define sid to associate to repository.
                    // sid is just the team name here.
                    sid = projectDef.DefaultTeamName;

                    // Get security namespace identifier.
                    securityNamespaceId = Constants.RepoSecNamespaceId;

                    foreach (SingleObjectDefinitionFile df in projectDef.RepositoryDefinitions)
                    {
                        try
                        {
                            // Extract the repository name from filename.
                            // Remove the suffix '-repository'.
                            string repositoryNameFromDefinitionFile = Path.GetFileNameWithoutExtension(df.FileName).Replace("-repository", null);

                            // Does it require a prefix?
                            if (string.IsNullOrEmpty(prefixName))
                                repositoryName = repositoryNameFromDefinitionFile;
                            else
                            {
                                // Add the prefix and a dot.
                                repositoryName = $"{prefixName}{prefixSeparator}{repositoryNameFromDefinitionFile}";
                            }

                            // Try to retrieve the repository identifier if it exists.
                            repositoryId = adorasRepositories.GetRepositoryIdByName(repositoryName);

                            var item = projectDef.AdoEntities.Repositories.Where(x => x.Key == repositoryName).FirstOrDefault();
                            // if can't find item
                            if (item.Equals(default(KeyValuePair<string, string>)))
                            {
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Unable to find imported repository with name: {repositoryName}. Unable to query security.");
                                _mySource.Value.Flush();
                                continue;
                            }

                            // Retrieve security token for repository (Git).
                            securityToken = Engine.Utility.GenerateRepositoryV2Token(projectDef.DestinationProject.Id, item.Value, null);

                            // Get acl.
                            acl = adorasACLs.GetAccessControlList(securityNamespaceId, securityToken);

                            // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                            if (acl.AcesDictionary == null)
                                haveACEsDefined = false;
                            else
                            {
                                haveACEsDefined = true;

                                // Extract ACE defined for project contributors.
                                ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Contributors"])];
                            }

                            // Add a security task only if specific ACLs are defined.
                            if (haveACEsDefined)
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation($"Add security task for repository: {repositoryName}({repositoryId})");
                                _mySource.Value.Flush();

                                // Add a security task to apply same ACE as project contributors to default team.
                                identityDescriptor = Engine.Utility.GenerateIdentityDescriptor(sid);
                                projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Send some traces.
                            string errorMsg = string.Format($"Error while querying repository security: {ex.Message}, Stack Trace: {ex.StackTrace}");
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                            _mySource.Value.Flush();
                        }
                    }
                }

                // Import code operation is an asynchronous task and can take some time to complete, so give a 15 seconds break to complete
                // as a shortcut to fix the problem.
                Thread.Sleep(15000);

                #endregion
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating repositories: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} source codes were imported.", importedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private void ImportServiceEndpoint(ProjectDefinition projectDef, string prefixName, string prefixSeparator)
        {
            // Initialize.
            bool hasAuthenticationTypeProperty;
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            string authType = null;
            string filename;
            string jsonContent;
            string reviewMeValue = "Review me";
            string username;
            string[] jsonPaths;
            string[] pathsToSelectForRemoval;
            string[] propertiesToRemove;
            JToken endpointAsJToken;
            JToken j;
            ServiceEndpointResponse.ServiceEndpoint createdEndpoint;
            ServiceEndpointResponse.ServiceEndpoints endpoints;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import service endpoints.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition("ServiceEndpoints");

                // Read definition of service endpoints.
                // It may contain one or many endpoints.
                jsonContent = projectDef.ReadDefinitionFile(projectDef.ServiceEndpointDefinitions.FilePath);
                endpoints = JsonConvert.DeserializeObject<ServiceEndpointResponse.ServiceEndpoints>(jsonContent);

                // Set how many entities or objects must be imported.
                entitiesCounter = endpoints.Count;

                // Some properties must be removed as they are not needed during creation.
                // Define JsonPath query to retrieve properties to remove.
                pathsToSelectForRemoval = new string[]
                {
                    "$.createdBy",
                    "$.id",
                    "$.isReady",
                    "$.operationStatus"
                };

                // Create an Azure DevOps REST api service object.
                ServiceEndPoints adorasServiceEndPoints = new ServiceEndPoints(projectDef.AdoRestApiConfigurations["ServiceEndpointApi"]);

                // Browse each definition.
                foreach (ServiceEndpointResponse.ServiceEndpoint ep in endpoints.Value)
                {
                    // Retain endpoint name from definition file.
                    string endPointNameFromDefinitionFile = ep.Name;

                    // Does it require a prefix?
                    if (!string.IsNullOrEmpty(prefixName))
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Add prefix: {prefixName}{prefixSeparator} in front of the service endpoint name.");
                        _mySource.Value.Flush();

                        // Add the prefix and a dot.
                        ep.Name = $"{prefixName}{prefixSeparator}{endPointNameFromDefinitionFile}";
                    }

                    try
                    {
                        // Add mapping record with current endpoint name.
                        projectDef.AddMappingRecord("ServiceEndpoint", ep.Id, endPointNameFromDefinitionFile);
                    }
                    catch
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Mapping record for service endpoint: {ep.Name} already exists, it indicates a previous import had been executed. Process will continue.");
                        _mySource.Value.Flush();
                    }

                    // Extract definition as JToken for curation.
                    jsonContent = JsonConvert.SerializeObject(ep);
                    endpointAsJToken = JsonConvert.DeserializeObject<JToken>(jsonContent);
                    endpointAsJToken.RemoveSelectProperties(pathsToSelectForRemoval);

                    // Replace security related properties with...
                    // Authorization Schemes: https://docs.microsoft.com/en-us/azure/devops/extend/develop/auth-schemes?view=azure-devops
                    switch (ep.Authorization.Scheme)
                    {
                        case "UsernamePassword":
                            // Username and password authorization scheme.
                            //
                            // The schema of an authorization object is the following:
                            // "authorization": {
                            //   "parameters": {
                            //     "username": "name",
                            //     "password": "password"
                            //   },
                            // "scheme": "UsernamePassword"
                            // }
                            //
                            // or
                            //
                            // "authorization": {
                            //   "parameters": null
                            // "scheme": "UsernamePassword"
                            // }

                            if (endpointAsJToken["authorization"]["parameters"].Type == JTokenType.Null)
                            {
                                // It happens when username and password are secret.

                                // Set a dummy value for username and password to force the team to create
                                // a new password or re-assign the password.
                                endpointAsJToken["authorization"]["parameters"] = new JObject(
                                    new JProperty("username", reviewMeValue),
                                    new JProperty("password", reviewMeValue)
                                );
                            }
                            else
                            {
                                // This property exists under several forms.
                                jsonPaths = new string[] { "$.username", "$.Username" };
                                j = endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths);

                                if (j == null)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"An username or Username property was not found as part of authorization parameters, it will be skipped.");
                                    _mySource.Value.Flush();

                                    // Skip this definition.
                                    continue;
                                }

                                // Get parent property name to get how is written the username property.
                                var usernamePropertyName = ((JProperty)j.Parent).Name;

                                // This property exists under several forms.
                                jsonPaths = new string[] { "$.password", "$.Password" };
                                j = endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths);

                                if (j == null)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"An password or Password property was not found as part of authorization parameters, it will be skipped.");
                                    _mySource.Value.Flush();

                                    // Skip this definition.
                                    continue;
                                }

                                // Get parent property name to get how is written the password property.
                                var passwordPropertyName = ((JProperty)j.Parent).Name;

                                // Prefix the username with 'reviewme_' and set a dummy value for password
                                // to force the team to create a new password or re-assign the password.
                                username = $"reviewme_{endpointAsJToken["authorization"]["parameters"][usernamePropertyName].ToString()}";
                                endpointAsJToken["authorization"]["parameters"][usernamePropertyName] = new JValue(username);
                                endpointAsJToken["authorization"]["parameters"][passwordPropertyName] = new JValue(reviewMeValue);
                            }

                            break;
                        case "Certificate":
                            // Certificate authorization scheme.
                            //
                            // The schema of an authorization object is the following:
                            // "authorization": {
                            //   "parameters": {
                            //     "servercertthumbprint": "Service certificate thumbprint",
                            //     "certificate": "Base64 encoding of the client certificate",
                            //     "certificatepassword": "Password for the certificate"
                            //   },
                            // "scheme": "Certificate"
                            // }

                            // This property exists under several forms.
                            jsonPaths = new string[] { "$.servercertthumbprint", "$.serverCertThumbprint", "$.ServerCertThumbprint" };
                            j = endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths);

                            if (j == null)
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"An servercertthumbprint or serverCertThumbprint or ServerCertThumbprint property was not found as part of authorization parameters, it will be skipped.");
                                _mySource.Value.Flush();

                                // Skip this definition.
                                continue;
                            }

                            // This property exists under several forms.
                            jsonPaths = new string[] { "$.certificate", "$.Certificate" };
                            j = endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths);

                            if (j == null)
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"An certificate or Certificate property was not found as part of authorization parameters, it will be skipped.");
                                _mySource.Value.Flush();

                                // Skip this definition.
                                continue;
                            }

                            // Get parent property name to get how is written the certificate property.
                            var certificatePropertyName = ((JProperty)j.Parent).Name;

                            // This property exists under several forms.
                            jsonPaths = new string[] { "$.certificatepassword", "$.certificatePassword", "$.CertificatePassword" };
                            j = endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths);

                            if (j == null)
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"An certificatepassword or certificatePassword or CertificatePassword property was not found as part of authorization parameters, it will be skipped.");
                                _mySource.Value.Flush();

                                // Skip this definition.
                                continue;
                            }

                            // Get parent property name to get how is written the certificate property.
                            var certificatePasswordPropertyName = ((JProperty)j.Parent).Name;

                            // Keep the same service certificate thumbprint.
                            // Use a dummy value to force the team to create a new certificate or re-import the certificate.
                            endpointAsJToken["authorization"]["parameters"][certificatePropertyName] = new JValue(reviewMeValue);
                            // Use a dummy value to force the team to re-generate password or re-assign it.
                            endpointAsJToken["authorization"]["parameters"][certificatePasswordPropertyName] = new JValue(reviewMeValue);

                            break;
                        case "Token":
                            // Api token authorization scheme.
                            //
                            // The schema of an authorization object is the following:
                            // "authorization": {
                            //   "parameters": {
                            //     "apitoken": "key but this parameter name can be also apiToken"
                            //   },
                            // "scheme": "Token"
                            // This property exists under several forms.
                            jsonPaths = new string[] { "$.apitoken", "$.apiToken", "$.ApiToken" };
                            if (endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths) == null)
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"An apitoken or apiToken or ApiToken property was not found as part of authorization parameters, it will be skipped.");
                                _mySource.Value.Flush();

                                // Skip this definition.
                                continue;
                            }

                            // Use a dummy value to force the team to create a new token or re-assign the token.
                            endpointAsJToken["authorization"]["parameters"] = new JObject(new JProperty("apitoken", reviewMeValue));

                            break;
                        case "PersonalAccessToken":
                            // PAT authorization scheme.
                            //
                            // The schema of an authorization object is the following:
                            // "authorization": {
                            //   "parameters": {
                            //     "accessToken": "token"
                            //   },
                            // "scheme": "PersonalAccessToken"
                            // This property exists under several forms.
                            jsonPaths = new string[] { "$.accesstoken", "$.accessToken" };
                            if (endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths) == null)
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"An accesstoken or accessToken property was not found as part of authorization parameters, it will be skipped.");
                                _mySource.Value.Flush();

                                // Skip this definition.
                                continue;
                            }

                            // Use a dummy value to force the team to create a new token or re-assign the token.
                            endpointAsJToken["authorization"]["parameters"] = new JObject(new JProperty("accessToken", reviewMeValue));

                            break;
                        case "OAuth":
                            // OAuth authorization scheme.
                            //
                            // The schema of an authorization object is the following:
                            // "authorization": {
                            //   "parameters": {
                            //     "AccessToken": "token"
                            //     "ConfigurationId": "key but this parameter name can be also apiToken"
                            //   },
                            // "scheme": "OAuth"
                            // This property exists under several forms.
                            jsonPaths = new string[] { "$.accesstoken", "$.accessToken", "$.AccessToken" };
                            if (endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths) == null)
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"An accesstoken or accessToken or AccessToken property was not found as part of authorization parameters, it will be skipped.");
                                _mySource.Value.Flush();

                                // Skip this definition.
                                continue;
                            }

                            // Keep the configuration id property.
                            // Use a dummy value to force the team to create a new token or re-assign the token.
                            endpointAsJToken["authorization"]["parameters"]["AccessToken"] = new JValue(reviewMeValue);

                            break;
                        case "ServicePrincipal":
                            // Service principal authorization scheme.
                            //
                            // The schema of an authorization object is the following:
                            // "authorization": {
                            //   "parameters": {
                            //     "tenantid": "guid",
                            //     "serviceprincipalid": "guid",
                            //     "authenticationType": "spnKey, sometimes it may be missing too",
                            //     "scope": "scope path",
                            //     "loginServer": "url, sometimes it may be missing too",
                            //     "role": "guid, sometimes it may be missing too",
                            //     "servicePrincipalKey": "key but this parameter name can be also serviceprincipalkey"
                            //   },
                            // "scheme": "ServicePrincipal"
                            //
                            // or
                            //
                            // "authorization": {
                            //   "parameters": {
                            //     "tenantid": "guid",
                            //     "serviceprincipalid": "guid",
                            //     "authenticationType": "spnCertificate, sometimes it may be missing too",
                            //     "scope": "scope path",
                            //     "loginServer": "url, sometimes it may be missing too",
                            //     "role": "guid, sometimes it may be missing too",
                            //     "servicePrincipalCertificate": "Base64 encoding of the service principal certificate but this parameter name can be also serviceprincipalcertificate"
                            //   },
                            // "scheme": "ServicePrincipal"

                            // Cannot pass a service principal key when 'creationMode' property is set to 'Automatic'.
                            endpointAsJToken["data"]["creationMode"] = "Manual";
                            var spcPropertyName = string.Empty;
                            var spkPropertyName = string.Empty;

                            // When creationMode is set to Manual, these fields must also be removed.
                            propertiesToRemove = new string[]
                                {
                                    "$.azureSpnRoleAssignmentId",
                                    "$.azureSpnPermissions",
                                    "$.spnObjectId",
                                    "$.appObjectId",
                                };
                            endpointAsJToken["data"].RemoveSelectProperties(propertiesToRemove);

                            // Determine if the authentication type property is present.
                            // For newer endpoint, this property has been implemented, otherwise check for the presence of
                            // spnKey or spnCertificate property to deduct the type of authentication.
                            jsonPaths = new string[] { "$.authenticationtype", "$.authenticationType", "$.AuthenticationType" };
                            hasAuthenticationTypeProperty = (endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths) != null);

                            if (hasAuthenticationTypeProperty)
                                authType = endpointAsJToken["authorization"]["parameters"]["authenticationType"].ToString();

                            // This property exists under several forms.
                            jsonPaths = new string[] { "$.serviceprincipalcertificate", "$.servicePrincipalCertificate", "$.ServicePrincipalCertificate" };
                            j = endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths);

                            if (j != null)
                            {
                                if (!hasAuthenticationTypeProperty)
                                    authType = "spnCertificate";

                                // Get parent property name to get how is written the service principal certificate.
                                spcPropertyName = ((JProperty)j.Parent).Name;
                            }
                            else
                            {
                                // This property exists under several forms.
                                jsonPaths = new string[] { "$.serviceprincipalkey", "$.servicePrincipalKey", "$.ServicePrincipalKey" };
                                j = endpointAsJToken["authorization"]["parameters"].SelectFirstToken(jsonPaths);

                                if (j != null)
                                {
                                    if (!hasAuthenticationTypeProperty)
                                        authType = "spnKey";

                                    // Get parent property name to get how is written the service principal key.
                                    spkPropertyName = ((JProperty)j.Parent).Name;
                                }
                                else
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"One or many required properties were not found as part of authorization parameters, it will be skipped.");
                                    _mySource.Value.Flush();

                                    // Skip this definition.
                                    continue;
                                }
                            }

                            // Use certificate or key.
                            if (authType == "spnCertificate")
                                endpointAsJToken["authorization"]["parameters"][spcPropertyName] = new JValue(reviewMeValue);
                            else if (authType == "spnKey")
                                endpointAsJToken["authorization"]["parameters"][spkPropertyName] = new JValue(reviewMeValue);

                            break;
                        case "ManagedServiceIdentity":
                            // Managed service identity authorization scheme.
                            //
                            // The schema of an authorization object is the following:
                            // "authorization": {
                            //   "parameters": {
                            //     "tenantid": "buid"
                            //   },
                            // "scheme": "ManagedServiceIdentity"

                            // No change needed
                            break;
                        case "InstallationToken":
                            // Installation token authorization scheme.
                            //
                            // The schema of an authorization object is the following:
                            // "authorization": {
                            //   "parameters": {
                            //     "IdToken": "???"
                            //     "IdSignature": "???"
                            //   },
                            // "scheme": "InstallationToken"

                            // Send some traces.
                            _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Authorization Scheme: {ep.Authorization.Scheme} is not configured through Azure DevOps, it will be skipped.");
                            _mySource.Value.Flush();

                            // Skip this definition.
                            continue;
                        case "None":
                            // None authorization scheme.

                            if (endpointAsJToken["type"].ToString().ToLower() == "externalnugetfeed")
                            {
                                // The schema of an authorization object is the following when service endpoint type is externalnugetfeed:
                                // "authorization": {
                                //   "parameters": {
                                //     "nugetkey": "key",
                                //   },
                                // "scheme": "None"

                                // When this is defined, use a dummy value to force the team to re-generate key or re-assign it.
                                endpointAsJToken["authorization"]["parameters"] = new JObject(new JProperty("nugetkey", reviewMeValue));
                            }
                            else if (endpointAsJToken["type"].ToString().ToLower() == "servicefabric")
                            {
                                // The schema of an authorization object is the following when service endpoint type is servicefabric:
                                // "authorization": {
                                //   "parameters": {
                                //     "UseWindowsSecurity": "boolean",
                                //     "ClusterSpn": "Fully qualified domain SPN for gMSA account",
                                //   },
                                // "scheme": "None"

                                // Leave the value
                                // Keep the 'UseWindowsSecurity' property.

                                // When this is defined, use a dummy value to force the team to re-generate key or re-assign it.
                                endpointAsJToken["authorization"]["parameters"]["ClusterSpn"] = new JValue(reviewMeValue);
                            }
                            // Other service endpoint type exists that use a 'None' authorization scheme
                            // but we don't use them, so we will leave the work for later.
                            // todo: define other cases.

                            break;
                        default:
                            // Unsupported authorization scheme.

                            // Send some traces.
                            _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Unsupported Authorization Scheme: {ep.Authorization.Scheme}");
                            _mySource.Value.Flush();
                            break;
                    }

                    // Serialize to pass after to REST api.
                    jsonContent = JsonConvert.SerializeObject(endpointAsJToken, Formatting.Indented);

                    // Write temporarily the content to create a new service endpoint.
                    jsonContent = JsonConvert.SerializeObject(endpointAsJToken, Formatting.Indented);
                    filename = projectDef.GetFileFor("Work.ImportServiceEndpoint", new object[] { ++entitiesToImportCounter });
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Try to create a new service endpoint.
                    // It may fail if an extension has changed name, type or authentication scheme.
                    createdEndpoint = adorasServiceEndPoints.CreateServiceEndPoint(jsonContent);
                    if (adorasServiceEndPoints.CRUDOperationSuccess)
                    {
                        // Increment the imported entities.
                        importedEntities++;

                        // Store service endpoint information.
                        projectDef.AdoEntities.ServiceEndpoints.Add(createdEndpoint.Name, createdEndpoint.Id);

                        // Update mapping record.
                        projectDef.UpdateMappingRecordByName("ServiceEndpoint", endPointNameFromDefinitionFile, createdEndpoint.Id, createdEndpoint.Name);
                    }
                    else
                    {
                        string errorMsg = string.Format("Error while creating {0}: {1}. Azure DevOps REST api error message: {2}", "service endpoint", ep.Name, adorasServiceEndPoints.LastApiErrorMessage);

                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        _mySource.Value.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating service endpoints: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} service endpoints were imported.", importedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private void ImportTaskGroup(ProjectDefinition projectDef, string prefixName, string prefixSeparator, ProjectImportBehavior behaviors,
            bool isOnPremiseMigration)
        {
            // Initialize.
            bool haveACEsDefined;
            bool needToPrefix = false;
            bool hasOriginalName;
            bool needCreateTaskGroup = false;
            bool skipAndRedirectTaskGroup = false;
            int entitiesCounter = 0;
            int entitiesToCreateCounter = 0;
            int entitiesToCreateDraftCounter = 0;
            int entitiesToPublishAsPreviewCounter = 0;
            int entitiesToPublishCounter = 0;
            int importedEntities = 0;
            int redirectedEntities = 0;
            int skippedEntities = 0;
            string filename;
            string identityDescriptor;
            string jsonContent;
            string securityNamespaceId;
            string securityToken;
            string sid;
            string taskGroupName;
            string taskGroupNameFromDefinitionFile;
            string originalTaskGroupNameFromDefinitionFile = null;
            string tokenName;
            string tokenValue;
            string versionToPublish;
            string[] pathsToSelectForRemoval;
            string[] possibleTaskgroupNames;
            JToken curatedJToken;
            List<KeyValuePair<string, string>> tokensList;
            Dictionary<int, string> fileIdToObjectIdMappings = new Dictionary<int, string>();
            ProjectDefinition.MappingRecord mappingRecord;
            SecurityResponse.AccessControlList acl;
            SecurityResponse.AccessControlEntry ace = null;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import task groups.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition("TaskGroups");

                // Set how many entities or objects must be imported.
                entitiesCounter = projectDef.TaskGroupDefinitions.Count;

                // Create an Azure DevOps REST api service object.
                TaskGroups adorasTaskGroups = new TaskGroups(projectDef.AdoRestApiConfigurations["TaskAgentApi"]);
                AccessControlLists adorasACLs = new AccessControlLists(projectDef.AdoRestApiConfigurations["SecurityApi"]);

                // Some properties must be removed as they are not needed during creation.
                // This artifical property is added to better retrieve name change with version change.
                pathsToSelectForRemoval = new string[] { "$.originalName" };

                foreach (SingleObjectDefinitionFile df in projectDef.TaskGroupDefinitions)
                {
                    try
                    {
                        // Reset flag.
                        skipAndRedirectTaskGroup = false;

                        // Send some traces.
                        _mySource.Value.TraceInformation($"Process task group definition stored in {df.FileName}.");
                        _mySource.Value.Flush();

                        // Read the content of definition file.
                        jsonContent = projectDef.ReadDefinitionFile(df.FilePath);

                        #region - Replace all generalization tokens.

                        // Define all tokens to replace from the definition file.
                        // Tokens must be created in order of resolution, so this is why
                        // a list of key/value pairs is used instead of a dictionary.
                        tokensList = new List<KeyValuePair<string, string>>();

                        // Set token replacements.
                        foreach (var item in fileIdToObjectIdMappings)
                        {
                            // .Key property stores the file identifier that corresponds to an abstracted identifier.
                            // .Value property stores the object identifier.
                            tokenName = Engine.Utility.CreateTokenName($"TaskGroup{item.Key}", ReplacementTokenValueType.Id);
                            tokenValue = item.Value;
                            tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                        }

                        // Replace tokens.
                        jsonContent = Engine.Utility.ReplaceTokensInJsonContent(jsonContent, tokensList);

                        #endregion

                        // Read modified task group definition.
                        JToken taskgroupAsJToken = JToken.Parse(jsonContent);
                        taskGroupName = taskgroupAsJToken["name"].ToString();

                        // Does it have an original name?
                        hasOriginalName = (taskgroupAsJToken["originalName"] != null);

                        #region - Prepend a prefix to task group name.

                        // Retain original task group name from definition file.
                        // If definition has a property called 'originalName', it means
                        // the task group has been renamed along with the version change.
                        if (hasOriginalName)
                            originalTaskGroupNameFromDefinitionFile = taskgroupAsJToken["originalName"].ToString();

                        // Does it require a prefix?
                        // Only the redirected task group never requires a prefix.
                        if (taskGroupName == "Redirected Task Group")
                            needToPrefix = false;
                        else if (string.IsNullOrEmpty(prefixName))
                            needToPrefix = false;
                        else
                            needToPrefix = true;

                        // Prepend the task grounp name with a prefix.
                        if (needToPrefix)
                        {
                            // Retain task group name from definition file.
                            taskGroupNameFromDefinitionFile = taskGroupName;

                            // Send some traces.
                            _mySource.Value.TraceInformation($"Add prefix: {prefixName}{prefixSeparator} in front of the task group name: {taskGroupNameFromDefinitionFile}.");
                            _mySource.Value.Flush();

                            // Add the prefix and a dot.
                            taskGroupName = $"{prefixName}{prefixSeparator}{taskGroupNameFromDefinitionFile}";

                            // Define all possible names the task group could have had.
                            if (hasOriginalName)
                            {
                                // Add the prefix and a dot.
                                string originalTaskGroupName = $"{prefixName}{prefixSeparator}{originalTaskGroupNameFromDefinitionFile}";

                                // Add the prefix to original task group name.
                                taskgroupAsJToken["originalName"] = new JValue(originalTaskGroupName);

                                // Define all possible names the task group could have had.
                                possibleTaskgroupNames = new string[] { originalTaskGroupNameFromDefinitionFile, originalTaskGroupName, taskGroupName };
                            }
                            else
                            {
                                // Define all possible names the task group could have had.
                                possibleTaskgroupNames = new string[] { taskGroupNameFromDefinitionFile, taskGroupName };
                            }

                            // Add the prefix to task group name.
                            taskgroupAsJToken["name"] = new JValue(taskGroupName);
                        }
                        else
                        {
                            // Define all possible names the task group could have had.
                            if (hasOriginalName)
                                possibleTaskgroupNames = new string[] { originalTaskGroupNameFromDefinitionFile, taskGroupName };
                            else
                                possibleTaskgroupNames = new string[] { taskGroupName };
                        }

                        #endregion

                        #region - Verify if this task group is the redirected task group.

                        if (taskGroupName == "Redirected Task Group")
                        {
                            // Validate if the redirected task group has been created already.
                            TaskAgentResponse.TaskGroup rtg = adorasTaskGroups.GetTaskGroupByName(taskGroupName);

                            if (rtg != null)
                            {
                                #region - Update mapping record.

                                // Form a composition key which is made of task identifier and version spec.
                                // Version spec. is a version mask made to get only major version.
                                string newEntityKey = NewEntityKeyForTaskGroupMapping(rtg.Id, rtg.Version.Major);

                                // Update mapping record for this task group which will allow translation.
                                projectDef.UpdateMappingRecordByName("TaskGroup", rtg.Name, newEntityKey, rtg.Name);

                                #endregion

                                // Send some traces.
                                _mySource.Value.TraceInformation($"Task group: {taskGroupName} already exists, it will be skipped.");
                                _mySource.Value.Flush();

                                // Increment skip counter.
                                skippedEntities++;

                                // Go to next task group definition.
                                continue;
                            }
                        }

                        #endregion

                        else
                        {
                            #region - Verify if this task group exists.

                            // Try to find if the task group exists.
                            TaskAgentResponse.TaskGroup tg = adorasTaskGroups.GetTaskGroupByName(taskGroupName);

                            // If it exists...
                            if (tg != null)
                            {
                                // Compare version spec.
                                string versionSpec1 = $"{tg.Version.Major}.*";
                                string versionSpec2 = $"{taskgroupAsJToken["version"]["major"].ToString()}.*";

                                // If they are the same, it means the task group already exists.
                                if (versionSpec1 == versionSpec2)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"Task group: {taskGroupName} already exists, it will be skipped.");
                                    _mySource.Value.Flush();

                                    // Increment skip counter.
                                    skippedEntities++;

                                    // Go to next task group definition.
                                    continue;
                                }
                            }

                            #endregion

                            #region - Remap service enpdoint identifiers.

                            foreach (JToken taskAsJToken in (JArray)taskgroupAsJToken["tasks"])
                            {
                                // Proceed only if inputs are defined for this task.
                                if (taskAsJToken["inputs"] == null)
                                    continue;

                                try
                                {
                                    // Try to remap service endpoint.
                                    RemapTask(projectDef, taskAsJToken, "Build");
                                    RemapServiceEndpoint(projectDef, taskAsJToken);

                                }
                                catch (RecoverableException ex)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                    _mySource.Value.Flush();

                                    // Will skip this definition.
                                    // Leave nested processing loop.
                                    skipAndRedirectTaskGroup = true;
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                    _mySource.Value.Flush();

                                    throw;
                                }
                            }

                            #endregion
                        }

                        #region - Create or update task group.

                        // Task group needs to be created with version 1.0.0
                        // otherwise it is updated.
                        needCreateTaskGroup = (taskgroupAsJToken["version"]["major"].ToObject<int>() == 1);

                        if (!skipAndRedirectTaskGroup)
                        {
                            try
                            {
                                // Create initial version of task group against destination collection/project.
                                if (needCreateTaskGroup)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation("Create task group: {0}.", taskGroupName);
                                    _mySource.Value.Flush();

                                    // Write temporarily the content to create a new task group.
                                    jsonContent = JsonConvert.SerializeObject(taskgroupAsJToken, Formatting.Indented);
                                    filename = projectDef.GetFileFor("Work.CreateTaskGroup", new object[] { ++entitiesToCreateCounter });
                                    projectDef.WriteDefinitionFile(filename, jsonContent);

                                    // Serialize the raw request message.
                                    jsonContent = JsonConvert.SerializeObject(taskgroupAsJToken);

                                    // Create task group.
                                    TaskAgentResponse.TaskGroup tg = adorasTaskGroups.CreateTaskGroup(jsonContent);

                                    if (adorasTaskGroups.CRUDOperationSuccess)
                                    {
                                        // Increment the imported entities.
                                        importedEntities++;

                                        // Store task group information.
                                        projectDef.AdoEntities.TaskGroups.Add(tg.Name, tg.Id);

                                        // Send some traces.
                                        _mySource.Value.TraceInformation($"FileIdentifier = {df.FileIdentifier}");
                                        _mySource.Value.Flush();

                                        // Add a new mapping between file identifier and task group identifier.
                                        // This mapping is only useful during creation.
                                        fileIdToObjectIdMappings.Add(df.FileIdentifier, tg.Id);

                                        #region - Update mapping record.

                                        // Form a composition key which is made of task identifier and version spec.
                                        // Version spec. is a version mask made to get only major version.
                                        string newEntityKey = NewEntityKeyForTaskGroupMapping(tg.Id, tg.Version.Major);

                                        if (taskGroupName == "Redirected Task Group")
                                        {
                                            // Update mapping record for this task group which will allow to use it as a redirection.
                                            projectDef.UpdateMappingRecordByName("TaskGroup", tg.Name, newEntityKey);
                                        }
                                        else
                                        {
                                            // Try to retrieve the mapping record corresponding to initial version of the task group.
                                            mappingRecord = GetMappingRecordForTaskGroupInitVersion(projectDef, possibleTaskgroupNames);
                                            if (mappingRecord == null)
                                                throw (new Exception("Cannot retrieve task group version 1.* in the mappings."));

                                            // Update mapping record for this task group which will allow translation.
                                            projectDef.UpdateMappingRecordByKey("TaskGroup", mappingRecord.OldEntityKey, newEntityKey, tg.Name);
                                        }

                                        #endregion
                                    }
                                    else
                                        throw (new Exception($"Error while creating task group: {taskGroupName}. Azure DevOps REST api error message: {adorasTaskGroups.LastApiErrorMessage}"));
                                }
                                // Create another version of the task group against destination collection/project.
                                else
                                {
                                    // Initialize.
                                    TaskAgentResponse.TaskGroup ltg = null;

                                    // Reset flag.
                                    bool isLatestVersionOfTaskGroupFound = false;

                                    // Reverse the array to make it start from most recent task group name first.
                                    // This will return the latest
                                    List<string> lptgns = possibleTaskgroupNames.ToList();
                                    lptgns.Reverse();

                                    foreach (string name in lptgns)
                                    {
                                        // Try to find task group.
                                        ltg = adorasTaskGroups.GetTaskGroupLatestVersionByName(name);
                                        if (ltg != null)
                                        {
                                            // Set found flag.
                                            isLatestVersionOfTaskGroupFound = true;

                                            // Send some traces.
                                            _mySource.Value.TraceInformation($"Update task group: {name}.");
                                            _mySource.Value.Flush();

                                            // Leave the search loop.
                                            break;
                                        }
                                    }

                                    if (!isLatestVersionOfTaskGroupFound)
                                        throw (new Exception($"Cannot update task group because task group: {possibleTaskgroupNames[0]} or any variants at version spec. 1.* do not exist."));

                                    // Define version to publish.
                                    versionToPublish = $"{ltg.Version.Major + 1}.{ltg.Version.Minor}.{ltg.Version.Patch}";

                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"Create a draft task group: {taskGroupName} version {versionToPublish}");
                                    _mySource.Value.Flush();

                                    // Inject a parent definition id with the current task group identifier.
                                    taskgroupAsJToken["parentDefinitionId"] = new JValue(ltg.Id);
                                    // Set this version to be a test.
                                    taskgroupAsJToken["version"]["isTest"] = new JValue(true);

                                    // Curate the JToken.
                                    curatedJToken = taskgroupAsJToken.RemoveSelectProperties(pathsToSelectForRemoval);

                                    // Write temporarily the content to create a draft task group.
                                    jsonContent = JsonConvert.SerializeObject(curatedJToken, Formatting.Indented);
                                    filename = projectDef.GetFileFor("Work.CreateDraftTaskGroup", new object[] { ++entitiesToCreateDraftCounter });
                                    projectDef.WriteDefinitionFile(filename, jsonContent);

                                    // Serialize the raw request message.
                                    jsonContent = JsonConvert.SerializeObject(curatedJToken);

                                    // Create a draft task group for the new version.
                                    // A draft task group identifier is considered at this point to be
                                    // an independent task group.
                                    TaskAgentResponse.TaskGroup dtg = adorasTaskGroups.CreateTaskGroup(jsonContent);

                                    if (adorasTaskGroups.CRUDOperationSuccess)
                                    {
                                        // Send some traces.
                                        _mySource.Value.TraceInformation($"Publish as preview task group: {taskGroupName} version {versionToPublish}");
                                        _mySource.Value.Flush();

                                        // Generate the raw request message to publish task group as preview.
                                        JObject requestBodyAsJObject = new JObject
                                            {
                                                { "comment", $"Published as preview for version {versionToPublish}" },
                                                { "parentDefinitionRevision", ltg.Revision },
                                                { "preview", true },
                                                { "taskGroupId", dtg.Id },
                                                { "taskGroupRevision", dtg.Revision }
                                            };

                                        // Write temporarily the content to publish draft task group as preview.
                                        jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);
                                        filename = projectDef.GetFileFor("Work.PublishDraftTaskGroupAsPreview", new object[] { ++entitiesToPublishAsPreviewCounter });
                                        projectDef.WriteDefinitionFile(filename, jsonContent);

                                        // Serialize the raw request message.
                                        jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject);

                                        // Publish task group as a preview.
                                        TaskAgentResponse.TaskGroup pptg = adorasTaskGroups.PublishAsPreviewTaskGroup(jsonContent, ltg.Id);

                                        if (adorasTaskGroups.CRUDOperationSuccess)
                                        {
                                            // Send some traces.
                                            _mySource.Value.TraceInformation($"Publish task group: {taskGroupName} version {versionToPublish}");
                                            _mySource.Value.Flush();

                                            // Generate the raw request message to publish task group as release.
                                            JObject versionAsJObject = new JObject
                                                {
                                                    { "major", pptg.Version.Major },
                                                    { "minor", pptg.Version.Minor },
                                                    { "patch", pptg.Version.Patch },
                                                    { "isTest", pptg.Version.IsTest }
                                                };

                                            requestBodyAsJObject = new JObject
                                                {
                                                    { "disablePriorVersions", false },
                                                    { "preview", false },
                                                    { "revision", pptg.Revision },
                                                    { "version", versionAsJObject } ,
                                                    { "comment", $"Published for version {versionToPublish}" }
                                                };

                                            // Write temporarily the content to publish task group.
                                            jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);
                                            filename = projectDef.GetFileFor("Work.PublishTaskGroup", new object[] { ++entitiesToPublishCounter });
                                            projectDef.WriteDefinitionFile(filename, jsonContent);

                                            // Serialize the raw request message.
                                            jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject);

                                            // Publish task group.
                                            TaskAgentResponse.TaskGroup ptg = adorasTaskGroups.PublishTaskGroup(jsonContent, ltg.Id);

                                            if (adorasTaskGroups.CRUDOperationSuccess)
                                            {
                                                // Increment the imported entities.
                                                importedEntities++;

                                                #region - Update mapping record.

                                                // Try to retrieve the mapping record corresponding to initial version of the task group.
                                                mappingRecord = GetMappingRecordForTaskGroupInitVersion(projectDef, possibleTaskgroupNames);
                                                if (mappingRecord == null)
                                                    throw (new Exception("Cannot retrieve task group version 1.* in the mappings."));

                                                // Extract the task group identifier from key and form another key with this version.
                                                // Version spec. is a version mask made to get only major version.
                                                string[] subkeys = SplitEntityKey(mappingRecord.OldEntityKey);
                                                string oldEntityKey = NewEntityKeyForTaskGroupMapping(subkeys[0], ptg.Version.Major);

                                                // Form a composition key which is made of task identifier and version spec.
                                                // Version spec. is a version mask made to get only major version.
                                                string newEntityKey = NewEntityKeyForTaskGroupMapping(ptg.Id, ptg.Version.Major);

                                                // Update mapping record for this task group which will allow translation.
                                                projectDef.UpdateMappingRecordByKey("TaskGroup", oldEntityKey, newEntityKey, ptg.Name);

                                                #endregion
                                            }
                                            else
                                                throw (new RecoverableException($"Error while publishing task group: {taskGroupName} version {versionToPublish}. Azure DevOps REST api error message: {adorasTaskGroups.LastApiErrorMessage}"));
                                        }
                                        else
                                            throw (new RecoverableException($"Error while publishing as preview task group: {taskGroupName} version {versionToPublish}. Azure DevOps REST api error message: {adorasTaskGroups.LastApiErrorMessage}"));
                                    }
                                    else
                                        throw (new RecoverableException($"Error while creating a draft task group: {taskGroupName} version {versionToPublish}. Azure DevOps REST api error message: {adorasTaskGroups.LastApiErrorMessage}"));
                                }
                            }
                            catch (RecoverableException ex)
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                _mySource.Value.Flush();

                                // Will skip this definition.
                                skipAndRedirectTaskGroup = true;
                            }
                            catch (Exception ex)
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                                _mySource.Value.Flush();

                                throw;
                            }
                        }

                        #endregion

                        // Skip definition and update mapping record to use the redirected task group.
                        if (skipAndRedirectTaskGroup)
                        {
                            // Increment the redirected entities counter.
                            redirectedEntities++;

                            // Send some traces.
                            _mySource.Value.TraceInformation($"Task group: {taskGroupName} had issues during import and it will be replaced with the redirected task group.");
                            _mySource.Value.Flush();

                            // Try to retrieve the mapping record corresponding to initial version of the task group.
                            mappingRecord = GetMappingRecordForTaskGroupInitVersion(projectDef, possibleTaskgroupNames);
                            if (mappingRecord == null)
                                throw (new Exception($"Cannot retrieve task group: {taskGroupName} version 1.* in the mappings."));
                            string oldEntityKey = mappingRecord.OldEntityKey;

                            // Retrieve mapping record for redirected task group.
                            mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityName == "Redirected Task Group" && x.Type == "TaskGroup");
                            string newEntityKey = mappingRecord.NewEntityKey;
                            string newEntityName = mappingRecord.NewEntityName;

                            string redirectedTaskGroupId = newEntityKey.Replace(",1.*", null);

                            // Update task group mappings for redirected task groups
                            fileIdToObjectIdMappings.Add(df.FileIdentifier, redirectedTaskGroupId);

                            // Update mapping record for this task group with the redirected task group one.
                            projectDef.UpdateMappingRecordByKey("TaskGroup", oldEntityKey, newEntityKey, newEntityName);

                            // Go to next task group definition.
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Send some traces.
                        string errorMsg = string.Format("Error while creating task group: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        _mySource.Value.Flush();
                    }
                }

                // Query security descriptors and generate tasks.
                if (behaviors.QuerySecurityDescriptors && !isOnPremiseMigration)
                {
                    // Get security namespace identifier.
                    securityNamespaceId = Constants.MetaTaskId;

                    // Define sid to associate to task group.
                    // sid is just the team name here.
                    sid = _engineConfig.DefaultTeamName;

                    foreach (SingleObjectDefinitionFile df in projectDef.TaskGroupDefinitions)
                    {
                        // Read the content of definition file.
                        jsonContent = projectDef.ReadDefinitionFile(df.FilePath);

                        // Read modified task group definition.
                        JToken taskgroupAsJToken = JToken.Parse(jsonContent);
                        taskGroupName = taskgroupAsJToken["name"].ToString();

                        // Does it have an original name?
                        hasOriginalName = (taskgroupAsJToken["originalName"] != null);

                        // Does it require a prefix?
                        // Only the redirected task group never requires a prefix.
                        if (taskGroupName == "Redirected Task Group")
                            needToPrefix = false;
                        else if (string.IsNullOrEmpty(prefixName))
                            needToPrefix = false;
                        else
                            needToPrefix = true;

                        // Prepend the task grounp name with a prefix.
                        if (needToPrefix)
                        {
                            // Retain task group name from definition file.
                            taskGroupNameFromDefinitionFile = taskGroupName;

                            // Add the prefix and a dot.
                            taskGroupName = $"{prefixName}{prefixSeparator}{taskGroupNameFromDefinitionFile}";
                        }

                        var item = projectDef.AdoEntities.TaskGroups.Where(x => x.Key == taskGroupName).FirstOrDefault();
                        // if can't find item
                        if (item.Equals(default(KeyValuePair<string, string>)))
                        {
                            _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Unable to find imported task group with name: {taskGroupName}. Unable to query security.");
                            _mySource.Value.Flush();
                            continue;
                        }

                        securityToken = Engine.Utility.GenerateTaskGroupToken(projectDef.DestinationProject.Id, item.Value);

                        // Get acl.
                        acl = adorasACLs.GetAccessControlList(securityNamespaceId, securityToken);

                        // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                        if (acl.AcesDictionary == null)
                            haveACEsDefined = false;
                        else
                        {
                            haveACEsDefined = true;

                            // Extract ACE defined for project administrators.
                            ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Administrators"])];
                        }

                        // Add a security task only if specific ACLs are defined.
                        if (haveACEsDefined)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Add security task for task group: {item.Key}");
                            _mySource.Value.Flush();

                            // Add a security task to apply same ACE as project administrators to default team.
                            identityDescriptor = Engine.Utility.GenerateIdentityDescriptor(sid);
                            projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                        }
                    }
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating task groups: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} task groups were imported, {2} were skipped and {3} redirected.", importedEntities, entitiesCounter, skippedEntities, redirectedEntities);
            _mySource.Value.Flush();
        }

        private void ImportTeam(
            ProjectDefinition projectDef,
            string prefixName,
            string prefixSeparator,
            bool isForProjectInitialization,
            List<string> teamExclusions,
            List<string> teamInclusions
            )
        {
            // Initialize.
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            string filename;
            string jsonContent;
            string teamName;
            string oldTeamName;
            JObject requestBodyAsJObject;
            CoreResponse.WebApiTeam nt;

            string definition;
            bool useTeamPrefix;
            string workFile;

            //regular or project Initialization?
            if (isForProjectInitialization)
            {
                definition = "TeamsInitialization";
                useTeamPrefix = false;
                workFile = "Work.ImportTeamInitialization";
            }
            else
            {
                definition = "Teams";
                useTeamPrefix = true;
                workFile = "Work.ImportTeam";
            }

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import teams.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition(definition);

                // Read definition of teams.
                // It may contain one or many teams.
                if (definition == "Teams")
                {
                    jsonContent = projectDef.ReadDefinitionFile(projectDef.TeamDefinitions.FilePath);
                }
                else if (definition == "TeamsInitialization")
                {
                    jsonContent = projectDef.ReadDefinitionFile(projectDef.TeamInitializationDefinitions.FilePath);
                }
                else
                    throw new NotImplementedException($"can't handle def {definition}");

                JArray teamsAsJArray = JArray.Parse(jsonContent);

                // Set how many entities or objects must be imported.
                entitiesCounter = teamsAsJArray.Count;

                // Create an Azure DevOps REST api service object.
                Teams adorasTeams = new Teams(projectDef.AdoRestApiConfigurations["TeamsApi"]);

                // Browse each team found.
                foreach (JToken teamAsJToken in teamsAsJArray)
                {
                    // Set team name.
                    teamName = teamAsJToken["name"].ToString();
                    oldTeamName = teamName;

                    string warningMsg = null;
                    if (
                        IsTeamIncluded(teamName, teamInclusions, teamExclusions,
                        out warningMsg)
                       )
                    {
                        if (_engineConfig.TeamMappings != null)
                        {
                            EngineConfiguration.TeamMapping teamMapping = null;
                            teamMapping = _engineConfig.TeamMappings.Where(x => x.SourceTeam == teamName).FirstOrDefault();
                            if (teamMapping != null)
                            {
                                _mySource.Value.TraceInformation($"{teamName} exists in team mappings. Skipping...");
                                continue;
                            }
                        }

                        // Does it require a prefix?
                        if (useTeamPrefix)
                        {
                            if (string.IsNullOrEmpty(prefixName))
                                teamName = oldTeamName;
                            else
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation($"Add prefix: {prefixName}{prefixSeparator} in front of the team name: {oldTeamName}.");
                                _mySource.Value.Flush();

                                // Add the prefix and a dot.
                                teamName = $"{prefixName}{prefixSeparator}{oldTeamName}";
                            }
                        }

                        // Generate the request message to create a team.
                        // Only name and description are used to create a new team.
                        requestBodyAsJObject = new JObject
                        {
                            { "name", teamName },
                            { "description", teamAsJToken["description"] }
                        };

                        // Write temporarily the content to create a new variable group.
                        jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);
                        filename = projectDef.GetFileFor(workFile, new object[] { ++entitiesToImportCounter });
                        projectDef.WriteDefinitionFile(filename, jsonContent);

                        // Serialize the raw request message.
                        jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);

                        // Send some traces.
                        _mySource.Value.TraceInformation("Import team: {0}.", teamName);
                        _mySource.Value.Flush();

                        // Create new team.
                        nt = adorasTeams.CreateTeam(jsonContent);

                        // Increment the imported entities.
                        if (adorasTeams.CRUDOperationSuccess)
                        {
                            // Increment the imported entities.
                            importedEntities++;

                            // Store team information.
                            projectDef.AdoEntities.Teams.Add(nt.Name, nt.Id);

                            // Update mapping record for this team.
                            projectDef.UpdateMappingRecordByName("Team", oldTeamName, nt.Id, nt.Name);
                        }
                        else
                        {
                            string errorMsg = string.Format("Error while creating {0}: {1}. Azure DevOps REST api error message: {2}", "teams", teamName, adorasTeams.LastApiErrorMessage);

                            // Send some traces.
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                            _mySource.Value.Flush();
                        }
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Warning, 0, warningMsg);
                        _mySource.Value.Flush();
                    }
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating teams: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} teams were imported.", importedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private bool IsTeamIncluded(
            string teamName,
            List<string> teamInclusions,
            List<string> teamExclusions,
            out string warningMsg)
        {
            bool teamIsIncluded
                = (teamInclusions == null || teamInclusions.Contains(teamName)) &&
                        (teamExclusions == null || !teamExclusions.Contains(teamName));
            warningMsg = null;
            if (!teamIsIncluded)
            {
                if (teamInclusions != null && !teamInclusions.Contains(teamName))
                {
                    warningMsg =
                        $"Skipping team {teamName} as it is not in the inclusion list";
                }
                else if (teamExclusions != null && teamExclusions.Contains(teamName))
                {
                    warningMsg =
                        $"Skipping team {teamName} as it is in the team exclusions list";
                }
                else
                    throw new NotImplementedException("should never see me");
            }
            return teamIsIncluded;
        }

        private void ImportTeamConfiguration(
            ProjectDefinition projectDef,
            string prefixName,
            string prefixSeparator,
            ProjectImportBehavior behaviors,
            bool isForProjectInitialization,
            RestAPI.ProcessMapping.Maps maps,
            List<string> teamExclusions,
            List<string> teamInclusions,
            bool isClone)
        {
            // Initialize.
            bool operationsStatus;
            bool subOperationStatus;
            int entitiesCounter = 0;
            int importedEntities = 0;
            string areaPrefix;
            string boardName;
            string boardNameField = "boardName";
            string currentPath;
            string identifier;
            string idField = "id";
            string iterationName;
            string iterationRootPath;
            string jsonContent;
            string path;
            string valueField = "value";
            Dictionary<string, JObject> iterationDictionary;
            JToken settingsAsJToken;
            JObject requestBodyAsJObject;
            ProjectDefinition.MappingRecord mappingRecord;

            string definition;
            bool useTeamPrefix;
            bool useAreaPrefixPath;
            bool useIterationPrefixPath;
            string filename;
            string workFileTeamArea;
            int teamAreaEntitiesToImportCounter = 0;
            string workFileTeamIteration;
            int teamIterationEntitiesToImportCounter = 0;
            string workFileTeamSetting;
            int teamSettingEntitiesToImportCounter = 0;

            string sourceBoardName;
            string targetBoardName;
            string sourceStateName;
            string targetStateName;

            string sourceField;
            string targetField;

            List<TeamConfigurationDefinitionFiles> projectDefTeamConfigurationDefinitions;

            //regular or project Initialization?
            if (isForProjectInitialization)
            {
                definition = "TeamConfigurationsInitialization";
                useTeamPrefix = false;
                useAreaPrefixPath = false;
                useIterationPrefixPath = false;
                workFileTeamArea = "Work.ImportTeamAreaInitialization";
                workFileTeamIteration = "Work.ImportTeamIterationInitialization";
                workFileTeamSetting = "Work.ImportTeamSettingInitialization";
            }
            else
            {
                definition = "TeamConfigurations";
                useTeamPrefix = true;
                useAreaPrefixPath = true;
                useIterationPrefixPath = true;
                workFileTeamArea = "Work.ImportTeamArea";
                workFileTeamIteration = "Work.ImportTeamIteration";
                workFileTeamSetting = "Work.ImportTeamSetting";
            }

            try
            {
                // Load definitions first.
                projectDef.LoadDefinition(definition);

                if (definition == "TeamConfigurations")
                {
                    projectDefTeamConfigurationDefinitions = projectDef.TeamConfigurationDefinitions;
                }
                else if (definition == "TeamConfigurationsInitialization")
                {
                    projectDefTeamConfigurationDefinitions = projectDef.TeamConfigurationInitializationDefinitions;
                }
                else
                    throw new NotImplementedException($"can't handle def {definition}");

                // Set how many entities or objects must be imported.
                entitiesCounter = projectDefTeamConfigurationDefinitions.Count;

                // Create an Azure DevOps REST api service object.
                ClassificationNodes adorasCN = new ClassificationNodes(projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"]);

                // Get the list of all iterations - used for setting team iterations
                iterationDictionary = adorasCN.GetIterationsDictionary(isClone);

                // Send some traces.
                _mySource.Value.TraceInformation("Import {0} team configurations.", projectDefTeamConfigurationDefinitions.Count);
                _mySource.Value.Flush();

                foreach (TeamConfigurationDefinitionFiles tcdf in projectDefTeamConfigurationDefinitions)
                {
                    // Reset the current operation status.
                    operationsStatus = true;
                    string teamName = tcdf.TeamName;
                    string oldTeamName = teamName;

                    string warningMsg = null;
                    if (
                        IsTeamIncluded(teamName, teamInclusions, teamExclusions,
                        out warningMsg)
                       )
                    {

                        if (_engineConfig.TeamMappings != null)
                        {
                            EngineConfiguration.TeamMapping teamMapping = null;
                            teamMapping = _engineConfig.TeamMappings.Where(x => x.SourceTeam == teamName).FirstOrDefault();
                            if (teamMapping != null)
                            {
                                _mySource.Value.TraceInformation($"{teamName} exists in team mappings. Mapping to {teamMapping.DestinationTeam}");
                                teamName = teamMapping.DestinationTeam;
                            }
                        }

                        // Does it have a prefix?
                        // Tokenization occurred before knowing a prefix is required, so
                        // the token name has no prefix.
                        if (useTeamPrefix)
                        {
                            if (string.IsNullOrEmpty(prefixName))
                            {
                                teamName = oldTeamName;
                            }
                            else
                            {
                                // Retrieve mapping record for team.
                                mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityName == oldTeamName && x.Type == "Team");
                                teamName = mappingRecord.NewEntityName;
                            }
                        }

                        // Create an Azure DevOps REST api service objects.
                        BoardConfiguration adorasBoardConfig = new BoardConfiguration(projectDef.AdoRestApiConfigurations["WorkBoardsApi"]) { Team = teamName };
                        TeamSettings adorasTeamSettings = new TeamSettings(projectDef.AdoRestApiConfigurations["WorkApi"]) { Team = teamName };

                        if (tcdf.BoardColumns == null && !isForProjectInitialization)
                        {
                            _mySource.Value.TraceInformation("{0} team configurations are invalid. Skipping...", teamName);
                            _mySource.Value.Flush();
                            continue;
                        }

                        // Send some traces.
                        _mySource.Value.TraceInformation("Import {0} team configurations.", teamName);
                        _mySource.Value.Flush();

                        #region - Process team areas.

                        if (behaviors.IncludeTeamAreas)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("Import {0} team areas.", teamName);
                            _mySource.Value.Flush();

                            // Initialize.
                            // Set area prefix to be the project name.
                            if (!isClone)
                            {
                                if (useAreaPrefixPath)
                                {
                                    if (!string.IsNullOrEmpty(_engineConfig.AreaPrefixPath))
                                        areaPrefix = projectDef.DestinationProject.Name + @"\" + _engineConfig.AreaPrefixPath + @"\";
                                    else
                                        areaPrefix = projectDef.DestinationProject.Name + @"\";
                                }
                                else
                                    areaPrefix = string.Empty;
                            }
                            else
                            {
                                areaPrefix = string.Empty;
                            }

                            // Read definition of team areas.
                            jsonContent = projectDef.ReadDefinitionFile(tcdf.Areas.FilePath);

                            // This is the raw request message which is a copy of the content of the *.json file.
                            requestBodyAsJObject = JObject.Parse(jsonContent);

                            // Add the project name as prefix to area for default value.
                            requestBodyAsJObject["defaultValue"] = areaPrefix + requestBodyAsJObject["defaultValue"];
                            JToken existingTeamAreas = adorasTeamSettings.GetTeamAreasAsJToken();

                            // Add the same project name as prefix for all other values.
                            foreach (JObject valueAsJObject in requestBodyAsJObject["values"])
                                valueAsJObject["value"] = areaPrefix + valueAsJObject["value"];

                            if (existingTeamAreas != null)
                            {
                                if (existingTeamAreas["values"] != null)
                                    foreach (JObject valueAsJObject in existingTeamAreas["values"])
                                        ((JArray)requestBodyAsJObject["values"]).Add(valueAsJObject);

                                if (existingTeamAreas["defaultValue"].Type != JTokenType.Null)
                                    requestBodyAsJObject["defaultValue"] = existingTeamAreas["defaultValue"];
                            }
                            // Generate the request message to set area for teams.
                            jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);

                            // Write temporarily the content to create a new variable group.
                            filename = projectDef.GetFileFor(workFileTeamArea, new object[] { ++teamAreaEntitiesToImportCounter });
                            projectDef.WriteDefinitionFile(filename, jsonContent);

                            // Set area for teams.
                            subOperationStatus = adorasTeamSettings.UpdateArea(jsonContent);
                            operationsStatus = operationsStatus && subOperationStatus;
                        }

                        #endregion

                        #region - Process team settings.

                        if (behaviors.IncludeTeamSettings)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("Import {0} team settings.", teamName);
                            _mySource.Value.Flush();

                            // Read definition of team settings.
                            jsonContent = projectDef.ReadDefinitionFile(tcdf.Settings.FilePath);

                            // This is the raw request message which is a copy of the content of the *.json file.
                            requestBodyAsJObject = JObject.Parse(jsonContent);

                            // Define iteration root path which is formed of the destination project and the source project.
                            // The source project represents a team in general.  
                            if (!isClone)
                            {
                                if (useIterationPrefixPath)
                                {
                                    iterationRootPath = Engine.Utility.CombineRelativePath(projectDef.DestinationProject.Name, _engineConfig.SourceProject);
                                    if (!string.IsNullOrEmpty(_engineConfig.IterationPrefixPath))
                                    {
                                        iterationRootPath = Engine.Utility.CombineRelativePath(projectDef.DestinationProject.Name, _engineConfig.IterationPrefixPath);
                                        iterationRootPath = Engine.Utility.CombineRelativePath(iterationRootPath, _engineConfig.SourceProject);
                                    }
                                    else
                                        iterationRootPath = Engine.Utility.CombineRelativePath(projectDef.DestinationProject.Name, _engineConfig.SourceProject);
                                }
                                else
                                {
                                    iterationRootPath = string.Empty;
                                }
                            }
                            else
                            {
                                iterationRootPath = string.Empty;
                            }

                            // If a default iteration is defined...
                            if (requestBodyAsJObject["defaultIteration"].Type != JTokenType.Null)
                            {
                                // Get the actual iteration path from definition file.
                                currentPath = requestBodyAsJObject["defaultIteration"]["path"].ToString();
                                if (useIterationPrefixPath && !isClone)
                                {
                                    // Add the root path.
                                    path = Engine.Utility.CombineRelativePath(iterationRootPath, currentPath);
                                }
                                else
                                {
                                    path = currentPath;
                                }

                                // Check if an identifier exists, if yes, assign it as the default iteration identifier.
                                if (iterationDictionary.ContainsKey(path))
                                    if (iterationDictionary[path].ContainsKey("identifier"))
                                        requestBodyAsJObject["defaultIteration"] = iterationDictionary[path]["identifier"];
                            }

                            // If default iteration macro is defined, remove default iteration.
                            if (requestBodyAsJObject["defaultIterationMacro"].Type != JTokenType.Null)
                                if (requestBodyAsJObject["defaultIteration"].Type != JTokenType.Null)
                                    requestBodyAsJObject.Property("defaultIteration").Remove();

                            // Set backlog iteration to use.
                            currentPath = requestBodyAsJObject["backlogIteration"]["path"].ToString();
                            if (useIterationPrefixPath && !isClone)
                            {
                                // Add the root path.
                                path = Engine.Utility.CombineRelativePath(iterationRootPath, currentPath);
                            }
                            else
                            {
                                path = currentPath;
                            }

                            JToken existingTeamSettings = adorasTeamSettings.GetTeamSettingsAsJToken();
                            if (existingTeamSettings != null)
                            {
                                // keep existing team settings if they exist
                                if (existingTeamSettings["backlogIteration"]["path"] != null)
                                    path = Utility.CombineRelativePath(projectDef.DestinationProject.Name, existingTeamSettings["backlogIteration"]["path"].ToString());
                            }

                            // Validate if this iteration path exists first.
                            if (!iterationDictionary.ContainsKey(path))
                            {
                                if (!isClone)
                                {
                                    throw (new Exception($"Cannot retrieve iteration with this path: {path}"));
                                }
                                else
                                {
                                    iterationRootPath = projectDef.DestinationProject.Name;
                                    path = Engine.Utility.CombineRelativePath(iterationRootPath, currentPath);
                                    if (!iterationDictionary.ContainsKey(path))
                                    {
                                        throw (new Exception($"Cannot retrieve iteration with this path: {path}"));
                                    }
                                }
                            }

                            // Assign backlog iteration.
                            requestBodyAsJObject["backlogIteration"] = iterationDictionary[path]["identifier"];

                            // Serialize the raw request message.
                            jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject, Formatting.Indented);

                            // Write temporarily the content to create a new variable group.
                            filename = projectDef.GetFileFor(workFileTeamSetting, new object[] { ++teamSettingEntitiesToImportCounter });
                            projectDef.WriteDefinitionFile(filename, jsonContent);

                            // Import team settings.
                            subOperationStatus = adorasBoardConfig.UpdateTeamSettings(jsonContent);
                            operationsStatus = operationsStatus && subOperationStatus;
                        }

                        #endregion

                        #region - Process team iterations.

                        if (behaviors.IncludeTeamIterations)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("Import {0} team iterations.", teamName);
                            _mySource.Value.Flush();

                            // Read definition of team iteration.
                            jsonContent = projectDef.ReadDefinitionFile(tcdf.Iterations.FilePath);

                            // Extract the iterations under the value JSON property.
                            // Get iteration identifier to pass to API.
                            foreach (JToken teamIterationAsJToken in (JArray)JToken.Parse(jsonContent)["value"])
                            {
                                // Extract the name + path.
                                iterationName = teamIterationAsJToken["name"].ToString();
                                currentPath = teamIterationAsJToken["path"].ToString();

                                // Define new iteration path which is formed of the destination project and current path.
                                if (!isClone)
                                {
                                    if (useIterationPrefixPath)
                                    {
                                        if (!string.IsNullOrEmpty(_engineConfig.IterationPrefixPath))
                                        {
                                            var iterationPrefix = Utility.CombineRelativePath(projectDef.DestinationProject.Name, _engineConfig.IterationPrefixPath);
                                            path = Engine.Utility.CombineRelativePath(iterationPrefix, currentPath);
                                        }
                                        else
                                        {
                                            path = Engine.Utility.CombineRelativePath(projectDef.DestinationProject.Name, currentPath);
                                        }
                                    }
                                    else
                                    {
                                        path = currentPath;
                                    }
                                }
                                else
                                {
                                    path = currentPath;
                                }

                                // Extract the identifier of this iteration path.
                                if (!isClone)
                                {
                                    if (iterationDictionary.ContainsKey(path))
                                    {
                                        identifier = iterationDictionary[path]["identifier"].ToString();
                                    }
                                    else
                                    {
                                        string errorMessage = $"Could not find {path} in iteration dictionary. Please ensure iterationName {iterationName} and currentPath {currentPath} is included in imported iterations in e.g. 'IterationBasePathList' of input csv.";
                                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMessage);
                                        _mySource.Value.Flush();
                                        throw new KeyNotFoundException(errorMessage);
                                    }
                                }
                                else
                                {
                                    string pathKey = $"\\{path}";

                                    pathKey = pathKey.Replace(_engineConfig.SourceProject, _engineConfig.DestinationProject);
                                    
                                    if (iterationDictionary.ContainsKey(pathKey))
                                    {
                                        identifier = iterationDictionary[pathKey]["identifier"].ToString();
                                    }
                                    else
                                    {
                                        string errorMessage = $"Could not find {pathKey} in iteration dictionary. Please ensure iterationName {iterationName} and currentPath {currentPath} is included in imported iterations in e.g. 'IterationBasePathList' of input csv.";
                                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMessage);
                                        _mySource.Value.Flush();
                                        throw new KeyNotFoundException(errorMessage);
                                    }
                                }

                                // Write temporarily the content to create a new variable group.
                                filename = projectDef.GetFileFor(workFileTeamIteration, new object[] { ++teamIterationEntitiesToImportCounter });
                                projectDef.WriteDefinitionFile(filename, identifier);

                                // Set iterations for teams.
                                subOperationStatus = adorasTeamSettings.UpdateIteration(identifier);
                                operationsStatus = operationsStatus && subOperationStatus;
                            }

                            JToken existingTeamIterations = adorasTeamSettings.GetTeamIterationsAsJToken();
                            foreach (JToken iteration in existingTeamIterations["value"])
                            {
                                // iterationDictionary has leading slash?
                                currentPath = "\\" + iteration["path"].ToString();
                                // Extract the identifier of this iteration path.
                                identifier = iterationDictionary[currentPath]["identifier"].ToString();

                                // Set iterations for teams.
                                subOperationStatus = adorasTeamSettings.UpdateIteration(identifier);
                                operationsStatus = operationsStatus && subOperationStatus;
                            }
                        }

                        #endregion

                        #region - Process board columns.

                        if (behaviors.IncludeBoardColumns && !isForProjectInitialization)
                        {
                            // Initialize.
                            boardName = null;

                            // Send some traces.
                            _mySource.Value.TraceInformation("Import {0} board columns.", teamName);
                            _mySource.Value.Flush();

                            // Read definition of board columns.
                            jsonContent = projectDef.ReadDefinitionFile(tcdf.BoardColumns.FilePath);

                            if (projectDef.SourceProject.ProcessTemplateTypeName.ToLower()
                                == projectDef.DestinationProject.ProcessTemplateTypeName.ToLower())
                            {
                                // Browse column definitions.
                                foreach (JToken boardColumnsAsJToken in JArray.Parse(jsonContent))
                                {
                                    // Get board name.
                                    boardName = boardColumnsAsJToken[boardNameField].ToString();

                                    // Get actual board columns from destination project.
                                    settingsAsJToken = adorasBoardConfig.GetBoardColumnsAsJToken(boardName);

                                    if (settingsAsJToken != null)
                                    {
                                        // Replace the New and Done columns with current values.
                                        boardColumnsAsJToken[valueField].First[idField] = settingsAsJToken[valueField].First[idField];
                                        boardColumnsAsJToken[valueField].Last[idField] = settingsAsJToken[valueField].Last[idField];

                                        // Serialize the configuration.
                                        jsonContent = JsonConvert.SerializeObject(boardColumnsAsJToken[valueField]);

                                        // Import board columns.
                                        subOperationStatus = adorasBoardConfig.UpdateBoardColumns(jsonContent, boardName);
                                        operationsStatus = operationsStatus && subOperationStatus;
                                    }
                                }
                            }
                            else
                            {
                                //need to do process map
                                // Send some traces.
                                _mySource.Value.TraceInformation("Mapping boards from Process {0} to Process {1}.",
                                    projectDef.SourceProject.ProcessTemplateTypeName,
                                    projectDef.DestinationProject.ProcessTemplateTypeName);
                                _mySource.Value.Flush();

                                var sourceParentProcess = maps.GetParentProcess(projectDef.SourceProject.ProcessTemplateTypeName);
                                var targetParentProcess = maps.GetParentProcess(projectDef.DestinationProject.ProcessTemplateTypeName);

                                _mySource.Value.TraceInformation($"Mapping boards from Process {projectDef.SourceProject.ProcessTemplateTypeName} (parent type: {sourceParentProcess}) to Process {projectDef.DestinationProject.ProcessTemplateTypeName} (parent type: {targetParentProcess}).");
                                _mySource.Value.Flush();

                                var sourceBoards = JsonConvert.DeserializeObject<List<ADO.RestAPI.Viewmodel50.WorkResponse.BoardColumns>>(jsonContent);

                                foreach (var sourceBoard in sourceBoards)
                                {
                                    sourceBoardName = sourceBoard.BoardName;

                                    targetBoardName = ProcessMappingUtility2.MapBoardName(sourceBoardName,
                                        projectDef.SourceProject.ProcessTemplateTypeName,
                                        projectDef.DestinationProject.ProcessTemplateTypeName,
                                        maps);
                                    settingsAsJToken = adorasBoardConfig.GetBoardColumnsAsJToken(targetBoardName);

                                    if (settingsAsJToken != null)
                                    {
                                        // Replace the New and Done (and everything in between) columns with current values.

                                        //STRATEGY:
                                        // 1) Get Current Board for Team
                                        // 2) Delete all columns except New and Done i.e. first and last aka columnType incoming and outgoing
                                        // 3) add all middle columns without an id BUT after mapping states:
                                        // {
                                        //    "id": "",      <---- NO ID since new column
                                        //    "name": "NEW COLUMN",    <-- name will match
                                        //    "itemLimit": 5,
                                        //    "stateMappings": {
                                        //        "User Story": "Resolved",    <-- these need to be changed by state mapping of processes
                                        //        "Bug": "Resolved"             <-- these need to be changed by state mapping of processes
                                        //    },
                                        //    "isSplit": false,
                                        //    "description": "",
                                        //    "columnType": "inProgress" <-- should all be this type for middle columns
                                        //},

                                        var currentTargetBoardColumns = JsonConvert.DeserializeObject<WorkResponse.BoardColumns>(settingsAsJToken.ToString());

                                        List<WorkResponse.BoardColumn> inProgressBoardColumns = new List<WorkResponse.BoardColumn>();

                                        //if (sourceBoardName != targetBoardName || sourceBoard.Count != currentTargetBoardColumns.Count)
                                        //{
                                        //    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Mapping board {sourceBoardName} ({sourceBoard.Count} columns) -> {targetBoardName} ({currentTargetBoardColumns.Count} columns).");
                                        //}
                                        //else
                                        //{
                                        //    _mySource.Value.TraceEvent(TraceEventType.Information, 0, $"Mapping board {sourceBoardName} ({sourceBoard.Count} columns) -> {targetBoardName} ({currentTargetBoardColumns.Count} columns).");
                                        //}
                                        //_mySource.Value.Flush();

                                        //var targetStateDictionary = currentTargetBoardColumns.Value.ToDictionary(cb => cb.Name, cb => cb.Id);

                                        //DO AGILE TO CMMI
                                        //a lot of hot air - more like a sanity check since will push back the same board we fetched
                                        //bool doSanityCheck = true;
                                        //if (doSanityCheck)
                                        //{
                                        foreach (var sourceBoardColumn in sourceBoard.Value.Skip(1).Take(sourceBoard.Count - 2))
                                        {
                                            WorkResponse.BoardColumn inProgressBoardColumn
                                                = new WorkResponse.BoardColumn()
                                                {
                                                    Id = "",
                                                    Name = sourceBoardColumn.Name,
                                                    ItemLimit = sourceBoardColumn.ItemLimit,
                                                    StateMappings = ProcessMappingUtility2.MapStateMappings(
                                                        sourceBoardColumn.StateMappings,
                                                        sourceBoardName,
                                                        projectDef.SourceProject.ProcessTemplateTypeName,
                                                        projectDef.DestinationProject.ProcessTemplateTypeName,
                                                        maps),
                                                    IsSplit = sourceBoardColumn.IsSplit,
                                                    Description = sourceBoardColumn.Description,
                                                    ColumnType = sourceBoardColumn.ColumnType,
                                                };

                                            //sourceStateName = sourceBoardColumn.Name;
                                            //targetStateName = ProcessMappingUtility2.MapState(sourceStateName,
                                            //    sourceBoardName,
                                            //    projectDef.SourceProject.ProcessTemplateTypeName,
                                            //    projectDef.DestinationProject.ProcessTemplateTypeName,
                                            //    maps);
                                            //if (sourceStateName != targetStateName)
                                            //{
                                            //    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Mapping column source state {sourceStateName} -> {targetStateName}.");
                                            //}
                                            //else
                                            //{
                                            //    _mySource.Value.TraceEvent(TraceEventType.Information, 0, $"Mapping column source state {sourceStateName} -> {targetStateName}.");
                                            //}
                                            //_mySource.Value.Flush();

                                            //if (targetStateDictionary.ContainsKey(targetStateName))
                                            //    sourceBoardColumn.Id = targetStateDictionary[targetStateName];
                                            //else
                                            //{
                                            //    throw new ArgumentException($"Please ensure you have a map from {sourceStateName} to {targetStateName} in dictionary");
                                            //}

                                            inProgressBoardColumns.Add(inProgressBoardColumn);

                                        }
                                        //}

                                        var currentTargetBoardColumnsValue = new List<WorkResponse.BoardColumn>();
                                        currentTargetBoardColumnsValue.Add(currentTargetBoardColumns.Value.First());
                                        currentTargetBoardColumnsValue.AddRange(inProgressBoardColumns);
                                        currentTargetBoardColumnsValue.Add(currentTargetBoardColumns.Value.Last());

                                        // Serialize the configuration.
                                        jsonContent = JsonConvert.SerializeObject(currentTargetBoardColumnsValue);

                                        // Import board columns.
                                        subOperationStatus = adorasBoardConfig.UpdateBoardColumns(jsonContent, targetBoardName);
                                        operationsStatus = operationsStatus && subOperationStatus;
                                    }

                                }
                            }
                        }

                        #endregion

                        #region - Process board rows.

                        if (behaviors.IncludeBoardRows && !isForProjectInitialization)
                        {
                            // Initialize.
                            boardName = null;

                            // Send some traces.
                            _mySource.Value.TraceInformation("Import {0} board rows.", teamName);
                            _mySource.Value.Flush();

                            // Read definition of board columns.
                            jsonContent = projectDef.ReadDefinitionFile(tcdf.BoardRows.FilePath);

                            // Browse row definitions.
                            foreach (JToken boardRowAsJToken in JArray.Parse(jsonContent))
                            {
                                // Get board name.
                                boardName = boardRowAsJToken[boardNameField].ToString();
                                //fix board name
                                boardName = ADO.RestAPI.ProcessMapping.ProcessMappingUtility2.MapBoardName(boardName,
                                    projectDef.SourceProject.ProcessTemplateTypeName,
                                    projectDef.DestinationProject.ProcessTemplateTypeName, maps);
                                boardRowAsJToken[boardNameField] = boardName;

                                // Prepare request message.
                                jsonContent = JsonConvert.SerializeObject(boardRowAsJToken[valueField]);

                                // Import board rows.
                                subOperationStatus = adorasBoardConfig.UpdateBoardRows(jsonContent, boardName);
                                operationsStatus = operationsStatus && subOperationStatus;
                            }
                        }

                        #endregion

                        #region - Process card field settings.

                        if (behaviors.IncludeCardFieldSettings && !isForProjectInitialization)
                        {
                            // Initialize.
                            boardName = null;

                            // Send some traces.
                            _mySource.Value.TraceInformation("Import {0} card fields settings.", teamName);
                            _mySource.Value.Flush();

                            // Read definition of card field settings.
                            jsonContent = projectDef.ReadDefinitionFile(tcdf.CardFields.FilePath);

                            if (projectDef.SourceProject.ProcessTemplateTypeName.ToLower()
                                == projectDef.DestinationProject.ProcessTemplateTypeName.ToLower())
                            {
                                // Browse field definitions.
                                foreach (JToken cardFieldsAsJToken in JArray.Parse(jsonContent))
                                {
                                    // Get board name.
                                    boardName = cardFieldsAsJToken[boardNameField].ToString();
                                    //fix board name
                                    boardName = ADO.RestAPI.ProcessMapping.ProcessMappingUtility2.MapBoardName(boardName,
                                        projectDef.SourceProject.ProcessTemplateTypeName,
                                        projectDef.DestinationProject.ProcessTemplateTypeName, maps);
                                    cardFieldsAsJToken[boardNameField] = boardName;

                                    // Prepare request message.
                                    jsonContent = JsonConvert.SerializeObject(cardFieldsAsJToken);

                                    // Import card fields.
                                    subOperationStatus = adorasBoardConfig.UpdateCardFields(jsonContent, boardName);
                                    operationsStatus = operationsStatus && subOperationStatus;
                                }
                            }
                            else
                            {
                                //need to do process map
                                // Send some traces.
                                _mySource.Value.TraceInformation("Mapping card fields from Process {0} to Process {1}.",
                                    projectDef.SourceProject.ProcessTemplateTypeName,
                                    projectDef.DestinationProject.ProcessTemplateTypeName);
                                _mySource.Value.Flush();

                                var sourceParentProcess = maps.GetParentProcess(projectDef.SourceProject.ProcessTemplateTypeName);
                                var targetParentProcess = maps.GetParentProcess(projectDef.DestinationProject.ProcessTemplateTypeName);

                                _mySource.Value.TraceInformation($"Mapping card fields from Process {projectDef.SourceProject.ProcessTemplateTypeName} (parent type: {sourceParentProcess}) to Process {projectDef.DestinationProject.ProcessTemplateTypeName} (parent type: {targetParentProcess}).");
                                _mySource.Value.Flush();

                                var sourceCards = JsonConvert.DeserializeObject<List<ADO.RestAPI.Viewmodel50.WorkResponse.UniversalCards>>(jsonContent);
                                var targetCards = new List<WorkResponse.UniversalCards>();

                                foreach (var sourceCard in sourceCards)
                                {
                                    sourceBoardName = sourceCard.BoardName;

                                    targetBoardName = ProcessMappingUtility2.MapBoardName(sourceBoardName,
                                        projectDef.SourceProject.ProcessTemplateTypeName,
                                        projectDef.DestinationProject.ProcessTemplateTypeName,
                                        maps);

                                    var targetCard = new WorkResponse.UniversalCards()
                                    {
                                        BoardName = targetBoardName,
                                        Cards = new WorkResponse.UniversalCard()
                                    };

                                    List<WorkResponse.CardField> sourceCardList = sourceCard.GetCardList(sourceBoardName);

                                    var targetCardList = targetCard.GetCardList(targetBoardName);

                                    targetCardList = new List<WorkResponse.CardField>();

                                    foreach (var cardField in sourceCardList)
                                    {
                                        sourceField = cardField.FieldIdentifier;
                                        targetField = ProcessMappingUtility2.MapField(sourceField,
                                            sourceBoardName,
                                            projectDef.SourceProject.ProcessTemplateTypeName,
                                            projectDef.DestinationProject.ProcessTemplateTypeName, maps);
                                        cardField.FieldIdentifier = targetField;

                                        if (sourceField != targetField)
                                        {
                                            _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Mapping field from source {sourceField} -> {targetField}.");
                                        }
                                        else
                                        {
                                            _mySource.Value.TraceEvent(TraceEventType.Information, 0, $"Mapping field from source {sourceField} -> {targetField}.");
                                        }
                                        _mySource.Value.Flush();

                                        //map it first
                                        targetCardList.Add(cardField);

                                    }

                                    targetCard = targetCard.AddCardList(targetBoardName, targetCardList);

                                    //transform source card to target card with card fields
                                    if (sourceBoardName != targetBoardName)
                                    {
                                        _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Mapping card from source {sourceBoardName} ({sourceCardList.Count} cards) -> {targetBoardName} ({targetCardList.Count} cards).");
                                    }
                                    else
                                    {
                                        _mySource.Value.TraceEvent(TraceEventType.Information, 0, $"Mapping card from source {sourceBoardName} ({sourceCardList.Count} cards) -> {targetBoardName} ({targetCardList.Count} cards).");
                                    }
                                    _mySource.Value.Flush();

                                    // Serialize the configuration.
                                    jsonContent = JsonConvert.SerializeObject(targetCard, Formatting.Indented);

                                    // Import card fields.
                                    subOperationStatus = adorasBoardConfig.UpdateCardFields(jsonContent, targetBoardName);
                                    operationsStatus = operationsStatus && subOperationStatus;

                                    targetCards.Add(targetCard);
                                }
                            }
                        }

                        #endregion

                        #region - Process card style settings.

                        if (behaviors.IncludeCardStyleSettings && !isForProjectInitialization)
                        {
                            // Initialize.
                            boardName = null;

                            // Send some traces.
                            _mySource.Value.TraceInformation("Import {0} card style settings.", teamName);
                            _mySource.Value.Flush();

                            // Read definition of card field settings.
                            jsonContent = projectDef.ReadDefinitionFile(tcdf.CardStyles.FilePath);

                            foreach (JToken cardStylesAsJToken in JArray.Parse(jsonContent))
                            {
                                // Get board name.
                                boardName = cardStylesAsJToken[boardNameField].ToString();

                                //fix board name
                                boardName = ADO.RestAPI.ProcessMapping.ProcessMappingUtility2.MapBoardName(boardName,
                                    projectDef.SourceProject.ProcessTemplateTypeName,
                                    projectDef.DestinationProject.ProcessTemplateTypeName, maps);
                                cardStylesAsJToken[boardNameField] = boardName;

                                // Prepare request message.
                                jsonContent = JsonConvert.SerializeObject(cardStylesAsJToken, Formatting.Indented);

                                // Import card styles.
                                subOperationStatus = adorasBoardConfig.UpdateCardStyles(jsonContent, boardName);
                                operationsStatus = operationsStatus && subOperationStatus;
                            }
                        }

                        #endregion

                        // Increment the imported entities.
                        if (operationsStatus)
                            importedEntities++;
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Warning, 0, warningMsg);
                        _mySource.Value.Flush();
                    }
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating team configurations: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} team configurations were imported.", importedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private void ImportVariableGroup(ProjectDefinition projectDef, string prefixName, string prefixSeparator, ProjectImportBehavior behaviors, bool isOnPremiseMigration)
        {
            // Initialize.
            bool haveACEsDefined;
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            string filename;
            string identityDescriptor;
            string jsonContent;
            string securityNamespaceId;
            string securityToken;
            string sid;
            string variableGroupName;
            JToken variableGroupAsJToken;
            SecurityResponse.AccessControlList acl;
            SecurityResponse.AccessControlEntry ace = null;
            TaskAgentResponse.VariableGroup nvg;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import variable groups.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition("VariableGroups");

                // Create Azure DevOps REST api service objects.
                VariableGroups adorasVariableGroups = new VariableGroups(projectDef.AdoRestApiConfigurations["TaskAgentApi"]);
                AccessControlLists adorasACLs = new AccessControlLists(projectDef.AdoRestApiConfigurations["SecurityApi"]);

                // Set how many entities or objects must be imported.
                entitiesCounter = projectDef.VariableGroupDefinitions.Count;

                foreach (SingleObjectDefinitionFile df in projectDef.VariableGroupDefinitions)
                {
                    // Read the content of definition file.
                    jsonContent = projectDef.ReadDefinitionFile(df.FilePath);

                    // Send some traces.
                    _mySource.Value.TraceInformation($"Process variable group definition stored in {df.FileName}.");
                    _mySource.Value.Flush();

                    // Read variable group definition.
                    variableGroupAsJToken = JToken.Parse(jsonContent);
                    variableGroupName = variableGroupAsJToken["name"].ToString();

                    // Does it require a prefix?
                    if (!string.IsNullOrEmpty(prefixName))
                    {
                        // Check if it already contains the prefix, there is no need to take action.
                        if (!variableGroupName.StartsWith($"{prefixName}{prefixSeparator}"))
                        {
                            // Retain variable group name from definition file.
                            string variableGroupNameFromDefinitionFile = variableGroupName;

                            // Send some traces.
                            _mySource.Value.TraceInformation($"Add prefix: {prefixName}{prefixSeparator} in front of the variable group name: {variableGroupNameFromDefinitionFile}.");
                            _mySource.Value.Flush();

                            // Add the prefix and a dot.
                            variableGroupName = $"{prefixName}{prefixSeparator}{variableGroupNameFromDefinitionFile}";
                            variableGroupAsJToken["name"] = variableGroupName;
                        }
                    }

                    // Send some traces.
                    _mySource.Value.TraceInformation("Create variable group: {0}.", variableGroupName);
                    _mySource.Value.Flush();

                    // Write temporarily the content to create a new variable group.
                    jsonContent = JsonConvert.SerializeObject(variableGroupAsJToken, Formatting.Indented);
                    filename = projectDef.GetFileFor("Work.ImportVariableGroup", new object[] { ++entitiesToImportCounter });
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Serialize the raw request message.
                    jsonContent = JsonConvert.SerializeObject(variableGroupAsJToken);

                    // Create variable group against destination collection/project.
                    nvg = adorasVariableGroups.CreateVariableGroup(jsonContent);

                    if (adorasVariableGroups.CRUDOperationSuccess)
                    {
                        // Increment the imported entities.
                        importedEntities++;

                        // Store variable group information.
                        projectDef.AdoEntities.VariableGroups.Add(nvg.Name, nvg.Id);
                    }
                    else
                    {
                        string errorMsg = string.Format("Error while creating {0}: {1}. Azure DevOps REST api error message: {2}", "variable group", variableGroupName, adorasVariableGroups.LastApiErrorMessage);

                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        _mySource.Value.Flush();
                    }
                }

                // Query security descriptors and generate tasks.
                if (behaviors.QuerySecurityDescriptors && !isOnPremiseMigration)
                {
                    // Retrieve all default security principals, some might miss at this step.
                    Engine.Utility.RetrieveDefaultSecurityPrincipal(projectDef);

                    // Get security namespace identifier.
                    securityNamespaceId = Constants.DistributedTaskLibraryId;

                    // Define sid to associate to variable group.
                    // sid is just the team name here.
                    sid = _engineConfig.DefaultTeamName;
                    foreach (SingleObjectDefinitionFile df in projectDef.VariableGroupDefinitions)
                    {
                        jsonContent = projectDef.ReadDefinitionFile(df.FilePath);

                        variableGroupAsJToken = JToken.Parse(jsonContent);
                        variableGroupName = variableGroupAsJToken["name"].ToString();

                        if (!string.IsNullOrEmpty(prefixName))
                        {
                            if (!variableGroupName.StartsWith($"{prefixName}{prefixSeparator}"))
                            {
                                string variableGroupNameFromDefinitionFile = variableGroupName;

                                variableGroupName = $"{prefixName}{prefixSeparator}{variableGroupNameFromDefinitionFile}";
                            }
                        }

                        var item = projectDef.AdoEntities.VariableGroups.Where(x => x.Key == variableGroupName).FirstOrDefault();
                        // if can't find item
                        if (item.Equals(default(KeyValuePair<string, int>)))
                        {
                            _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Unable to find imported variable group with name: {variableGroupName}. Unable to query security.");
                            _mySource.Value.Flush();
                            continue;
                        }

                        // Retrieve security token for variable group.
                        // .Key property stores the variable group name.
                        // .Value property stores the variable group identifier needed to create the security token.
                        securityToken = Engine.Utility.GenerateVariableGroupToken(projectDef.DestinationProject.Id, item.Value);

                        // Get acl.
                        acl = adorasACLs.GetAccessControlList(securityNamespaceId, securityToken);

                        // When nothing is configured outside the default permissions, the ACEs dictionary will be null.
                        if (acl.AcesDictionary == null)
                            haveACEsDefined = false;
                        else
                        {
                            haveACEsDefined = true;

                            // Extract ACE defined for project administrators.
                            ace = acl.AcesDictionary[Engine.Utility.GenerateIdentityDescriptor(projectDef.SidCache["Administrators"])];
                        }

                        // Add a security task only if specific ACLs are defined.
                        if (haveACEsDefined)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Add security task for variable group: {item.Key}");
                            _mySource.Value.Flush();

                            // Add a security task to apply same ACE as project administrators to default team.
                            identityDescriptor = Engine.Utility.GenerateIdentityDescriptor(sid);
                            projectDef.SecurityTasks.AddTask(securityNamespaceId, securityToken, false, identityDescriptor, ace);
                        }
                    }
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating variable groups: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} variable groups were imported.", importedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private void ImportWiki(ProjectDefinition projectDef, string prefixName, string prefixSeparator)
        {
            // Initialize.
            int entitiesCounter = 0;
            int entitiesToImportCounter = 0;
            int importedEntities = 0;
            int skippedEntities = 0;
            string filename;
            string jsonContent = null;
            string repositoryId = null;
            string tokenName;
            string tokenValue;
            string wikiName;
            string[] pathsToSelectForRemoval = null;
            List<KeyValuePair<string, string>> tokensList;
            JToken curatedJToken = null;
            JToken versionsAsJToken = null;
            JToken wikisAsJToken;
            JObject requestBodyAsJObject = null;
            WikiResponse.Wikis allExistingWikis = null;
            WikiResponse.Wiki alreadyCreatedWiki = null;
            WikiResponse.Wiki createdWiki = null;
            WikiResponse.Wiki updatedWiki = null;
            Wikis adorasWikis = null;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Import wikis.");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition("Wikis");

                // Create an Azure DevOps REST api service object.
                adorasWikis = new Wikis(projectDef.AdoRestApiConfigurations["WikiApi"]);

                // Read definition of wikis.
                // It may contain one or many wikis.
                jsonContent = projectDef.ReadDefinitionFile(projectDef.WikiDefinitions.FilePath);

                #region - Replace all generalization tokens.

                // Send some traces.
                _mySource.Value.TraceInformation("Replace tokens with values.");
                _mySource.Value.Flush();

                // Define all tokens to replace from the definition file.
                // Tokens must be created in order of resolution, so this is why
                // a list of key/value pairs is used instead of a dictionary.
                tokensList = new List<KeyValuePair<string, string>>();

                // Add token and replacement value to list.
                tokenName = Engine.Utility.CreateTokenName("Project", ReplacementTokenValueType.Id);
                tokenValue = projectDef.DestinationProject.Id;
                tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                // Add tokens and replacement values related to repositories to list.
                foreach (string repositoryName in projectDef.AdoEntities.Repositories.Keys)
                {
                    // Does it have a prefix?
                    // Tokenization occurred before knowing a prefix is required, so
                    // the token name has no prefix.
                    if (string.IsNullOrEmpty(prefixName))
                    {
                        tokenName = Engine.Utility.CreateTokenName(repositoryName, "Repository", ReplacementTokenValueType.Id);
                        tokenValue = projectDef.AdoEntities.Repositories[repositoryName];
                    }
                    else
                    {
                        string repositoryNameWOPrefix = GetEntityNameWithoutPrefix(repositoryName, prefixName, prefixSeparator);
                        tokenName = Engine.Utility.CreateTokenName(repositoryNameWOPrefix, "Repository", ReplacementTokenValueType.Id);
                        tokenValue = projectDef.AdoEntities.Repositories[repositoryName];
                    }
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                }

                // Replace tokens.
                jsonContent = Engine.Utility.ReplaceTokensInJsonContent(jsonContent, tokensList);

                #endregion

                // Read build definition.
                wikisAsJToken = JToken.Parse(jsonContent);

                // Set how many entities or objects must be imported.
                entitiesCounter = wikisAsJToken["count"].ToObject<int>();

                // Get existing wikis.
                allExistingWikis = adorasWikis.GetWikis();

                // Some properties must be removed as they are not needed during creation.
                // Define JsonPath query to retrieve properties to remove.
                pathsToSelectForRemoval = new string[] { "$.versions" };

                // Extract one wiki at a time.
                foreach (JToken wikiAsJToken in (JArray)wikisAsJToken["value"])
                {
                    // Get wiki name.
                    wikiName = wikiAsJToken["name"].ToString();

                    // Try to find if the wiki exists
                    alreadyCreatedWiki = allExistingWikis.Value.Where(x => x.Name == wikiName).FirstOrDefault();

                    // Skip existing wikis.
                    if (alreadyCreatedWiki != null)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Wiki: {wikiName} already exists, it will be skipped.");
                        _mySource.Value.Flush();

                        // Increment skip counter.
                        skippedEntities++;

                        // Go to next wiki definition.
                        continue;
                    }

                    // In wiki output, property is versions, but for import, can only import one version, so property is
                    // version, so store version here before removing it.
                    versionsAsJToken = wikiAsJToken["versions"];

                    // Enforcing change of wiki type to be codeWiki (this is the only way to have multiple project's
                    // wiki's side by side).
                    wikiAsJToken["type"] = "codeWiki";

                    // Get repository identifier.
                    repositoryId = wikiAsJToken["repositoryId"].ToString();

                    // Add the first version in a version property. The versions property will be removed.
                    wikiAsJToken["version"] = new JObject(
                                                    new JProperty("version", versionsAsJToken[0]["version"])
                                                );

                    // Remove useless properties.
                    curatedJToken = wikiAsJToken.RemoveSelectProperties(pathsToSelectForRemoval);

                    // Write temporarily the content to create a new wiki.
                    jsonContent = JsonConvert.SerializeObject(curatedJToken, Formatting.Indented);
                    filename = projectDef.GetFileFor("Work.ImportWiki", new object[] { ++entitiesToImportCounter });
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Serialize the raw request message.
                    jsonContent = JsonConvert.SerializeObject(curatedJToken);

                    // Send some traces.
                    _mySource.Value.TraceInformation("Create wiki: {0}.", wikiName);
                    _mySource.Value.Flush();

                    // Create the wiki.
                    createdWiki = adorasWikis.CreateWiki(jsonContent);

                    if (adorasWikis.CRUDOperationSuccess)
                    {
                        // Increment the imported entities.
                        importedEntities++;

                        // Generate the raw request message to update the wiki.
                        requestBodyAsJObject = new JObject(
                                                    new JProperty("versions", versionsAsJToken)
                                                );
                        jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject);

                        // Update the wiki.
                        updatedWiki = adorasWikis.UpdateWiki(jsonContent, createdWiki.Id);

                        if (adorasWikis.CRUDOperationSuccess)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Wiki: {wikiName} has been successfully updated.");
                            _mySource.Value.Flush();
                        }
                        else
                        {
                            string errorMsg = string.Format("Error while updating {0}: {1}. Azure DevOps REST api error message: {2}", "wiki", wikiName, adorasWikis.LastApiErrorMessage);

                            // Send some traces.
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                            _mySource.Value.Flush();
                        }
                    }
                    else
                    {
                        string errorMsg = string.Format("Error while creating {0}: {1}. Azure DevOps REST api error message: {2}", "wiki", wikiName, adorasWikis.LastApiErrorMessage);

                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                        _mySource.Value.Flush();
                    }
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while creating wikis: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} wikis were imported and {2} were skipped.", importedEntities, entitiesCounter, skippedEntities);
            _mySource.Value.Flush();
        }

        private void InitializeProject(ProjectDefinition projectDef, Maps maps)
        {
            JObject requestBodyAsJObject;
            string jsonContent;
            int entitiesCounter;
            int importedEntities = 0;
            // int skippedEntities = 0;
            ClassificationNodes adorasCN = new ClassificationNodes(projectDef.AdoRestApiConfigurations["GetClassificationNodesApi"]);
            Teams adorasTeams = new Teams(projectDef.AdoRestApiConfigurations["TeamsApi"]);
            Graph adorasGraph = new Graph(projectDef.AdoRestApiConfigurations["GraphApi"]);
            AccessControlLists adorasACLs = new AccessControlLists(projectDef.AdoRestApiConfigurations["SecurityApi"]);
            Dictionary<string, JObject> iterationDictionary;
            string defaultAreaSecurityToken = adorasCN.GetClassificationNodeToken(projectDef.DestinationProject.Name);
            Engine.Utility.RetrieveDefaultSecurityPrincipal(projectDef);

            //new init
            InitializeIteration(projectDef, _engineConfig.IterationInitializationTreePath);
            InitializeArea(projectDef, _engineConfig.AreaInitializationTreePath);

            //old init
            ImportIteration(projectDef, true, _engineConfig.IterationInclusions, _engineConfig.IsClone);
            ImportArea(projectDef, true, _engineConfig.AreaInclusions, _engineConfig.IsClone);

            //team depends on old init
            ImportTeam(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator, true,
                _engineConfig.TeamExclusions, _engineConfig.TeamInclusions);

            ImportTeamConfiguration(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator, _engineConfig.Behaviors, true, maps,
                _engineConfig.TeamExclusions, _engineConfig.TeamInclusions, _engineConfig.IsClone);
        }

        private void InstallExtension(ProjectDefinition projectDef)
        {
            // Initialize.
            bool builtinExtension = false;
            int entitiesCounter = 0;
            int installedEntities = 0;
            int resultCompare = 0;
            string jsonContent = null;
            string extensionId = null;
            string publisherId = null;
            string version = null;
            string flags = null;
            JToken extensionsAsJToken = null;
            ExtentionMgmtMinimalResponse.InstalledExtensions allInstalledExtensions = null;
            ExtentionMgmtMinimalResponse.InstalledExtension alreadyInstalledExtension = null;
            ExtentionMgmtMinimalResponse.InstalledExtension ie = null;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Install extensions");
                _mySource.Value.Flush();

                // Load definitions first.
                projectDef.LoadDefinition("Extensions");

                if (projectDef.ExtensionDefinitions != null)
                {
                    // Create an Azure DevOps REST api service object.
                    ExtensionManagement adorasExtensionMgmt = new ExtensionManagement(projectDef.AdoRestApiConfigurations["ExtensionMgmtApi"]);

                    // Retrieve all installed extensions first.
                    allInstalledExtensions = adorasExtensionMgmt.GetInstalledExtensions();

                    // Read the content of definition file.
                    jsonContent = projectDef.ReadDefinitionFile(projectDef.ExtensionDefinitions.FilePath);

                    // Get list of extensions.
                    extensionsAsJToken = JsonConvert.DeserializeObject<JToken>(jsonContent);

                    // Set how many entities or objects must be installed.
                    entitiesCounter = extensionsAsJToken["count"].ToObject<int>();

                    foreach (JToken extensionAsJToken in extensionsAsJToken["value"])
                    {
                        extensionId = extensionAsJToken["extensionId"].ToString();
                        publisherId = extensionAsJToken["publisherId"].ToString();
                        version = extensionAsJToken["version"].ToString();

                        // Extract extension flags if it exists.
                        if (extensionAsJToken["flags"] != null)
                            flags = extensionAsJToken["flags"].ToString();
                        else
                            flags = string.Empty;

                        // Is it a built-in extension?
                        builtinExtension = (flags.ToLower() == "builtin, trusted");

                        // Try to find if it is already installed.
                        alreadyInstalledExtension = allInstalledExtensions.Value.Where(x => x.PublisherId == publisherId && x.ExtensionId == extensionId).FirstOrDefault();

                        if (alreadyInstalledExtension == null)
                        {
                            if (builtinExtension)
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation(@"Built-in {0}.{1} version {2} extension would need to be installed manually.", publisherId, extensionId, version);
                                _mySource.Value.Flush();
                            }
                            else
                            {
                                // Send some traces.
                                if (builtinExtension)
                                    _mySource.Value.TraceInformation(@"Install built-in extension: {0}.{1} version {2}", publisherId, extensionId, version);
                                else
                                    _mySource.Value.TraceInformation(@"Install extension: {0}.{1} version {2}", publisherId, extensionId, version);
                                _mySource.Value.Flush();

                                // Install the extension specified.
                                ie = adorasExtensionMgmt.InstallExtension(publisherId, extensionId, version);

                                if (adorasExtensionMgmt.CRUDOperationSuccess)
                                {
                                    // Send some traces.
                                    if (builtinExtension)
                                        _mySource.Value.TraceInformation(@"Built-in extension {0}.{1} version {2} is installed", publisherId, extensionId, version);
                                    else
                                        _mySource.Value.TraceInformation(@"Extension {0}.{1} version {2} is installed", publisherId, extensionId, version);
                                    _mySource.Value.Flush();

                                    // Increment counter of installed extensions.
                                    installedEntities++;
                                }
                                else
                                {
                                    string errorMsg = string.Format("Error while installing extension {0}.{1} version {2}. Azure DevOps REST api error message: {3}", publisherId, extensionId, version, adorasExtensionMgmt.LastApiErrorMessage);

                                    // Send some traces.
                                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                                    _mySource.Value.Flush();
                                }
                            }
                        }
                        else
                        {
                            // The version installed can be different.
                            var v = Version.Parse(alreadyInstalledExtension.Version);
                            var cv = Version.Parse(version);

                            // Compare
                            resultCompare = cv.CompareTo(v);

                            if (resultCompare == 0)
                            {
                                // Send some traces.
                                if (builtinExtension)
                                    _mySource.Value.TraceInformation(@"Built-in {0}.{1} version {2} extension is already installed.", publisherId, extensionId, version);
                                else
                                    _mySource.Value.TraceInformation(@"{0}.{1} version {2} extension is already installed.", publisherId, extensionId, version);
                                _mySource.Value.Flush();
                            }
                            else if (resultCompare <= 0)
                            {
                                // Version to install is older than version installed.
                                // Send some traces.
                                if (builtinExtension)
                                    _mySource.Value.TraceInformation(@"Built-in {0}.{1} version {2} is already installed which is newer than {3}.", publisherId, extensionId, alreadyInstalledExtension.Version, version);
                                else
                                    _mySource.Value.TraceInformation(@"{0}.{1} version {2} is already installed which is newer than {3}.", publisherId, extensionId, alreadyInstalledExtension.Version, version);
                                _mySource.Value.Flush();
                            }
                            else if (resultCompare > 0)
                            {
                                // Version to install is newer than version installed.
                                if (builtinExtension)
                                    _mySource.Value.TraceInformation("A newer built-in version of {0}.{1} should be installed", publisherId, extensionId);
                                else
                                    _mySource.Value.TraceInformation("A newer version of {0}.{1} should be installed", publisherId, extensionId);
                                _mySource.Value.Flush();
                            }

                            // Increment counter anyway as it is already installed.
                            installedEntities++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Error while installing extensions: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} extensions were installed.", installedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private string NewEntityKeyForTaskGroupMapping(string taskGroupId, int majorDigit)
        {
            string entityKey = $"{taskGroupId},{majorDigit}.*";
            return entityKey;
        }

        private string NewEntityKeyForTaskGroupMapping(string taskGroupId, string versionSpec)
        {
            string entityKey = $"{taskGroupId},{versionSpec}";
            return entityKey;
        }

        private void RedirectTask(ProjectDefinition projectDef, JToken taskAsJToken, string pipelineType = "Build")
        {
            try
            {
                _mySource.Value.TraceInformation($"Attempting to redirect task");
                _mySource.Value.Flush();

                string powershellV2TaskId;
                string presetRedirectedTaskJsonContent;
                string redirectedTaskName;
                string taskName;
                string taskJson;
                string taskIdPropertyName;
                string versionSpecPropertyName;
                string taskId;
                string versionSpec;
                JToken dummyTask;

                // Set property names to read task identifier and version spec.
                if (pipelineType == "Build")
                {
                    taskIdPropertyName = "id";
                    versionSpecPropertyName = "versionSpec";
                    // Extract task group identifier + version spec.
                    taskId = taskAsJToken["task"][taskIdPropertyName].ToString();
                    versionSpec = taskAsJToken["task"][versionSpecPropertyName].ToString();

                    presetRedirectedTaskJsonContent = Constants.RedirectedTaskBuild;
                    taskName = taskAsJToken["displayName"].ToString();
                }
                else
                {
                    taskIdPropertyName = "taskId";
                    versionSpecPropertyName = "version";
                    // Extract task group identifier + version spec.
                    taskId = taskAsJToken[taskIdPropertyName].ToString();
                    versionSpec = taskAsJToken[versionSpecPropertyName].ToString();

                    presetRedirectedTaskJsonContent = Constants.RedirectedTaskRelease;
                    taskName = taskAsJToken["name"].ToString();
                }

                redirectedTaskName = $"RedirectedTask-{taskName}";
                taskJson = JsonConvert.SerializeObject(taskAsJToken);

                var tokensList = new List<KeyValuePair<string, string>>();


                if (projectDef.AdoEntities.TasksByNameVersionSpec.ContainsKey("PowerShell,2.*"))
                {
                    powershellV2TaskId = projectDef.AdoEntities.TasksByNameVersionSpec["PowerShell,2.*"].TaskId;
                }
                else
                    throw (new Exception("PowerShell 2.* task does not exist."));

                // Add token and replacement value to list.
                var tokenName = Engine.Utility.CreateTokenName("PowerShell", "Task", ReplacementTokenValueType.Id);
                var tokenValue = powershellV2TaskId;
                tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));
                tokensList.Add(new KeyValuePair<string, string>("RedirectedName", redirectedTaskName));
                tokensList.Add(new KeyValuePair<string, string>("RedirectedContent", JsonConvert.ToString(taskJson)));


                // Replace tokens.
                var jsonContent = Engine.Utility.ReplaceTokensInJsonContent(presetRedirectedTaskJsonContent, tokensList);
                dummyTask = JsonConvert.DeserializeObject<JToken>(jsonContent);

                // Update values in input JToken
                if (pipelineType == "Build")
                {
                    taskAsJToken["task"][taskIdPropertyName] = new JValue(powershellV2TaskId);
                    taskAsJToken["task"][versionSpecPropertyName] = new JValue("2.*");
                    taskAsJToken["inputs"] = dummyTask["inputs"];
                    taskAsJToken["displayName"] = new JValue(redirectedTaskName);
                }
                else
                {
                    taskAsJToken[taskIdPropertyName] = new JValue(powershellV2TaskId);
                    taskAsJToken[versionSpecPropertyName] = new JValue("2.*");
                    taskAsJToken["inputs"] = dummyTask["inputs"];
                    taskAsJToken["name"] = new JValue(redirectedTaskName);
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, $"Error while trying to redirect task, Error: {ex.Message}, Stack Trace: {ex.StackTrace}");
                _mySource.Value.Flush();
                throw (new RecoverableException($"Unable to redirect task"));
            }
        }
        private void RemapMetatask(ProjectDefinition projectDef, JToken taskAsJToken, string pipelineType = "Build")
        {
            // Initialize.
            string taskType = taskAsJToken["definitionType"].ToString().ToLower();

            try
            {
                // Ignore if not a metatask (also known as task group).
                if (taskType == "metatask")
                {
                    // Initialize.
                    string taskgroupId = null;
                    string versionSpec = null;
                    string oldTaskgroupId = null;
                    string oldVersionSpec = null;
                    string newTaskgroupId = null;
                    string newVersionSpec = null;
                    string entityKey;
                    string[] subkeys;
                    string taskIdPropertyName = null;
                    string versionSpecPropertyName = null;
                    ProjectDefinition.MappingRecord mappingRecord;

                    #region - Validate passed JToken object representing a task.

                    if (pipelineType == "Build")
                    {
                        // The schema of a step object is the following:
                        // {
                        //   "environment": { ... },
                        //   "enabled": true or false,
                        //   "continueOnError":  true or false,
                        //   "alwaysRun":  true or false,
                        //   "displayName": "Description",
                        //   "timeoutInMinutes": numerical value,
                        //   "condition": "expression",
                        //   "task": {
                        //     "id": "guid",
                        //     "versionSpec": "version spec. like 1.* or 2.*",
                        //     "definitionType": "metaTask"
                        //   },
                        //   "inputs": { ... }
                        // }

                        // Initialize.
                        int c1 = 0;     // Will check the total number of JProperty. To be valid this counter must be equal to 3.
                        int c2 = 0;     // Will check the number of JToken that are not JProperty. To be valid this counter must be equal to 0.
                        int c3 = 0;     // Will check the number of invalid JProperty. To be valid this counter must be equal to 0.
                        string[] validBuildPipelineProperties = new string[] { "id", "definitionType", "versionSpec" };

                        // Browse every properties
                        foreach (JToken j in taskAsJToken)
                        {
                            if (j.Type == JTokenType.Property)
                                c1++;
                            if (j.Type == JTokenType.Object || j.Type == JTokenType.Array)
                                c2++;
                            if (j is JProperty p)
                                if (!validBuildPipelineProperties.Contains(p.Name))
                                    c3++;
                        }

                        // Validate all rules.
                        if (!(c1 == 3 && c2 == 0 && c3 == 0))
                            throw (new ArgumentException("JToken object containing the definition of a build task is corrupted or unrecognized"));
                    }
                    else if (pipelineType == "Release")
                    {
                        // The schema of a workflowTask object is the following:
                        // {
                        //   "environment": { ... },
                        //   "taskId": "buid",
                        //   "version spec. like 1.* or 2.*",
                        //   "name": "Task name",
                        //   "refName": "Reference prefix name to use with output variables",
                        //   "enabled": true,
                        //   "alwaysRun":  true or false,
                        //   "continueOnError":  true or false,
                        //   "timeoutInMinutes": numerical value,
                        //   "definitionType": "metaTask",
                        //   "overrideInputs": { ... },
                        //   "condition": "expression",
                        //   "inputs": { ... }
                        // }

                        // Initialize.
                        int c1 = 0;     // Will check the number of nvalid JProperty. To be valid this counter must be equal to 3.
                        string[] validBuildPipelineProperties = new string[] { "taskId", "definitionType", "version" };

                        // Browse every properties
                        foreach (JToken j in taskAsJToken)
                        {
                            if (j is JProperty p)
                                if (validBuildPipelineProperties.Contains(p.Name))
                                    c1++;
                        }

                        // Validate all rules.
                        if (c1 != 3)
                            throw (new ArgumentException("JToken object containing the definition of a release task is corrupted or unrecognized"));
                    }

                    #endregion

                    // Set property names to read task identifier and version spec.
                    if (pipelineType == "Build")
                    {
                        taskIdPropertyName = "id";
                        versionSpecPropertyName = "versionSpec";
                    }
                    else if (pipelineType == "Release")
                    {
                        taskIdPropertyName = "taskId";
                        versionSpecPropertyName = "version";
                    }

                    // Extract task group identifier + version spec.
                    taskgroupId = taskAsJToken[taskIdPropertyName].ToString();
                    versionSpec = taskAsJToken[versionSpecPropertyName].ToString();

                    // Form the entity key which is a composition key which is made of task identifier and version spec.
                    entityKey = $"{taskgroupId},{versionSpec}";

                    // Retrieve mapping record.
                    // For task group, the key is composite and it is formed of <task identifier>,<version spec.>
                    mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityKey == entityKey && x.Type == "TaskGroup");

                    if (mappingRecord == null)
                        throw (new RecoverableException($"Unable to find mapping for task group with id: {taskgroupId}"));
                    else
                    {
                        if (string.IsNullOrEmpty(mappingRecord.OldEntityKey))
                            throw (new RecoverableException($"Unable to map because mapping task group with id: {taskgroupId} has no old entity key."));

                        if (string.IsNullOrEmpty(mappingRecord.NewEntityKey))
                            throw (new RecoverableException($"Unable to map because mapping task group with id: {taskgroupId} has no new entity key."));
                    }

                    // This entity key is a composition key made of task group identifier and version spec.
                    subkeys = SplitEntityKey(mappingRecord.OldEntityKey);
                    oldTaskgroupId = subkeys[0];
                    oldVersionSpec = subkeys[1];
                    subkeys = SplitEntityKey(mappingRecord.NewEntityKey);
                    newTaskgroupId = subkeys[0];
                    newVersionSpec = subkeys[1];

                    // Send some traces.
                    if (mappingRecord.OldEntityName != mappingRecord.NewEntityName)
                        _mySource.Value.TraceInformation($"Replacing task group: {mappingRecord.OldEntityName} [{oldTaskgroupId}] with {mappingRecord.NewEntityName} [{newTaskgroupId}]");
                    else
                        _mySource.Value.TraceInformation($"Replacing task group: {mappingRecord.OldEntityName} [{oldTaskgroupId}] to [{newTaskgroupId}]");

                    // Replace task group identifier and version spec.
                    taskAsJToken[taskIdPropertyName] = new JValue(newTaskgroupId);
                    taskAsJToken[versionSpecPropertyName] = new JValue(newVersionSpec);
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while remapping metatask: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }
        }

        private void RemapServiceEndpoint(ProjectDefinition projectDef, JToken taskAsJToken, string pipelineType = "Build")
        {
            // Initialize.
            bool isValidInputProperty;
            string endpointIdentifier;
            string externalEndpointsAsString;
            string originalInputProperty;
            JToken inputsAsJToken = taskAsJToken["inputs"];
            ProjectDefinition.MappingRecord mappingRecord;
            Match m;
            Guid g = Guid.Empty;

            // There is no way to validate the passed JToken object representing all task inputs
            // as there is no standards.\
            try
            {
                foreach (JProperty p in inputsAsJToken)
                {
                    // Keep the name of original property for further usage.
                    originalInputProperty = p.Name;

                    // Validate if it is valid service endpoint input property.
                    m = _validServiceEndpointInputPropertyNamesRegex.Match(originalInputProperty);
                    isValidInputProperty = m.Success;

                    // Proceed only if it is valid service endpoint input property.
                    if (isValidInputProperty)
                    {
                        // Set lower case for better comparison.
                        if (originalInputProperty.ToLower() == "externalendpoints" || originalInputProperty.ToLower() == "nugetserviceconnections")
                        {
                            // Extract the list of endpoints utilized.
                            externalEndpointsAsString = p.Value.ToString();

                            // Proceed only if defined.
                            if (!string.IsNullOrEmpty(externalEndpointsAsString))
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation($"Found service endpoint input parameter: {originalInputProperty}");
                                _mySource.Value.Flush();

                                foreach (string identifier in externalEndpointsAsString.Split(new char[] { ',' }))
                                {
                                    // Retrieve mapping record.
                                    mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityKey == identifier && x.Type == "ServiceEndpoint");

                                    if (mappingRecord == null)
                                    {
                                        _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Unable to find mapping for service endpoint with identifier: {identifier}. Removing from list of endpoints...");
                                        externalEndpointsAsString = externalEndpointsAsString.Replace(identifier, "");
                                        externalEndpointsAsString = externalEndpointsAsString.Replace(",,", ",");
                                        externalEndpointsAsString = externalEndpointsAsString.TrimEnd(',');
                                    }
                                    else
                                    {
                                        externalEndpointsAsString = externalEndpointsAsString.Replace(mappingRecord.OldEntityKey, mappingRecord.NewEntityKey);
                                    }

                                    // Remap.

                                }

                                // Remap service endpoint identifiers.
                                p.Value = externalEndpointsAsString;
                            }
                        }
                        else
                        {
                            // Sometimes a task use an input parameter which has the same name
                            // as input parameter for connected service. To distinguish that case
                            // check if the value of this input parameter is a guid, if not this is
                            // not an input parameter for a connected service.
                            if (Guid.TryParse(p.Value.ToString(), out g))
                            {
                                // Get endpoint identifier.
                                endpointIdentifier = p.Value.ToString();

                                // Proceed only if defined.
                                if (!string.IsNullOrEmpty(endpointIdentifier))
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"Found service endpoint input parameter: {originalInputProperty}");
                                    _mySource.Value.Flush();

                                    // Retrieve mapping record.
                                    // For service endpoint, the key relates to endpoint identifier.
                                    mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityKey == endpointIdentifier && x.Type == "ServiceEndpoint");

                                    if (mappingRecord == null)
                                        throw (new RecoverableException($"Unable to find mapping for service endpoint with identifier: {endpointIdentifier}"));
                                    else
                                        if (string.IsNullOrEmpty(mappingRecord.NewEntityKey))
                                        throw (new RecoverableException($"No new identifier is defined for service endpoint {mappingRecord.OldEntityName} with identifier: {mappingRecord.OldEntityKey}"));

                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"Replacing service endpoint {mappingRecord.OldEntityName} identifier from: {mappingRecord.OldEntityKey} to {mappingRecord.NewEntityKey}");
                                    _mySource.Value.Flush();

                                    // Remap service endpoint identifiers.
                                    p.Value = mappingRecord.NewEntityKey;
                                }
                            }
                        }
                    }
                }
            }
            catch (RecoverableException ex)
            {
                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Error remapping service endpoints {ex.Message}. Redirecting task...");
                _mySource.Value.Flush();
                RedirectTask(projectDef, taskAsJToken, pipelineType);
            }

        }

        private void RemapTask(ProjectDefinition projectDef, JToken taskAsJToken, string pipelineType = "Build")
        {
            // Initialize.
            string taskType;
            try
            {
                if (pipelineType == "Build")
                {
                    taskType = taskAsJToken["task"]["definitionType"].ToString().ToLower();
                }
                else
                {
                    taskType = taskAsJToken["definitionType"].ToString().ToLower();
                }

                // Ignore if not a task.
                if (taskType == "task")
                {
                    // Initialize.
                    int i = -1;
                    int compareResult;
                    string highestVersionFound = null;
                    string highestVersionSpecFound = null;
                    string taskId = "";
                    string taskIdPropertyName = null;
                    string versionSpec = "";
                    string versionSpecPropertyName = null;
                    Version cv;
                    ProjectDefinition.TaskReference tf;

                    // Set property names to read task identifier and version spec.
                    if (pipelineType == "Build")
                    {
                        taskIdPropertyName = "id";
                        versionSpecPropertyName = "versionSpec";
                        // Extract task group identifier + version spec.
                        taskId = taskAsJToken["task"][taskIdPropertyName].ToString();
                        versionSpec = taskAsJToken["task"][versionSpecPropertyName].ToString();
                    }
                    else if (pipelineType == "Release")
                    {
                        taskIdPropertyName = "taskId";
                        versionSpecPropertyName = "version";
                        // Extract task group identifier + version spec.
                        taskId = taskAsJToken[taskIdPropertyName].ToString();
                        versionSpec = taskAsJToken[versionSpecPropertyName].ToString();
                    }

                    // If it is found, it means it exists. Search by identifier and version spec.
                    if (projectDef.AdoEntities.TasksById.ContainsKey(taskId))
                    {
                        if (!string.IsNullOrEmpty(versionSpec))
                        {
                            // Generate a version mask with the version spec.
                            string version = versionSpec.Replace(".*", null) + ".0.0";
                            cv = new Version(version);

                            // Get task reference.
                            tf = projectDef.AdoEntities.TasksById[taskId];

                            // Check all possible version of this task.
                            foreach (string versionAsString in tf.Versions)
                            {
                                // Increment index to synchronize .Versions and .VersionSpecs properties.
                                i++;

                                // Compare version of task found and version mask.
                                compareResult = cv.CompareTo(new Version(versionAsString));

                                if (compareResult <= 0)
                                {
                                    // It means that version found is higher than required.
                                    if (highestVersionFound != null)
                                    {
                                        // Keep the highest version and version spec. only.
                                        if (new Version(highestVersionFound).CompareTo(new Version(versionAsString)) < 0)
                                        {
                                            // Store new highest version and version spec.
                                            highestVersionFound = versionAsString;
                                            highestVersionSpecFound = tf.VersionSpecs[i];
                                        }
                                    }
                                    else
                                    {
                                        // Store version and version spec. to compare later.
                                        highestVersionFound = versionAsString;
                                        highestVersionSpecFound = tf.VersionSpecs[i];
                                    }
                                }
                                else
                                {
                                    // Greater than zero, it means that version needed is older than required.
                                    // Continue to search.
                                    continue;
                                }
                            }

                            if (highestVersionFound == null)
                                throw (new RecoverableException($"Unable to find task: {tf.TaskName} [{taskId}] that meet version spec: {versionSpec}"));
                            else
                            {
                                // Same version spec.
                                if (versionSpec == highestVersionSpecFound)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"Found task: {tf.TaskName} [{taskId}] with the same version spec: {versionSpec}. Task version is {highestVersionFound}.");
                                    _mySource.Value.Flush();
                                }
                                // An higher version spec.
                                else
                                {
                                    _mySource.Value.TraceInformation($"Replace task: {tf.TaskName} [{taskId}]: {taskId} version spec: {versionSpec} with {highestVersionSpecFound}. Task version is {highestVersionFound}."); _mySource.Value.Flush();

                                    // Replace version spec.
                                    if (pipelineType == "Build")
                                    {
                                        taskAsJToken["task"][versionSpecPropertyName] = new JValue(highestVersionSpecFound);
                                    }
                                    else if (pipelineType == "Release")
                                    {
                                        taskAsJToken[versionSpecPropertyName] = new JValue(highestVersionSpecFound);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _mySource.Value.TraceEvent(TraceEventType.Warning,
                                0,
                                $"Version spec for task {taskId.ToString()} is empty");
                        }
                    }
                    else
                    {
                        _mySource.Value.TraceInformation($"Unable to find task with id: {taskId}");
                        _mySource.Value.Flush();
                        RedirectTask(projectDef, taskAsJToken, pipelineType);
                    }
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error while remapping task: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "No longer in use for now.")]
        private void RemapVariableGroup(ProjectDefinition projectDef, JToken variableGroupAsJToken)
        {
            // Initialize.
            int variableGroupId;
            ProjectDefinition.MappingRecord mappingRecord;

            if (variableGroupAsJToken.Type == JTokenType.Integer)
            {
                // Extract variable group id.
                variableGroupId = (int)variableGroupAsJToken;

                // Retrieve mapping record.
                mappingRecord = projectDef.FindMappingRecord(x => x.OldEntityKey == variableGroupId.ToString() && x.Type == "VariableGroup");

                if (mappingRecord == null)
                    throw (new RecoverableException($"Unable to find mapping for variable group with id: {variableGroupId}"));

                // Send some traces.
                _mySource.Value.TraceInformation($"Replacing variable group: {mappingRecord.OldEntityName} [{mappingRecord.OldEntityKey}] to [{mappingRecord.NewEntityKey}]");

                // Replace identifier.
                variableGroupId = Convert.ToInt32(mappingRecord.NewEntityKey);
            }
        }

        private void RenameRepository(ProjectDefinition projectDef)
        {
            // Initialize.
            bool operationStatus;
            int entitiesCounter = 0;
            int renamedEntities = 0;
            string jsonContent;
            string repositoryName;
            string newRepositoryName;
            JObject requestBodyAsJObject;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Rename repositories.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                Repositories adorasRepositories = new Repositories(projectDef.AdoRestApiConfigurations["GitRepositoriesApi"]);

                // Set how many entities or objects must be renamed.
                entitiesCounter = projectDef.AdoEntities.Repositories.Count;

                foreach (var item in projectDef.AdoEntities.Repositories)
                {
                    // Extract name.
                    // Prefix it with project name.
                    repositoryName = item.Key;
                    newRepositoryName = string.Format("{0}.{1}", _engineConfig.SourceProject, repositoryName);

                    try
                    {
                        // Generate the request message.
                        requestBodyAsJObject = new JObject(new JProperty("name", newRepositoryName));

                        // Serialize.
                        jsonContent = JsonConvert.SerializeObject(requestBodyAsJObject);

                        // Send some traces.
                        _mySource.Value.TraceInformation("Rename repository {0} to {1}", repositoryName, newRepositoryName);
                        _mySource.Value.Flush();

                        // Try to update.
                        // todo: make the method to returned resulting object.
                        adorasRepositories.UpdateRepository(repositoryName, jsonContent);

                        // Update mapping record for this repository.
                        projectDef.UpdateMappingRecordByName("Repository", repositoryName, null, newRepositoryName);

                        // Increment the renamed entities.
                        renamedEntities++;
                    }
                    catch (Exception ex)
                    {
                        // Send some traces.
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, $"Error while renaming repository {repositoryName} to {newRepositoryName}, Error: {ex.Message}, Stack Trace: {ex.StackTrace}");
                        _mySource.Value.Flush();
                    }
                }

                // Operation is successful.
                operationStatus = true;

                if (operationStatus)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("Repositories renamed.");
                    _mySource.Value.Flush();
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Error while renaming repositories: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} repositories were renamed.", renamedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private void RenameTeam(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter = 0;
            int renamedEntities = 0;
            string jsonContent;
            string newTeamName;
            string teamId;
            string teamName;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Rename teams.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                Teams adorasTeams = new Teams(projectDef.AdoRestApiConfigurations["TeamsApi"]);

                // Set how many entities or objects must be renamed.
                entitiesCounter = projectDef.AdoEntities.Teams.Count;

                foreach (var item in projectDef.AdoEntities.Teams)
                {
                    // Extract name.
                    // Prefix it with project name.
                    teamName = item.Key;
                    teamId = item.Value;
                    newTeamName = String.Format("{0}.{1}", _engineConfig.SourceProject, teamName);
                    if (_engineConfig.TeamMappings != null)
                    {
                        EngineConfiguration.TeamMapping teamMapping = null;
                        teamMapping = _engineConfig.TeamMappings.Where(x => x.SourceTeam == teamName).FirstOrDefault();
                        if (teamMapping != null)
                        {
                            _mySource.Value.TraceInformation($"{teamName} exists in team mappings. Skipping...");
                            continue;
                        }
                    }
                    _mySource.Value.TraceInformation("Attempting to rename {0} to {1}.", teamName, newTeamName);
                    _mySource.Value.Flush();

                    // Create JSON document to rename team.
                    jsonContent = JsonConvert.SerializeObject(new JObject(
                                new JProperty("name", newTeamName)
                            ));

                    // Retrieve team identifier.
                    teamId = projectDef.AdoEntities.Teams[teamName];

                    // Rename team.
                    adorasTeams.UpdateTeam(teamId, jsonContent);
                    // Increment the renamed entities.
                    renamedEntities++;
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Error while renaming teams: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} teams were renamed.", renamedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private string[] SplitEntityKey(string entityKey)
        {
            // Initialize.
            string[] subkeys;

            // Split...
            subkeys = entityKey.Split(new char[] { ',' });

            // Return split keys.
            return subkeys;
        }

        #endregion

        #region - Public Members

        public ProjectImportEngine(EngineConfiguration config)
        {
            // Store configuration.
            _engineConfig = config;

            // Generate the regex to validate if it is a valid service endpoint input property name.
            string expr = Engine.Utility.GenerateRegexForPossibleServiceEndpointInputPropertyNames(_engineConfig.CustomInputParameterNamesForServiceEndpoint);
            _validServiceEndpointInputPropertyNamesRegex = new Regex(expr, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Run the import engine.
        /// </summary>
        public void Run()
        {
            // Hardcoded validation to prevent to erase anything from our production collections/accounts.
            if (
                _engineConfig.DestinationCollection == "devopsabcs"
                || _engineConfig.DestinationCollection == "devsecopsabcs"
                )
            {
                string errorMsg = "General fault: Production instance of ADO could be modified or deleted";
                throw (new Exception(errorMsg));
            }

            // Define the project to import.
            ProjectDefinition projectDef = new ProjectDefinition(_engineConfig.ImportPath, _engineConfig.SourceCollection, _engineConfig.SourceProject, _engineConfig.DestinationCollection, _engineConfig.DestinationProject, _engineConfig.Description, _engineConfig.DestinationProjectProcessName, _engineConfig.DefaultTeamName, _engineConfig.PAT, _engineConfig.RestApiService);

            // Clean work folder.
            // projectDef.CleanWorkFolder();
            string mapLocation = _engineConfig.ProcessMapLocation;
            RestAPI.ProcessMapping.Maps maps = Maps.LoadFromJson(mapLocation);

            // Create ADO destination project.
            if (_engineConfig.Behaviors.CreateDestinationProject)
                CreateDestinationProject(projectDef);

            if (_engineConfig.Behaviors.InitializeProject)
                InitializeProject(projectDef, maps);

            // Retrieve ADO source project state and query some properties.
            string filename = projectDef.GetFileFor("ProjectInfo.Source");
            string json = File.ReadAllText(filename);
            projectDef.SourceProject = JsonConvert.DeserializeObject<ProjectDefinition.ProjectProperties>(json);

            // Validate ADO destination project state and query some properties.
            Engine.Utility.ValidateProject(projectDef, OperationLocation.Destination, false, true);

            // Retrieve all security namespaces.
            Engine.Utility.RetrieveSecurityNamespace(projectDef, OperationLocation.Destination);

            // Get all members of default team.
            Engine.Utility.GetMembersOfDefaultTeam(projectDef, OperationLocation.Destination);

            // Retrieve all default security principals and cache them, some might miss at this step.
            if (!_engineConfig.IsOnPremiseMigration)
            {
                Engine.Utility.RetrieveDefaultSecurityPrincipal(projectDef);
            }

            // Retrieve all agent queues.
            Engine.Utility.RetrieveAgentQueueIdentifiers(projectDef);

            // Retrieve repository identifiers.
            Engine.Utility.RetrieveRepositoryIdentifiers(projectDef);

            // Retrieve build definition identifiers.
            Engine.Utility.RetrieveBuildDefinitionIdentifiers(projectDef);

            // Retrieve variable group identifiers.
            Engine.Utility.RetrieveVariableGroupIdentifiers(projectDef);

            // Retrieve task identifiers.
            Engine.Utility.RetrieveTaskIdentifiers(projectDef, OperationLocation.Destination);

            // Retrieve team identifiers.
            Engine.Utility.RetrieveTeamIdentifiers(projectDef);

            // Install required extensions.
            if (_engineConfig.Behaviors.InstallExtensions)
                InstallExtension(projectDef);

            // Import areas.
            if (_engineConfig.Behaviors.ImportArea)
                ImportArea(projectDef, false, _engineConfig.AreaInclusions, _engineConfig.IsClone);

            // Import iterations.
            if (_engineConfig.Behaviors.ImportIteration)
                ImportIteration(projectDef, false, _engineConfig.IterationInclusions, _engineConfig.IsClone);

            // Import teams.
            if (_engineConfig.Behaviors.ImportTeam)
                ImportTeam(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator, false,
                    _engineConfig.TeamExclusions, _engineConfig.TeamInclusions);

            // Import team configurations.
            if (_engineConfig.Behaviors.ImportTeamConfiguration)
                ImportTeamConfiguration(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator, _engineConfig.Behaviors, false, maps,
                    _engineConfig.TeamExclusions, _engineConfig.TeamInclusions, _engineConfig.IsClone);

            // Import repositories.
            if (_engineConfig.Behaviors.ImportRepository)
                ImportRepository(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator, _engineConfig.Behaviors,
                    _engineConfig.IsOnPremiseMigration);

            // Import policy configurations.
            // todo: can it be moved elsewhere?
            if (_engineConfig.Behaviors.ImportPolicyConfiguration)
                ImportPolicyConfiguration(projectDef);

            if (_engineConfig.Behaviors.ImportWiki)
                ImportWiki(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator);

            // Import pull requests.
            if (_engineConfig.Behaviors.ImportPullRequest)
                ImportPullRequest(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator);

            // Import service endpoints.
            if (_engineConfig.Behaviors.ImportServiceEndpoint)
                ImportServiceEndpoint(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator);

            // Import variable groups.
            if (_engineConfig.Behaviors.ImportVariableGroup)
                ImportVariableGroup(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator, _engineConfig.Behaviors,
                    _engineConfig.IsOnPremiseMigration);

            // Import task groups.
            if (_engineConfig.Behaviors.ImportTaskGroup)
                ImportTaskGroup(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator, _engineConfig.Behaviors,
                    _engineConfig.IsOnPremiseMigration);

            // Import agent queues.
            if (_engineConfig.Behaviors.ImportAgentQueue)
                ImportAgentQueue(projectDef);

            // Import build Definitions.
            if (_engineConfig.Behaviors.ImportBuildDefinition)
                ImportBuildDefinition(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator, _engineConfig.Behaviors,
                    _engineConfig.IsOnPremiseMigration);

            // Import release Definitions.
            if (_engineConfig.Behaviors.ImportReleaseDefinition)
                ImportReleaseDefinition(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator, _engineConfig.Behaviors,
                    _engineConfig.IsOnPremiseMigration);

            // Handle security.
            if (!_engineConfig.IsOnPremiseMigration)
                HandleSecurity(projectDef, _engineConfig.PrefixName, _engineConfig.PrefixSeparator, _engineConfig.SecurityTasksFile, _engineConfig.Behaviors,
                    _engineConfig.TeamInclusions, _engineConfig.TeamExclusions);

            // Delete default Git repository.
            if (_engineConfig.Behaviors.DeleteDefaultRepository)
                DeleteDefaultRepository(projectDef);

            // Rename Repositories.
            if (_engineConfig.Behaviors.RenameRepository)
                RenameRepository(projectDef);

            // Rename teams.
            // todo: can it be moved elsewhere?
            if (_engineConfig.Behaviors.RenameTeam)
                RenameTeam(projectDef);

            // Send some traces.
            _mySource.Value.TraceInformation("Import process has completed");
            _mySource.Value.Flush();
        }
    }

    #endregion
}