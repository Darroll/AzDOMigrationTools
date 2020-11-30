using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;

namespace ADO.RestAPI.Security
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Security Namespaces
    /// <para>
    /// Documentation about Azure DevOps\Security\Security Namespaces can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/security/?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class SecurityNamespaces : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.SecurityNamespaces"));

        #endregion

        #endregion

        public SecurityNamespaces(IConfiguration configuration) : base(configuration) { }

        public SecurityResponse.SecurityNamespaces GetSecurityNamespaces()
        {
            // Initialize.
            SecurityResponse.SecurityNamespaces ns = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/securitynamespaces?api-version={Version}");

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
                        ns = DeserializeResponseToObject<SecurityResponse.SecurityNamespaces>();
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

            // Return security namespaces.
            return ns;
        }

        public SecurityResponse.SecurityNamespaces GetSecurityNamespaceById(string securityNamespaceId)
        {
            // Initialize.
            SecurityResponse.SecurityNamespaces n = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/securitynamespaces/{securityNamespaceId}?api-version={Version}");

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
                    // It returns one namespace but still returns it as an enumerable collection.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        n = DeserializeResponseToObject<SecurityResponse.SecurityNamespaces>();
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

            // Return one security namespace.
            return n;
        }
    }
}