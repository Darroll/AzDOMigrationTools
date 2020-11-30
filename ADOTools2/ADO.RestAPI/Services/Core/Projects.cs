using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using ADO.RestAPI.Viewmodel50;
using ADO.Tools;
using System.Net.Http.Headers;

namespace ADO.RestAPI.Core
{
    /// <summary>
    /// Class for managing objects and performing operations with Azure DevOps
    /// on these categories:
    ///   - Projects
    /// <para>
    /// Documentation about Azure DevOps\Core\Project can be found <a href="https://docs.microsoft.com/en-us/rest/api/azure/devops/core/projects?view=azure-devops-rest-5.0">here</a>.
    /// </para>
    /// </summary>
    public class Projects : ApiServiceBase
    {
        #region - Static Declarations

        #region - Private Members

        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.RestAPI.Services.Projects"));

        #endregion

        #endregion

        #region - Private Members

        private bool TestIfProjectExist(string projectNameOrId)
        {
            // Initialize.
            bool isValidGuid = false;
            bool existFlag = false;

            try
            {
                // Try to determine if a project name or project identifier was passed.
                isValidGuid = Guid.TryParse(projectNameOrId, out Guid projectGuid);

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/projects/{projectNameOrId}?api-version={Version}");

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
                        existFlag = true;
                    }
                    else
                        existFlag = false;
                }
            }
            catch (Exception)
            {
                string errorMsg;
                if (isValidGuid)
                    errorMsg = string.Format("Project with identifier: {0} does not exist", projectNameOrId);
                else
                    errorMsg = string.Format("Project: {0} does not exist", projectNameOrId);
                throw (new RecoverableException(errorMsg));
            }

            // Return existence flag.
            return existFlag;
        }

        #endregion

        #region - Public Members

        public Projects(IConfiguration configuration) : base(configuration) { }

        public string CreateProject(string jsonContent)
        {
            // Initialize.
            string operationId = null;

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/projects?api-version={Version}");

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

                    if (ResponseMessage.IsSuccessStatusCode && ResponseMessage.StatusCode == HttpStatusCode.Accepted)
                    {
                        SetSuccessfulCRUDOperation();

                        // Extract the operation identifier.
                        operationId = JToken.Parse(RawResponseMessage)["id"].ToString();
                    }
                    else
                    {
                        if (ResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                            throw (new RecoverableException(RawResponseMessage));
                        else
                            throw (new RecoverableException(LastApiErrorMessage));
                    }
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

            // Return operation identifier.
            return operationId;
        }

        

        public bool DeleteProject(string projectId)
        {
            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/projects/{projectId}?api-version={Version}");

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

                    // Regenerate object.
                    if (ValidateServiceCall())
                        SetSuccessfulCRUDOperation();
                }
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

        

        public CoreResponse.TeamProject GetProjectByName(string projectName)
        {
            // Initialize.
            string projectId;
            CoreResponse.TeamProject tp = null;

            try
            {
                // Retrieve project identifier.
                projectId = GetProjectIdByName(projectName);

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/projects/{projectId}?api-version={Version}");

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
                        tp = DeserializeResponseToObject<CoreResponse.TeamProject>();
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

            // Return project.
            return tp;
        }

        

        public CoreResponse.ProjectProperties GetProjectExtendedProperties(string projectName = null)
        {
            // Initialize.
            string identifier = null;
            string projectId = null;
            CoreResponse.ProjectProperty pp = null;
            CoreResponse.ProjectProperties pps = null;
            WorkItemTrackingResponse.ProcessInfo pi = null;

            try
            {
                // Get project identifier.
                if(string.IsNullOrEmpty(projectName))
                    projectId = ProjectId;
                else
                    projectId = GetProjectIdByName(projectName);

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/projects/{projectId}/properties?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        // Get project propeties.
                        pps = DeserializeResponseToObject<CoreResponse.ProjectProperties>();

                        // Extract the process template type.
                        pp = pps.Value.Where(x => x.Name == "System.ProcessTemplateType").FirstOrDefault();

                        // Get the identifier.
                        if (pp != null)
                            identifier = pp.Value;

                        // Define uri to call.
                        SetServiceUri($"{BaseUri}/_apis/work/processes/{identifier}?api-version={Version}");

                        using (var client2 = GetHttpClient())
                        {
                            // Send.
                            ResponseMessage = client2.GetAsync(Uri).Result;

                            // Validate if the process tempate can be found.
                            if (ValidateServiceCall())
                            {
                                SetSuccessfulCRUDOperation();

                                // Get process template information.
                                pi = DeserializeResponseToObject<WorkItemTrackingResponse.ProcessInfo>();

                                // Send some traces.
                                _mySource.Value.TraceInformation("Process template {0} has been found.", pi.Name);
                                _mySource.Value.Flush();
                            }
                            else
                                throw (new RecoverableException(LastApiErrorMessage));
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

            // Return project properties.
            return pps;
        }

        public string GetProjectIdByName(string projectName)
        {
            // Initialize.
            string projectId = Guid.Empty.ToString();

            try
            {
                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/projects/{projectName}?api-version={Version}");

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

                    // Get project identifier.
                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();
                        projectId = JToken.Parse(RawResponseMessage)["id"].ToString();
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

            // Return project identifier.
            return projectId;
        }

        public ProjectState GetProjectStateByName(string projectName)
        {
            // Initialize.
            CoreResponse.TeamProject project;

            // Get project.
            project = GetProjectByName(projectName);

            // Return the project state.
            return project.State;
        }

        public bool TestIfProjectExistByName(string projectName)
        {
            return (TestIfProjectExist(projectName));
        }

        public bool TestIfProjectExistById(string projectId)
        {
            return (TestIfProjectExist(projectId));
        }

        public WorkItemTypes GetSystemProcessworkitemtypes(string systemProcessId)
        {
            // Initialize.
            WorkItemTypes workitemtypes = null;

            try
            {

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/work/processes/{systemProcessId}/workitemtypes?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        workitemtypes = DeserializeResponseToObject<WorkItemTypes>();

                        // Send some traces.
                        _mySource.Value.TraceInformation("system Process template {0} has been found.", systemProcessId);
                        _mySource.Value.Flush();
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

            // Return project properties.
            return workitemtypes;
        }

        public WorkItemTypeStates GetSystemProcessworkitemtypeStates(string systemProcessId, string witRefName)
        {
            // Initialize.
            WorkItemTypeStates states = null;

            try
            {

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/work/processes/{systemProcessId}/workitemtypes/{witRefName}/states?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        states = DeserializeResponseToObject<WorkItemTypeStates>();

                        // Send some traces.
                        _mySource.Value.TraceInformation("system Process template {0} has been found.", systemProcessId);
                        _mySource.Value.Flush();
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

            // Return project properties.
            return states;
        }

        public InheritedProcess GetInheritedProcess(string processTypeId)
        {
            // Initialize.
            InheritedProcess inheritedProcess = null;

            try
            {

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/work/processadmin/processes/export/{processTypeId}?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        inheritedProcess = DeserializeResponseToObject<InheritedProcess>();

                        // Send some traces.
                        _mySource.Value.TraceInformation("inherited Process {0} found.", inheritedProcess.Process.Name);
                        _mySource.Value.Flush();
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

            // Return project properties.
            return inheritedProcess;
        }

        public Field1 GetSystemProcessworkitemtypeFields(
            string systemProcessId, string witRefName, string fieldRefName)
        {
            // Initialize.
            Field1 layouts = null;

            try
            {

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/work/processes/{systemProcessId}/workitemtypes/{witRefName}/fields/{fieldRefName}?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        layouts = DeserializeResponseToObject<Field1>();

                        // Send some traces.
                        _mySource.Value.TraceInformation("system Process template {0} has been found.", systemProcessId);
                        _mySource.Value.Flush();
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

            // Return project properties.
            return layouts;
        }

        public WorkItemTypeFields GetSystemProcessworkitemtypeworkItemTypeFields(string systemProcessId, string witRefName)
        {
            // Initialize.
            WorkItemTypeFields layouts = null;

            try
            {

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/work/processes/{systemProcessId}/workitemtypes/{witRefName}/fields?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        layouts = DeserializeResponseToObject<WorkItemTypeFields>();

                        // Send some traces.
                        _mySource.Value.TraceInformation("system Process template {0} has been found.", systemProcessId);
                        _mySource.Value.Flush();
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

            // Return project properties.
            return layouts;
        }

        public Picklists GetSystemProcessPicklists()
        {
            // Initialize.
            Picklists picklists = null;

            try
            {

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/work/processes/lists?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        picklists = DeserializeResponseToObject<Picklists>();

                        // Send some traces.
                        _mySource.Value.TraceInformation("system Process has {0} picklists found.", picklists.count.ToString());
                        _mySource.Value.Flush();
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

            // Return project properties.
            return picklists;
        }

        public Picklist2 GetSystemProcessPicklists(string listId)
        {
            // Initialize.
            Picklist2 picklists = null;

            try
            {

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/work/processes/lists/{listId}?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        picklists = DeserializeResponseToObject<Picklist2>();

                        // Send some traces.
                        _mySource.Value.TraceInformation("system Process has picklist {0}.", picklists.name);
                        _mySource.Value.Flush();
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

            // Return project properties.
            return picklists;
        }

        public WorkItemTypeRules GetSystemProcessworkitemtyperules(string systemProcessId, string witRefName)
        {
            // Initialize.
            WorkItemTypeRules layouts = null;

            try
            {

                // Define uri to call.
                SetServiceUri($"{BaseUri}/_apis/work/processes/{systemProcessId}/workitemtypes/{witRefName}/rules?api-version={Version}");

                using (var client1 = GetHttpClient())
                {
                    // Send.
                    ResponseMessage = client1.GetAsync(Uri).Result;

                    if (ValidateServiceCall())
                    {
                        SetSuccessfulCRUDOperation();

                        layouts = DeserializeResponseToObject<WorkItemTypeRules>();

                        // Send some traces.
                        _mySource.Value.TraceInformation("system Process template {0} has been found.", systemProcessId);
                        _mySource.Value.Flush();
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

            // Return project properties.
            return layouts;
        }

        #endregion
    }
}