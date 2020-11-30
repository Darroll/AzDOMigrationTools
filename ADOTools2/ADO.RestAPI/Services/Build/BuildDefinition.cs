using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.Build
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Build Definitions
    /// <para>
    /// Documentation about Azure DevOps\Build\Definitions can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/build/?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class BuildDefinition : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.BuildDefinition"));

        #endregion

        #endregion

        public BuildDefinition(IConfiguration configuration) : base(configuration) { }

        public BuildMinimalResponse.BuildDefinitionReference CreateBuildDefinition(string jsonContent)
        {
            // Initialize.
            BuildMinimalResponse.BuildDefinitionReference bd = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/build/definitions?api-version={Version}");

                // todo: use the json model to extract definition.
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
                        bd = DeserializeResponseToObject<BuildMinimalResponse.BuildDefinitionReference>();
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

            // Return build definition created.
            return bd;
        }

        public BuildMinimalResponse.BuildDefinitionReferences GetAllBuildDefinitions()
        {
            // Initialize.
            BuildMinimalResponse.BuildDefinitionReferences bds = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/build/definitions?api-version={Version}");

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
                        bds = DeserializeResponseToObject<BuildMinimalResponse.BuildDefinitionReferences>();
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

            // Return all build definitions.
            return bds;
        }

        public BuildMinimalResponse.BuildDefinitionReference GetBuildDefinition(int id)
        {
            // Initialize.
            BuildMinimalResponse.BuildDefinitionReference bd = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/build/definitions/{id}?api-version={Version}");

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
                        bd = DeserializeResponseToObject<BuildMinimalResponse.BuildDefinitionReference>();
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

            // Return build definition.
            return bd;
        }

        public List<JToken> GetBuildDefinitionsAsJToken()
        {
            // Initialize.
            JToken j;
            List <JToken> definitionsAsJToken = new List<JToken>();
            BuildMinimalResponse.BuildDefinitionReferences bds = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/build/definitions?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        // Deserialize.
                        // The build definition won't contain the complete set of properties.
                        bds = DeserializeResponseToObject<BuildMinimalResponse.BuildDefinitionReferences>();

                        if (bds.Count > 0)
                        {
                            foreach (var buildDefinition in bds.Value)
                            {
                                // Define uri to call.
                                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/build/definitions/{buildDefinition.Id}?api-version={Version}");

                                using (var client2 = GetHttpClient())
                                {
                                    // Send.
                                    ResponseMessage = client2.GetAsync(Uri).Result;

                                    if (ValidateServiceCall())
                                    {
                                        SetSuccessfulCRUDOperation();

                                        // It must use special instructions via the JsonSerializerSettings to recreate types when deserializing. 
                                        j = DeserializeResponseToObject<JToken>();

                                        // Add to list.
                                        definitionsAsJToken.Add(j);
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

            // Return the list of JTokens.
            return definitionsAsJToken;
        }        
    }
}