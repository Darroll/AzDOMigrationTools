using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.Security
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Access Control Entries
    /// <para>
    /// Documentation about Azure DevOps\Security\Access Control Entries can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/security/?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class AccessControlEntries : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.Security.AccessControlEntries"));

        #endregion

        #endregion

        public AccessControlEntries(IConfiguration configuration) : base(configuration) { }

        public SecurityResponse.AccessControlEntries SetAccessControlEntries(string securityNamespaceId, string securityToken, bool mergeBehavior, IEnumerable<SecurityResponse.AccessControlEntry> aces)
        {
            // Instantiate the ACEs object to enable method to read the ACEs dictionary.
            SecurityResponse.AccessControlEntries appliedACEs = new SecurityResponse.AccessControlEntries();
            
            try
            {
                // Create the request body.
                object requestBody = new
                {
                    token = securityToken,
                    merge = mergeBehavior,
                    accessControlEntries = aces
                };
                JsonRequestBody = JsonConvert.SerializeObject(requestBody);

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/accesscontrolentries/{securityNamespaceId}?api-version={Version}");

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

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        appliedACEs = DeserializeResponseToObject<SecurityResponse.AccessControlEntries>();
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

            // Return the applied aces.
            return appliedACEs;
        }
    }
}