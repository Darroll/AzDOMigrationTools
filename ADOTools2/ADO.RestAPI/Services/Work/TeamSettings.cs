using System;
using System.Diagnostics;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.Work
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Capacities,
    ///   - Iterations,
    ///   - Teamdaysoff,
    ///   - Teamfieldvalues,
    ///   - Teamsettings
    /// <para>
    /// Documentation about Azure DevOps\Work can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/work/?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class TeamSettings : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("AzureDevOps.RestAPI.Services.Work.TeamSettings"));

        #endregion

        #endregion

        public TeamSettings(IConfiguration configuration) : base(configuration) { }

        // Export Team settings
        public JToken GetTeamSettingsAsJToken(bool ignoreNullValue = false)
        {
            // Initialize.
            JToken j = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/teamsettings?api-version={Version}");

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
                        j = DeserializeResponseToObject<JToken>(ignoreNullValue);
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

            // Return team settings as JToken.
            return j;
        }

        public JToken GetTeamIterationsAsJToken(bool ignoreNullValue = false)
        {
            // Initialize.
            JToken j = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/teamsettings/iterations?api-version={Version}");

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
                        j = DeserializeResponseToObject<JToken>(ignoreNullValue);
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

            // Return the team iterations as JToken.
            return j;
        }

        public JToken GetTeamAreasAsJToken(bool ignoreNullValue = false)
        {
            // Initialize.
            JToken j = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/teamsettings/teamfieldvalues?api-version={Version}");

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
                        j = DeserializeResponseToObject<JToken>(ignoreNullValue);
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

            // Return the team areas as JToken.
            return j;
        }

        public bool UpdateArea(string jsonContent)
        {
            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/teamsettings/teamfieldvalues?api-version={Version}");

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

                    if (ValidateServiceCall())
                        SetSuccessfulCRUDOperation();
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

            // Return operation status.
            return CRUDOperationSuccess;
        }

        public bool UpdateIteration(string iterationId)
        {
            // Initialize.
            object requestBody;

            try
            {
                // Genereate the request message as object.
                // todo: should change this way of forming the message.
                requestBody = new { id = iterationId };
                JsonRequestBody = JsonConvert.SerializeObject(requestBody);

                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/teamsettings/iterations?api-version={Version}");

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
                        SetSuccessfulCRUDOperation();
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

            // Return operation status.
            return CRUDOperationSuccess;
        }
    }
}