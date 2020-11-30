using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.Work
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Boardcolumns,
    ///   - Boardparents,
    ///   - Boardrows,
    ///   - Boards,
    ///   - Boardusersettings,
    ///   - Cardrulesettings,
    ///   - Cardsettings,
    ///   - Charts,
    ///   - Columns,
    ///   - Rows
    /// <para>
    /// Documentation about Azure DevOps\Work can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/work/?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class BoardConfiguration : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("AzureDevOps.RestAPI.Services.Work.BoardConfiguration"));

        #endregion

        #endregion

        public BoardConfiguration(IConfiguration configuration) : base(configuration) { }

        public JToken GetBoardColumnsAsJToken(string boardType, bool ignoreNullValue = false)
        {
            // Initialize.
            JToken j = null;
            boardType = boardType.ToLower();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/boards/{boardType}/columns?api-version={Version}");

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

            // Return the board columns for a given board as JToken.
            return j;
        }

        public JToken GetBoardRowsAsJToken(string boardType, bool ignoreNullValue = false)
        {
            // Initialize.
            JToken j = null;
            boardType = boardType.ToLower();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/boards/{boardType}/rows?api-version={Version}");

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

            // Return the board rows for a given board as JToken.
            return j;
        }

        public JToken GetCardFieldSettingsAsJToken(string boardType, bool ignoreNullValue = false)
        {
            // Initialize.
            JToken j = null;
            boardType = boardType.ToLower();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/boards/{boardType}/cardsettings?api-version={Version}");

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

            // Return the card field settings for a given board as JToken.
            return j;
        }

        public JToken GetCardStyleSettingsAsJToken(string boardType, bool ignoreNullValue = false)
        {
            // Initialize.
            JToken j = null;
            boardType = boardType.ToLower();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/boards/{boardType}/cardrulesettings?api-version={Version}");

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

            // Return the card style settings for a given board as JToken.
            return j;
        }
        
        public bool UpdateBoardColumns(string jsonContent, string boardType)
        {
            // Initialize.
            boardType = boardType.ToLower();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/boards/{boardType}/columns?api-version={Version}");

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

                    // Send.
                    ResponseMessage = client.PutAsync(Uri, RequestMessageContent).Result;

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

        public bool UpdateBoardRows(string jsonContent, string boardType)
        {
            // Initialize.
            boardType = boardType.ToLower();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/boards/{boardType}/rows?api-version={Version}");

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

                    // Send.
                    ResponseMessage = client.PutAsync(Uri, RequestMessageContent).Result;

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

        public bool UpdateCardFields(string jsonContent, string boardType)
        {
            // Initialize.
            boardType = boardType.ToLower();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/boards/{boardType}/cardsettings?api-version={Version}");

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

                    // Send.
                    ResponseMessage = client.PutAsync(Uri, RequestMessageContent).Result;

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

        public bool UpdateCardStyles(string jsonContent, string boardType)
        {
            // Initialize.
            boardType = boardType.ToLower();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/{Team}/_apis/work/boards/{boardType}/cardrulesettings?api-version={Version}");

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

        public bool UpdateTeamSettings(string jsonContent)
        {
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
    }
}