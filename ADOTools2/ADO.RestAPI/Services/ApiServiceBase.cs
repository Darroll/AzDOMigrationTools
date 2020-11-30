using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ADO.Tools;

namespace ADO.RestAPI
{
    /// <summary>
    /// Abstract class for managing objects and performing operations with Azure DevOps.
    /// </summary>
    public abstract class ApiServiceBase : IConfiguration
    {
        #region - Static Declarations

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Utility"));

        #endregion

        #region - Private Members.

        /// <summary>
        /// Return the continuation token from REST api call.
        /// </summary>
        /// <returns>string</returns>
        public string GetContinuationToken(HttpResponseMessage response)
        {
            // Initialize.
            string continuationToken = null;

            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
            {
                if (header.Key == "X-MS-ContinuationToken")
                    continuationToken = header.Value.FirstOrDefault();
            }

            // Return the continuation token.
            return continuationToken;
        }

        /// <summary>
        /// Retrieve the error message from a JSON message.
        /// </summary>
        /// <returns>string</returns>
        public string GetErrorMessageFromJsonContent(string jsonContent)
        {
            // Initialize.
            string errorMsg = string.Empty;

            try
            {
                // Only care if...
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    JToken j = JToken.Parse(jsonContent);

                    // Proceed only if there is a message property.
                    if (j["message"] != null)
                        errorMsg = j["message"].ToString();
                }
            }
            catch (Exception ex)
            {
                // todo: eventually see if we can better understand the type of problem
                // returned from a REST api call.

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                _mySource.Value.Flush();
            }

            // Return error message.
            return errorMsg;
        }

        private AuthenticationHeaderValue SetBasicAuthorizationHeaderValue(string PAT)
        {
            string identifier = string.Format("{0}:{1}", string.Empty, PAT);
            Byte[] rawBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(identifier);
            string authorization = Convert.ToBase64String(rawBytes);

            // Define an authentication header value.
            AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", authorization);

            // Return it.
            return ahv;
        }

        public string ValidateUriPath(string uri)
        {
            // Validate if the uri local path contains double slashes 
            // that would indicate a missing parameter in the path.
            Uri testUri = new Uri(uri, UriKind.Absolute);
            if (testUri.LocalPath.Contains(@"//"))
                throw (new ArgumentException($"Malformed uri: {uri}"));

            // Remove trailing slash.
            if (uri.EndsWith(@"/"))
                uri = uri.Substring(0, uri.Length - 1);

            // Return the validated uri.
            return uri;
        }

        private void SetUriInternal(string resourceString, object[] args)
        {
            // Define the uri to call the api.
            string nonValidatedUri = string.Format(resourceString, args);

            // Validate uri.
            Uri = this.ValidateUriPath(nonValidatedUri);
        }

        #endregion

        #region - Protected Members.

        #region - Properties.

        protected HttpMethod ApiMethod { get; set; }

        protected string ContinuationToken
        {
            get
            {
                // Initialize.
                string token = null;

                if (this.ResponseMessage != null)
                    token = this.GetContinuationToken(this.ResponseMessage);

                // Return token.
                return token;
            }
        }

        protected string EncodedProject
        {
            get
            {
                // Initialize.
                string encodedValue = null;

                // Encode this parameter.
                if (!string.IsNullOrEmpty(Project))
                    encodedValue = System.Uri.EscapeUriString(Project);

                return encodedValue;
            }
        }

        protected bool HasContinuationToken
        {
            get
            {
                return (this.ContinuationToken != null);
            }
        }

        protected string RawResponseMessage
        {
            get
            {
                // Initialize.
                string rawContent = null;

                // Read the raw message as it is a string.
                if (ResponseMessage != null)
                    rawContent = this.ResponseMessage.Content.ReadAsStringAsync().Result;

                // Return raw content.
                return rawContent;
            }
        }

        protected HttpResponseMessage ResponseMessage { get; set; }

        protected HttpRequestMessage RequestMessage { get; set; }

        protected StringContent RequestMessageContent { get; set; }

        protected string Uri { get; set; }

        #endregion

        #region - Methods

        protected T DeserializeResponseToObject<T>(bool ignoreNullValue = false, bool handleType = false)
        {
            // Initialize.
            T generatedObject;
            bool needJsonSerializerSettings = ignoreNullValue || handleType;

            // Deserialize.
            if (needJsonSerializerSettings)
            {
                // Initialize settings.
                JsonSerializerSettings jsonSerializerNullSetting = new JsonSerializerSettings();

                // Adjust settings to ignore null values.
                if (ignoreNullValue)
                    jsonSerializerNullSetting.NullValueHandling = NullValueHandling.Ignore;

                // Adjust settings to handle serialized types.
                if (handleType)
                    jsonSerializerNullSetting.TypeNameHandling = TypeNameHandling.Auto;

                // Deserialize.
                generatedObject = JsonConvert.DeserializeObject<T>(this.RawResponseMessage, jsonSerializerNullSetting);
            }
            else
                generatedObject = JsonConvert.DeserializeObject<T>(this.RawResponseMessage);

            // Return the generated object from deserialization process.
            return generatedObject;
        }

        protected HttpClient GetHttpClient()
        {
            // Instantiate and set base address.
            HttpClient client = new HttpClient { BaseAddress = new Uri(BaseUri) };

            // Clear default headers.
            client.DefaultRequestHeaders.Accept.Clear();

            // Add media type.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Add authorization.
            client.DefaultRequestHeaders.Authorization = this.SetBasicAuthorizationHeaderValue(PersonalAccessToken);

            // Return newly formed http client.
            return client;
        }

        protected void SetHttpContentForRequestMessageBody(string jsonContent, Encoding encoding, string mediaType = @"application/json")
        {
            // Generate the request message with encoding and media type.
            this.RequestMessageContent = new StringContent(jsonContent, encoding, mediaType);
        }

        protected void SetSuccessfulCRUDOperation()
        {
            // For any methods that are not GET, set the operation status to true.
            if (this.ApiMethod.Method.ToLower() != "get")
                this.CRUDOperationSuccess = true;
        }

        protected void SetServiceUri(string nonValidatedUri)
        {
            // Validate uri.
            this.Uri = this.ValidateUriPath(nonValidatedUri);
        }

        protected void SetServiceUri(string resourceString, object arg0)
        {
            // Initialize.
            object[] args = new object[1] { arg0 };

            // Define the uri to call the api.
            this.SetUriInternal(resourceString, args);
        }

        protected void SetServiceUri(string resourceString, object arg0, object arg1)
        {
            // Initialize.
            object[] args = new object[2] { arg0, arg1 };

            // Define the uri to call the api.
            this.SetUriInternal(resourceString, args);
        }

        protected void SetServiceUri(string resourceString, object arg0, object arg1, object arg2)
        {
            // Initialize.
            object[] args = new object[3] { arg0, arg1, arg2 };

            // Define the uri to call the api.
            this.SetUriInternal(resourceString, args);
        }

        protected void SetServiceUri(string resourceString, object arg0, object arg1, object arg2, object arg3)
        {
            // Initialize.
            object[] args = new object[4] { arg0, arg1, arg2, arg3 };

            // Define the uri to call the api.
            this.SetUriInternal(resourceString, args);
        }

        protected void SetServiceUri(string resourceString, object arg0, object arg1, object arg2, object arg3, object arg4)
        {
            // Initialize.
            object[] args = new object[5] { arg0, arg1, arg2, arg3, arg4 };

            // Define the uri to call the api.
            this.SetUriInternal(resourceString, args);
        }

        protected void SetServiceUri(string resourceString, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            // Initialize.
            object[] args = new object[6] { arg0, arg1, arg2, arg3, arg4, arg5 };

            // Define the uri to call the api.
            this.SetUriInternal(resourceString, args);
        }

        protected bool ValidateServiceCall(IEnumerable<HttpStatusCode> statusCodesToValidate = null)
        {
            // Initialize.
            bool successFlag = false;
            bool foundCode = false;

            // Reset CRUD operation success flag.
            this.CRUDOperationSuccess = false;

            if (this.ResponseMessage != null)
            {
                if (statusCodesToValidate == null)
                    successFlag = this.ResponseMessage.IsSuccessStatusCode;
                else
                {
                    if (this.ResponseMessage.IsSuccessStatusCode)
                    {
                        foreach (HttpStatusCode statusCode in statusCodesToValidate)
                        {
                            // Have you found the code?
                            if (this.ResponseMessage.StatusCode == statusCode)
                            {
                                foundCode = true;
                                break;
                            }
                        }

                        if (foundCode)
                            successFlag = true;
                    }
                    else
                        successFlag = false;
                }
            }

            // Return flag.
            return successFlag;
        }

        #endregion

        #endregion

        #region - Public Members.

        #region - Properties.

        #region - Properties implemented via the IConfiguration interface.

        public string BaseUri { get; set; }

        public string Collection { get; set; }

        public string PersonalAccessToken { get; set; }

        public string Project { get; set; }

        public string Version { get; set; }

        #endregion

        public bool CRUDOperationSuccess { get; private set; }

        public string JsonRequestBody { get; set; }

        public string LastApiErrorMessage {
            get
            {
                // Initialize.
                string errorMsg = null;

                // Extract the message.
                if(this.RawResponseMessage != null)
                    errorMsg = this.GetErrorMessageFromJsonContent(this.RawResponseMessage);

                // Return message found in the raw response.
                return errorMsg;
            }
        }

        public string ProjectId { get; set; }

        public string Team { get; set; }

        #endregion

        #region - Constructors

        public ApiServiceBase(IConfiguration configuration)
        {
            // Set properties brought by the IConfiguration interface.
            this.BaseUri = configuration.BaseUri;
            this.Collection = configuration.Collection;
            this.PersonalAccessToken = configuration.PersonalAccessToken;
            this.Project = configuration.Project;
            this.Version = configuration.Version;

            // Set default REST method.
            this.ApiMethod = new HttpMethod("GET");

            // CRUD operation success flag is set as false by default.
            this.CRUDOperationSuccess = false;

            // Set uri to call with the base uri.
            this.Uri = this.BaseUri;
        }

        #endregion

        #endregion
    }
}