using System;
using System.Diagnostics;
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
    ///   - Variablegroups
    /// <para>
    /// Documentation about Azure DevOps\Task Agent\Variablegroups can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/distributedtask/variablegroups?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class VariableGroups : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.VariableGroups"));

        #endregion

        #endregion

        public VariableGroups(IConfiguration configuration) : base(configuration) { }

        public TaskAgentResponse.VariableGroup CreateVariableGroup(string jsonContent)
        {
            // Initialize.
            TaskAgentResponse.VariableGroup vg = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/variablegroups?api-version={Version}");

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
                        vg = DeserializeResponseToObject<TaskAgentResponse.VariableGroup>();
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

            // Return variable group created.
            return vg;
        }

        public TaskAgentResponse.VariableGroups GetAllVariableGroups()
        {
            // Initialize.
            TaskAgentResponse.VariableGroups vgs = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/variablegroups?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        vgs = DeserializeResponseToObject<TaskAgentResponse.VariableGroups>();
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

            // Return variable groups.
            return vgs;
        }

        public List<JToken> GetVariableGroupsAsJToken()
        {
            // Initialize.
            JToken j;
            List<JToken> variableGroupsAsJToken = new List<JToken>();
            TaskAgentResponse.VariableGroups vgs = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/variablegroups?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        // Regenerate object.
                        vgs = DeserializeResponseToObject<TaskAgentResponse.VariableGroups>();

                        // Proceed if there are definitions.
                        if (vgs.Count > 0)
                        {
                            foreach (var variableGroup in vgs.Value)
                            {
                                // Define uri to call.
                                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/variablegroups/{variableGroup.Id}?api-version={Version}");

                                using (var client2 = GetHttpClient())
                                {
                                    // Send.
                                    ResponseMessage = client2.GetAsync(Uri).Result;

                                    if (ValidateServiceCall())
                                    {
                                        SetSuccessfulCRUDOperation();

                                        // Generate object as JToken.
                                        j = DeserializeResponseToObject<JToken>();

                                        // Add to output list.
                                        variableGroupsAsJToken.Add(j);
                                    }
                                    else
                                        throw (new RecoverableException(LastApiErrorMessage));
                                }
                            }
                        }
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

            // Return variable groups as a list of JToken.
            return variableGroupsAsJToken;
        }
    }
}