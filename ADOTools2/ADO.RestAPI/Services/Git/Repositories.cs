using System;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.Git
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Repositories
    /// <para>
    /// Documentation about Azure DevOps\Git\Repositories can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/git/?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class Repositories : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.Git"));

        #endregion

        #endregion

        public Repositories(IConfiguration configuration) : base(configuration) { }

        public GitMinimalResponse.GitRepository CreateRepository(string repositoryName)
        {
            // Initialize.
            GitMinimalResponse.GitRepository repository = null;

            try
            {
                // Create the request message from repository name and project identifier.
                object requestBody = new
                {
                    name = repositoryName,
                    project = new { id = ProjectId }
                };
                JsonRequestBody = JsonConvert.SerializeObject(requestBody);

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/git/repositories?api-version={Version}");

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
                        repository = DeserializeResponseToObject<GitMinimalResponse.GitRepository>();
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

            // Return repository created.
            return repository;
        }

        public bool DeleteRepository(string repositoryId)
        {
            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/git/repositories/{repositoryId}?api-version={Version}");

                using (var client = GetHttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PersonalAccessToken))));

                    // This api is called with a DELETE method.
                    ApiMethod = new HttpMethod("DELETE");

                    // Form the request.
                    RequestMessage = new HttpRequestMessage(ApiMethod, Uri);

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

        // Get Repository list to create service end point json and import source code json
        public GitMinimalResponse.GitRepositories GetAllRepositories()
        {
            // Initialize.
            GitMinimalResponse.GitRepositories listOfRepositories = null;

            try
            {
                // Define uri to call.
                // Include hidden repos to get project wiki repositories which are hidden
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/git/repositories?includeHidden=true&api-version={Version}");

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
                        listOfRepositories = DeserializeResponseToObject<GitMinimalResponse.GitRepositories>();
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

            // Return the list of repositories (just ids and names).
            return listOfRepositories;
        }

        public GitResponse.GitRepositories GetAllRepositoriesFullDetails()
        {
            // Initialize.
            GitResponse.GitRepositories listOfRepositories = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/git/repositories?api-version={Version}");

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
                        listOfRepositories = DeserializeResponseToObject<GitResponse.GitRepositories>();
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

            // Return the list of repositories (just ids and names).
            return listOfRepositories;
        }

        public void GetSourceCodeFromGit(string jsonContent, string repositoryId)
        {
            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/git/repositories/{repositoryId}/importRequests?api-version={Version}");

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
        }

        public GitMinimalResponse.GitRepository GetRepositoryById(string repositoryId)
        {
            // Get all repositories.
            GitMinimalResponse.GitRepositories allGitRepositories = GetAllRepositories();

            // Look for one only.
            GitMinimalResponse.GitRepository repository = allGitRepositories.Value.Where(x => x.Id == repositoryId).FirstOrDefault();

            // Return the repository.
            return repository;
        }

        public string GetRepositoryIdByName(string repositoryName)
        {
            // Initialize.
            string repositoryIdentifier = null;

            // Get all repositories.
            GitMinimalResponse.GitRepositories allGitRepositories = GetAllRepositories();

            // Look for one only.
            GitMinimalResponse.GitRepository repository = allGitRepositories.Value.Where(x => x.Name.ToLower() == repositoryName.ToLower()).FirstOrDefault();

            // If it exists, get identifier.
            if (repository != null)
                repositoryIdentifier = repository.Id;

            // Return the repository identifier.
            return repositoryIdentifier;
        }

        public GitMinimalResponse.GitRepository GetDefaultRepository()
        {
            // Get all repositories.
            GitMinimalResponse.GitRepositories allGitRepositories = GetAllRepositories();
            GitMinimalResponse.GitRepository repository = null;

            // Look for the first/default one.
            if (allGitRepositories.Count > 0)
                repository = allGitRepositories.Value.FirstOrDefault();

            // Return repository found.
            return repository;
        }

        public GitMinimalResponse.GitRepository UpdateRepository(string repositoryName, string jsonContent)
        {
            // Initialize.
            string repositoryId;
            GitMinimalResponse.GitRepository repository = null;

            // Retrieve repository identifier.
            repositoryId = GetRepositoryIdByName(repositoryName);

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/{EncodedProject}/_apis/git/repositories/{repositoryId}?api-version={Version}");

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
                        repository = DeserializeResponseToObject<GitMinimalResponse.GitRepository>();
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

            // Return Git repository details.
            return repository;
        }
    }
}