using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ADO.Collections;
using ADO.Engine.Configuration;
using ADO.Engine.Configuration.ProjectExport;
using ADO.Extensions;
using ADO.RestAPI;
using ADO.RestAPI.Build;
using ADO.RestAPI.Core;
using ADO.RestAPI.DistributedTasks;
using ADO.RestAPI.ExtensionManagement;
using ADO.RestAPI.Git;
using ADO.RestAPI.Policy;
using ADO.RestAPI.Release;
using ADO.RestAPI.Service;
using ADO.RestAPI.Viewmodel50;
using ADO.RestAPI.Wiki;
using ADO.RestAPI.Work;
using ADO.RestAPI.WorkItemTracking.ClassificationNodes;
using ADO.Tools;
using ADO.RestAPI.ProcessMapping;

namespace ADO.Engine
{
    /// <summary>
    /// Class to manage Azure DevOps export processes.
    /// </summary>
    public sealed class ProjectExportEngine
    {
        #region - Static Declarations

        #region - Private Members

        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.Engine.ProjectExport"));
        private static Regex _validServiceEndpointInputPropertyNamesRegex;

        #endregion

        #endregion

        #region - Private Members

        private readonly EngineConfiguration _engineConfig;

        private void CountServiceEndpoint(JToken inputsAsJToken, Dictionary<string, int> endpointsCountingTable)
        {
            // Initialize.
            bool isValidInputProperty;
            string endpointIdentifier;
            string externalEndpointsAsString;
            string originalInputProperty;
            Match m;

            // There is no way to validate the passed JToken object representing all task inputs
            // as there is no standards. 

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
                            foreach (string identifier in externalEndpointsAsString.Split(new char[] { ',' }))
                            {
                                // Increment counter for endpoint identifier or create new entry.
                                if (endpointsCountingTable.ContainsKey(identifier))
                                    endpointsCountingTable[identifier]++;
                                else
                                    endpointsCountingTable.Add(identifier, 1);
                            }
                        }
                    }
                    else
                    {
                        // Get endpoint identifier.
                        endpointIdentifier = p.Value.ToString();

                        // Increment counter for endpoint identifier or create new entry.
                        if (endpointsCountingTable.ContainsKey(endpointIdentifier))
                            endpointsCountingTable[endpointIdentifier]++;
                        else
                            endpointsCountingTable.Add(endpointIdentifier, 1);
                    }
                }
            }

            // The results are passed by reference via the dictionary.
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It will use this parameter but this feature is not implemented yet.")]
        private void ExportAgentQueue(ProjectDefinition projectDef)
        {
            string errorMsg = string.Format("{0} behavior has not been implemented yet.", "ExportAgentQueue");
            throw (new NotImplementedException(errorMsg));
        }

        /// <summary>
        /// Get project areas and write them in a definition file for later import.
        /// </summary>
        private void ExportArea(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter = 0;
            int exportedEntities;
            string[] jsonPropertiesToRemove;
            JToken settingsAsJToken;
            JToken curatedSettingsAsJToken;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export Areas.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                ClassificationNodes adorasCN = new ClassificationNodes(projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"]);

                // Get areas.
                settingsAsJToken = adorasCN.GetAreasAsJToken();

                if (settingsAsJToken != null)
                {
                    // todo: need to write something to traverse the full structure to count
                    // how many iterations exist.

                    // Some properties must be removed as they are not needed during creation.
                    // Define properties of json object to remove.
                    jsonPropertiesToRemove = new string[] { "_links", "url", "id", "identifier" };
                    curatedSettingsAsJToken = settingsAsJToken.RemoveProperties(jsonPropertiesToRemove);

                    // Define the export file.
                    var filename = projectDef.GetFileFor("Areas");

                    // Write definition file.
                    projectDef.WriteDefinitionFile(filename, curatedSettingsAsJToken);

                    // All entities have been exported.
                    exportedEntities = entitiesCounter;
                }
            }
            catch (Exception ex)
            {
                // Send some traces.                
                string errorMsg = string.Format("Error occured while exporting areas: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation("Areas were exported");
            // todo: will enable this back when the counters work.
            // _mySource.Value.TraceInformation(@"{0}/{1} areas were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get project build definitions and write them in definition files for later import.
        /// </summary>
        private void ExportBuildDefinition(ProjectDefinition projectDef, ProjectExportBehavior behaviors, JValue defaultAgentPoolName)
        {
            // Initialize.
            int entitiesCounter;
            int exportedEntities = 0;
            int exportFileCounter = 0;
            int skippedEntities = 0;
            int processType;
            string filename;
            string buildDefinitionName;
            string jsonContent;
            string repositoryType;
            List<string> unsupportedRepositoryType = new List<string> { "TfsVersionControl" };
            JToken generalizedDef;
            List<JToken> listOfBuildDefinitionsAsJToken;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export build definitions after generalization.");
                _mySource.Value.Flush();

                // Create Azure DevOps REST api service objects.
                BuildDefinition adorasBuild = new BuildDefinition(projectDef.AdoRestApiConfigurations["BuildApi"]);

                // Get build definitions.
                listOfBuildDefinitionsAsJToken = adorasBuild.GetBuildDefinitionsAsJToken();

                // Set how many entities or objects must be exported.
                entitiesCounter = listOfBuildDefinitionsAsJToken.Count;

                if (entitiesCounter > 0)
                {
                    // Create folder for this type of definitions.
                    projectDef.GetFolderFor("BuildDefinitions", true);

                    foreach (JToken definitionAsJToken in listOfBuildDefinitionsAsJToken)
                    {
                        // Increment the export file counter.
                        exportFileCounter++;

                        try
                        {
                            // Get build definition name.
                            buildDefinitionName = definitionAsJToken["name"].ToString();

                            // See what type of process it is. 1 = JSON based build definition, 2 = YAML based build definition.
                            processType = definitionAsJToken["process"]["type"].ToObject<int>();

                            if (processType == 1)
                            {
                                // Extract the repository type used with the build definition.
                                repositoryType = definitionAsJToken["repository"]["type"].ToString();

                                // Check if this type is unsupported.
                                if (unsupportedRepositoryType.Contains(repositoryType))
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"{buildDefinitionName} uses a {repositoryType} repository type which is not supported for now, it will be skipped.");
                                    _mySource.Value.Flush();

                                    // Increment counter of skipped entities.
                                    skippedEntities++;

                                    // Skip this definition.
                                    continue;
                                }
                                if (definitionAsJToken["quality"].ToString() == "draft")
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"{buildDefinitionName} is a draft which is not supported for now, it will be skipped.");
                                    _mySource.Value.Flush();

                                    // Increment counter of skipped entities.
                                    skippedEntities++;

                                    // Skip this definition.
                                    continue;
                                }

                                // Generalize.
                                generalizedDef = GeneralizeBuildDefinition(projectDef, definitionAsJToken, behaviors.AddProjectNameToBuildDefinitionPath,
                                    defaultAgentPoolName);

                                // Save data.
                                jsonContent = JsonConvert.SerializeObject(generalizedDef, Formatting.Indented);

                                // Define the export file.
                                filename = projectDef.GetFileFor("BuildDefinition", new object[] { exportFileCounter });

                                // Write definition file.
                                projectDef.WriteDefinitionFile(filename, jsonContent);

                                // Increment the exported entities.
                                exportedEntities++;
                            }
                            else if (processType == 2)
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation($"{buildDefinitionName} is a YAML build definition which is not supported for now, it will be skipped.");
                                _mySource.Value.Flush();

                                // Increment counter of skipped entities.
                                skippedEntities++;

                                // Skip this definition.
                                continue;
                            }
                        }
                        catch (RecoverableException)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("A problem occurred during generalization process with {0}, it will be skipped.", definitionAsJToken["name"].ToString());
                            _mySource.Value.Flush();

                            // Increment counter of skipped entities.
                            skippedEntities++;

                            // Skip this definition.
                            continue;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting build definitions: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} build definitions were exported and {2} were skipped.", exportedEntities, entitiesCounter, skippedEntities);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get the full list of installed extensions and write them in a definition file for later import.
        /// </summary>
        private void ExportInstalledExtension(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter = 0;
            int exportedEntities = 0;
            string[] pathsToSelectForRemoval;
            JToken extensionsAsJToken;
            JToken curatedJToken;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export installed extensions.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                ExtensionManagement adorasExtensionManagement = new ExtensionManagement(projectDef.AdoRestApiConfigurations["ExtensionMgmtApi"]);

                // Get iterations.
                extensionsAsJToken = adorasExtensionManagement.GetInstalledExtensionsAsJToken();

                if (extensionsAsJToken != null)
                {
                    // Set how many entities or objects must be exported.
                    entitiesCounter = extensionsAsJToken["count"].ToObject<int>();

                    // Some properties must be removed as they are not needed during creation.
                    // Define JsonPath query to retrieve properties to remove.
                    pathsToSelectForRemoval = new string[]
                    {
                        "$.value[*].contributions",
                        "$.value[*].contributionTypes",
                        "$.value[*].extensionName",
                        "$.value[*].publisherName",
                        "$.value[*].manifestVersion",
                        "$.value[*].registrationId",
                        "$.value[*].scopes",
                        "$.value[*].baseUri",
                        "$.value[*].fallbackBaseUri",
                        "$.value[*].installState",
                        "$.value[*].lastPublished",
                        "$.value[*].eventCallbacks",
                        "$.value[*].files"
                    };

                    // Some properties must be removed as they are not needed during creation.
                    // Curate the JToken.
                    curatedJToken = extensionsAsJToken.RemoveSelectProperties(pathsToSelectForRemoval);

                    // Define the export file.
                    var filename = projectDef.GetFileFor("InstalledExtensions");

                    // Write definition file.
                    projectDef.WriteDefinitionFile(filename, curatedJToken);

                    // All entities have been exported.
                    exportedEntities = entitiesCounter;
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting extensions: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} extensions were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get project iterations and write them in definition files for later import.
        /// </summary>
        private void ExportIteration(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter = 0;
            int exportedEntities;
            string[] jsonPropertiesToRemove;
            JToken settingsAsJToken;
            JToken curatedSettingsAsJToken;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export Iterations.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                ClassificationNodes adorasCN = new ClassificationNodes(projectDef.AdoRestApiConfigurations["WorkItemTrackingApi"]);

                // Get iterations.
                settingsAsJToken = adorasCN.GetIterationsAsJToken();

                if (settingsAsJToken != null)
                {
                    // todo: need to write something to traverse the full structure to count
                    // how many iterations exist.

                    // Some properties must be removed as they are not needed during creation.
                    // Define properties of json object to remove.
                    jsonPropertiesToRemove = new string[] { "_links", "url", "id", "identifier" };
                    curatedSettingsAsJToken = settingsAsJToken.RemoveProperties(jsonPropertiesToRemove);

                    // Define the export file.
                    var filename = projectDef.GetFileFor("Iterations");

                    // Write definition file.
                    projectDef.WriteDefinitionFile(filename, curatedSettingsAsJToken);

                    // All entities have been exported.
                    exportedEntities = entitiesCounter;
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting iterations: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"Iterations were exported.");
            // todo: will enable this back when the counters work.
            // _mySource.Value.TraceInformation(@"{0}/{1} iterations were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Export Organization Processes.
        /// </summary>
        private void ExportOrganizationProcess(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter = 0;
            int exportedEntities = 0;
            string jsonContent;
            CoreResponse.Processes processes;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export Organization Processes.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                Processes adorasProcesses = new Processes(projectDef.AdoRestApiConfigurations["CoreApi"]);

                // Get processes.
                processes = adorasProcesses.GetProcesses();

                if (processes != null)
                {
                    // Set how many entities or objects must be exported.
                    entitiesCounter = processes.Count;

                    // Generate an output file to keep track of all organization processes.
                    // This is useful when importing to a different organization 
                    // that may not have the corresponding process (inherited or Hosted XML).

                    // Save data.
                    jsonContent = JsonConvert.SerializeObject(processes, Formatting.Indented);

                    // Define the export file.
                    var filename = projectDef.GetFileFor("Processes");

                    // Write definition file.
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // All entities have been exported.
                    exportedEntities = entitiesCounter;
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting processes: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"Processes were exported.");
            // todo: will enable this back when the counters work.
            _mySource.Value.TraceInformation(@"{0}/{1} processes were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get project policy configurations and write them in a definition file for later import.
        /// </summary>
        private void ExportPolicyConfiguration(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter = 0;
            int exportedEntities = 0;
            string jsonContent;
            JToken policyConfigAsJToken;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export policy configurations.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                PolicyConfigurations adorasPolicyConfigurations = new PolicyConfigurations(projectDef.AdoRestApiConfigurations["PolicyConfigurationsApi"]);

                // Get all policy configurations.
                policyConfigAsJToken = adorasPolicyConfigurations.GetPolicyConfigurations();

                if (policyConfigAsJToken != null)
                {
                    // Set how many entities or objects must be exported.
                    entitiesCounter = policyConfigAsJToken["count"].ToObject<int>();

                    // Save data.
                    jsonContent = JsonConvert.SerializeObject(policyConfigAsJToken, Formatting.Indented);

                    // Define the export file.
                    var filename = projectDef.GetFileFor("Policies");

                    // Write definition file.
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // All entities have been exported.
                    exportedEntities = entitiesCounter;
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting policy configuration: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} policy configurations were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get project open pull requests and write them in definition files for later import.
        /// </summary>
        /// 
        private void ExportPullRequest(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter = 0;
            int entitiesToExportCounter1 = 0;
            int entitiesToExportCounter2 = 0;
            int exportedEntities = 0;
            string filename;
            string jsonContent;
            string[] pathsToSelectForRemoval;
            string[] storageRecord;
            JToken curatedJToken;
            JToken pullRequestsAsJToken;
            JToken PRCommentThreadsAsJToken;
            List<JToken> removalList;
            GitMinimalResponse.GitRepositories allGitRepositories;

            // Use a numerical abstracted identifier with pull requests.
            AbstractIdentification absId = new AbstractIdentification(AbstractIdentifierType.NumericalIdentifier);

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export pull requests.");
                _mySource.Value.Flush();

                // Create Azure DevOps REST api service objects.
                PullRequests adorasPullRequests = new PullRequests(projectDef.AdoRestApiConfigurations["GitPullRequestsApi"]);
                Repositories adorasRepositories = new Repositories(projectDef.AdoRestApiConfigurations["GitRepositoriesApi"]);

                // Get all repositories.
                allGitRepositories = adorasRepositories.GetAllRepositories();

                // Create folder for this type of definitions.
                projectDef.GetFolderFor("PullRequests", true);

                foreach (var repository in allGitRepositories.Value)
                {
                    // Get all open pull requests.
                    pullRequestsAsJToken = adorasPullRequests.GetOpenPullRequestsAsJToken(repository.Id);

                    // Write temporarily the content of pull request.
                    jsonContent = JsonConvert.SerializeObject(pullRequestsAsJToken, Formatting.Indented);
                    filename = projectDef.GetFileFor("Work.ExportPullRequest", new object[] { ++entitiesToExportCounter1 });
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Store the number of pull requests found.
                    entitiesCounter += pullRequestsAsJToken["count"].ToObject<int>();

                    if (entitiesCounter > 0)
                    {
                        // Create folder for this type of definitions.
                        projectDef.GetFolderFor("RepositoryPullRequests", new object[] { repository.Name }, true);

                        // Some properties must be removed as they are not needed during creation.
                        // Define JsonPath query to retrieve properties to remove.
                        pathsToSelectForRemoval = new string[]
                        {
                            "$._links",
                            "$.codeReviewId",
                            "$.createdBy",
                            "$.creationDate",
                            "$.isDraft",
                            "$.lastMergeCommit",
                            "$.lastMergeSourceCommit",
                            "$.lastMergeTargetCommit",
                            "$.mergeId",
                            "$.mergeStatus",
                            "$.pullRequestId",
                            "$.repository",
                            "$.reviewers[*].displayName",
                            "$.reviewers[*].imageUrl",
                            "$.reviewers[*].reviewerUrl",
                            "$.reviewers[*].uniqueName",
                            "$.reviewers[*].url",
                            "$.reviewers[*].vote",
                            "$.reviewers[*]._links",
                            "$.status",
                            "$.url"
                        };

                        foreach (JToken pullRequestAsJToken in pullRequestsAsJToken["value"])
                        {
                            // Initialize removal list.
                            removalList = new List<JToken>();

                            // Remove all reviewers and simplify the process by creating a single reviewer for now.
                            // It must be accomplished in two steps: first set the list of JTokens to remove
                            // and second remove JToken node.
                            foreach (JToken reviewerAsJToken in pullRequestAsJToken["reviewers"])
                                removalList.Add(reviewerAsJToken);
                            foreach (JToken item in removalList)
                                item.Remove();

                            // Add a single reviewer with a name to be determined.
                            JArray reviewersAsJArray = (JArray)pullRequestAsJToken["reviewers"];
                            reviewersAsJArray.Add(new JObject(new JProperty("id", Engine.Utility.CreateToken("Reviewer", ReplacementTokenValueType.Id))));

                            // Serialize pull request data and regenerate as JToken up for raw manipulation.
                            jsonContent = JsonConvert.SerializeObject(pullRequestAsJToken);
                            curatedJToken = JsonConvert.DeserializeObject<JToken>(jsonContent).RemoveSelectProperties(pathsToSelectForRemoval);

                            // Serialize again.
                            jsonContent = JsonConvert.SerializeObject(curatedJToken, Formatting.Indented);

                            // Use the repository name (index = 2) as the key to generate an abstracted key.
                            // The storage record will be formed like the following:
                            // storageRecord[0] = repository identifier.
                            // storageRecord[1] = repository name.
                            // storageRecord[2] = pull request identifier.
                            // storageRecord[3] = abstracted pull request identifier.
                            storageRecord = absId.AddValueFields(repository.Id, repository.Name, pullRequestAsJToken["pullRequestId"].ToString(), "1");

                            // Define path where to write pull request file.
                            filename = projectDef.GetFileFor("PullRequest", new object[] { storageRecord[1], storageRecord[3] });

                            // Export pull request.
                            projectDef.WriteDefinitionFile(filename, jsonContent);

                            // Increment the exported entities.
                            exportedEntities++;
                        }
                    }
                }

                // Proceed if pull requests exist.
                if (absId.Count > 0)
                {
                    // Some properties must be removed as they are not needed during creation.
                    // Define JsonPath query to retrieve properties to remove.
                    pathsToSelectForRemoval = new string[]
                    {
                            "$.value[*]._links",
                            "$.value[*].comments[0].author",
                            "$.value[*].comments[0].author.displayName",
                            "$.value[*].comments[0].author.imageUrl",
                            "$.value[*].comments[0].author.url",
                            "$.value[*].comments[0].author._links",
                            "$.value[*].comments[0].id",
                            "$.value[*].comments[0].lastContentUpdatedDate",
                            "$.value[*].comments[0].lastUpdatedDate",
                            "$.value[*].comments[0].publishedDate",
                            "$.value[*].comments[0].usersLiked",
                            "$.value[*].comments[0]._links",
                            "$.value[*].id",
                            "$.value[*].identities",
                            "$.value[*].lastUpdatedDate",
                            "$.value[*].publishedDate"
                    };

                    foreach (string[] record in absId.ValueFields)
                    {
                        // Get all pull request comment threads.
                        var pullRequestId = Convert.ToInt32(record[2]);
                        PRCommentThreadsAsJToken = adorasPullRequests.GetPullRequestThreadsAsJToken(record[0], pullRequestId);

                        // Write temporarily the content of pull request threads.
                        jsonContent = JsonConvert.SerializeObject(PRCommentThreadsAsJToken, Formatting.Indented);
                        filename = projectDef.GetFileFor("Work.ExportPullRequestThreads", new object[] { ++entitiesToExportCounter2 });
                        projectDef.WriteDefinitionFile(filename, jsonContent);

                        // Process only if items were found.
                        if (PRCommentThreadsAsJToken["count"].ToObject<int>() > 0)
                        {
                            // Reset removal list.
                            removalList = new List<JToken>();

                            // Explanations on PropertiesCollection from https://docs.microsoft.com/en-us/rest/api/azure/devops/git/pull%20request%20threads/create?view=azure-devops-rest-5.0#propertiescollection
                            // are not really giving much, but looking at past version of Visual Studio Services from https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2013/dn386028(v%3Dvs.120)
                            // give more insightful information about how to manipulate this class with JSON.
                            // Based on another REST api call that shows an example using PropertiesCollection based type too,
                            // It looks, the type is not passed for string and int32 only, the other supported types are passing
                            // the '$type' property.
                            // Based on these information, the '$type' property is removed only when a string or int32 value
                            // is met.
                            // JSONPath query for this type of token does not work for unexplained reason.
                            foreach (JToken PRCommentThreadAsJToken in PRCommentThreadsAsJToken["value"])
                                foreach (JToken prtPropertyAsJToken in PRCommentThreadAsJToken["properties"])
                                    foreach (JToken oneToken in prtPropertyAsJToken.Values())
                                        if (oneToken is JProperty oneProperty)
                                            if (oneProperty.Name == "$type")
                                                if (oneProperty.Value.ToString() == "System.String" || oneProperty.Value.ToString() == "System.Int32")
                                                    removalList.Add(oneToken);

                            // Remove these useless JSON properties.
                            foreach (JToken t in removalList)
                                t.Remove();

                            // Serialize pull request data and regenerate as JToken up for raw manipulation.
                            jsonContent = JsonConvert.SerializeObject(PRCommentThreadsAsJToken);
                            curatedJToken = JsonConvert.DeserializeObject<JToken>(jsonContent).RemoveSelectProperties(pathsToSelectForRemoval);

                            // Serialize again.
                            jsonContent = JsonConvert.SerializeObject(curatedJToken, Formatting.Indented);

                            // Define path where to write pull request threads file.
                            filename = projectDef.GetFileFor("PullRequestThreads", new object[] { record[1], record[3] });

                            // Export pull request threads.
                            projectDef.WriteDefinitionFile(filename, jsonContent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting pull requests: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} pull requests were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get project settings and write them in a definition file for later import.
        /// </summary>
        private void ExportProjectSettings(ProjectDefinition projectDef)
        {
            // Initialize.
            string jsonContent;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export project settings");
                _mySource.Value.Flush();

                // Read content.
                jsonContent = projectDef.ReadTemplate("CreateProject");

                // Define the export file.
                var filename = projectDef.GetFileFor("CreateDestinationProject");

                // Write definition file.
                projectDef.WriteDefinitionFile(filename, jsonContent);
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting project settings: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation("Project settings were exported");
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get project release definitions and write them in definition files for later import.
        /// </summary>
        private void ExportReleaseDefinition(ProjectDefinition projectDef, ProjectExportBehavior behaviors)
        {
            // Initialize.
            bool skipDefinition;
            int exportFileCounter = 0;
            int entitiesCounter = 0;
            int exportedEntities = 0;
            int skippedEntities = 0;
            string filename;
            string jsonContent;
            string artifactType = null;
            JToken generalizedDef;
            List<JToken> listOfReleaseDefinitionsAsJToken;
            List<string> unsupportedRepositoryType = new List<string> { "TFVC" };

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export release definitions after generalization.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                ReleaseDefinition adorasRelease = new ReleaseDefinition(projectDef.AdoRestApiConfigurations["ReleaseApi"]);

                // Get all release definitions that exist under this project.
                listOfReleaseDefinitionsAsJToken = adorasRelease.GetReleaseDefinitionsAsJToken();

                // Set how many entities or objects must be exported.
                entitiesCounter = listOfReleaseDefinitionsAsJToken.Count;

                if (entitiesCounter > 0)
                {
                    // Create folder for this type of definitions.
                    projectDef.GetFolderFor("ReleaseDefinitions", true);

                    foreach (JToken definitionAsJToken in listOfReleaseDefinitionsAsJToken)
                    {
                        // Reset flag.
                        skipDefinition = false;

                        // Increment the export file counter.
                        exportFileCounter++;

                        try
                        {
                            // Extract the repository type used with the build definition.
                            foreach (JToken artifactAsJToken in (JArray)definitionAsJToken["artifacts"])
                            {
                                // Extract the artifact type used with the release definition.
                                artifactType = artifactAsJToken["type"].ToString();

                                // Check if this type is unsupported.
                                if (unsupportedRepositoryType.Contains(artifactType))
                                {
                                    // Will skip this definition.
                                    // Leave nested processing loop.
                                    skipDefinition = true;
                                }
                            }

                            // Will skip this definition.
                            // Leave nested processing loop.
                            if (skipDefinition)
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation($"{artifactType} artifact type is not supported for now, it will be skipped.");
                                _mySource.Value.Flush();

                                // Increment counter of skipped entities.
                                skippedEntities++;

                                // Skip this definition.
                                continue;
                            }
                            try
                            {
                                // Generalize a release definition.
                                generalizedDef = GeneralizeReleaseDefinition(projectDef, definitionAsJToken, behaviors.AddProjectNameToBuildDefinitionPath, behaviors.AddProjectNameToReleaseDefinitionPath, behaviors.ResetPipelineQueue);
                            }
                            catch (Exception ex)
                            {
                                _mySource.Value.TraceEvent(TraceEventType.Error, 0, $"Error generalizing release definition: {ex.Message}, {ex.StackTrace}. skipping...");
                                _mySource.Value.Flush();
                                continue;
                            }

                            // Serialize data.
                            jsonContent = JsonConvert.SerializeObject(generalizedDef, Formatting.Indented);

                            // Define the export file.
                            filename = projectDef.GetFileFor("ReleaseDefinition", new object[] { exportFileCounter });

                            // Write definition file.
                            projectDef.WriteDefinitionFile(filename, jsonContent);

                            // Increment the exported entities.
                            exportedEntities++;
                        }
                        catch (RecoverableException)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("A problem occurred during generalization process with {0}, it will be skipped.", definitionAsJToken["name"].ToString());
                            _mySource.Value.Flush();

                            // Increment counter of skipped entities.
                            skippedEntities++;

                            // Skip this definition.
                            continue;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting release build definitions: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} release definitions were exported and {2} were skipped.", exportedEntities, entitiesCounter, skippedEntities);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get the full list of Git repositories and write a service endpoint and a source code definition files for later import.
        /// <para>
        /// * It works only for the user who is having access to both source and target repositories in the organization with the same UserID.
        /// </para>
        /// </summary>
        private void ExportRepository(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter;
            int exportedEntities = 0;
            string host;
            string filename;
            string jsonContent;
            string presetImportSourceCodeJsonContent;
            string presetGitServiceEndPointJsonContent;
            string tokenName;
            string tokenValue;
            List<KeyValuePair<string, string>> tokensList;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export repositories.");
                _mySource.Value.Flush();

                // Create import repository definitions folder.
                projectDef.GetFolderFor("ImportRepositories", true);

                // Create an Azure DevOps REST api service object.
                Repositories adorasRepositories = new Repositories(projectDef.AdoRestApiConfigurations["GitRepositoriesApi"]);

                // Get the list of all repositories.
                GitMinimalResponse.GitRepositories listOfRepositories = adorasRepositories.GetAllRepositories();

                // Set how many entities or objects must be exported.
                entitiesCounter = listOfRepositories.Count;

                // Proceed only if there is at least one repository.
                if (entitiesCounter > 0)
                {
                    // Determine the host value to utilize, which is formed with uri + the project name.
                    // It must not end with a trailing slash.
                    host = string.Format("{0}/{1}", projectDef.AdoRestApiConfigurations["GitRepositoriesApi"].BaseUri, projectDef.AdoRestApiConfigurations["GitRepositoriesApi"].Project);

                    // Read the template files which is formatted already.
                    presetImportSourceCodeJsonContent = projectDef.ReadTemplate("ImportSourceCode");
                    presetGitServiceEndPointJsonContent = projectDef.ReadTemplate("GitServiceEndpoint");

                    // Browse each repository to generate an import source code and service endpoint definitions.
                    foreach (var repository in listOfRepositories.Value)
                    {
                        // Verify if some repositories need to be excluded.
                        if (_engineConfig.ExcludeRepositories != null)
                        {
                            if (_engineConfig.ExcludeRepositories.Contains(repository.Name))
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation($"Part of excluded repository list: {repository.Name}, exclude from export.");
                                _mySource.Value.Flush();

                                // Skip this repository.
                                continue;
                            }
                        }

                        // If defined all repositories must be explicitely described to be exported.
                        if (_engineConfig.IncludeRepositories != null)
                        {
                            if (!(_engineConfig.IncludeRepositories.Contains(repository.Name)))
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation($"Not part of included repository list: {repository.Name}, exclude from export.");
                                _mySource.Value.Flush();

                                // Skip this repository.
                                continue;
                            }
                        }

                        // Send some traces.
                        _mySource.Value.TraceInformation($"Process repository: {repository.Name} [{repository.Id}]");
                        _mySource.Value.Flush();

                        #region - Replace all generalization tokens.

                        // Define all tokens to replace from the preset file.
                        // Tokens must be created in order of resolution, so this is why
                        // a list of key/value pairs is used instead of a dictionary.
                        tokensList = new List<KeyValuePair<string, string>>();

                        // Add token and replacement value to list.                        
                        tokenName = Engine.Utility.CreateTokenName("Url");
                        tokenValue = "$Host$/_git/$Repository:RepositoryName$";
                        tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                        tokenName = Engine.Utility.CreateTokenName("Host");
                        tokenValue = host;
                        tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                        tokenName = Engine.Utility.CreateTokenName("RepositoryName", "Repository");
                        tokenValue = repository.Name;
                        tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                        tokenName = Engine.Utility.CreateTokenName("EndpointName", "Endpoint");
                        tokenValue = $"{repository.Name}-code";
                        tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                        tokenName = Engine.Utility.CreateTokenName("EndpointName", "Endpoint", ReplacementTokenValueType.Id);
                        tokenValue = Engine.Utility.CreateToken($"{repository.Name}-code", "Endpoint");
                        tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                        // Replace tokens.
                        jsonContent = Engine.Utility.ReplaceTokensInJsonContent(presetImportSourceCodeJsonContent, tokensList);

                        #endregion

                        // Add a new mapping record to keep track of repositories.
                        projectDef.AddMappingRecord("Repository", repository.Id, repository.Name);

                        // Define the export file.
                        filename = projectDef.GetFileFor("Repository", new object[] { repository.Name });

                        // Write definition file.
                        projectDef.WriteDefinitionFile(filename, jsonContent);

                        // Replace tokens.
                        jsonContent = Engine.Utility.ReplaceTokensInJsonContent(presetGitServiceEndPointJsonContent, tokensList);

                        // Define the export file.
                        filename = projectDef.GetFileFor("ServiceEndpointForCodeImport", new object[] { repository.Name });

                        // Write definition file.
                        projectDef.WriteDefinitionFile(filename, jsonContent);

                        // Increment the exported entities.
                        exportedEntities++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting repositories: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} repositories were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.TraceInformation(@"{0}/{1} service endpoints were exported that will be used for import source code.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get the full list of service endpoints for later import.
        /// </summary>
        private void ExportServiceEndpoint(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter = 0;
            int exportedEntities = 0;
            string jsonContent;
            ServiceEndpointResponse.ServiceEndpoints eps;

            try
            {
                _mySource.Value.TraceInformation("Export service endpoints");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                ServiceEndPoints adorasServiceEndpoint = new ServiceEndPoints(projectDef.AdoRestApiConfigurations["ServiceEndpointApi"]);

                // Get all service endpoints.
                eps = adorasServiceEndpoint.GetServiceEndpoints();

                if (eps != null)
                {
                    // Set how many entities or objects must be exported.
                    entitiesCounter = eps.Count;

                    // Save data.
                    jsonContent = JsonConvert.SerializeObject(eps, Formatting.Indented);

                    // Define the export file.
                    var filename = projectDef.GetFileFor("ServiceEndpoints");

                    // Write definition file.
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // All entities have been exported.
                    exportedEntities = entitiesCounter;
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting service endpoints: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} service endpoints were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();

        }

        /// <summary>
        /// Get project team configurations and write them in definition files for later import.
        /// </summary>
        private void ExportTeamConfiguration(ProjectDefinition projectDef, RestAPI.ProcessMapping.Maps maps)
        {
            #region - Initialize.

            // Initialize variables.
            bool hasAtLeastOneTeam = false;
            int entitiesCounter = 0;
            int exportedEntities = 0;
            string boardNameField = "boardName";
            string currentTeamName = null;
            string currentTeamId = null;
            string defaultTeamIdentifier = null;
            string filename = null;
            string[] jsonPropertiesToRemove = null;
            List<string> boardTypes = null;
            JToken settingsAsJToken = null;
            JToken curatedSettingsAsJToken = null;
            JArray teamsAsJArray = null;
            JArray boardColumnsAsJArray = null;
            JArray boardRowsAsJArray = null;
            JArray cardFieldsAsJArray = null;
            JArray cardStylesAsJArray = null;
            BoardConfiguration adorasBoardConfig = null;
            TeamSettings adorasTeamSettings = null;

            #endregion

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export teams");
                _mySource.Value.Flush();

                // Create directory if it does not exist.
                projectDef.GetFolderFor("TeamConfigurations", true);

                #region - Get default team identifier.

                // Create an Azure DevOps REST api service object.
                Projects adorasProjects = new Projects(projectDef.AdoRestApiConfigurations["GetProjectPropertiesApi"])
                { ProjectId = projectDef.SourceProject.Id };

                // Extract project properties.
                CoreResponse.ProjectProperties projectProperties = adorasProjects.GetProjectExtendedProperties();

                // Get default team identifier.
                if (projectProperties.Count > 0)
                    defaultTeamIdentifier = projectProperties.Value.Where(x => x.Name == @"System.Microsoft.TeamFoundation.Team.Default").FirstOrDefault().Value;

                #endregion

                #region - Process teams.

                // Create an Azure DevOps REST api service object.                
                Teams adorasTeams = new Teams(projectDef.AdoRestApiConfigurations["TeamsApi"]);

                // Get all teams as a JObject.
                settingsAsJToken = adorasTeams.GetTeamsAsJToken();

                if (settingsAsJToken != null)
                {
                    // Set how many entities or objects must be exported.
                    // Validate if any teams were found by validating the 'count' property.
                    entitiesCounter = settingsAsJToken["count"].ToObject<int>();
                    hasAtLeastOneTeam = entitiesCounter > 0;

                    // before removing json properties
                    // Add new mapping records to keep track of teams.
                    foreach (var oneJToken in (JArray)settingsAsJToken["value"])
                    {
                        // Extract team name.
                        // Store which team is being processed.
                        currentTeamName = oneJToken["name"].ToString();
                        currentTeamId = oneJToken["id"].ToString();

                        projectDef.AddMappingRecord("Team", currentTeamId, currentTeamName);
                    }

                    // Some properties must be removed as they are not needed during creation.
                    // Define properties of json object to remove.
                    jsonPropertiesToRemove = new string[] { "id", "identityUrl", "url", "projectId" };
                    curatedSettingsAsJToken = settingsAsJToken.RemoveProperties(jsonPropertiesToRemove);

                    // Read all the teams under the 'value' property which is a JArray.
                    teamsAsJArray = (JArray)curatedSettingsAsJToken["value"];

                    // Export teams.
                    filename = projectDef.GetFileFor("Teams");
                    projectDef.WriteDefinitionFile(filename, teamsAsJArray);
                }

                #endregion

                // Proceed only if teams are defined in this project.
                if (hasAtLeastOneTeam)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("Export team configurations");
                    _mySource.Value.Flush();

                    #region - Create board types.

                    boardTypes = RestAPI.ProcessMapping.ProcessMappingUtility2
                        .GetBoardTypes(projectDef.SourceProject.ProcessTemplateTypeName, maps);

                    #endregion

                    foreach (var oneJToken in teamsAsJArray)
                    {
                        #region - Initialize processes.

                        // Extract team name.
                        // Store which team is being processed.
                        currentTeamName = oneJToken["name"].ToString();

                        // Send some traces.
                        _mySource.Value.TraceInformation("Export team {0} configurations: settings, boards, iterations and areas", currentTeamName);
                        _mySource.Value.Flush();

                        // Reset configuration JArray objects.
                        boardColumnsAsJArray = new JArray();
                        boardRowsAsJArray = new JArray();
                        cardFieldsAsJArray = new JArray();
                        cardStylesAsJArray = new JArray();

                        // Create output path if it does not exist.
                        projectDef.GetFolderFor("SpecificTeamConfiguration", new object[] { currentTeamName }, true);

                        #endregion

                        #region - Process team settings.

                        // Create an Azure DevOps REST api service object.
                        adorasTeamSettings = new TeamSettings(projectDef.AdoRestApiConfigurations["WorkTeamSettingsApi"]) { Team = currentTeamName };

                        // Get team settings as a JToken.
                        settingsAsJToken = adorasTeamSettings.GetTeamSettingsAsJToken();

                        // Some properties must be removed as they are not needed during creation.
                        // Define properties of json object to remove.
                        jsonPropertiesToRemove = new string[] { "_links", "url", "id" };
                        curatedSettingsAsJToken = settingsAsJToken.RemoveProperties(jsonPropertiesToRemove);

                        // Export team settings.
                        filename = projectDef.GetFileFor("TeamSettings", new object[] { currentTeamName });
                        projectDef.WriteDefinitionFile(filename, curatedSettingsAsJToken);

                        #endregion

                        #region - Process boards.

                        // Create an Azure DevOps REST api service object.
                        adorasBoardConfig = new BoardConfiguration(projectDef.AdoRestApiConfigurations["WorkBoardsApi"]) { Team = currentTeamName };

                        // Proceed with one board type at a time.
                        foreach (string boardType in boardTypes)
                        {
                            // Get columns for a given board and add it to list.
                            settingsAsJToken = adorasBoardConfig.GetBoardColumnsAsJToken(boardType);

                            // Validate if board configurations were found.
                            if (settingsAsJToken == null)
                            {
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, "Unable to read board settings, check team settings of source team");
                                _mySource.Value.Flush();
                                break;
                            }

                            // Add a new property to keep the board name.
                            settingsAsJToken[boardNameField] = boardType;

                            // Some properties must be removed as they are not needed during creation.
                            // Define properties of json object to remove.
                            jsonPropertiesToRemove = new string[] { "id" };
                            curatedSettingsAsJToken = settingsAsJToken.RemoveProperties(jsonPropertiesToRemove);
                            boardColumnsAsJArray.Add(curatedSettingsAsJToken);

                            // Get rows for a given board and add it to list.
                            settingsAsJToken = adorasBoardConfig.GetBoardRowsAsJToken(boardType);

                            // No properties must be removed.
                            // Add a new property to keep the board name.
                            settingsAsJToken[boardNameField] = boardType;
                            boardRowsAsJArray.Add(settingsAsJToken);

                            // Get rows for a given board and add it to list.
                            settingsAsJToken = adorasBoardConfig.GetCardFieldSettingsAsJToken(boardType);

                            // No properties must be removed.
                            // Add a new property to keep the board name.
                            settingsAsJToken[boardNameField] = boardType;
                            cardFieldsAsJArray.Add(settingsAsJToken);

                            // Get rows for a given board and add it to list.
                            settingsAsJToken = adorasBoardConfig.GetCardStyleSettingsAsJToken(boardType);

                            // Add a new property to keep the board name.
                            settingsAsJToken[boardNameField] = boardType;

                            // Some properties must be removed as they are not needed during creation.
                            // Define properties of json object to remove.
                            jsonPropertiesToRemove = new string[] { "_links", "url" };
                            curatedSettingsAsJToken = settingsAsJToken.RemoveProperties(jsonPropertiesToRemove);
                            cardStylesAsJArray.Add(curatedSettingsAsJToken);
                        }

                        // Export board columns.
                        if (boardColumnsAsJArray.Count > 0)
                        {
                            filename = projectDef.GetFileFor("BoardColumns", new object[] { currentTeamName });
                            projectDef.WriteDefinitionFile(filename, boardColumnsAsJArray);
                        }

                        // Export board rows.
                        if (boardRowsAsJArray.Count > 0)
                        {
                            filename = projectDef.GetFileFor("BoardRows", new object[] { currentTeamName });
                            projectDef.WriteDefinitionFile(filename, boardRowsAsJArray);
                        }

                        // Export card fields.
                        if (cardFieldsAsJArray.Count > 0)
                        {
                            filename = projectDef.GetFileFor("CardFields", new object[] { currentTeamName });
                            projectDef.WriteDefinitionFile(filename, cardFieldsAsJArray);
                        }

                        // Export card styles.
                        if (cardStylesAsJArray.Count > 0)
                        {
                            filename = projectDef.GetFileFor("CardStyles", new object[] { currentTeamName });
                            projectDef.WriteDefinitionFile(filename, cardStylesAsJArray);
                        }

                        #endregion

                        #region - Process team iterations.

                        // Get team iterations as a JToken.
                        settingsAsJToken = adorasTeamSettings.GetTeamIterationsAsJToken();

                        // Some properties must be removed as they are not needed during creation.
                        // Define properties of json object to remove.
                        jsonPropertiesToRemove = new string[] { "_links", "url", "id" };
                        curatedSettingsAsJToken = settingsAsJToken.RemoveProperties(jsonPropertiesToRemove);

                        // Export team iterations.
                        filename = projectDef.GetFileFor("TeamIterations", new object[] { currentTeamName });
                        projectDef.WriteDefinitionFile(filename, curatedSettingsAsJToken);

                        #endregion

                        #region - Process team areas.

                        // Get team areas as a JToken.
                        settingsAsJToken = adorasTeamSettings.GetTeamAreasAsJToken();

                        // Some properties must be removed as they are not needed during creation.
                        // Define properties of json object to remove.
                        jsonPropertiesToRemove = new string[] { "_links", "url" };
                        curatedSettingsAsJToken = settingsAsJToken.RemoveProperties(jsonPropertiesToRemove);

                        // Export team areas.
                        filename = projectDef.GetFileFor("TeamAreas", new object[] { currentTeamName });
                        projectDef.WriteDefinitionFile(filename, curatedSettingsAsJToken);

                        #endregion

                        // Increment the exported entities.
                        exportedEntities++;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Error occured while exporting team configurations: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} teams and its configurations were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }



        /// <summary>
        /// Get project variable groups and write them in definition files for later import.
        /// </summary>
        private void ExportVariableGroup(ProjectDefinition projectDef)
        {
            // Send some traces.
            _mySource.Value.TraceInformation("Export variable groups.");
            _mySource.Value.Flush();

            // Initialize.
            int entitiesCounter = 0;
            int exportedEntities = 0;
            int exportFileCounter = 0;
            string exportFQFN;
            string jsonContent;
            string outputPath;
            string[] jsonPropertiesToRemove;
            JToken curatedJToken;
            List<JToken> listOfVariableGroupsAsJToken;

            try
            {
                // Define the output path where to create the export files.
                outputPath = projectDef.GetFolderFor("VariableGroups", true);

                // Create an Azure DevOps REST api service object.
                VariableGroups adorasVariableGroups = new VariableGroups(projectDef.AdoRestApiConfigurations["TaskAgentApi"]);

                // Get all variable groups that exist under this project.
                listOfVariableGroupsAsJToken = adorasVariableGroups.GetVariableGroupsAsJToken();

                // Set how many entities or objects must be exported.
                entitiesCounter = listOfVariableGroupsAsJToken.Count;

                if (entitiesCounter > 0)
                {
                    // Some properties must be removed as they are not needed during creation.
                    // Define properties of json object to remove.
                    jsonPropertiesToRemove = new string[] { "id", "variableGroupProjectReferences", "createdOn", "modifiedOn", "createdBy", "modifiedBy" };

                    foreach (JToken variableGroupAsJToken in listOfVariableGroupsAsJToken)
                    {
                        // Increment the export file counter.
                        exportFileCounter++;

                        // No generalization is required.

                        // Curate the JToken.
                        curatedJToken = variableGroupAsJToken.RemoveProperties(jsonPropertiesToRemove);

                        // Serialize again.
                        jsonContent = JsonConvert.SerializeObject(curatedJToken, Formatting.Indented);

                        // Define the export file.
                        exportFQFN = projectDef.GetFileFor("VariableGroup", new object[] { exportFileCounter });

                        // Write definition file.
                        projectDef.WriteDefinitionFile(exportFQFN, jsonContent);

                        // Increment the exported entities.
                        exportedEntities++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting variable groups: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                //throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} variable groups were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get project task groups and write them in definition files for later import.
        /// </summary>
        private void ExportTaskGroup(ProjectDefinition projectDef)
        {
            // Initialize.
            bool isDraftTaskGroup;
            bool hasDependency;
            int abstractedIdentifier = 0;
            int entitiesCounter = 0;
            int exportedEntities = 0;
            int entitiesToExportCounter1 = 0;
            int skippedEntities = 0;
            int majorValue;
            int nbOfVersions;
            string filename;
            string jsonContent;
            string powershellV2TaskId;
            string presetRedirectedTaskGroupJsonContent;
            string taskGroupVersionSpecString;
            string taskVersionSpecString;
            string taskId;
            string taskGroupId;
            string taskGroupNameAtV1 = null;
            string taskGroupName;
            string tokenName;
            string tokenValue;
            string[] pathsToSelectForRemoval;
            JToken curatedJToken;
            JToken j;
            List<KeyValuePair<string, string>> tokensList;
            List<JToken> taskGroups;
            List<string> sortedListOfTaskGroupKeys;
            Dictionary<string, int> abstractedIdentificationTaskGroups = new Dictionary<string, int>();
            Dictionary<string, JToken> taskGroupDefinitions = new Dictionary<string, JToken>();
            DependencyGraph<string> graphOfTaskGroupKeys = new DependencyGraph<string>();

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export task groups.");
                _mySource.Value.Flush();

                // Define the output path where to create the export files.
                projectDef.GetFolderFor("TaskGroups", true);

                // Create Azure DevOps REST api service objects.
                TaskGroups adorasTaskGroups = new TaskGroups(projectDef.AdoRestApiConfigurations["TaskAgentApi"]);

                // Get all variable groups that exist under this project.
                taskGroups = adorasTaskGroups.GetTaskGroupsAsJToken();

                // Proceed if task groups exist.
                if (taskGroups.Count > 0)
                {
                    #region - Create a redirected task group.

                    // Read the template files which is formatted already.
                    presetRedirectedTaskGroupJsonContent = projectDef.ReadTemplate("RedirectedTaskGroup");

                    // Increment the export identifier.
                    abstractedIdentifier++;

                    #region - Replace all generalization tokens.

                    // Define all tokens to replace from the preset file.
                    // Tokens must be created in order of resolution, so this is why
                    // a list of key/value pairs is used instead of a dictionary.
                    tokensList = new List<KeyValuePair<string, string>>();

                    if (projectDef.AdoEntities.TasksByNameVersionSpec.ContainsKey("PowerShell,2.*"))
                        powershellV2TaskId = projectDef.AdoEntities.TasksByNameVersionSpec["PowerShell,2.*"].TaskId;
                    else
                        throw (new Exception("PowerShell 2.* task does not exist."));

                    // Add token and replacement value to list.
                    tokenName = Engine.Utility.CreateTokenName("PowerShell", "Task", ReplacementTokenValueType.Id);
                    tokenValue = powershellV2TaskId;
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    // Replace tokens.
                    jsonContent = Engine.Utility.ReplaceTokensInJsonContent(presetRedirectedTaskGroupJsonContent, tokensList);

                    #endregion

                    // Define the export file.
                    filename = projectDef.GetFileFor("TaskGroup", new object[] { abstractedIdentifier });

                    // Write definition file.
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    #endregion

                    #region - Analyze task group dependencies.

                    foreach (JToken taskGroupVersionAsJToken in taskGroups)
                    {
                        // Write temporarily the content of task group.
                        jsonContent = JsonConvert.SerializeObject(taskGroupVersionAsJToken, Formatting.Indented);
                        filename = projectDef.GetFileFor("Work.TaskGroupVersion", new object[] { ++entitiesToExportCounter1 });
                        projectDef.WriteDefinitionFile(filename, jsonContent);

                        // Peek to know the task group name.
                        // Get the number of versions found for this task group.
                        taskGroupName = taskGroupVersionAsJToken["value"][0]["name"].ToString();
                        nbOfVersions = taskGroupVersionAsJToken["count"].ToObject<int>();

                        // Send some traces.
                        if (nbOfVersions <= 1)
                            _mySource.Value.TraceInformation($"Process task group: {taskGroupName}, {nbOfVersions} has been found");
                        else
                            _mySource.Value.TraceInformation($"Process task group: {taskGroupName}, {nbOfVersions} have been found");
                        _mySource.Value.Flush();

                        // Check if it is a draft version.
                        j = taskGroupVersionAsJToken.SelectToken("$.value[0].parentDefinitionId");
                        isDraftTaskGroup = (j != null);

                        // Skip definition.
                        if (isDraftTaskGroup)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"A draft version was found for task group: {taskGroupName}, it will be skipped.");
                            _mySource.Value.Flush();

                            // Increment counter of skipped entities.
                            skippedEntities++;

                            // Skip this definition.
                            continue;
                        }

                        // A task group definition can contain different versions of the same task group.
                        // Task uniqueness is based on task identifier + version spec.
                        JArray taskGroupVersionAsJArray = (JArray)taskGroupVersionAsJToken["value"];

                        // Highest version is found first.
                        // Get the name at initial version.
                        if (taskGroupVersionAsJArray.Count > 0)
                            taskGroupNameAtV1 = taskGroupVersionAsJArray[taskGroupVersionAsJArray.Count - 1]["name"].ToString();

                        // Current and previous node must be read at the same time...
                        for (int n = 0, m = 1; n < taskGroupVersionAsJArray.Count; n++, m++)
                        {
                            // Reset flag.
                            hasDependency = false;

                            // Increment the entities counter.
                            entitiesCounter++;

                            // Get task group name and identifier.
                            taskGroupId = taskGroupVersionAsJArray[n]["id"].ToString();
                            taskGroupName = taskGroupVersionAsJArray[n]["name"].ToString();

                            // Get major value from version property.
                            majorValue = taskGroupVersionAsJArray[n]["version"]["major"].ToObject<int>();

                            // Generate version spec.
                            taskGroupVersionSpecString = $"{majorValue}.*";

                            // Form a composition key which is made of task identifier and version spec.
                            string taskGroupKey = $"{taskGroupId},{taskGroupVersionSpecString}";

                            // Add a new mapping record to keep track of task group definition.
                            projectDef.AddMappingRecord("TaskGroup", taskGroupKey, taskGroupName);

                            // Skip if last item as it cannot be compared with next item.
                            if (n < taskGroupVersionAsJArray.Count - 1)
                            {
                                // Has at least one task group dependency.
                                hasDependency = true;

                                // Has it been renamed?.
                                // Create a new property about the original name of the task group.
                                if (taskGroupNameAtV1 != taskGroupName)
                                    taskGroupVersionAsJArray[n]["originalName"] = new JValue(taskGroupNameAtV1);

                                // Get task group name and identifier.
                                taskGroupId = taskGroupVersionAsJArray[m]["id"].ToString();
                                taskGroupName = taskGroupVersionAsJArray[m]["name"].ToString();

                                // Get major value from version property.
                                majorValue = taskGroupVersionAsJArray[m]["version"]["major"].ToObject<int>();

                                // Generate version spec.
                                string taskGroupPreviousVersionSpecString = $"{majorValue}.*";

                                // Form a composition key which is made of the same task identifier but previous version spec.
                                string taskGroupPreviousVersionKey = $"{taskGroupId},{taskGroupPreviousVersionSpecString}";

                                // Send some traces.
                                _mySource.Value.TraceInformation($"For task group {taskGroupName} with version spec: {taskGroupVersionSpecString}, add a dependency to same task group with version spec: {taskGroupPreviousVersionSpecString}");
                                _mySource.Value.Flush();

                                // Add dependency to graph.
                                graphOfTaskGroupKeys.AddDependency(taskGroupKey, taskGroupPreviousVersionKey);
                            }

                            foreach (JToken taskAsJToken in taskGroupVersionAsJArray[n]["tasks"])
                            {
                                // Care only about metatask composing this metatask.
                                if (taskAsJToken["task"]["definitionType"].ToString() == "metaTask")
                                {
                                    // Has at least one task group dependency.
                                    hasDependency = true;

                                    // Get task identifier (which is in fact a metatask or task group) and version spec.
                                    taskId = taskAsJToken["task"]["id"].ToString();
                                    taskVersionSpecString = taskAsJToken["task"]["versionSpec"].ToString();

                                    // Form a composition dependency key which is made of task identifier and version spec.
                                    string dependencyTaskGroupKey = $"{taskId},{taskVersionSpecString}";

                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"For task group {taskGroupName} with version spec: {taskGroupVersionSpecString}, add a dependency to task id: {taskId} with version spec: {taskVersionSpecString}");
                                    _mySource.Value.Flush();

                                    // Add dependency to graph.
                                    graphOfTaskGroupKeys.AddDependency(taskGroupKey, dependencyTaskGroupKey);
                                }
                            }

                            // When no dependency is found, add to independent list of task groups.
                            if (!hasDependency)
                                graphOfTaskGroupKeys.Add(taskGroupKey);

                            // Store definition for later usage.
                            taskGroupDefinitions.Add(taskGroupKey, taskGroupVersionAsJArray[n]);
                        }
                    }

                    // Remove edges from the graph without changing the reachability of nodes.
                    // It also improves search performance.
                    graphOfTaskGroupKeys.TransitiveReduce();

                    #endregion

                    #region - Export task groups which have no inter-dependencies first and dependencies after for a successful re-creation.

                    // Some properties must be removed as they are not needed during creation.
                    pathsToSelectForRemoval = new string[]
                    {
                        "$.id",
                        "$.definitionType",
                        "$.createdBy",
                        "$.createdOn",
                        "$.modifiedBy",
                        "$.modifiedOn",
                        "$.revision"
                    };

                    // Sort graph nodes to get the most dependent ones first.
                    sortedListOfTaskGroupKeys = Utility.GetSortedDependencyGraphNodes(graphOfTaskGroupKeys, true);

                    foreach (string key in sortedListOfTaskGroupKeys)
                    {
                        // Proceed only if the task has not been exported yet, otherwise it is just a dependency.
                        if (!abstractedIdentificationTaskGroups.ContainsKey(key))
                        {
                            // Increment the export identifier.
                            abstractedIdentifier++;

                            // Replace all task identifiers with tokens.
                            foreach (JToken taskAsJToken in taskGroupDefinitions[key]["tasks"])
                            {
                                if (taskAsJToken["task"]["definitionType"].ToString() == "metaTask")
                                {
                                    // Get task identifier (which is in fact a metatask or task group) and version spec.
                                    taskId = taskAsJToken["task"]["id"].ToString();
                                    taskVersionSpecString = taskAsJToken["task"]["versionSpec"].ToString();

                                    // Form a composition dependency key which is made of task identifier and version spec.
                                    string dependencyTaskGroupKey = $"{taskId},{taskVersionSpecString}";

                                    // Generate a token name for replacement at runtime. This token contains the abstracted task group identifier.
                                    tokenName = Engine.Utility.CreateTokenName($"TaskGroup{abstractedIdentificationTaskGroups[dependencyTaskGroupKey]}", ReplacementTokenValueType.Id);

                                    // Replace identifier with token name.
                                    taskAsJToken["task"]["id"] = Engine.Utility.CreateToken(tokenName);
                                }
                            }

                            // Curate the JToken.
                            curatedJToken = taskGroupDefinitions[key].RemoveSelectProperties(pathsToSelectForRemoval);

                            // Save data.
                            jsonContent = JsonConvert.SerializeObject(curatedJToken, Formatting.Indented);

                            // Define the export file.
                            filename = projectDef.GetFileFor("TaskGroup", new object[] { abstractedIdentifier });

                            // Write definition file.
                            projectDef.WriteDefinitionFile(filename, jsonContent);

                            // Add to exported task groups dictionary.
                            abstractedIdentificationTaskGroups.Add(key, abstractedIdentifier);
                        }
                    }

                    // Get the number of task groups exported.
                    exportedEntities = abstractedIdentificationTaskGroups.Count;

                    #endregion
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting task groups: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                //throw;
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} task groups were exported.", exportedEntities, exportedEntities);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Get project wikis and write them in definition files for later import.
        /// </summary>
        private void ExportWiki(ProjectDefinition projectDef)
        {
            // Initialize.
            int entitiesCounter = 0;
            int exportedEntities = 0;
            string filename;
            string jsonContent;
            JToken generalizedDef;
            JToken wikisAsJToken;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Export wikis.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                Wikis adorasWikis = new Wikis(projectDef.AdoRestApiConfigurations["WikiApi"]);

                // Get all wikis.
                wikisAsJToken = adorasWikis.GetWikisAsJToken();

                // Set how many entities or objects must be exported.
                entitiesCounter = wikisAsJToken["count"].ToObject<int>();

                // Generalize.
                generalizedDef = GeneralizeWikis(projectDef, wikisAsJToken);

                // Save data.
                jsonContent = JsonConvert.SerializeObject(generalizedDef, Formatting.Indented);

                // Define the export file.
                filename = projectDef.GetFileFor("Wikis");

                // Write definition file.
                projectDef.WriteDefinitionFile(filename, jsonContent);

                // All entities have been exported.
                exportedEntities = entitiesCounter;
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = string.Format("Error occured while exporting wikis: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0}/{1} wikis were exported.", exportedEntities, entitiesCounter);
            _mySource.Value.Flush();
        }

        private JToken GeneralizeBuildDefinition(ProjectDefinition projectDef, JToken definitionAsJToken, bool addSourceProjectNameToPath, JValue defaultAgentPoolName)
        {
            // Initialize.
            bool foundFlag;
            int itemCounter;
            int optionEnabledCounter = 0;
            int distinctEndpointCounter;
            int endpointCounter = 0;
            int variableGroupId;
            string definitionName;
            string filename;
            string githubPath;
            string githubPathHash;
            string gitPath;
            string gitPathHash;
            string jsonContent;
            string repositoryName;
            string repositoryType;
            string tokenName;
            string tokenValue;
            string valueAsString;
            JToken curatedDefinition;
            List<string> listOfPathsToSelectForRemoval;
            List<KeyValuePair<string, string>> tokensList;
            Dictionary<string, int> endpointsCountingTable = new Dictionary<string, int>();

            try
            {
                // Extract definition name.
                definitionName = definitionAsJToken["name"].ToString();

                // Some properties must be removed as they are not needed during creation of build definition.
                // Other properites can be added based on definition.
                listOfPathsToSelectForRemoval = new List<string>
                {
                    "$._links",
                    "$.authoredBy",
                    "$.createdDate",
                    "$.drafts",
                    "$.id",
                    "$.project",
                    "$.queue.id",
                    "$.queue.pool.id",
                    "$.queue.url",
                    "$.queue._links",
                    "$.queueStatus",
                    "$.uri",
                    "$.url"
                };

                #region - Handle root and option properties.

                // Add source project name to relative root path if needed.
                if (addSourceProjectNameToPath)
                {
                    string relativeRootPath = @"\" + _engineConfig.BuildReleasePrefixPath + @"\";
                    definitionAsJToken["path"] = Engine.Utility.CombineRelativePath(relativeRootPath, definitionAsJToken["path"].ToString());
                }

                // Browse options if any.
                if (definitionAsJToken["options"] != null)
                {
                    // Reset counter.
                    itemCounter = -1;

                    // Instantiate a new list of paths to add for removal.
                    var jpqs = new List<string>();

                    if (((JArray)definitionAsJToken["options"]).Count > 0)
                    {
                        // Set the options to drop.
                        foreach (JToken optionAsJToken in (JArray)definitionAsJToken["options"])
                        {
                            // Is it enabled?
                            if (optionAsJToken["enabled"].ToObject<bool>())
                                optionEnabledCounter++;
                            else
                                jpqs.Add($"$.options[{++itemCounter}]");
                        }

                        // If no options are enabled, drop the property completely.
                        if (optionEnabledCounter == 0)
                            jpqs = new List<string>() { "$.options" };

                        // Concatenate these items to list for removal.
                        listOfPathsToSelectForRemoval.AddRange(jpqs);
                    }
                }

                #endregion

                #region - Handle repository property.

                // Extract repository type.
                repositoryType = definitionAsJToken["repository"]["type"].ToString().ToLower();

                if (repositoryType == "github")
                {
                    // todo: ask why Github connection should be converted to Git connection. What is the advantage?

                    if (string.IsNullOrEmpty(definitionAsJToken["repository"]["url"].ToString()))
                        throw (new RecoverableException($"{definitionName} build definition: Cannot convert GitHub connection to Git as no url is defined"));

                    // Form a GitHub path with url.
                    githubPath = definitionAsJToken["url"].ToString();

                    // Define definition filename.
                    githubPathHash = Engine.Utility.GenerateMD5Hash(githubPath);

                    // Form an arbitrary repository name to prevent collision.
                    repositoryName = $"GitHub-{githubPathHash}";

                    // Read the content of definition file.
                    jsonContent = projectDef.ReadTemplate("GitServiceEndpoint");

                    #region - Replace all generalization tokens.

                    // Define all tokens to replace from the preset file.
                    // Tokens must be created in order of resolution, so this is why
                    // a list of key/value pairs is used instead of a dictionary.
                    tokensList = new List<KeyValuePair<string, string>>();

                    // Add token and replacement value to list.
                    tokenName = Engine.Utility.CreateTokenName("Name", "Endpoint");
                    tokenValue = repositoryName;
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    tokenName = Engine.Utility.CreateTokenName("Url");
                    tokenValue = definitionAsJToken["repository"]["url"].ToString();
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    tokenName = Engine.Utility.CreateTokenName("Username");
                    tokenValue = Engine.Utility.CreateToken("GitUsername");
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    tokenName = Engine.Utility.CreateTokenName("Password");
                    tokenValue = Engine.Utility.CreateToken("GitPassword");
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    // Replace tokens.
                    jsonContent = Engine.Utility.ReplaceTokensInJsonContent(jsonContent, tokensList);

                    // Define filename.
                    filename = projectDef.GetFileFor("GitHubEndpoint", new object[] { githubPathHash });

                    #endregion

                    // Write definition file.
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Convert this connection into git.
                    definitionAsJToken["repository"]["type"] = "Git";
                    definitionAsJToken["repository"]["properties"]["fullName"] = "repository";

                    // Replace service endpoint identifier with replaceable token.
                    definitionAsJToken["repository"]["properties"]["connectedServiceId"] = Engine.Utility.CreateToken(repositoryName, "Endpoint", ReplacementTokenValueType.Id);

                    // Add other properties to remove.
                    listOfPathsToSelectForRemoval.Add("$.repository.apiUrl");
                    listOfPathsToSelectForRemoval.Add("$.repository.branchesUrl");
                    listOfPathsToSelectForRemoval.Add("$.repository.defaultBranch");
                    listOfPathsToSelectForRemoval.Add("$.repository.hasAdminPermissions");
                    listOfPathsToSelectForRemoval.Add("$.repository.isFork");
                    listOfPathsToSelectForRemoval.Add("$.repository.lastUpdated");
                    listOfPathsToSelectForRemoval.Add("$.repository.manageUrl");
                    listOfPathsToSelectForRemoval.Add("$.repository.nodeId");
                    listOfPathsToSelectForRemoval.Add("$.repository.ownerId");
                    listOfPathsToSelectForRemoval.Add("$.repository.orgName");
                    listOfPathsToSelectForRemoval.Add("$.repository.refsUrl");
                    listOfPathsToSelectForRemoval.Add("$.repository.safeRepository");
                    listOfPathsToSelectForRemoval.Add("$.repository.shortName");
                    listOfPathsToSelectForRemoval.Add("$.repository.ownerAvatarUrl");
                    listOfPathsToSelectForRemoval.Add("$.repository.archived");
                    listOfPathsToSelectForRemoval.Add("$.repository.externalId");
                    listOfPathsToSelectForRemoval.Add("$.repository.ownerIsAUser");
                }
                else if (repositoryType == "git")
                {
                    // Form a GitHub path with url.
                    gitPath = definitionAsJToken["url"].ToString();

                    // Define definition filename.
                    gitPathHash = Engine.Utility.GenerateMD5Hash(gitPath);

                    // Form an arbitrary repository name to prevent collision.
                    repositoryName = definitionAsJToken["repository"]["name"].ToString();

                    // Read the content of definition file.
                    jsonContent = projectDef.ReadTemplate("GitServiceEndpoint");

                    #region - Replace all generalization tokens.

                    // Define all tokens to replace from the preset file.
                    // Tokens must be created in order of resolution, so this is why
                    // a list of key/value pairs is used instead of a dictionary.
                    tokensList = new List<KeyValuePair<string, string>>();

                    // Add token and replacement value to list.
                    tokenName = Engine.Utility.CreateTokenName("EndpointName", "Endpoint");
                    tokenValue = repositoryName;
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    tokenName = Engine.Utility.CreateTokenName("Url");
                    tokenValue = definitionAsJToken["repository"]["url"].ToString();
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    tokenName = Engine.Utility.CreateTokenName("Username");
                    tokenValue = Engine.Utility.CreateToken("GitUsername");
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    tokenName = Engine.Utility.CreateTokenName("Password");
                    tokenValue = Engine.Utility.CreateToken("GitPassword");
                    tokensList.Add(new KeyValuePair<string, string>(tokenName, tokenValue));

                    // Replace tokens.
                    jsonContent = Engine.Utility.ReplaceTokensInJsonContent(jsonContent, tokensList);

                    #endregion

                    // Define filename.
                    filename = projectDef.GetFileFor("GitEndpoint", new object[] { repositoryName, gitPathHash });

                    // Write definition file.
                    projectDef.WriteDefinitionFile(filename, jsonContent);

                    // Replace service endpoint identifier with replaceable token.
                    definitionAsJToken["repository"]["properties"]["connectedServiceId"] = Engine.Utility.CreateToken(repositoryName, "Endpoint", ReplacementTokenValueType.Id);
                }
                else if (repositoryType == "tfsgit")
                {
                    // Add other properties to remove.
                    listOfPathsToSelectForRemoval.Add("$.repository.url");
                    listOfPathsToSelectForRemoval.Add("$.repository.properties.connectedServiceId");

                    // Replace identifier (guid) with a replaceable token.
                    valueAsString = definitionAsJToken["repository"]["name"].ToString();
                    definitionAsJToken["repository"]["id"] = Engine.Utility.CreateToken(valueAsString, "Repository", ReplacementTokenValueType.Id);
                }

                #endregion

                #region - Handle queue property.

                if (definitionAsJToken["queue"] != null)
                {
                    // Set default queue and pool name to Hosted VS2017.
                    definitionAsJToken["queue"]["name"] = new JValue(defaultAgentPoolName);

                    // Proceed if pool is defined.
                    // Be careful as some definition might have the value for this set to null.
                    // Set pool name to use if defined.
                    if (definitionAsJToken["queue"]["pool"] != null)
                        if (definitionAsJToken["queue"]["pool"].Type != JTokenType.Null)
                            definitionAsJToken["queue"]["pool"]["name"] = new JValue(defaultAgentPoolName);
                }

                #endregion

                #region - Handle process property.

                if (definitionAsJToken["process"] != null)
                {
                    // Remove queue defined at phase level.
                    listOfPathsToSelectForRemoval.Add("$.process.phases[*].target.queue");

                    // Get each phase.
                    foreach (JToken phaseAsJToken in (JArray)definitionAsJToken["process"]["phases"])
                    {
                        if (phaseAsJToken["steps"] != null)
                            foreach (JToken stepAsJToken in (JArray)phaseAsJToken["steps"])
                                if (stepAsJToken["inputs"] != null)
                                    CountServiceEndpoint(stepAsJToken["inputs"], endpointsCountingTable);
                    }

                    // Calculate distinct and total endpoints.
                    distinctEndpointCounter = endpointsCountingTable.Count;
                    foreach (var item in endpointsCountingTable)
                        endpointCounter += item.Value;

                    // Send some traces.
                    if (distinctEndpointCounter == 1)
                        _mySource.Value.TraceInformation($"{definitionName} utilizes 1 service endpoint.");
                    else if (distinctEndpointCounter == 1)
                        _mySource.Value.TraceInformation($"{definitionName} utilizes {endpointCounter} service endpoints.");
                    else if (distinctEndpointCounter == endpointCounter)
                        _mySource.Value.TraceInformation($"{definitionName} utilizes {endpointCounter} different service endpoints.");
                    else
                        _mySource.Value.TraceInformation($"{definitionName} utilizes {distinctEndpointCounter} types of service endpoints for a total of {endpointCounter} service endpoints.");
                    _mySource.Value.Flush();
                }

                #endregion

                #region - Handle process parameters property.

                if (definitionAsJToken["processParameters"] != null)
                {
                    if (definitionAsJToken["processParameters"]["inputs"] != null)
                    {
                        // Reset default value of process parameters.
                        // todo: be more selective on what to reset.
                        foreach (JToken inputAsJToken in (JArray)definitionAsJToken["processParameters"]["inputs"])
                            inputAsJToken["defaultValue"] = Engine.Utility.CreateEmptyJValue();
                    }
                }

                #endregion

                #region - Handle variable groups property.

                if (definitionAsJToken["variableGroups"] != null)
                {
                    // Create new JArray to re-create variable groups.
                    JArray variableGroups = new JArray();

                    foreach (JToken variableGroupAsJToken in (JArray)definitionAsJToken["variableGroups"])
                    {
                        // Sometimes you get only the id of the variable group in use, and other times
                        // you get metadata and a copy of variable group definitions.
                        if (variableGroupAsJToken.HasValues)
                        {
                            // Replace identifier (int) with a replaceable token.
                            valueAsString = variableGroupAsJToken["name"].ToString();
                            variableGroupAsJToken["id"] = Engine.Utility.CreateToken(valueAsString, "VariableGroup", ReplacementTokenValueType.Id);
                        }
                        else
                        {
                            // Reset flag.
                            foundFlag = false;

                            // Read the identifier.
                            variableGroupId = variableGroupAsJToken.ToObject<int>();

                            foreach (var item in projectDef.AdoEntities.VariableGroups)
                            {
                                // .Key property stores the variable group name.
                                // .Value property stores the variable group identifier.
                                if (item.Value == variableGroupId)
                                {
                                    foundFlag = true;
                                    tokenValue = Engine.Utility.CreateToken(item.Key, "VariableGroup", ReplacementTokenValueType.Id);
                                    variableGroups.Add(new JValue(tokenValue));

                                    // Leave the search loop.
                                    break;
                                }
                            }

                            // If not found...
                            if (!foundFlag)
                                throw (new Exception($"Cannot retrieve variable group with identifier: {variableGroupId}"));
                        }
                    }

                    // Replace the list of variable group identifiers with tokens.
                    definitionAsJToken["variableGroups"] = variableGroups;
                }

                #endregion

                // Remove useless properties from definition.
                curatedDefinition = definitionAsJToken.RemoveSelectProperties(listOfPathsToSelectForRemoval.ToArray());
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = "Error occured while generalizing build definition: " + ex.Message;
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Return the generalized definition.
            return curatedDefinition;
        }

        /// <summary>
        /// Generalize a release definition so it can be imported to a destination collection/project.
        /// </summary>
        private JToken GeneralizeReleaseDefinition(
            ProjectDefinition projectDef,
            JToken definitionAsJToken,
            bool addSourceProjectNameToBuildPath,
            bool addSourceProjectNameToReleasePath,
            bool resetPipelineQueue)
        {
            // Initialize.
            JToken curatedDefinition;

            try
            {
                // Initialize.
                bool entityFound;
                bool foundFlag;
                int buildDefinitionId = 0;
                int queueId = 0;
                int variableGroupId = 0;
                string buildDefinitionNameWithPath = null;
                string containerRepositoryName;
                string projectName;
                string queueName;
                string registryUrl;
                string repositoryId;
                string repositoryName = null;
                string serviceEndpointName;
                string tokenValue;
                string valueAsString;
                string[] pathsToSelectForRemoval;
                JToken environmentsAsJToken;
                JToken deployPhasesAsJToken;
                JToken queueIdAsJToken;

                // Add source project name to relative root path if needed.
                if (addSourceProjectNameToReleasePath)
                {
                    string relativeRootPath = @"\" + _engineConfig.BuildReleasePrefixPath + @"\";
                    definitionAsJToken["path"] = Engine.Utility.CombineRelativePath(relativeRootPath, definitionAsJToken["path"].ToString());
                }

                #region - Handle variable groups property.

                if (definitionAsJToken["variableGroups"] != null)
                {
                    // Create new JArray to re-create variable groups.
                    JArray variableGroups = new JArray();

                    foreach (JToken variableGroupAsJToken in (JArray)definitionAsJToken["variableGroups"])
                    {
                        // Sometimes you get only the id of the variable group in use, and other times
                        // you get metadata and a copy of variable group definitions.
                        if (variableGroupAsJToken.HasValues)
                        {
                            // Replace identifier (int) with a replaceable token.
                            valueAsString = variableGroupAsJToken["name"].ToString();
                            variableGroupAsJToken["id"] = Engine.Utility.CreateToken(valueAsString, "VariableGroup", ReplacementTokenValueType.Id);
                        }
                        else
                        {
                            // Reset flag.
                            foundFlag = false;

                            // Read the identifier.
                            variableGroupId = variableGroupAsJToken.ToObject<int>();

                            foreach (var item in projectDef.AdoEntities.VariableGroups)
                            {
                                // .Key property stores the variable group name.
                                // .Value property stores the variable group identifier.
                                if (item.Value == variableGroupId)
                                {
                                    foundFlag = true;
                                    tokenValue = Engine.Utility.CreateToken(item.Key, "VariableGroup", ReplacementTokenValueType.Id);
                                    variableGroups.Add(new JValue(tokenValue));

                                    // Leave the search loop.
                                    break;
                                }
                            }

                            // If not found...
                            if (!foundFlag)
                                throw (new Exception($"Cannot retrieve variable group with identifier: {variableGroupId}"));
                        }
                    }

                    // Replace the list of variable group identifiers with tokens.
                    definitionAsJToken["variableGroups"] = variableGroups;
                }

                #endregion

                // Extract Environments section.
                environmentsAsJToken = definitionAsJToken["environments"];
                if (environmentsAsJToken.HasValues)
                {
                    foreach (JToken environmentAsJToken in environmentsAsJToken)
                    {
                        // Replace owner properties with tokens for replacement at runtime.
                        environmentAsJToken["owner"]["id"] = Engine.Utility.CreateToken("Owner", ReplacementTokenValueType.Id);
                        environmentAsJToken["owner"]["displayName"] = Engine.Utility.CreateToken("Owner", ReplacementTokenValueType.DisplayName);
                        environmentAsJToken["owner"]["uniqueName"] = Engine.Utility.CreateToken("Owner", ReplacementTokenValueType.UniqueName);

                        #region - Inspect phases.

                        deployPhasesAsJToken = environmentAsJToken["deployPhases"];

                        if (deployPhasesAsJToken.HasValues)
                        {
                            foreach (JToken phaseAsJToken in deployPhasesAsJToken)
                            {
                                // Reset flag.
                                entityFound = false;

                                // Reset queue name to use.
                                queueName = null;

                                #region - Inspect queues.

                                if (resetPipelineQueue)
                                    phaseAsJToken["deploymentInput"]["queueId"] = Engine.Utility.CreateEmptyJValue();
                                else
                                {
                                    queueIdAsJToken = phaseAsJToken["deploymentInput"]["queueId"];

                                    if (queueIdAsJToken != null)
                                    {
                                        if (projectDef.AdoEntities.Queues.Count > 0)
                                        {
                                            // Extract queue identifier.
                                            queueId = Convert.ToInt32(queueIdAsJToken.ToString());

                                            // Retrieve agent queue name.
                                            foreach (var item in projectDef.AdoEntities.Queues)
                                            {
                                                // .Key property stores the agent queue name.
                                                // .Value property stores the agent queue identifier.
                                                if (item.Value == queueId)
                                                {
                                                    entityFound = true;
                                                    queueName = item.Key;
                                                    break;
                                                }
                                            }

                                            // Was it found?
                                            if (!entityFound)
                                                throw (new Exception($"Agent queue with identifier: {queueId} cannot be retrieved."));

                                            // Set token for replacement at runtime.
                                            phaseAsJToken["deploymentInput"]["queueId"] = Engine.Utility.CreateToken(queueName);
                                        }
                                        else
                                            phaseAsJToken["deploymentInput"]["queueId"] = Engine.Utility.CreateEmptyJValue();
                                    }
                                    else
                                        phaseAsJToken["deploymentInput"]["queueId"] = Engine.Utility.CreateEmptyJValue();
                                }

                                #endregion
                            }
                        }

                        #endregion
                    }
                }

                // Extract Artifacts section.
                JToken artifactsAsJToken = definitionAsJToken["artifacts"];

                // Proceed only if there are values.
                if (artifactsAsJToken.HasValues)
                {
                    #region - Inspect artifacts.

                    foreach (JToken artifactAsJToken in artifactsAsJToken)
                    {
                        // todo: this breaks when artifact type is not Build or Git.
                        // Charlston would find all artifact types we are using.
                        // ignore everything else for now.
                        if (artifactAsJToken["type"].ToString() == "Git")
                        {
                            // The schema of an artifact object is the following when it is Git artifact:
                            // "sourceId": "<Project identifier>:<Repository identifier>",
                            // "type": "Git",
                            // "alias": "artifact alias",
                            // "definitionReference": {
                            //   "branches": {
                            //     "id": "branch name",
                            //     "name": "branch name"
                            //   },
                            //   "checkoutNestedSubmodules": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "checkoutSubmodules": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "defaultVersionSpecific": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "defaultVersionType": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "definition": {
                            //     "id": "Repository identifier",
                            //     "name": "Repository name"
                            //   },
                            //   "fetchDepth": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "gitLfsSupport": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "project": {
                            //     "id": "Project identifier",
                            //     "name": "Project name"
                            //   }
                            // },
                            // "isPrimary": true or false
                            // "isRetained": true or false

                            // Extract the project name.
                            projectName = artifactAsJToken["definitionReference"]["project"]["name"].ToString();

                            // Extract repository id.
                            repositoryId = artifactAsJToken["definitionReference"]["definition"]["id"].ToString();

                            // Is this Git repository within this project or another one?
                            if (artifactAsJToken["definitionReference"]["project"]["id"].ToString() == projectDef.SourceProject.Id)
                            {
                                // Reset flag.
                                entityFound = false;

                                // Retrieve repository name.
                                foreach (var item in projectDef.AdoEntities.Repositories)
                                {
                                    // .Key property stores the repository name.
                                    // .Value property stores the repository identifier.
                                    if (item.Value == repositoryId)
                                    {
                                        entityFound = true;
                                        repositoryName = item.Key;
                                        break;
                                    }
                                }

                                // Was it found?
                                if (!entityFound)
                                    throw (new Exception($"Repository with identifier: {repositoryId} cannot be retrieved. Repository might have been deleted."));
                            }
                            else
                            {
                                // Create an Azure DevOps REST api configuration and service object for this other project.
                                RestAPI.Configuration adorasRepositoriesConfig = new RestAPI.Configuration(ServiceHost.DefaultHost, projectDef.AdoRestApiConfigurations["GitRepositoriesApi"])
                                {
                                    Project = projectName
                                };
                                Repositories adorasRepositories = new Repositories(adorasRepositoriesConfig);

                                // Retrieve repository name.
                                var r = adorasRepositories.GetRepositoryById(repositoryId);

                                // Was it found?
                                if (r == null)
                                    throw (new Exception($"Repository with identifier: {repositoryId} from project: {projectName} cannot be retrieved. Repository might have been deleted."));

                                // Extract repository name.
                                repositoryName = r.Name;
                            }

                            // Change definition identifier.
                            // Use Git repository name returned by query instead of the property from definitionReference\definition\name.
                            artifactAsJToken["definitionReference"]["definition"]["id"] = Engine.Utility.CreateToken(repositoryName, "Repository", ReplacementTokenValueType.Id);

                            // Reform the source identifier.
                            artifactAsJToken["sourceId"] = Engine.Utility.CreateToken("Project", ReplacementTokenValueType.Id) + @":" + Engine.Utility.CreateToken(repositoryName, "Repository", ReplacementTokenValueType.Id);

                            // Change project name and identifier if needed.
                            if (!string.IsNullOrEmpty(projectName))
                            {
                                // Replace project identifier with token for replacement at runtime.
                                artifactAsJToken["definitionReference"]["project"]["id"] = Engine.Utility.CreateToken("Project", ReplacementTokenValueType.Id);

                                // Replace project name with token for replacement at runtime.
                                artifactAsJToken["definitionReference"]["project"]["name"] = Engine.Utility.CreateToken("Project");
                            }
                        }
                        else if (artifactAsJToken["type"].ToString() == "Build")
                        {
                            // The schema of an artifact object is the following when it is Build artifact:
                            // "sourceId": "<Project identifier>:<Build definition identifier>",
                            // "type": "Build",
                            // "alias": "artifact alias",
                            // "definitionReference": {
                            //   "artifactSourceDefinitionUrl": {
                            //     "id": "Url",
                            //     "name": ""
                            //   },
                            //   "defaultVersionBranch": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "defaultVersionSpecific": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "defaultVersionTags": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "defaultVersionType": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "definition": {
                            //     "id": "Build definition identifier",
                            //     "name": "Build definition name, sometimes this value is wrong. Query the definition name instead."
                            //   },
                            //   "definitions": {
                            //     "id": "??? we don't use this at any time",
                            //     "name": "??? we don't use this at any time"
                            //   },
                            //   "IsMultiDefinitionType": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "project": {
                            //     "id": "Project identifier",
                            //     "name": "Project name"
                            //   }
                            //   "repository": {
                            //     "id": "Repository identifier",
                            //     "name": "Repository name"
                            //   }
                            // },
                            // "isPrimary": true or false
                            // "isRetained": true or false

                            // Extract the project name.
                            projectName = artifactAsJToken["definitionReference"]["project"]["name"].ToString();

                            // Extract build definition id.
                            buildDefinitionId = artifactAsJToken["definitionReference"]["definition"]["id"].ToObject<int>();

                            // Is this build definition within this project or another one?
                            if (artifactAsJToken["definitionReference"]["project"]["id"].ToString() == projectDef.SourceProject.Id)
                            {
                                // Reset flag.
                                entityFound = false;

                                // Retrieve build definition name from all ADO entities created.
                                foreach (var item in projectDef.AdoEntities.BuildDefinitions)
                                {
                                    // .Key property stores the definition name with its path.
                                    // .Value property stores ADO entity reference object.
                                    // Extract the reference object.
                                    ProjectDefinition.AdoEntityReference.IdBasedReferenceWithPath adoer = item.Value;

                                    // If the build definition identifier has been found.
                                    if (adoer.Identifier == buildDefinitionId)
                                    {
                                        entityFound = true;

                                        // Add the source project name to relative path and name if needed.
                                        if (addSourceProjectNameToBuildPath)
                                            buildDefinitionNameWithPath = Utility.CombineRelativePath(_engineConfig.BuildReleasePrefixPath, adoer.RelativePath);
                                        else
                                            buildDefinitionNameWithPath = adoer.RelativePath;

                                        break;
                                    }
                                }

                                // Was it found?
                                if (!entityFound)
                                    throw (new RecoverableException($"Build definition with identifier: {buildDefinitionId} cannot be retrieved. Build definition might have been deleted."));
                            }
                            else
                            {
                                // Create an Azure DevOps REST api configuration and service object for this other project.
                                RestAPI.Configuration adorasBuildConfig = new RestAPI.Configuration(ServiceHost.DefaultHost, projectDef.AdoRestApiConfigurations["BuildApi"])
                                {
                                    Project = projectName
                                };
                                BuildDefinition adorasBuild = new BuildDefinition(adorasBuildConfig);

                                // Retrieve all build definitions.
                                BuildMinimalResponse.BuildDefinitionReference bd = adorasBuild.GetBuildDefinition(buildDefinitionId);

                                // Was it found?
                                if (bd == null)
                                    throw (new RecoverableException($"Build definition with identifier: {buildDefinitionId} from project: {projectName} cannot be retrieved. Build definition might have been deleted."));

                                // Extract build definition name and its path.
                                buildDefinitionNameWithPath = Utility.CombineRelativePath(bd.Path, bd.Name);

                                // Add the source project name to relative path and name if needed.
                                if (addSourceProjectNameToBuildPath)
                                    buildDefinitionNameWithPath = Utility.CombineRelativePath(_engineConfig.BuildReleasePrefixPath, buildDefinitionNameWithPath);
                            }

                            // Replace build definition identifier with token for replacement at runtime.
                            artifactAsJToken["definitionReference"]["definition"]["id"] = Engine.Utility.CreateToken(buildDefinitionNameWithPath, "BuildDefinition", ReplacementTokenValueType.Id);

                            // Reform the source identifier.
                            artifactAsJToken["sourceId"] = Engine.Utility.CreateToken("Project", ReplacementTokenValueType.Id) + @":" + Engine.Utility.CreateToken(buildDefinitionNameWithPath, "BuildDefinition", ReplacementTokenValueType.Id);

                            // Change project name and identifier if needed.
                            if (!string.IsNullOrEmpty(projectName))
                            {
                                // Replace project identifier with token for replacement at runtime.
                                artifactAsJToken["definitionReference"]["project"]["id"] = Engine.Utility.CreateToken("Project", ReplacementTokenValueType.Id);

                                // Replace project name with token for replacement at runtime.
                                artifactAsJToken["definitionReference"]["project"]["name"] = Engine.Utility.CreateToken("Project");
                            }

                            // Proceed only if the property from definitionReference\definition exists.
                            if (artifactAsJToken["definitionReference"]["repository"] != null)
                            {
                                // Extract the repository name.
                                repositoryName = artifactAsJToken["definitionReference"]["repository"]["name"].ToString();

                                // Replace project name with token for replacement at runtime.
                                if (!string.IsNullOrEmpty(repositoryName))
                                    artifactAsJToken["definitionReference"]["repository"]["id"] = Engine.Utility.CreateToken(repositoryName, "Repository", ReplacementTokenValueType.Id);
                            }
                        }
                        else if (artifactAsJToken["type"].ToString() == "ExternalTfsBuild")
                        {
                            // The schema of an artifact object is the following when it is an ExternalTfsBuild artifact:
                            // "sourceId": "<Account/collection identifier>:<Project identifier>:<Build identifier>",
                            // "type": "ExternalTfsBuild",
                            // "alias": "artifact alias",
                            // "definitionReference": {
                            //   "connection": {
                            //     "id": "Account/collection identifier",
                            //     "name": "Account/collection name"
                            //   },
                            //   "defaultVersionType": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "definition": {
                            //     "id": "Build identifier",
                            //     "name": "Build name"
                            //   },
                            //   "project": {
                            //     "id": "Project identifier",
                            //     "name": "Project name"
                            //   }
                            // },
                            // "isRetained": true or false
                            throw (new RecoverableException($"This artifact type: {artifactAsJToken["type"].ToString()} is not meant to be migrated as it will be merged in the same collection."));
                        }
                        else if (artifactAsJToken["type"].ToString() == "AzureContainerRepository")
                        {
                            // The schema of an artifact object is the following when it is AzureContainerRepository artifact:
                            // "sourceId": "<Service endpoint identifier>:<Container registry url>:<Container repository identifier>",
                            // "type": "AzureContainerRepository",
                            // "alias": "artifact alias",
                            // "definitionReference": {
                            //   "connection": {
                            //     "id": "Service endpoint identifier",
                            //     "name": "Service endpoint name"
                            //   },
                            //   "defaultVersionType": {
                            //     "id": "Value",
                            //     "name": "Meaning"
                            //   },
                            //   "definition": {
                            //     "id": "Container repository identifier",
                            //     "name": "Container repository name"
                            //   },
                            //   "registryurl": {
                            //     "id": "Container registry url",
                            //     "name": "Container registry name"
                            //   },
                            //   "resourcegroup": {
                            //     "id": "Resource group id where the container registry exists",
                            //     "name": "Resource group name where the container registry exists"
                            //   }
                            // },
                            // "isPrimary": true or false
                            // "isRetained": true or false

                            // Change definition identifier.
                            serviceEndpointName = artifactAsJToken["definitionReference"]["connection"]["name"].ToString();
                            artifactAsJToken["definitionReference"]["connection"]["id"] = Engine.Utility.CreateToken(serviceEndpointName, "Endpoint", ReplacementTokenValueType.Id);

                            // Reform the source identifier.
                            registryUrl = artifactAsJToken["definitionReference"]["registryurl"]["id"].ToString();
                            containerRepositoryName = artifactAsJToken["definitionReference"]["definition"]["id"].ToString();
                            artifactAsJToken["sourceId"] = Engine.Utility.CreateToken(serviceEndpointName, "Endpoint", ReplacementTokenValueType.Id) + @":" + registryUrl + @":" + containerRepositoryName;
                        }
                    }

                    #endregion
                }

                // Some properties must be removed as they are not needed during creation of release definition.
                pathsToSelectForRemoval = new string[]
                {
                        "$.id",
                        "$._links",
                        "$.url",
                        "$.createdBy",
                        "$.createdOn",
                        "$.modifiedBy",
                        "$.modifiedOn",
                        "$.lastRelease",
                        "$.revision",
                        "$.environments[*].id",
                        "$.environments[*].badgeUrl",
                        "$.environments[*].currentRelease",
                        "$.environments[*].owner._links",
                        "$.environments[*].owner.url",
                        "$.environments[*].owner.imageUrl",
                        "$.environments[*].owner.descriptor",
                        "$.environments[*].preDeployApprovals.approvals[*].id",
                        "$.environments[*].preDeployApprovals.approvals[*].approver.url",
                        "$.environments[*].preDeployApprovals.approvals[*].approver._links",
                        "$.environments[*].preDeployApprovals.approvals[*].approver.imageUrl",
                        "$.environments[*].preDeployApprovals.approvals[*].approver.descriptor",
                        "$.environments[*].postDeployApprovals.approvals[*].id",
                        "$.environments[*].postDeployApprovals.approvals[*].approver.url",
                        "$.environments[*].postDeployApprovals.approvals[*].approver._links",
                        "$.environments[*].postDeployApprovals.approvals[*].approver.imageUrl",
                        "$.environments[*].postDeployApprovals.approvals[*].approver.descriptor",
                        "$.artifacts[*].definitionReference.artifactSourceDefinitionUrl"
                };
                curatedDefinition = definitionAsJToken.RemoveSelectProperties(pathsToSelectForRemoval);
            }
            catch (Exception ex)
            {
                // Send some traces.
                string errorMsg = "Error occured while generalizing release definition: " + ex.Message;
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();

                throw;
            }

            // Return the generalized definition.
            return curatedDefinition;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This parameter will eventually be used.")]
        private JToken GeneralizeWikis(ProjectDefinition projectDef, JToken wikisAsJToken)
        {
            // Initialize.
            bool entityFound;
            string repositoryId;
            string repositoryName = null;
            string[] pathsToSelectForRemoval;
            JToken curatedDefinition;

            // Some properties must be removed as they are not needed during creation of wiki.
            pathsToSelectForRemoval = new string[]
                {
                    "$.value[*].id",
                    "$.value[*].url",
                    "$.value[*].remoteUrl"
                };

            foreach (JToken wikiAsJToken in (JArray)wikisAsJToken["value"])
            {
                // Reset flag.
                entityFound = false;

                // Replace project identifier with token for replacement at runtime.
                wikiAsJToken["projectId"] = Engine.Utility.CreateToken("Project", ReplacementTokenValueType.Id);

                // Extract the repository identifier found.
                repositoryId = wikiAsJToken["repositoryId"].ToString();

                // Retrieve repository name.
                foreach (var item in projectDef.AdoEntities.Repositories)
                {
                    // .Key property stores the repository name.
                    // .Value property stores the repository identifier.
                    if (item.Value == repositoryId)
                    {
                        entityFound = true;
                        repositoryName = item.Key;
                        break;
                    }
                }

                // Was it found?
                if (!entityFound)
                    throw (new Exception($"Repository with identifier: {repositoryId} cannot be retrieved. Repository might have been deleted."));

                // Replace repository identifer with token for replacement at runtime.
                wikiAsJToken["repositoryId"] = Engine.Utility.CreateToken(repositoryName, "Repository", ReplacementTokenValueType.Id);
            }

            // Remove useless properties from definition.
            curatedDefinition = wikisAsJToken.RemoveSelectProperties(pathsToSelectForRemoval);

            // Return the generalized definition.
            return curatedDefinition;
        }

        #endregion

        #region - Public Members

        /// <summary>
        /// Export engine constructor.
        /// </summary>
        public ProjectExportEngine(EngineConfiguration config)
        {
            _engineConfig = config;

            // Generate the regex to validate if it is a valid service endpoint input property name.
            string expr = Engine.Utility.GenerateRegexForPossibleServiceEndpointInputPropertyNames(_engineConfig.CustomInputParameterNamesForServiceEndpoint);
            _validServiceEndpointInputPropertyNamesRegex = new Regex(expr, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Run the export engine.
        /// </summary>
        public void Run()
        {
            // Send some traces.
            _mySource.Value.TraceInformation("Load engine configuration");
            _mySource.Value.Flush();

            // Define the possible project to export from source collection.
            ProjectDefinition projectDef = new ProjectDefinition(_engineConfig.ExportPath, _engineConfig.SourceCollection, _engineConfig.SourceProject, _engineConfig.SourceProjectProcessName, _engineConfig.PAT, _engineConfig.TemplatesPath, _engineConfig.RestApiService);

            // Clean work folder.
            // projectDef.CleanWorkFolder();

            string mapLocation = _engineConfig.ProcessMapLocation;
            RestAPI.ProcessMapping.Maps maps = Maps.LoadFromJson(mapLocation);

            // Reset mappings.
            projectDef.ResetMappings();

            // Validate source project.
            Engine.Utility.ValidateProject(projectDef, OperationLocation.Source, true, false);
            if (!projectDef.SourceProject.Validated)
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Invalid collection, project or PAT, re-run the application and try again!");
                _mySource.Value.Flush();

                // Raise an exception.
                throw (new Exception("Source project has not been validated"));
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
            Engine.Utility.RetrieveTaskIdentifiers(projectDef, OperationLocation.Source);

            // Export project releated settings such as installed extensions, process template type, users, etc.
            ExportProjectSettings(projectDef);

            // Export organization processes
            if (_engineConfig.Behaviors.ExportOrganizationProcess)
                ExportOrganizationProcess(projectDef);

            // Export installed extensions.
            if (_engineConfig.Behaviors.ExportInstalledExtension)
                ExportInstalledExtension(projectDef);

            // Export teams configuration.
            if (_engineConfig.Behaviors.ExportTeamConfiguration)
                ExportTeamConfiguration(projectDef, maps);

            // Export iterations.
            if (_engineConfig.Behaviors.ExportIteration)
                ExportIteration(projectDef);

            // Export areas.
            if (_engineConfig.Behaviors.ExportArea)
                ExportArea(projectDef);

            // Export repositories.
            if (_engineConfig.Behaviors.ExportRepository)
                ExportRepository(projectDef);

            // Export policy configurations.
            if (_engineConfig.Behaviors.ExportPolicyConfiguration)
                ExportPolicyConfiguration(projectDef);

            // Export wikis.
            if (_engineConfig.Behaviors.ExportWiki)
                ExportWiki(projectDef);

            // Export pull requests.
            if (_engineConfig.Behaviors.ExportPullRequest)
                ExportPullRequest(projectDef);

            // Export service endpoints..
            if (_engineConfig.Behaviors.ExportServiceEndpoint)
                ExportServiceEndpoint(projectDef);

            // Export variable groups.
            if (_engineConfig.Behaviors.ExportVariableGroup)
                ExportVariableGroup(projectDef);

            // Export task groups.
            if (_engineConfig.Behaviors.ExportTaskGroup)
                ExportTaskGroup(projectDef);

            // Export agent queues.
            if (_engineConfig.Behaviors.ExportAgentQueue)
                ExportAgentQueue(projectDef);

            // Export build definitions.
            if (_engineConfig.Behaviors.ExportBuildDefinition)
                ExportBuildDefinition(projectDef, _engineConfig.Behaviors,
                    _engineConfig.IsOnPremiseMigration ? Constants.DefaultAgentPoolNameForOnPrem : Constants.DefaultAgentPoolNameForOnCloud);

            // Export release definitions.
            if (_engineConfig.Behaviors.ExportReleaseDefinition)
                ExportReleaseDefinition(projectDef, _engineConfig.Behaviors);
        }

        #endregion
    }
}