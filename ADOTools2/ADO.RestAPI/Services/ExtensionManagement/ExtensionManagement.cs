using System;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.ExtensionManagement
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Extension Management
    /// <para>
    /// Documentation about Azure DevOps\Extension Management\Installed Extensions can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/extensionmanagement/installed%20extensions?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class ExtensionManagement : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.ExtensionManagement"));

        #endregion

        #endregion

        public ExtensionManagement(IConfiguration configuration) : base(configuration) { }

        public ExtentionMgmtMinimalResponse.InstalledExtension InstallExtension(string publisherId, string extensionId, string extensionVersion)
        {
            // Initialize.
            ExtentionMgmtMinimalResponse.InstalledExtension extension = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/extensionmanagement/installedextensionsbyname/{publisherId}/{extensionId}/{extensionVersion}?api-version={Version}");

                using (var client = GetHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PersonalAccessToken))));

                    // This api is called with a POST method.
                    ApiMethod = new HttpMethod("POST");

                    // Form the request.
                    RequestMessage = new HttpRequestMessage(ApiMethod, Uri);

                    // Send.
                    ResponseMessage = client.SendAsync(RequestMessage).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        extension = DeserializeResponseToObject<ExtentionMgmtMinimalResponse.InstalledExtension>();
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

            // Return the just installed extension details.
            return extension;
        }

        public ExtentionMgmtMinimalResponse.InstalledExtensions GetInstalledExtensions()
        {
            // Initialize.
            ExtentionMgmtMinimalResponse.InstalledExtensions extensions = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/extensionmanagement/installedextensions?api-version={Version}");

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
                        extensions = DeserializeResponseToObject<ExtentionMgmtMinimalResponse.InstalledExtensions>();
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

            // Return the installed extensions.
            return extensions;
        }

        public JToken GetInstalledExtensionsAsJToken()
        {
            // Initialize.
            JToken extensionsAsJToken = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/extensionmanagement/installedextensions?api-version={Version}");

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
                        extensionsAsJToken = DeserializeResponseToObject<JToken>();
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

            // Return the installed extensions as JToken.
            return extensionsAsJToken;
        }
    }
}