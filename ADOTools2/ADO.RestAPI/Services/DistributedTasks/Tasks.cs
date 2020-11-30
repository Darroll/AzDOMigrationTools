using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;

namespace ADO.RestAPI.DistributedTasks
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Tasks
    /// <para>
    /// Documentation is nowhere.
    /// </para>
    /// </summary>
    public class Tasks : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.DistributedTasks"));

        #endregion

        #endregion

        public Tasks(IConfiguration configuration) : base(configuration) { }

        public DistributedTaskMinimalResponse.Tasks GetPipelineTasks()
        {
            // Initialize.
            DistributedTaskMinimalResponse.Tasks tsks = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/distributedtask/tasks?api-version={Version}");

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
                        tsks = DeserializeResponseToObject<DistributedTaskMinimalResponse.Tasks>();
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

            // Return tasks.
            return tsks;
        }
    }
}