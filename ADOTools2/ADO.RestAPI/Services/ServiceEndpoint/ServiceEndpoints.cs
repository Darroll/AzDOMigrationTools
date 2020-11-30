using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.Service
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Service Endpoints
    /// <para>
    /// Documentation about Azure DevOps\Service Endpoint\Service Endpoints can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/serviceendpoint/endpoints?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class ServiceEndPoints : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.ServiceEndPoint"));

        #endregion

        #endregion

        public ServiceEndPoints(IConfiguration configuration) : base(configuration) { }

        public ServiceEndpointResponse.ServiceEndpoint CreateServiceEndPoint(string jsonContent)
        {
            // Initialize.
            ServiceEndpointResponse.ServiceEndpoint se = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/serviceendpoint/endpoints?api-version={Version}");

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
                        se = DeserializeResponseToObject<ServiceEndpointResponse.ServiceEndpoint>();
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

            // Return the endpoint.
            return se;
        }

        public ServiceEndpointResponse.ServiceEndpoint GetServiceEndpointByName(string endpointName)
        {
            // Initialize.
            ServiceEndpointResponse.ServiceEndpoint se = null;
            ServiceEndpointResponse.ServiceEndpoints ses = null;

            try
            {
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/serviceendpoint/endpoints?endpointNames={endpointName}&api-version={Version}");

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
                        ses = DeserializeResponseToObject<ServiceEndpointResponse.ServiceEndpoints>();
                    }
                    else
                        throw (new RecoverableException(LastApiErrorMessage));

                    if (ses.Count > 0)
                        se = ses.Value[0];
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

            // Return service endpoint.
            return se;
        }

        public ServiceEndpointResponse.ServiceEndpoints GetServiceEndpoints()
        {
            // Initialize.
            ServiceEndpointResponse.ServiceEndpoints ses = null;        // Incomplete service endpoints.
            ServiceEndpointResponse.ServiceEndpoint dse;                // Detailed service endpoint.

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/serviceendpoint/endpoints?api-version={Version}");

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
                        ses = DeserializeResponseToObject<ServiceEndpointResponse.ServiceEndpoints>();

                        // If any endpoints exist.
                        if (ses.Count > 0)
                        {
                            // Need to loop over every service endpoint because the rest api to get all endpoints doesn't include all details
                            var dses = new List<ServiceEndpointResponse.ServiceEndpoint>();

                            foreach (ServiceEndpointResponse.ServiceEndpoint se in ses.Value)
                            {
                                // Define uri to call.
                                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/serviceendpoint/endpoints/{se.Id}?api-version={Version}");

                                // Send.
                                ResponseMessage = client.GetAsync(Uri).Result;

                                if (ValidateServiceCall())
                                {
                                    SetSuccessfulCRUDOperation();

                                    // Regenerate object.
                                    dse = DeserializeResponseToObject<ServiceEndpointResponse.ServiceEndpoint>();

                                    // Add to list.
                                    dses.Add(dse);
                                }
                                else
                                    throw (new RecoverableException(LastApiErrorMessage));
                            }

                            // Replace service endpoints information with more detailed one.
                            ses.Value = dses;
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

            // Return service endpoints.
            return ses;
        }
    }
}