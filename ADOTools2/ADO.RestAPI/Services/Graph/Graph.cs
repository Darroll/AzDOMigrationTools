using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using ADO.RestAPI.Core;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;

namespace ADO.RestAPI.Graph
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Groups
    /// <para>
    /// Documentation about Azure DevOps\Graph\Groups can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/graph/groups?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class Graph : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.Graph.Group"));

        #endregion

        #endregion

        public Graph(IConfiguration configuration) : base(configuration) { }

        public GraphResponse.GraphGroups GetAllGroups()
        {
            // Initialize.
            bool proceedFlag = true;                        // Set to true to enter the loop at least once.
            GraphResponse.GraphGroups ggs = new GraphResponse.GraphGroups();
            GraphResponse.GraphGroups bggs;
            
            try
            {
                using (var client = GetHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PersonalAccessToken))));

                    while (proceedFlag)
                    {
                        // Define uri to call.
                        if (HasContinuationToken)
                            SetServiceUri($"{BaseUri}/_apis/graph/groups?continuationToken={ContinuationToken}&api-version={Version}");
                        else
                            SetServiceUri($"{BaseUri}/_apis/graph/groups?api-version={Version}");

                        // Send.
                        ResponseMessage = client.GetAsync(Uri).Result;

                        // If successful.
                        if (ValidateServiceCall())
                        {
                            SetSuccessfulCRUDOperation();

                            // Regenerate object.
                            bggs = DeserializeResponseToObject<GraphResponse.GraphGroups>();

                            // Initialize internal properties.
                            if(ggs.Value == null)
                            {
                                // Initialize.
                                ggs.Value = new List<GraphResponse.GraphGroup>();
                                ggs.Count = 0;
                            }

                            // Add groups found within this batch to full list.
                            foreach (var item in bggs.Value)
                            {
                                // Add item.
                                ggs.Value.Add(item);

                                // Increment counter.
                                ggs.Count++;
                            }

                            // Stay in the loop as long there is a continuation token.
                            proceedFlag = HasContinuationToken;
                        }
                        else
                        {
                            // Stop the query operation.
                            proceedFlag = false;

                            throw (new RecoverableException(LastApiErrorMessage));
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
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();

                throw;
            }

            // Return all groups found.
            return ggs;
        }

        public GraphResponse.GraphGroup GetGroupByPrincipalName(string principalName)
        {
            // Initialize.
            GraphResponse.GraphGroup gg = null;
            GraphResponse.GraphGroups ggs = null;

            try
            {
                // Retrieve all groups first.
                ggs = GetAllGroups();

                // Get the group corresponding to this principal name.
                gg = ggs.Value.Where(x => x.PrincipalName == principalName).FirstOrDefault();

                if(gg == null)
                {
                    string errorMsg = string.Format("Group: {0} was not found. It may not be created yet! For instance, 'Endpoint Administrators' and 'Endpoint Creators' get created the first time you create a 'Service Connection' in a project.", principalName);

                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                    _mySource.Value.Flush();
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Error while getting {0}: {1}, Stack Trace: {2}", "groups", ex.Message, ex.StackTrace);

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Return group found.
            return gg;
        }

        public string GetGroupSid(GraphResponse.GraphGroup group)
        {
            // Initialize.
            string sid = null;

            if (group != null)
            {
                if (!string.IsNullOrEmpty(group.OriginId))
                {
                    // Get descriptor.
                    GraphResponse.GraphDescriptorResult graphDescriptor = GetDescriptor(group.OriginId);

                    // Get sid from descriptor.
                    if (!string.IsNullOrEmpty(graphDescriptor.Value))
                        sid = ADO.Tools.Utility.GetSidFromDescriptor(graphDescriptor.Value);
                }
            }

            // Return sid.
            return sid;
        }

        public string GetGroupSidByPrincipalName(string principalName)
        {
            // Initialize.
            string sid = null;
            GraphResponse.GraphGroup group = null;

            // Get the group corresponding to this principal name.
            GraphResponse.GraphGroups allGroups = GetAllGroups();
            group = allGroups.Value.Single(x => x.PrincipalName == principalName);

            // Retrieve sid.
            sid = GetGroupSid(group);

            // Return sid.
            return sid;
        }

        public GraphResponse.GraphGroup GetGroupByDescriptor(string groupDescriptor)
        {
            // Initialize.
            GraphResponse.GraphGroup gg = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/graph/groups/{groupDescriptor}?api-version={Version}");

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
                        gg = DeserializeResponseToObject<GraphResponse.GraphGroup>();
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

            // Return the group found.
            return gg;
        }

        public GraphResponse.GraphDescriptorResult GetDescriptor(string storageKey)
        {
            // Initialize.
            GraphResponse.GraphDescriptorResult gdr = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/graph/descriptors/{storageKey}?api-version={Version}");

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

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        gdr = DeserializeResponseToObject<GraphResponse.GraphDescriptorResult>();
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

            // Return descriptor.
            return gdr;
        }

        public GraphResponse.GroupMembers GetGroupMembers(string subjectDescriptor, string direction = "down")
        {
            // Initialize.
            GraphResponse.GroupMembers gms = new GraphResponse.GroupMembers();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/graph/memberships/{subjectDescriptor}?direction={direction}&api-version={Version}");

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
                        gms = DeserializeResponseToObject<GraphResponse.GroupMembers>();
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

            // Return members.
            return gms;
        }

        public GraphResponse.GroupMember AddGroupMembership(string subjectDescriptor, string containerDescriptor)
        {
            // Initialize.
            GraphResponse.GroupMember gm = new GraphResponse.GroupMember();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/graph/memberships/{subjectDescriptor}/{containerDescriptor}?api-version={Version}");

                using (var client = GetHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PersonalAccessToken))));

                    // This api is called with a POST method.
                    ApiMethod = new HttpMethod("PUT");

                    // Form the request.
                    RequestMessage = new HttpRequestMessage(ApiMethod, Uri);

                    // Send.
                    ResponseMessage = client.SendAsync(RequestMessage).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        gm = DeserializeResponseToObject<GraphResponse.GroupMember>();
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

            // Return group member.
            return gm;
        }    

        #region - Sid retrieval.

        public string GetTeamSid(string teamName, RestAPI.Configuration adorasTeamConfig)
        {
            // Initialize.
            string sid = null;

            try
            {
                // Create an Azure DevOps REST api service objects.
                Teams adorasTeams = new Teams(adorasTeamConfig);

                // Find team first.
                CoreResponse.WebApiTeam team = adorasTeams.GetTeamByName(teamName);

                if (string.IsNullOrEmpty(adorasTeams.LastApiErrorMessage))
                {
                    GraphResponse.GraphDescriptorResult graphDescriptor = GetDescriptor(team.Id);
                    if (!string.IsNullOrEmpty(graphDescriptor.Value))
                        sid = ADO.Tools.Utility.GetSidFromDescriptor(graphDescriptor.Value);
                }
                else
                {
                    string errorMsg = string.Format("Error while getting {0}: {1}", "team SID", adorasTeams.LastApiErrorMessage);

                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                    _mySource.Value.Flush();
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Error while getting {0}: {1}, Stack Trace: {2}", "team SID", ex.Message, ex.StackTrace);

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Return sid found.
            return sid;
        }

        #endregion
    }
}
