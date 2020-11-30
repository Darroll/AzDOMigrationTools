using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.WorkItemTracking.ClassificationNodes
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Classification Nodes
    /// <para>
    /// Documentation about Azure DevOps\Work Item Tracking can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class ClassificationNodes : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.ClassificationNodes"));

        #endregion

        #region - Public Members

        public static Queue<JObject> GetClassificationNodesAsQueue(string jsonContent,
            List<string> areaOrIterationInclusions, bool isClone, string prefixPath = "")
        {
            // Initialize process.
            int nodeLevel = 0;
            string fullPath = null;
            List<string> nodePaths = new List<string>();
            Stack<JObject> processStack = new Stack<JObject>();
            Queue<JObject> nodeQueue = new Queue<JObject>();

            // todo: should this try to load an array? No array of nodes were seen at the root so far.
            JObject rootNode = JObject.Parse(jsonContent);

            if (string.IsNullOrEmpty(prefixPath))
            {
                // Determine the full path.
                if (!isClone)
                {
                    fullPath = @"\" + rootNode["name"].ToString();
                }
                else
                {
                    //fullPath = string.Empty;
                    fullPath = @"\" + rootNode["name"].ToString();
                }

                // Add artificially nodeLevel, isRoot, processedFullPath and processedParentPath properties to JObject which
                // will help traversing the structure.
                rootNode.Add("isRoot", JValue.CreateString("true"));
                rootNode.Add("nodeLevel", nodeLevel);
                if (rootNode["processedParentPath"] == null) rootNode.Add("processedParentPath", JValue.CreateNull());
                if (rootNode["processedFullPath"] == null) { rootNode.Add("processedFullPath", fullPath); }
                else rootNode["processedFullPath"] = fullPath;

                if(rootNode["path"] == null) rootNode.Add("path", JValue.CreateNull());
            }
            else
            {
                string[] prefixNodeNames = prefixPath.Split('\\');
                List<JObject> prefixNodes = new List<JObject>();

                fullPath = $"\\{prefixPath}\\{rootNode["name"].ToString()}";
                rootNode.Add("isRoot", JValue.CreateString("false"));
                rootNode.Add("nodeLevel", prefixNodeNames.Length);
                rootNode.Add("processedParentPath", prefixPath);
                rootNode.Add("processedFullPath", fullPath);

                if (prefixNodeNames.Length == 1)
                {
                    JObject previousNodeJObject = new JObject(
                        new JProperty("identifier", "00000000-0000-0000-0000-000000000000"),
                        new JProperty("name", prefixNodeNames[prefixNodeNames.Length - 1]),
                        new JProperty("structureType", rootNode["structureType"].ToString()),
                        new JProperty("hasChildren", prefixNodeNames.Length == 1), //If there is only one level in the prefix path then this is the root
                        new JProperty("path", prefixPath),
                        new JProperty("processedParentPath", JValue.CreateNull()),
                        new JProperty("processedFullPath", prefixPath),
                        new JProperty("children", new JArray(){
                            rootNode
                        })
                    );
                    processStack.Push(previousNodeJObject);
                }
                else if (prefixNodeNames.Length > 1)
                {
                    JObject previousNodeJObject = new JObject(
                        new JProperty("identifier", "00000000-0000-0000-0000-000000000000"),
                        new JProperty("name", prefixNodeNames[prefixNodeNames.Length - 1]),
                        new JProperty("structureType", rootNode["structureType"].ToString()),
                        new JProperty("hasChildren", prefixNodeNames.Length == 1), //If there is only one level in the prefix path then this is the root
                        new JProperty("path", prefixPath),
                        new JProperty("processedParentPath", prefixNodeNames.Take(prefixNodeNames.Length - 1).Aggregate((i, j) => i + @"\" + j)),
                        new JProperty("processedFullPath", prefixPath),
                        new JProperty("children", new JArray(){
                            rootNode
                        })
                    );
                    processStack.Push(previousNodeJObject);
                    for (var i = 2; i < (prefixNodeNames.Length + 1); i++)
                    {
                        string nodeName = prefixNodeNames[prefixNodeNames.Length - i];
                        var parentPathProperty = new JProperty("processedParentPath", JValue.CreateNull());
                        if (i < prefixNodeNames.Length)
                        {
                            parentPathProperty = new JProperty("processedParentPath", prefixNodeNames.Take(prefixNodeNames.Length - i).Aggregate((j, k) => j + @"\" + k));
                        }

                        var nodeJObject = new JObject(
                            new JProperty("identifier", "00000000-0000-0000-0000-000000000000"),
                            new JProperty("name", nodeName),
                            new JProperty("structureType", rootNode["structureType"].ToString()),
                            new JProperty("hasChildren", JValue.CreateString("true")),
                            new JProperty("path", prefixNodeNames.Take(prefixNodeNames.Length - i + 1).Aggregate((j, k) => j + @"\" + k)),
                            parentPathProperty,
                            new JProperty("processedFullPath", prefixNodeNames.Take(prefixNodeNames.Length - i + 1).Aggregate((j, k) => j + @"\" + k)),
                            new JProperty("children", new JArray(){
                                previousNodeJObject
                            })
                        );
                        previousNodeJObject = nodeJObject;
                        processStack.Push(nodeJObject);
                    }
                }
                while (processStack.Count > 0)
                {
                    JObject tempNode = processStack.Pop();
                    nodeQueue.Enqueue(tempNode);
                    nodePaths.Add(tempNode["name"].ToString());
                }
            }

            // Add the first/root node to process stack (LIFO).
            if (ProcessNode(rootNode, areaOrIterationInclusions))
            {
                processStack.Push(rootNode);

                // Traverse JToken/JObject hierarchy.
                while (processStack.Count > 0)
                {
                    // Get current node.
                    JObject currentNode = processStack.Pop();

                    // Extract the node level.
                    nodeLevel = currentNode["nodeLevel"].ToObject<int>();

                    // If the number of path tokens is higher than current node level, it means that this
                    // node is located at an higher level.
                    while (nodePaths.Count > nodeLevel)
                        nodePaths.RemoveAt(nodePaths.Count - 1);

                    // Add this path token.
                    nodePaths.Add(currentNode["name"].ToString());

                    // Concatenate tokens and form a path but exclude the leaf token.
                    if (currentNode["isRoot"].ToObject<bool>() == false)
                    {
                        // Determine the full path.
                        fullPath = @"\" + nodePaths.Aggregate((i, j) => i + @"\" + j);

                        // Concatenate tokens to define full path and parent path which exclude the leaf token.
                        currentNode["processedFullPath"] = fullPath;
                        currentNode["processedParentPath"] = nodePaths.Take(nodePaths.Count - 1).Aggregate((i, j) => i + @"\" + j);
                    }

                    // Add to node queue (FIFO).
                    nodeQueue.Enqueue(currentNode);

                    // Visit child nodes.
                    if (currentNode["hasChildren"].ToObject<bool>())
                    {
                        // Increment node level.
                        nodeLevel++;

                        // Visit each child and store into process stack for order processing.
                        foreach (JObject childNode in currentNode["children"])
                        {
                            // Add artificially nodeLevel, isRoot, processedFullPath and processedParentPath properties.
                            childNode.Add("nodeLevel", nodeLevel);
                            // It is never the root node here.
                            childNode.Add("isRoot", JValue.CreateString("false"));
                            // These properties will be resolved at a later time.
                            childNode.Add("processedFullPath", JValue.CreateNull());
                            childNode.Add("processedParentPath", JValue.CreateNull());
                            if (rootNode["path"] == null) childNode.Add("path", JValue.CreateNull());

                            if (ProcessNode(childNode, areaOrIterationInclusions))
                            {
                                // Add to stack.
                                processStack.Push(childNode);
                            }
                        }
                    }
                    // Remove path token.
                    else
                        nodePaths.RemoveAt(nodePaths.Count - 1);
                }
            }

            // Return nodes.
            return nodeQueue;
        }

        private static bool ProcessNode(JObject rootNode,
            List<string> areaOrIterationInclusions)
        {
            var structureType = rootNode["structureType"].ToString();
            var path = rootNode["path"].ToString();
            var normalizedPath = NormalizePath(path);

            if (areaOrIterationInclusions == null)
                return true;

            bool areaOrIterationIsIncluded = false;

            foreach (string item in areaOrIterationInclusions)
            {
                if (item.StartsWith(normalizedPath) || normalizedPath.StartsWith(item))
                {
                    areaOrIterationIsIncluded = true;
                    break;
                }
            }

            string warningMsg = null;
            if (!areaOrIterationIsIncluded)
            {
                warningMsg =
                    $"Skipping {structureType} {path} as it is not in the inclusion list";
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, warningMsg);
                _mySource.Value.Flush();
            }
            else
            {
                var infoMsg = $"Including {structureType} {path}";
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Information, 0, infoMsg);
                _mySource.Value.Flush();
            }
            return areaOrIterationIsIncluded;
        }

        private static string NormalizePath(string path)
        {
            var components = path.Split(new string[] { Constants.DefaultPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            return String.Join(Constants.DefaultPathSeparator, components.Take(1).Concat(components.Skip(2)));
        }


        #endregion

        #endregion

        public ClassificationNodes(IConfiguration configuration) : base(configuration) { }

        public WorkItemTrackingResponse.WorkItemClassificationNode CreateArea(string jsonContent, string path = null)
        {
            // Initialize.
            WorkItemTrackingResponse.WorkItemClassificationNode node = null;

            try
            {
                // Define uri to call.
                if (string.IsNullOrEmpty(path))
                    SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/classificationnodes/areas?api-version={Version}");
                else
                    SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/classificationnodes/areas/{path}?api-version={Version}");

                using (var client = GetHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PersonalAccessToken))));

                    // Generate the request message with encoding and media type.
                    SetHttpContentForRequestMessageBody(jsonContent, Encoding.UTF8);

                    // This api is called with a POST method.
                    ApiMethod = new HttpMethod("POST");

                    // Send.
                    ResponseMessage = client.PostAsync(Uri, RequestMessageContent).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        node = DeserializeResponseToObject<WorkItemTrackingResponse.WorkItemClassificationNode>();
                    }
                    else
                        throw (new RecoverableException(LastApiErrorMessage));
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                //{"VS402371: Classification node name Demo is already in use by a different child of parent classification node c9a7620e-2ea5-4167-a50e-2a6e373353de. Choose a different name and try again."}
                if (ex.Message.StartsWith(Constants.ErrorVS402371))
                {
                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, ex.Message);
                }
                else
                {
                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                }
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();

                throw;
            }

            // Return classification node created.
            return node;
        }

        public WorkItemTrackingResponse.WorkItemClassificationNode CreateIteration(string jsonContent, string path = null)
        {
            // Initialize.
            WorkItemTrackingResponse.WorkItemClassificationNode node = null;

            try
            {
                // Define uri to call.
                if (string.IsNullOrEmpty(path))
                    SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/classificationnodes/iterations?api-version={Version}");
                else
                    SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/classificationnodes/iterations/{path}?api-version={Version}");

                using (var client = GetHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PersonalAccessToken))));

                    // Generate the request message with encoding and media type.
                    SetHttpContentForRequestMessageBody(jsonContent, Encoding.UTF8);

                    // This api is called with a POST method.
                    ApiMethod = new HttpMethod("POST");

                    // Send.
                    ResponseMessage = client.PostAsync(Uri, RequestMessageContent).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        node = DeserializeResponseToObject<WorkItemTrackingResponse.WorkItemClassificationNode>();
                    }
                    else
                        throw (new RecoverableException(LastApiErrorMessage));
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                if (ex.Message.StartsWith(Constants.ErrorVS402371))
                {
                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, ex.Message);
                }
                else
                {
                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                }
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();

                throw;
            }

            // Return classification node created.
            return node;
        }

        public WorkItemTrackingResponse.WorkItemClassificationNodes GetClassificationNodes(int depth, string ids = null)
        {
            // Initialize.
            WorkItemTrackingResponse.WorkItemClassificationNodes nodes = new WorkItemTrackingResponse.WorkItemClassificationNodes();

            try
            {
                // Define uri to call.
                if (string.IsNullOrEmpty(ids))
                    SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/classificationnodes?$depth={depth}&api-version={Version}");
                else
                    SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/classificationnodes?ids={ids}&$depth={depth}&api-version={Version}");

                using (var client = GetHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PersonalAccessToken))));

                    // Send.
                    ResponseMessage = client.GetAsync(Uri).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        nodes = DeserializeResponseToObject<WorkItemTrackingResponse.WorkItemClassificationNodes>();
                    }
                    else
                        throw (new RecoverableException(LastApiErrorMessage));
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
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();

                throw;
            }

            // Return nodes found.
            return nodes;
        }

        public string GetClassificationNodeToken(string path)
        {
            // Initialize.
            int depth = 0;
            int pathTokenIndex = 0;
            string securityToken = string.Empty;
            string nodeName = string.Empty;
            string lastPathToken = string.Empty;
            string ids = string.Empty;
            string securityTokenTemplate = "vstfs:///Classification/Node/{0}";
            string securityTokenSeparator = @":";
            string[] pathTokens = null;
            WorkItemTrackingResponse.WorkItemClassificationNode nextNode = null;
            WorkItemTrackingResponse.WorkItemClassificationNode rootNode = null;

            // Tokenize the classification node path.
            pathTokens = path.Split(new char[] { '/', '\\' });

            // Set the depth of search with the number of path tokens.
            depth = pathTokens.Length;

            foreach (string item in pathTokens.Take(pathTokens.Length - 1))
            {
                // Get all classfication nodes.
                WorkItemTrackingResponse.WorkItemClassificationNodes result = GetClassificationNodes(depth, ids);

                // Search only nodes of type area.
                rootNode = result.Value.Where(x => x.StructureType == "area").Single();

                // Check if the area node has the same name.
                if (rootNode.Name != item)
                    throw (new InvalidOperationException($"Expecting classification node [{item}] but got [{rootNode.Name}]"));

                // get next classification nodes id if possible.
                if (rootNode.HasChildren && pathTokenIndex < pathTokens.Length - 1)
                {
                    // Get next path token.
                    nodeName = pathTokens[++pathTokenIndex];

                    // Check if the area node has the same name.
                    nextNode = rootNode.Children.Single(x => x.Name == nodeName);

                    // Set the comma separated integer classification nodes ids.
                    ids = nextNode.Id.ToString();
                }
                // If there are no children or path tokens, define the security token with actual path.

                // add token.
                securityToken += string.Format(securityTokenTemplate, rootNode.Identifier) + securityTokenSeparator;

            }

            // Get last token.
            lastPathToken = pathTokens.Last();

            if (depth == 1)
            {
                // Get all classfication nodes.
                WorkItemTrackingResponse.WorkItemClassificationNodes result = GetClassificationNodes(depth, ids);

                // Search only nodes of type area.
                rootNode = result.Value.Where(x => x.StructureType == "area").Single();
                securityToken += string.Format(securityTokenTemplate, rootNode.Identifier);
            }
            else
            {
                if (nextNode.HasChildren && nextNode.Name != lastPathToken) //may not need this case
                {
                    WorkItemTrackingResponse.WorkItemClassificationNode lastNode = nextNode.Children.Single(x => x.Name == lastPathToken);

                    // add token.
                    securityToken += string.Format(securityTokenTemplate, lastNode.Identifier);
                }
                else if (nextNode.Name == lastPathToken)
                {
                    // add token.
                    securityToken += string.Format(securityTokenTemplate, nextNode.Identifier);
                }
                else
                    throw new InvalidOperationException($"Expecting last child {lastPathToken}");
            }

            // Return the security token.
            return securityToken;
        }

        public Dictionary<string, JObject> GetIterationsDictionary(bool isClone, int depth = 10)
        {
            // Initialize.
            string jsonContent;
            string hashKey;
            JObject currentIterationNode;
            JToken iterationsAsJToken;
            Queue<JObject> iterationNodeQueue;
            Dictionary<string, JObject> iterationsDictionary = new Dictionary<string, JObject>();

            try
            {
                // Get List of all iterations that exists and export as a JSON string.
                iterationsAsJToken = GetIterationsAsJToken(depth);
                jsonContent = JsonConvert.SerializeObject(iterationsAsJToken);

                // Get all classification nodes as queue.
                iterationNodeQueue = GetClassificationNodesAsQueue(jsonContent, null, isClone);

                // Convert nodes stored within queue to dictionary.
                while (iterationNodeQueue.Count > 0)
                {
                    // Get current iteration node.
                    currentIterationNode = iterationNodeQueue.Dequeue();

                    // Set hash key to be the full path.
                    hashKey = currentIterationNode["processedFullPath"].ToString();

                    // Add to dictionary.
                    iterationsDictionary.Add(hashKey, currentIterationNode);
                }
            }
            catch
            {
                // todo: add more details on failure.
                throw;
            }

            // Return dictionary.
            return iterationsDictionary;
        }

        public JToken GetIterationsAsJToken(int depth = 10)
        {
            // Initialize.
            JToken j = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/classificationnodes/iterations?$depth={depth}&api-version={Version}");

                using (var client = GetHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PersonalAccessToken))));

                    // Send.
                    ResponseMessage = client.GetAsync(Uri).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        j = DeserializeResponseToObject<JToken>();
                    }
                    else
                        throw (new RecoverableException(LastApiErrorMessage));
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
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();

                throw;
            }

            // Return classification nodes as a JToken.
            return j;
        }

        public JToken GetAreasAsJToken(int depth = 10)
        {
            // Initialize.
            JToken j = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/classificationnodes/areas?$depth={depth}&api-version={Version}");

                using (var client = GetHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PersonalAccessToken))));

                    // Send.
                    ResponseMessage = client.GetAsync(Uri).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        j = DeserializeResponseToObject<JToken>();
                    }
                    else
                        throw (new RecoverableException(LastApiErrorMessage));
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
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();

                throw;
            }

            // Return classification nodes as a JToken.
            return j;
        }
    }
}