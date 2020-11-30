using System;
using System.Diagnostics;
using System.Net.Http;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Text;
using System.Net.Http.Headers;

namespace ADO.RestAPI.Queries
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Queries
    /// <para>
    /// Documentation about Azure DevOps\Work Item Tracking can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class Queries : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.Queries"));

        #endregion

        #endregion

        public Queries(IConfiguration configuration) : base(configuration) { }
        
        public WorkItemTrackingResponse.QueryHierarchyItem GetQueries(string path, int depth)
        {
            // Initialize.
            WorkItemTrackingResponse.QueryHierarchyItem qhi = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/queries{path}?$depth={depth}&api-version={Version}");

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
                        qhi = DeserializeResponseToObject<WorkItemTrackingResponse.QueryHierarchyItem>();
                    }
                    else
                        throw (new RecoverableException(LastApiErrorMessage));
                }
            }
            catch (RecoverableException ex)
            {
                // Send some traces.
                //"TF401243: The query Shared Queries/Demo/Migrations/SmartHotel360 does not exist, or you do not have permission to read it."
                if (ex.Message.StartsWith(Constants.ErrorTF401243))
                {
                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, ex.Message);
                }
                else
                {
                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                }
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();

                throw;
            }

            // Return query item.
            return qhi;
        }           

        public WorkItemTrackingResponse.QueryHierarchyItem CreateQueryFolder(string jsonContent, string path = null)
        {
            // Initialize.
            WorkItemTrackingResponse.QueryHierarchyItem node = null;

            try
            {
                // Define uri to call.
                if (string.IsNullOrEmpty(path))
                    SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/queries?api-version={Version}");
                else
                    SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/wit/queries{path}?api-version={Version}");

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

                    // Send.
                    ResponseMessage = client.PostAsync(Uri, RequestMessageContent).Result;

                    // Regenerate object.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        node = DeserializeResponseToObject<WorkItemTrackingResponse.QueryHierarchyItem>();
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

            // Return classification node created.
            return node;
        }

        public string GenerateQueryFolderToken(string queryFolderPath)
        {
            // Initialize.
            int depth = 0;
            string path = string.Empty;
            string queryFolderToken;
            string[] tokens;

            // Generate the query folder token.
            queryFolderToken = string.Format(@"$/{0}", ProjectId);

            // Extract tokens.
            tokens = queryFolderPath.Split(new char[] { '/', '\\' });
            
            foreach (string token in tokens)
            {
                // Add to path.
                path += string.Format(@"/{0}", token);

                // Get queries for that path.
                var qhi = GetQueries(path, depth);

                // Add to token.
                queryFolderToken += string.Format(@"/{0}", qhi.Id);
            }

            // Return token.
            return queryFolderToken;
        }
    }
}