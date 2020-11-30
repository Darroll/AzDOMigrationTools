using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.DistributedTasks
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Queue
    /// <para>
    /// Documentation about Azure DevOps\Build\Definitions can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/...">here</a>.
    /// </para>
    /// </summary>
    public class Queue : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.Queues"));

        #endregion

        #endregion

        public Queue(IConfiguration configuration) : base(configuration) { }

        public int CreateQueue(string name)
        {
            // Initialize.
            int queueId = 0;
            TaskAgentResponse.TaskAgentQueue taq = new TaskAgentResponse.TaskAgentQueue();

            try
            {
                // Create the request message dynamically.
                // todo: Is it the best way to achieve this?
                var requestBody = new TaskAgentResponse.TaskAgentQueue { Name = name };
                JsonRequestBody = JsonConvert.SerializeObject(requestBody);

                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/distributedtask/queues?api-version={Version}");

                using (var client = GetHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PersonalAccessToken))));

                    // Generate the request message with encoding and media type.
                    SetHttpContentForRequestMessageBody(JsonRequestBody, Encoding.UTF8);

                    // This api is called with a POST method.
                    ApiMethod = new HttpMethod("POST");

                    // Form the request.
                    RequestMessage = new HttpRequestMessage(ApiMethod, Uri) { Content = RequestMessageContent };

                    // Send.
                    ResponseMessage = client.SendAsync(RequestMessage).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        taq = DeserializeResponseToObject<TaskAgentResponse.TaskAgentQueue>();
                        queueId = taq.Id;
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

            // Return the queue identifier created.
            return queueId;
        }

        public Dictionary<string, int> GetQueuesDictionary()
        {
            // Initialize.
            Dictionary<string, int> queuesAsDictionary = new Dictionary<string, int>();
            TaskAgentResponse.TaskAgentQueues taqs = new TaskAgentResponse.TaskAgentQueues();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/distributedtask/queues?api-version={Version}");

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

                        // Regenerate object.
                        taqs = DeserializeResponseToObject<TaskAgentResponse.TaskAgentQueues>();

                        if (taqs != null && taqs.Value != null)
                            foreach (var queue in taqs.Value)
                                queuesAsDictionary[queue.Name] = queue.Id;
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

            // Return all queues as dictionary.
            return queuesAsDictionary;
        }
    }
}