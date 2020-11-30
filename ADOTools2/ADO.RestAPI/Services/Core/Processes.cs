using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;

namespace ADO.RestAPI.Core
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Processes
    /// <para>
    /// Documentation about Azure DevOps\Core\Processes can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/core/processes?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class Processes : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.Processes"));

        #endregion

        #endregion

        #region - Private Members

        #endregion

        #region - Public Members

        public Processes(IConfiguration configuration) : base(configuration) { }

        public CoreResponse.Processes GetProcesses()
        {
            // Initialize.
            CoreResponse.Processes processes = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/process/processes?api-version={Version}");

                using (HttpClient client = new HttpClient())
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
                        processes = DeserializeResponseToObject<CoreResponse.Processes>();
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

            // Return processes.
            return processes;

            #endregion
        }


        private AuthenticationHeaderValue SetBasicAuthorizationHeaderValue(string PAT)
        {
            string identifier = string.Format("{0}:{1}", string.Empty, PAT);
            Byte[] rawBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(identifier);
            string authorization = Convert.ToBase64String(rawBytes);

            // Define an authentication header value.
            AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", authorization);

            // Return it.
            return ahv;
        }
    }
}