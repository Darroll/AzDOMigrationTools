using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.Http;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.Security
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Access Control Lists
    /// <para>
    /// Documentation about Azure DevOps\Security\Access Control Lists can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/security/?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class AccessControlLists : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.AccessControlLists"));

        #endregion

        #endregion

        public AccessControlLists(IConfiguration configuration) : base(configuration) { }

        public SecurityResponse.AccessControlLists GetAccessControlList(string securityNamespaceId)
        {
            // Instantiate the ACLS object to enable method to read the list of ACLs.
            SecurityResponse.AccessControlLists acls = new SecurityResponse.AccessControlLists();

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Fetch ACLs for security namespace id: {0}.", securityNamespaceId);
                _mySource.Value.Flush();

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/accesscontrollists/{securityNamespaceId}?includeExtendedInfo=true&api-version={Version}");

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
                        acls = DeserializeResponseToObject<SecurityResponse.AccessControlLists>();
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

            // Return the ACLs.
            return acls;
        }

        public SecurityResponse.AccessControlList GetAccessControlList(string securityNamespaceId, string securityToken)
        {
            // Initialize.
            SecurityResponse.AccessControlLists acls = null;
            SecurityResponse.AccessControlList acl = new SecurityResponse.AccessControlList();

            try
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Fetch ACLs for security namespace id: {0}.", securityNamespaceId);
                _mySource.Value.Flush();

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/accesscontrollists/{securityNamespaceId}?includeExtendedInfo=true&token={securityToken}&api-version={Version}");

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
                        acls = DeserializeResponseToObject<SecurityResponse.AccessControlLists>();
                        acl = acls.Value.First();
                    }
                    else
                        throw (new RecoverableException(LastApiErrorMessage));
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Error while getting {0}: {1}, Stack Trace: {2}", "ACEs", ex.Message, ex.StackTrace);

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMsg);
                _mySource.Value.Flush();
            }

            // Return acl found.
            return acl;
        }
        public void UpdateAccessControlList(string securityNamespaceId, string jsonContent)
        {

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/accesscontrollists/{securityNamespaceId}?api-version={Version}");

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
                        _mySource.Value.TraceInformation($"Successfully updated ACL for {securityNamespaceId}");
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
        }
    }
}