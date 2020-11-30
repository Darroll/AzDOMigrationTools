using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ADO.Extensions;
using ADO.RestAPI.Build;
using ADO.RestAPI.Core;
using ADO.RestAPI.DistributedTasks;
using ADO.RestAPI.Git;
using ADO.RestAPI.Graph;
using ADO.RestAPI.Security;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;

namespace ADO.Engine
{
    public static class Utility
    {
        #region - Static Declarations

        #region - Private Members

        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.Engine.Utility"));
        private static readonly string EnvironmentVariablePrefix = "Environment.";

        private static string CreateReplacementTokenInternal(string tokenExpression, string tokenType, ReplacementTokenValueType valueType, string tokenSeparator)
        {
            // Initialize.
            string token;
            string tokenTemplate;
            string tokenWithValueTypeTemplate;
            string tokenWithTokenTypeTemplate;
            string tokenWithTokenAndValueTypeTemplate;

            // Generate expressions.
            tokenTemplate = tokenSeparator + @"{0}" + tokenSeparator;
            tokenWithValueTypeTemplate = tokenSeparator + @"{0}-{1}" + tokenSeparator;
            tokenWithTokenTypeTemplate = tokenSeparator + @"{0}:{1}" + tokenSeparator;
            tokenWithTokenAndValueTypeTemplate = tokenSeparator + @"{0}:{1}-{2}" + tokenSeparator;

            // Form the token.
            if (string.IsNullOrEmpty(tokenType))
            {
                if (valueType == ReplacementTokenValueType.Name)
                    token = string.Format(tokenTemplate, tokenExpression);
                else
                    token = string.Format(tokenWithValueTypeTemplate, tokenExpression, valueType.ToString());
            }
            else
            {
                if (valueType == ReplacementTokenValueType.Name)
                    token = string.Format(tokenWithTokenTypeTemplate, tokenType, tokenExpression);
                else
                    token = string.Format(tokenWithTokenAndValueTypeTemplate, tokenType, tokenExpression, valueType.ToString());
            }

            // Return token.
            return token;
        }

        /// <summary>
        /// Topological Sorting (Kahn's algorithm) 
        /// </summary>
        /// <remarks>https://en.wikipedia.org/wiki/Topological_sorting</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodes">All nodes of directed acyclic graph.</param>
        /// <param name="edges">All edges of directed acyclic graph.</param>
        /// <returns>Sorted node in topological order.</returns>
        private static List<T> TopologicalSort<T>(HashSet<T> nodes, HashSet<Tuple<T, T>> edges) where T : IEquatable<T>
        {
            // Empty list that will contain the sorted elements.
            var l = new List<T>();

            // Set of all nodes with no incoming edges.
            var s = new HashSet<T>(nodes.Where(n => edges.All(e => e.Item2.Equals(n) == false)));

            // while s is non-empty do.
            while (s.Any())
            {
                //  remove a node n from s.
                var n = s.First();
                s.Remove(n);

                // add n to tail of l.
                l.Add(n);

                // for each node m with an edge e from n to m do.
                foreach (var e in edges.Where(e => e.Item1.Equals(n)).ToList())
                {
                    var m = e.Item2;

                    // remove edge e from the graph.
                    edges.Remove(e);

                    // if m has no other incoming edges then.
                    if (edges.All(me => me.Item2.Equals(m) == false))
                    {
                        // insert m into s.
                        s.Add(m);
                    }
                }
            }

            // if graph has edges then.
            if (edges.Any())
            {
                // return error (graph has at least one cycle).
                return null;
            }
            else
            {
                // return l (a topologically sorted order).
                return l;
            }
        }

        #endregion

        #region - Public Members.

        public static string CombineRelativePath(string path1, string path2)
        {
            // Initialize.
            string relativePath;
            bool hasStartingBackslash;
            bool hasTrailingBackslash;

            try
            {
                // Set relative path.
                // Root is defined as '\'.
                if (string.IsNullOrEmpty(path2))
                {
                    // Remove starting and trailing backslashes from path1.
                    hasTrailingBackslash = path1.EndsWith(@"\");
                    if (hasTrailingBackslash)
                        path1 = path1.Substring(0, path1.Length - 1);

                    hasStartingBackslash = path1.StartsWith(@"\");
                    if (hasStartingBackslash)
                        path1 = path1.Substring(1);

                    // Form relative path.
                    relativePath = @"\" + path1;
                }
                else if (path1 == @"\")
                {
                    // Remove starting and trailing backslashes from path2.
                    hasTrailingBackslash = path2.EndsWith(@"\");
                    if (hasTrailingBackslash)
                        path2 = path2.Substring(0, path2.Length - 1);

                    hasStartingBackslash = path2.StartsWith(@"\");
                    if (hasStartingBackslash)
                        path2 = path2.Substring(1);

                    // Form relative path, ignore path 1 as it is the root.
                    relativePath = path2;
                }
                else
                {
                    // Remove starting and trailing backslashes from path1.
                    hasTrailingBackslash = path1.EndsWith(@"\");
                    if (hasTrailingBackslash)
                        path1 = path1.Substring(0, path1.Length - 1);

                    hasStartingBackslash = path1.StartsWith(@"\");
                    if (hasStartingBackslash)
                        path1 = path1.Substring(1);

                    // Remove starting and trailing backslashes from path2.
                    hasTrailingBackslash = path2.EndsWith(@"\");
                    if (hasTrailingBackslash)
                        path2 = path2.Substring(0, path2.Length - 1);

                    hasStartingBackslash = path2.StartsWith(@"\");
                    if (hasStartingBackslash)
                        path2 = path2.Substring(1);

                    // Form relative path.
                    relativePath = @"\" + path1 + @"\" + path2;
                }
            }
            catch (Exception)
            {
                throw;
            }

            // Return combined relative path.
            return relativePath;
        }

        public static string ConvertToWebPath(string path)
        {
            try
            {
                path = path.Replace('\\', '/');
            }
            catch (Exception)
            {
                throw;
            }

            // Return transformed path.
            return path;
        }

        /// <summary>
        /// Return an empty JSON value.
        /// </summary>
        /// <returns>JToken</returns>
        public static JToken CreateEmptyJValue()
        {
            return new JValue(String.Empty);
        }

        /// <summary>
        /// Return token/placeholder using '$' separator that can be easily replaced with direct replacement or regular expression.
        /// </summary>
        /// <returns>string</returns>
        public static string CreateTokenName(string tokenName, ReplacementTokenValueType valueType = ReplacementTokenValueType.Name)
        {
            return (CreateReplacementTokenInternal(tokenName, null, valueType, null));
        }

        public static string CreateTokenName(string tokenName, string tokenType, ReplacementTokenValueType valueType = ReplacementTokenValueType.Name)
        {
            return (CreateReplacementTokenInternal(tokenName, tokenType, valueType, null));
        }

        /// <summary>
        /// Return token/placeholder using '$' separator that can be easily replaced with direct replacement or regular expression.
        /// </summary>
        /// <returns>string</returns>
        public static string CreateToken(string tokenName, ReplacementTokenValueType valueType = ReplacementTokenValueType.Name)
        {
            return (CreateReplacementTokenInternal(tokenName, null, valueType, @"$"));
        }

        /// <summary>
        /// Return token/placeholder using '$' separator and show type for easier replacement
        /// that can be easily replaced with direct replacement or regular expression.
        /// </summary>
        /// <returns>string</returns>
        public static string CreateToken(string tokenName, string tokenType, ReplacementTokenValueType valueType = ReplacementTokenValueType.Name)
        {
            return (CreateReplacementTokenInternal(tokenName, tokenType, valueType, @"$"));
        }

        /// <summary>
        /// Return token/placeholder using specific separator and show type for easier replacement
        /// that can be easily replaced with direct replacement or regular expression.
        /// </summary>
        /// <returns>string</returns>
        public static string CreateToken(string tokenName, string tokenType, string tokenSeparator, ReplacementTokenValueType valueType = ReplacementTokenValueType.Name)
        {
            return (CreateReplacementTokenInternal(tokenName, tokenType, valueType, tokenSeparator));
        }

        public static string ExtractSidFromIdentityDescriptor(string descriptor)
        {
            return descriptor.Split(new char[] { ';' }).Last();
        }

        public static List<string> GetAllPossibleServiceEndpointInputPropertyNames()
        {
            // Find some valuable information about service endpoint --> https://docs.microsoft.com/en-us/azure/devops/pipelines/library/service-endpoints?view=azure-devops&tabs=yaml#sep-azure-classic
            //
            // ACS DC/OS SSH service connection uses: acsDcosSshEndpoint
            // App Center service connection uses: serverEndpoint
            // AWS connection uses: awsCredentials
            // Azure Container Registry connection uses: azureContainerRegistry or AzureContainerRegistry
            // Azure Pipelines service connection uses: deploymentGroupEndpoint
            // Azure Classic Subscription connection uses: connectedServiceName, ConnectedServiceName or ConnectedServiceNameClassic
            // AzureRM connection uses: azureSubscription, azureSubscriptionEndpoint, azureResourceManagerEndpoint, connectedServiceName, ConnectedServiceName, connectedServiceNameARM or ConnectedServiceNameARM
            // Azure Service Bus service connection uses: connectedServiceName or ConnectedServiceName
            // Black Duck Hub Proxy service connection uses: BlackDuckHubProxyService
            // Black Duck Hub service connection uses: BlackDuckHubService
            // Black Duck Endpoint service connection uses: BlackDuckService
            // Chef subscirption service connection uses: connectedServiceName or ConnectedServiceName
            // Credential service connection uses: connectionName or ConnectionName
            // Docker Host service connection uses: dockerHostEndpoint
            // Docker Registry service connection uses: RegistryConnectedServiceName, dockerRegistryEndpoint or containerRegistry
            // External Maven repository service connection uses: mavenServiceConnections
            // External Npm repository service connection uses: customEndpoint
            // External Npm registry service connection uses: customEndpoint
            // External NuGet service connections uses: externalEndpoints or nuGetServiceConnections
            // External Python Upload Feed service connection uses: externalSources or pythonUploadServiceConnection
            // External Team Foundation Server/Team Services service connection uses: connection
            // External TFS service connection uses: connection, connectedServiceName, ConnectedServiceName, externalEndpoint or externalEndpoints
            // Generic service connection uses: connectedServiceName, ConnectedServiceName or serviceEndpoint
            // GitHub service connection uses: connection
            // GitHub (OAuth or PAT) service connection uses: gitHubConnection
            // Kubernetes service connection uses: kubernetesServiceEndpoint or kubernetesServiceConnection
            // FTP service connection uses: serverEndpoint
            // Jenkins service connection uses: serverEndpoint
            // NuGet service connections uses: connectedServiceName or ConnectedServiceName
            // NuGet Server service connections uses: externalEndpoint
            // Polaris Endpoint service connection uses: PolarisService
            // PyPI service connection uses: serviceEndpoint
            // Python Download Feed service connection uses: externalSources
            // SCVMM service connection uses: connectedServiceName or ConnectedServiceName
            // Service Fabric Cluster service connection uses: serviceConnectionName
            // Sonar Qube Generic service connection uses: sqConnectedServiceName
            // Sonar Qube Endpoint service connection uses: connectedServiceName or ConnectedServiceName
            // Sonar Qube Server Endpoint service connection uses: SonarQube
            // SSH service connection uses: sshEndpoint
            // Web hook service connection uses: serviceEndpointName
            //
            // ***************************************************************
            //
            // These properties: externalEndpoints or nuGetServiceConnections
            // Use a comma-separated list of service connection identifiers.

            // Initialize.
            List<string> listOfServiceEndpointInputPropertyNames = new List<string>
            {
                "AzureContainerRegistry",
                "BlackDuckHubProxyService",
                "BlackDuckHubService",
                "BlackDuckService",
                "ConnectedServiceName",
                "ConnectedServiceNameARM",
                "ConnectedServiceNameClassic",
                "ConnectionName",
                "PolarisService",
                "RegistryConnectedServiceName",
                "SonarQube",
                "acsDcosSshEndpoint",
                "awsCredentials",
                "azureContainerRegistry",
                "azureResourceManagerEndpoint",
                "azureSubscription",
                "azureSubscriptionEndpoint",
                "connectedServiceName",
                "connectedServiceNameARM",
                "connection",
                "connectionName",
                "containerRegistry",
                "customEndpoint",
                "deploymentGroupEndpoint",
                "dockerHostEndpoint",
                "dockerRegistryEndpoint",
                "externalEndpoint",
                "externalEndpoints",
                "externalSources",
                "gitHubConnection",
                "kubernetesServiceConnection",
                "kubernetesServiceEndpoint",
                "mavenServiceConnections",
                "nuGetServiceConnections",
                "pythonUploadServiceConnection",
                "serverEndpoint",
                "serviceConnectionName",
                "serviceEndpoint",
                "serviceEndpointName",
                "sqConnectedServiceName",
                "sshEndpoint"
            };

            // Return the list of all possible input names for a service endpoint.
            return listOfServiceEndpointInputPropertyNames;
        }

        public static string GenerateBuildToken(string projectId, int? buildId)
        {
            // Initialize.
            string token;

            // Form the token.
            if (buildId == null)
                token = projectId;
            else
                token = string.Format("{0}/{1}", projectId, buildId);

            // Return token.
            return token;
        }

        public static string GenerateMD5Hash(string value)
        {
            // Initialize.
            byte[] hashBuffer;
            StringBuilder sb = new StringBuilder();

            using (MD5 md5 = MD5.Create())
            {
                md5.Initialize();
                md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                hashBuffer = md5.Hash;
            }

            // Combine the buffer and form a hexadecimal string.
            for (int i = 0; i < hashBuffer.Length; i++)
                sb.Append(hashBuffer[i].ToString("X2"));

            // Return the hash;
            return sb.ToString();
        }

        public static string GenerateIdentityDescriptor(string sid, IdentityType identityType = IdentityType.MicrosoftTeamFoundationIdentity)
        {
            // Initialize.
            string descriptor;

            // An identity descriptor is formed like: <identity>;<sid>
            descriptor = string.Format("{0};{1}", EnumExtensions.GetDescription(identityType), sid);            

            // Return identity descriptor newly formed.
            return descriptor;
        }

        public static string GenerateProjectToken(string projectId)
        {
            return $"$PROJECT:vstfs:///Classification/TeamProject/{projectId}";
        }

        public static string GenerateRegexForPossibleServiceEndpointInputPropertyNames(List<string> customInputParameterNames)
        {
            // Initialize.
            int i = 0;
            string expr1 = string.Empty;
            string expr2;
            Dictionary<string, int> d = new Dictionary<string, int>();

            // Get distinct property names discarding case sensitivity.
            foreach (string propertyName in Utility.GetAllPossibleServiceEndpointInputPropertyNames())
                if (!d.ContainsKey(propertyName.ToLower()))
                    d.Add(propertyName.ToLower(), 1);

            // If custom parameter names have been added, add to list.
            if (customInputParameterNames != null)
                foreach (string propertyName in customInputParameterNames)
                    if (!d.ContainsKey(propertyName.ToLower()))
                        d.Add(propertyName.ToLower(), 1);

            // Generate the regular expression to find property names faster.
            foreach (var item in d)
            {
                // Increment the counter.
                i++;

                if (i == 1)
                    expr1 = item.Key;
                else
                    expr1 += @"|" + item.Key;
            }

            // Add start and end of line to all expressions.
            expr2 = @"^(" + expr1 + @")$";

            // Return expression.
            return expr2;
        }

        public static string GenerateRepositoryV2Token(string projectId, string repoId, string branchId)
        {
            // https://devblogs.microsoft.com/devops/git-repo-tokens-for-the-security-service/
            // refs/heads/6d0061007300740065007200
            // branch – this is “master”
            if (string.IsNullOrEmpty(projectId))
                return "repoV2";
            else if (string.IsNullOrEmpty(repoId))
                return $"repoV2/{projectId}";
            else if (string.IsNullOrEmpty(branchId))
                return $"repoV2/{projectId}/{repoId}";
            else
                return $"repoV2/{projectId}/{repoId}/{branchId}";
        }

        public static string GenerateSecureFileToken(string secureFileId)
        {
            return $"Library/{secureFileId}";
        }

        public static string GenerateTaskGroupToken(string projectId, string taskGroupId)
        {
            string token = $"{projectId}/{taskGroupId}";
            return token;
        }
        public static string GenerateVariableGroupToken(string projectId, int variableGroupId)
        {
            // Initialize.
            string token;

            // Form the token.
            token = string.Format("Library/{0}/VariableGroup/{1}", projectId, variableGroupId);

            // Return token.
            return token;
        }

        public static string GenerateReleaseToken(string projectId, int? releaseId)
        {
            // Initialize.
            string token;

            // Form the token.
            if (releaseId == null)
                token = projectId;
            else
                token = string.Format("{0}/{1}", projectId, releaseId);

            // Return token.
            return token;
        }

        public static string GeneratePipelineFolderToken(string projectId, string path = null)
        {
            // Initialize.
            string token;

            // Form the token.
            if (string.IsNullOrEmpty(path))
                token = projectId;
            else
                token = string.Format("{0}/{1}", projectId, path);

            // Return token.
            return token;
        }

        public static void GetMembersOfDefaultTeam(ProjectDefinition projectDef, OperationLocation location)
        {
            // Initialize.
            string name = null;

            // Send some traces.
            if (location == OperationLocation.Source)
                _mySource.Value.TraceInformation("Retrieve default user and default security principals from project: {0}", projectDef.SourceProject.Name);
            else if (location == OperationLocation.Destination)
                _mySource.Value.TraceInformation("Retrieve default user and default security principals from project: {0}", projectDef.DestinationProject.Name);
            _mySource.Value.Flush();

            // Create an Azure DevOps REST api service object.
            Teams adorasTeams = new Teams(projectDef.AdoRestApiConfigurations["TeamsApi"]);

            // Get members.
            // todo: add some logic to choose which user to use for some circumstances.
            if (location == OperationLocation.Source)
                name = projectDef.SourceProject.Name + " team";
            else if (location == OperationLocation.Destination)
                name = projectDef.DestinationProject.Name + " team";
            projectDef.DefaultTeamMembers = adorasTeams.GetTeamMembers(name);

            // Assign default user sid.
            projectDef.DefaultUser = projectDef.DefaultTeamMembers.Value.FirstOrDefault();
        }

        /// <summary>
        /// Verify if descriptor is a security identifier (SID).
        /// </summary>
        /// <returns>bool</returns>
        public static bool IsSid(string descriptor)
        {
            // https://docs.microsoft.com/en-us/windows/win32/secauthz/security-identifiers
            return descriptor.StartsWith("S-");
        }

        public static string ReplaceTokensInJsonContent(string jsonContent, IEnumerable<KeyValuePair<string, string>> tokensToReplace, string tokenSeparator = @"$")
        {
            // Initialize.
            string token;
            string tokenTemplate = tokenSeparator + "{0}" + tokenSeparator;

            // Replace each token with its respective value.
            foreach (var item in tokensToReplace)
            {
                // Define token to search.
                token = string.Format(tokenTemplate, item.Key);

                // Any backslash in JSON is doubled.
                token = token.Replace(@"\", @"\\");

                // Replace it.
                jsonContent = jsonContent.Replace(token, item.Value);
            }

            // Return json content replaced of all tokens.
            return jsonContent;
        }

        public static void RetrieveAgentQueueIdentifiers(ProjectDefinition projectDef)
        {
            // Initialize.
            Dictionary<string, int> queuesFound;

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Retrieve agent queues.");
                _mySource.Value.Flush();

                // Create an Azure DevOps REST api service object.
                Queue adorasQueue = new Queue(projectDef.AdoRestApiConfigurations["DistributedTaskApi"]);

                // Store queues found.
                queuesFound = adorasQueue.GetQueuesDictionary();

                // Store agent queues information.
                if (queuesFound.Count > 0)
                    foreach (var item in queuesFound)
                        projectDef.AdoEntities.AgentQueues.Add(item.Key, item.Value);
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
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();

                throw;
            }
        }

        public static void RetrieveBuildDefinitionIdentifiers(ProjectDefinition projectDef)
        {
            // Create Azure DevOps REST api service object.
            BuildDefinition adorasBuild = new BuildDefinition(projectDef.AdoRestApiConfigurations["BuildApi"]);

            // Retrieve all build definitions.
            BuildMinimalResponse.BuildDefinitionReferences allBuildDefinitions = adorasBuild.GetAllBuildDefinitions();

            // Store reference on ADO entity.
            foreach (BuildMinimalResponse.BuildDefinitionReference bd in allBuildDefinitions.Value)
            {
                // Instantiate a reference
                ProjectDefinition.AdoEntityReference.IdBasedReferenceWithPath adoer = new ProjectDefinition.AdoEntityReference.IdBasedReferenceWithPath
                    {
                        Identifier = bd.Id,
                        Name = bd.Name,
                        Path = bd.Path
                    };

                // Store the ADO entity reference.
                if (!projectDef.AdoEntities.BuildDefinitions.ContainsKey(adoer.RelativePath))
                    projectDef.AdoEntities.BuildDefinitions.Add(adoer.RelativePath, adoer);
            }
        }

        public static void RetrieveDefaultSecurityPrincipal(ProjectDefinition projectDef)
        {
            // Initialize.
            string sid;

            // List of security principals that are used.
            List<string> principalNameList = new List<string>() { "Contributors", "Administrators", "ValidUsers", "EndpointCreators", "DefaultTeam", "Readers" };
            GraphResponse.GraphGroup group;

            try
            {
                // Create Azure DevOps REST api service object.
                Graph adorasGraph = new Graph(projectDef.AdoRestApiConfigurations["GraphApi"]);

                // Browse the list of all security principals that we will be utilized.
                foreach (string principalName in principalNameList)
                {
                    if (!projectDef.SecurityPrincipalCache.ContainsKey(principalName))
                    {
                        // Fetch graph group.
                        group = adorasGraph.GetGroupByPrincipalName(projectDef.ResolveShortPrincipalName(principalName));
                        if (group != null)
                        {
                            // Cache the group.
                            projectDef.SecurityPrincipalCache.Add(principalName, group);

                            // Send some traces.
                            _mySource.Value.TraceInformation("Cache graph group: {0}", projectDef.SecurityPrincipalCache[principalName].DisplayName);

                            // Retrieve sid from security principal name and cache it.
                            sid = adorasGraph.GetGroupSid(projectDef.SecurityPrincipalCache[principalName]);
                            projectDef.SidCache.Add(principalName, sid);

                            // Send some traces.
                            _mySource.Value.TraceInformation("Cache corresponding SID: {0}", projectDef.SidCache[principalName]);
                            _mySource.Value.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Error while queueing build: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }
        }

        public static void RetrieveRepositoryIdentifiers(ProjectDefinition projectDef)
        {
            // Create Azure DevOps REST api service object.
            Repositories adorasRepositories = new Repositories(projectDef.AdoRestApiConfigurations["GitRepositoriesApi"]);

            // Get all repositories.
            GitMinimalResponse.GitRepositories repositories = adorasRepositories.GetAllRepositories();

            // Store repositories information.
            foreach (var repository in repositories.Value)
                if (!projectDef.AdoEntities.Repositories.ContainsKey(repository.Name))
                    projectDef.AdoEntities.Repositories.Add(repository.Name, repository.Id);
        }

        public static void RetrieveVariableGroupIdentifiers(ProjectDefinition projectDef)
        {
            // Create Azure DevOps REST api service object.
            VariableGroups adorasVariableGroups = new VariableGroups(projectDef.AdoRestApiConfigurations["TaskAgentApi"]);

            // Get all variable groups.
            TaskAgentResponse.VariableGroups variableGroups = adorasVariableGroups.GetAllVariableGroups();

            // Store variable groups information.
            foreach (var variableGroup in variableGroups.Value)
                if (!projectDef.AdoEntities.VariableGroups.ContainsKey(variableGroup.Name))
                    projectDef.AdoEntities.VariableGroups.Add(variableGroup.Name, variableGroup.Id);
        }

        public static void RetrieveTaskIdentifiers(ProjectDefinition projectDef, OperationLocation location)
        {
            // Initialize.
            string keyById;
            string keyByName;
            string filename = null;
            string jsonContent;
            string versionSpec;
            string version;
            ProjectDefinition.TaskVersionReference tvr;
            ProjectDefinition.TaskReference tr;
            DistributedTaskMinimalResponse.Tasks allPipelineTasks;

            // Create Azure DevOps REST api service object.
            Tasks adorasTasks = new Tasks(projectDef.AdoRestApiConfigurations["DistributedTaskApi"]);

            // Get all pipeline tasks.
            allPipelineTasks = adorasTasks.GetPipelineTasks();

            // Write temporarily the content of pull request.
            jsonContent = JsonConvert.SerializeObject(allPipelineTasks, Formatting.Indented);
            if(location == OperationLocation.Source)
                filename = projectDef.GetFileFor("Work.Source.Tasks");
            else if (location == OperationLocation.Destination)
                filename = projectDef.GetFileFor("Work.Destination.Tasks");
            projectDef.WriteDefinitionFile(filename, jsonContent);

            // Store tasks related information.
            foreach (var task in allPipelineTasks.Value)
            {
                // Generate the semantic version and version spec.
                version = $"{task.Version.Major}.{task.Version.Minor}.{task.Version.Patch}";
                versionSpec = $"{task.Version.Major}.*";

                // Generate absolute task key.
                keyById = $"{task.Id},{versionSpec}";
                keyByName = $"{task.Name},{versionSpec}";

                // Multiple versions of the same tasks may exist.
                // They will have all the same identifier.
                if (!projectDef.AdoEntities.TasksByNameVersionSpec.ContainsKey(keyByName))
                {
                    // Instantiate a new task version reference.
                    tvr = new ProjectDefinition.TaskVersionReference
                    {
                        TaskId = task.Id,
                        TaskName = task.Name,
                        VersionSpec = versionSpec
                    };

                    // Add to dictionary.
                    projectDef.AdoEntities.TasksByNameVersionSpec.Add(keyByName, tvr);
                }

                // Add task identifier and instantiate the list of version.
                if (!projectDef.AdoEntities.TasksById.ContainsKey(task.Id))
                {
                    // Instantiate a new task reference.
                    tr = new ProjectDefinition.TaskReference
                        {
                            TaskId = task.Id,
                            TaskName = task.Name,
                            Versions = new List<string>(),
                            VersionSpecs = new List<string>()
                        };

                    // Add to dictionary.
                    projectDef.AdoEntities.TasksById.Add(task.Id, tr);
                }

                // Add version and version spec.
                projectDef.AdoEntities.TasksById[task.Id].Versions.Add(version);
                projectDef.AdoEntities.TasksById[task.Id].VersionSpecs.Add(versionSpec);
            }
        }
        
        public static void RetrieveSecurityNamespace(ProjectDefinition projectDef, OperationLocation location)
        {
            // Send some traces.
            if (location == OperationLocation.Source)
                _mySource.Value.TraceInformation("Query all security namespaces from source collection: {0}", projectDef.SourceProject.Collection);
            else if (location == OperationLocation.Destination)
                _mySource.Value.TraceInformation("Query all security namespaces from destination collection: {0}", projectDef.DestinationProject.Collection);
            _mySource.Value.Flush();

            // Create an Azure DevOps REST api service object.
            SecurityNamespaces adorasSecurityNamespace = new SecurityNamespaces(projectDef.AdoRestApiConfigurations["SecurityApi"]);

            // Get all security namespace definitions.                
            projectDef.SecurityNamespaces = adorasSecurityNamespace.GetSecurityNamespaces();

            // An interesting way to find the security namespace identifier is to...
            // securityNamespaceId = allSecurityNamespaces.Value.Single(x => x.Name == "Put the name here").NamespaceId;
        }

        public static void RetrieveTeamIdentifiers(ProjectDefinition projectDef)
        {
            // Create an Azure DevOps REST api service object.
            Teams adorasTeams = new Teams(projectDef.AdoRestApiConfigurations["TeamsApi"]);

            // Get all teams.
            CoreResponse.WebApiTeams teams = adorasTeams.GetTeams();

            // Store teams information.
            foreach (var team in teams.Value)
                if (!projectDef.AdoEntities.Teams.ContainsKey(team.Name))
                    projectDef.AdoEntities.Teams.Add(team.Name, team.Id);
        }

        public static string Set8CharRandomString()
        {
            // Initialize.
            Guid g = Guid.NewGuid();
            string randomString;

            try
            {
                randomString = g.ToString().Substring(0, 8);
            }
            catch (Exception)
            {
                throw;
            }

            // Return random string.
            return randomString;
        }

        /// <summary>
        /// Get sorted dependency graph nodes in order of most dependent to less dependent nodes. Use the
        /// <paramref name="reverseSort"/> parameter to get this in order of less dependent to most dependent nodes.
        /// </summary>
        /// <param name="dpg">Dependency graph</param>
        /// <param name="reverseSort">Reverse the result to get less dependent to most dependent.</param>
        /// <returns>Sorted node in topological order.</returns>
        public static List<string> GetSortedDependencyGraphNodes(ADO.Collections.DependencyGraph<string> dpg, bool reverseSort = false)
        {
            // Initialize.
            HashSet<string> setOfNodes = new HashSet<string>();
            HashSet<Tuple<string, string>> setOfEdges = new HashSet<Tuple<string, string>>();
            List<string> listOfGraphDependentNodes;
            List<string> listOfGraphIndependentNodes;
            List<string> sortedListOfGraphDependentNodes;
            NumberAsStringComparer customComparer = new NumberAsStringComparer();

            // Sort the graph nodes.
            listOfGraphDependentNodes = dpg.DependentNodes.ToList();            
            listOfGraphDependentNodes.Sort(customComparer);
            listOfGraphIndependentNodes = dpg.IndependentNodes.ToList();
            listOfGraphIndependentNodes.Sort(customComparer);

            // Generate nodes and edges.
            foreach (string node in listOfGraphDependentNodes)
            {
                // Add a node to set.
                setOfNodes.Add(node);

                // Create the edges and add them to set.
                foreach (string dependentNodeValue in dpg.GetDependenciesForNode(node).ToList())
                    setOfEdges.Add(new Tuple<string, string>(node, dependentNodeValue));
            }

            // Sort to get the most dependent element first.
            sortedListOfGraphDependentNodes = TopologicalSort(setOfNodes, setOfEdges);

            // Reverse the list to get the most independent element first.
            if (reverseSort)
            {
                sortedListOfGraphDependentNodes.Reverse();
                listOfGraphIndependentNodes.AddRange(sortedListOfGraphDependentNodes);
                return listOfGraphIndependentNodes;
            }
            // List to get the most dependent element first.
            else
            {
                sortedListOfGraphDependentNodes.AddRange(listOfGraphIndependentNodes);
                return sortedListOfGraphDependentNodes;
            }
        }

        public static void ValidateProject(ProjectDefinition projectDef, OperationLocation location, 
            bool overrideSourceProcessTemplateNameWithOneSuppliedFromConfigFile,
            bool overrideDestinationProcessTemplateNameWithOneSuppliedFromConfigFile,
            int timeout = 5)
        {
            // Initialize.
            string jsonContent = null;
            string filename = null;
            JObject bodyAsJObject = null;
            CoreResponse.TeamProject project = null;
            Stopwatch validationTimer = new Stopwatch();

            // Send some traces.
            if(location == OperationLocation.Source)
                _mySource.Value.TraceInformation("Validate source project.");
            else if (location == OperationLocation.Destination)
                _mySource.Value.TraceInformation("Validate destination project.");
            _mySource.Value.Flush();

            // Start timer.
            validationTimer.Start();

            // Create Azure DevOps REST api service object.
            Projects adorasProjects = new Projects(projectDef.AdoRestApiConfigurations["ProjectsApi"]);
            Projects adorasProjectsExtProperties = new Projects(projectDef.AdoRestApiConfigurations["GetProjectPropertiesApi"]);

            if (location == OperationLocation.Source)
            {
                // Check if the project can be found.            
                projectDef.SourceProject.Validated = adorasProjects.TestIfProjectExistByName(projectDef.SourceProject.Name);

                try
                {
                    // Wait until timeout or that project state is retrieved.                
                    while (projectDef.SourceProject.State != ProjectState.WellFormed)
                    {
                        // Try to get state of project.
                        projectDef.SourceProject.State = adorasProjects.GetProjectStateByName(projectDef.SourceProject.Name);

                        // Has it timed out?
                        if (validationTimer.Elapsed.Minutes >= timeout)
                        {
                            string errorMsg = string.Format("Source project {0} retrieval timed out.", projectDef.SourceProject.Name);
                            throw (new Exception(errorMsg));
                        }

                        // Pause the validation of 500 milliseconds.
                        if (projectDef.SourceProject.State != ProjectState.WellFormed)
                            Thread.Sleep(500);
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    // Stop timer.
                    validationTimer.Stop();
                }

                // Send some traces.
                _mySource.Value.TraceInformation("Source project state is {0}", projectDef.SourceProject.State.ToString());
                _mySource.Value.Flush();

                // Retrieve the project.
                project = adorasProjects.GetProjectByName(projectDef.SourceProject.Name);
                projectDef.SourceProject.Id = project.Id;
                projectDef.SourceProject.Description = project.Description;
                projectDef.SourceProject.Visibility = project.Visibility;

                string sourceProcessNameFromConfigFile = projectDef.SourceProject.ProcessTemplateTypeName;                

                // Get extended properties of project.
                CoreResponse.ProjectProperties pps = adorasProjectsExtProperties.GetProjectExtendedProperties(projectDef.SourceProject.Name);
                //CAREFUL this may be null
                projectDef.SourceProject.ProcessTemplateTypeId = pps.Value.Where(x => x.Name.ToLower() == "system.processtemplatetype").FirstOrDefault()?.Value;
                if (projectDef.SourceProject.ProcessTemplateTypeId == null)
                {
                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"ProcessTemplateTypeId is NULL, are you migrating FROM an onprem project???");
                    _mySource.Value.Flush();
                }
                var templateNameProperty = pps.Value.Where(x => x.Name.ToLower() == "system.process template");
                if (templateNameProperty != null)
                {
                    if (templateNameProperty.FirstOrDefault() != null)
                    {
                        projectDef.SourceProject.ProcessTemplateTypeName = templateNameProperty.FirstOrDefault().Value;
                        if (overrideSourceProcessTemplateNameWithOneSuppliedFromConfigFile)
                        {
                            if (projectDef.SourceProject.ProcessTemplateTypeName != sourceProcessNameFromConfigFile)
                            {                               
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"The process fetched from the config file {sourceProcessNameFromConfigFile} does not match the one fetched with the Rest API {projectDef.SourceProject.ProcessTemplateTypeName}");
                                _mySource.Value.Flush();

                                if (overrideSourceProcessTemplateNameWithOneSuppliedFromConfigFile)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Since overrideSourceProcessTemplateNameWithOneSuppliedFromConfigFile={overrideSourceProcessTemplateNameWithOneSuppliedFromConfigFile}, we are overriding with the value set in the config file of {sourceProcessNameFromConfigFile}");
                                    _mySource.Value.Flush();
                                    projectDef.SourceProject.ProcessTemplateTypeName = sourceProcessNameFromConfigFile;
                                }
                            }
                        }
                    }
                }
                projectDef.SourceProject.SourceControlType = pps.Value.Where(x => x.Name.ToLower() == "system.sourcecontrolcapabilityflags").FirstOrDefault().Value;

                // Generate an output file to keep track of project name, id and collection.
                bodyAsJObject = new JObject(
                        new JProperty("Name", projectDef.SourceProject.Name),
                        new JProperty("Id", projectDef.SourceProject.Id),
                        new JProperty("Collection", projectDef.SourceProject.Collection),
                        new JProperty("Description", projectDef.SourceProject.Description),
                        new JProperty("Visibility", projectDef.SourceProject.Visibility),
                        new JProperty("ProcessTemplateTypeId", projectDef.SourceProject.ProcessTemplateTypeId),
                        new JProperty("ProcessTemplateTypeName", projectDef.SourceProject.ProcessTemplateTypeName),
                        new JProperty("SourceControlType", projectDef.SourceProject.SourceControlType),
                        new JProperty("State", projectDef.SourceProject.State),
                        new JProperty("Validated", projectDef.SourceProject.Validated)
                    );

                // Save data.
                jsonContent = JsonConvert.SerializeObject(bodyAsJObject, Formatting.Indented);

                // Define the export file.
                filename = projectDef.GetFileFor("ProjectInfo.Source");
            }
            else if (location == OperationLocation.Destination)
            {
                // Check if the project can be found.            
                projectDef.DestinationProject.Validated = adorasProjects.TestIfProjectExistByName(projectDef.DestinationProject.Name);

                try
                {
                    // Wait until timeout or that project state is retrieved.
                    while (projectDef.DestinationProject.State != ProjectState.WellFormed)
                    {
                        // Try to get state of project.
                        projectDef.DestinationProject.State = adorasProjects.GetProjectStateByName(projectDef.DestinationProject.Name);

                        // Has it timed out?
                        if (validationTimer.Elapsed.Minutes >= timeout)
                        {
                            string errorMsg = string.Format("Destination project {0} retrieval timed out.", projectDef.DestinationProject.Name);
                            throw (new Exception(errorMsg));
                        }

                        // Pause the validation of 500 milliseconds.
                        if (projectDef.DestinationProject.State != ProjectState.WellFormed)
                            Thread.Sleep(500);
                    }
                }
                catch
                { throw; }
                finally
                {
                    // Stop timer.
                    validationTimer.Stop();
                }

                // Send some traces.
                _mySource.Value.TraceInformation("Destination project state is {0}", projectDef.DestinationProject.State.ToString());
                _mySource.Value.Flush();

                // Retrieve the project.
                project = adorasProjects.GetProjectByName(projectDef.DestinationProject.Name);
                projectDef.DestinationProject.Id = project.Id;

                // Description and visibility must match with import parameters.
                if (projectDef.DestinationProject.Description != project.Description)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("Destination project description is different than required");
                    _mySource.Value.Flush();
                }

                if (projectDef.DestinationProject.Visibility != project.Visibility)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("Destination project visibility is different than required");
                    _mySource.Value.Flush();
                }

                string destinationProcessNameFromConfigFile = projectDef.DestinationProject.ProcessTemplateTypeName;

                // Get extended properties of project.
                CoreResponse.ProjectProperties pp = adorasProjectsExtProperties.GetProjectExtendedProperties(projectDef.DestinationProject.Name);
                projectDef.DestinationProject.ProcessTemplateTypeId = pp.Value.Where(x => x.Name.ToLower() == "system.processtemplatetype").FirstOrDefault()?.Value;
                if (projectDef.DestinationProject.ProcessTemplateTypeId == null)
                {
                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"ProcessTemplateTypeId is NULL, are you migrating TO an onprem project???");
                    _mySource.Value.Flush();
                }
                var templateNameProperty = pp.Value.Where(x => x.Name.ToLower() == "system.process template");
                if (templateNameProperty != null)
                {
                    if (templateNameProperty.FirstOrDefault() != null)
                    {
                        projectDef.DestinationProject.ProcessTemplateTypeName = templateNameProperty.FirstOrDefault().Value;
                        if (overrideDestinationProcessTemplateNameWithOneSuppliedFromConfigFile)
                        {
                            if (projectDef.DestinationProject.ProcessTemplateTypeName != destinationProcessNameFromConfigFile)
                            {
                                // Send some traces.
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"The process fetched from the config file {destinationProcessNameFromConfigFile} does not match the one fetched with the Rest API {projectDef.DestinationProject.ProcessTemplateTypeName}");
                                _mySource.Value.Flush();

                                if (overrideDestinationProcessTemplateNameWithOneSuppliedFromConfigFile)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Since overrideDestinationProcessTemplateNameWithOneSuppliedFromConfigFile={overrideDestinationProcessTemplateNameWithOneSuppliedFromConfigFile}, we are overriding with the value set in the config file of {destinationProcessNameFromConfigFile}");
                                    _mySource.Value.Flush();
                                    projectDef.DestinationProject.ProcessTemplateTypeName = destinationProcessNameFromConfigFile;
                                }
                            }
                        }
                    }
                }
                projectDef.DestinationProject.SourceControlType = pp.Value.Where(x => x.Name.ToLower() == "system.sourcecontrolcapabilityflags").FirstOrDefault().Value;

                // Generate an output file to keep track of project name, id and collection.
                bodyAsJObject = new JObject(
                        new JProperty("Name", projectDef.DestinationProject.Name),
                        new JProperty("Id", projectDef.DestinationProject.Id),
                        new JProperty("Collection", projectDef.DestinationProject.Collection),
                        new JProperty("Description", projectDef.DestinationProject.Description),
                        new JProperty("Visibility", projectDef.DestinationProject.Visibility),
                        new JProperty("ProcessTemplateTypeId", projectDef.DestinationProject.ProcessTemplateTypeId),
                        new JProperty("ProcessTemplateTypeName", projectDef.DestinationProject.ProcessTemplateTypeName),
                        new JProperty("SourceControlType", projectDef.DestinationProject.SourceControlType),
                        new JProperty("State", projectDef.DestinationProject.State),
                        new JProperty("Validated", projectDef.DestinationProject.Validated)
                    );

                // Save data.
                jsonContent = JsonConvert.SerializeObject(bodyAsJObject, Formatting.Indented);

                // Define the export file.
                filename = projectDef.GetFileFor("ProjectInfo.Destination");
            }

            // Write definition file.
            projectDef.WriteDefinitionFile(filename, jsonContent);
        }

        public static string LoadFromEnvironmentVariables(string rawValue)
        {
            if (!string.IsNullOrEmpty(rawValue))
            {
                if (rawValue.StartsWith(EnvironmentVariablePrefix))
                {
                    string variable = rawValue.Substring(EnvironmentVariablePrefix.Length);
                    var value = Environment.GetEnvironmentVariable(variable);
                    
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                    else
                    {
                        throw new ArgumentNullException($"{rawValue} should point to an existing environment variable that is non-null");
                    }
                }
                else
                {
                    throw new ArgumentException($"{rawValue} should start with '{EnvironmentVariablePrefix}'");
                }
            }
            else
            {
                return rawValue;
            }
        }

        #endregion

        #endregion
    }
}
