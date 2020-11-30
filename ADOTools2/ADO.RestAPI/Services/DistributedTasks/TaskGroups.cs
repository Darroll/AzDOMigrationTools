using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.DistributedTasks
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Taskgroups
    /// <para>
    /// Documentation about Azure DevOps\Task Agent\Taskgroups can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/distributedtask/taskgroups?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class TaskGroups : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.TaskGroups"));

        #endregion

        #endregion

        public TaskGroups(IConfiguration configuration) : base(configuration) { }

        public TaskAgentResponse.TaskGroup CreateTaskGroup(string jsonContent)
        {
            // Initialize.
            TaskAgentResponse.TaskGroup tg = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/taskgroups?api-version={Version}");

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

                    // Form the request.
                    RequestMessage = new HttpRequestMessage(ApiMethod, Uri) { Content = RequestMessageContent };

                    // Send.
                    ResponseMessage = client.SendAsync(RequestMessage).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        tg = DeserializeResponseToObject<TaskAgentResponse.TaskGroup>();
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

            // Return task group created.
            return tg;
        }

        public TaskAgentResponse.TaskGroups GetAllTaskGroups()
        {
            // Initialize.
            bool proceedFlag = true;                                    // Set to true to enter the loop at least once.
            List<string> taskGroupIdList = new List<string>();
            TaskAgentResponse.TaskGroups tgs;

            // Instantiate output task groups.
            TaskAgentResponse.TaskGroups outputTaskGroups = new TaskAgentResponse.TaskGroups
                {
                    Count = 0,
                    Value = new List<TaskAgentResponse.TaskGroup>()
                };

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

                    // Allow the request to continue if a continuation token is returned.
                    while (proceedFlag)
                    {
                        // Define uri to call.
                        if (HasContinuationToken)
                            SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/taskgroups?continuationToken={ContinuationToken}&api-version={Version}");
                        else
                            SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/taskgroups?api-version={Version}");

                        // Send.
                        ResponseMessage = client.GetAsync(Uri).Result;

                        if (ValidateServiceCall())
                        {
                            SetSuccessfulCRUDOperation();

                            // Regenerate object.
                            tgs = DeserializeResponseToObject<TaskAgentResponse.TaskGroups>();

                            // Proceed if there are definitions.
                            if (tgs.Count > 0)
                            {
                                // Different version of a task group will be counted as a different task group
                                // but here, we are only looking for distinct task group identifier, as another call
                                // is made to get all versions of task groups.
                                foreach (var taskGroup in tgs.Value)
                                    if (!taskGroupIdList.Contains(taskGroup.Id))
                                        taskGroupIdList.Add(taskGroup.Id);
                            }

                            // Stay in the loop as long there is a continuation token.
                            proceedFlag = HasContinuationToken;
                        }
                        else
                        {
                            // Stop the query operation.
                            proceedFlag = false;

                            // Raise an exception.
                            throw (new RecoverableException(LastApiErrorMessage));
                        }
                    }
                }

                // Proceed only task groups were found.
                if (taskGroupIdList.Count > 0)
                {
                    // Instantiate output object.
                    outputTaskGroups = new TaskAgentResponse.TaskGroups
                        {
                            Count = 0,
                            Value = new List<TaskAgentResponse.TaskGroup>()
                        };

                    foreach (string identifier in taskGroupIdList)
                    {
                        using (var client = GetHttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Add(
                                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                                Convert.ToBase64String(
                                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                                        string.Format("{0}:{1}", "", PersonalAccessToken))));

                            // Define uri to call.
                            SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/taskgroups/{identifier}?api-version={Version}");

                            // Send.
                            ResponseMessage = client.GetAsync(Uri).Result;

                            if (ValidateServiceCall())
                            {
                                SetSuccessfulCRUDOperation();

                                // Regenerate object.
                                // It returns always task groups and not task group response.
                                tgs = DeserializeResponseToObject<TaskAgentResponse.TaskGroups>();

                                // Add to list.
                                if (tgs.Count > 0)
                                    foreach (var tg in tgs.Value)
                                        outputTaskGroups.Value.Add(tg);
                            }
                            else
                                throw (new RecoverableException(LastApiErrorMessage));
                        }
                    }

                    // Set the number of task groups added.
                    outputTaskGroups.Count = outputTaskGroups.Value.Count;
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

            // Return task groups.
            return outputTaskGroups;
        }

        public TaskAgentResponse.TaskGroup GetTaskGroupById(string taskGroupId)
        {
            // Initialize.
            TaskAgentResponse.TaskGroups tgs;
            TaskAgentResponse.TaskGroup tg = null;

            try
            {
                tgs = GetAllTaskGroups();
                tg = tgs.Value.Where(x => x.Id == taskGroupId).FirstOrDefault();
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

            // Return task group found.
            return tg;
        }

        public TaskAgentResponse.TaskGroup GetTaskGroupByName(string name)
        {
            // Initialize.
            TaskAgentResponse.TaskGroups tgs;
            TaskAgentResponse.TaskGroup tg = null;

            try
            {
                tgs = GetAllTaskGroups();
                tg = tgs.Value.Where(x => x.Name == name).FirstOrDefault();
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

            // Return task group found.
            return tg;
        }

        public TaskAgentResponse.TaskGroup GetTaskGroupLatestVersionByName(string name)
        {
            // Initialize.
            int highestVersion = 0;
            TaskAgentResponse.TaskGroups tgs;
            IEnumerable<TaskAgentResponse.TaskGroup> filteredResults;
            TaskAgentResponse.TaskGroup ltg = null;     // Latest version of task group.

            try
            {
                // Get all task groups that exist.
                tgs = GetAllTaskGroups();

                // Retrieve all versions for that task group.
                filteredResults = tgs.Value.Where(x => x.Name == name);
                
                foreach(TaskAgentResponse.TaskGroup tg in filteredResults)
                {
                    if (highestVersion < tg.Version.Major)
                    {
                        // Retain latest version.
                        highestVersion = tg.Version.Major;

                        // Retain task group also.
                        ltg = tg;
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

            // Return task group found.
            return ltg;
        }

        public List<JToken> GetTaskGroupsAsJToken()
        {
            // Initialize.
            bool proceedFlag = true;                                    // Set to true to enter the loop at least once.
            JToken j;
            List<string> taskGroupIdList = new List<string>();
            List<JToken> taskGroupsAsJToken = new List<JToken>();
            TaskAgentResponse.TaskGroups tgs;

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

                    // Allow the request to continue if a continuation token is returned.
                    while (proceedFlag)
                    {
                        // Define uri to call.
                        if (HasContinuationToken)
                            SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/taskgroups?continuationToken={ContinuationToken}&api-version={Version}");
                        else
                            SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/taskgroups?api-version={Version}");

                        // Send.
                        ResponseMessage = client.GetAsync(Uri).Result;

                        if (ValidateServiceCall())
                        {
                            SetSuccessfulCRUDOperation();

                            // Regenerate object.
                            tgs = DeserializeResponseToObject<TaskAgentResponse.TaskGroups>();

                            // Proceed if there are definitions.
                            if (tgs.Count > 0)
                            {
                                // Different version of a task group will be counted as a different task group
                                // but here, we are only looking for distinct task group identifier, as another call
                                // is made to get all versions of task groups.
                                foreach (var taskGroup in tgs.Value)
                                    if (!taskGroupIdList.Contains(taskGroup.Id))
                                        taskGroupIdList.Add(taskGroup.Id);
                            }

                            // Stay in the loop as long there is a continuation token.
                            proceedFlag = HasContinuationToken;
                        }
                        else
                        {
                            // Stop the query operation.
                            proceedFlag = false;

                            // Raise an exception.
                            throw (new RecoverableException(LastApiErrorMessage));
                        }
                    }
                }

                // Proceed only task groups were found.
                if (taskGroupIdList.Count > 0)
                {
                    foreach (string identifier in taskGroupIdList)
                    {
                        using (var client = GetHttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Add(
                                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                                Convert.ToBase64String(
                                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                                        string.Format("{0}:{1}", "", PersonalAccessToken))));

                            // Define uri to call.
                            SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/taskgroups/{identifier}?api-version={Version}");

                            // Send.
                            ResponseMessage = client.GetAsync(Uri).Result;

                            if (ValidateServiceCall())
                            {
                                SetSuccessfulCRUDOperation();

                                // Generate object as JToken with count and value properties.
                                j = DeserializeResponseToObject<JToken>();

                                // Add to output list.
                                taskGroupsAsJToken.Add(j);
                            }
                            else
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

            // Return task groups as a list of JTokens.
            return taskGroupsAsJToken;
        }

        public TaskAgentResponse.TaskGroup PublishAsPreviewTaskGroup(string jsonContent, string parentTaskGroupId)
        {
            // Initialize.
            TaskAgentResponse.TaskGroups tgs;
            TaskAgentResponse.TaskGroup tg = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/taskgroups?parentTaskGroupId={parentTaskGroupId}&api-version={Version}");

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

                    // This api is called with a PUT method.
                    ApiMethod = new HttpMethod("PUT");

                    // Form the request.
                    RequestMessage = new HttpRequestMessage(ApiMethod, Uri) { Content = RequestMessageContent };

                    // Send.
                    ResponseMessage = client.SendAsync(RequestMessage).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        // Response is always returned as TaskGroups.
                        tgs = DeserializeResponseToObject<TaskAgentResponse.TaskGroups>();
                        tg = tgs.Value[0];
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

            // Return published as preview task group.
            return tg;
        }

        public TaskAgentResponse.TaskGroup PublishTaskGroup(string jsonContent, string taskGroupId)
        {
            // Initialize.
            TaskAgentResponse.TaskGroups tgs;
            TaskAgentResponse.TaskGroup tg = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/taskgroups/{taskGroupId}?api-version={Version}");

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

                    // This api is called with a PATCH method.
                    ApiMethod = new HttpMethod("PATCH");

                    // Form the request.
                    RequestMessage = new HttpRequestMessage(ApiMethod, Uri) { Content = RequestMessageContent };

                    // Send.
                    ResponseMessage = client.SendAsync(RequestMessage).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        // Response is always returned as TaskGroups.
                        tgs = DeserializeResponseToObject<TaskAgentResponse.TaskGroups>();
                        tg = tgs.Value[0];
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

            // Return published task group.
            return tg;
        }
    }
}