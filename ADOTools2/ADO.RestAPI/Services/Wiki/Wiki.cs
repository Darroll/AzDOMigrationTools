using System;
using System.Diagnostics;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.Wiki
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Wikis
    /// <para>
    /// Documentation for the Wiki rest api can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/wikis?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class Wikis : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.Wikis"));

        #endregion

        #endregion

        #region - Public Members

        public Wikis(IConfiguration configuration) : base(configuration) { }

        public WikiResponse.Wiki CreateWiki(string jsonContent)
        {
            // Initialize.
            WikiResponse.Wiki nw = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wiki/wikis?api-version={Version}");

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
                        nw = DeserializeResponseToObject<WikiResponse.Wiki>();
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

            // Return wiki created.
            return nw;
        }

        public WikiResponse.Wikis GetWikis()
        {
            // Initialize.
            WikiResponse.Wikis wikis = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wiki/wikis?api-version={Version}");

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
                        wikis = DeserializeResponseToObject<WikiResponse.Wikis>();
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

            // Return wikis if any.
            return wikis;
        }

        public JToken GetWikisAsJToken()
        {
            // Initialize.
            JToken j = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wiki/wikis?api-version={Version}");

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

            // Return wikis as a list of JToken.
            return j;
        }

        public WikiResponse.Wiki UpdateWiki(string jsonContent, string wikiId)
        {
            // Initialize.
            WikiResponse.Wiki uw = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wiki/wikis/{wikiId}?api-version={Version}");

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
                        uw = DeserializeResponseToObject<WikiResponse.Wiki>();
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

            // Return wiki updated.
            return uw;
        }

        #endregion
    }
}