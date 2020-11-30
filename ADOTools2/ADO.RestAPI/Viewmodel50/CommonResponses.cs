using System.Runtime.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADO.RestAPI.Viewmodel50
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemContentType
    {
        [EnumMember(Value = "base64Encoded")]
        Base64Encoded,
        [EnumMember(Value = "rawText")]
        RawText
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProjectState
    {
        [EnumMember(Value = "all")]
        All,
        [EnumMember(Value = "createPending")]
        CreatePending,
        [EnumMember(Value = "deleted")]
        Deleted,
        [EnumMember(Value = "deleting")]
        Deleting,
        [EnumMember(Value = "new")]
        New,
        [EnumMember(Value = "unchanged")]
        Unchanged,
        [EnumMember(Value = "wellFormed")]
        WellFormed
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProjectVisibility
    {
        [EnumMember(Value = "private")]
        Private,
        [EnumMember(Value = "public")]
        Public
    }

    public class AbstractJObject
    {
        [JsonProperty(PropertyName = "item")]
        public AbstractJToken Item { get; set; }

        [JsonProperty(PropertyName = "type")]
        public object Type { get; set; }
    }

    public class AbstractJToken
    {
        [JsonProperty(PropertyName = "first")]
        public AbstractJToken First { get; set; }

        [JsonProperty(PropertyName = "hasValues")]
        public bool HasValues { get; set; }

        [JsonProperty(PropertyName = "item")]
        public AbstractJToken Item { get; set; }

        [JsonProperty(PropertyName = "last")]
        public AbstractJToken Last { get; set; }

        [JsonProperty(PropertyName = "next")]
        public AbstractJToken Next { get; set; }

        [JsonProperty(PropertyName = "parent")]
        public string Parent { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "previous")]
        public AbstractJToken Previous { get; set; }

        [JsonProperty(PropertyName = "root")]
        public AbstractJToken Root { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    public class AuthorizationHeader
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }

    public class DataSourceBinding
    {
        [JsonProperty(PropertyName = "callbackContextTemplate")]
        public string CallbackContextTemplate { get; set; }

        [JsonProperty(PropertyName = "callbackRequiredTemplate")]
        public string CallbackRequiredTemplate { get; set; }

        [JsonProperty(PropertyName = "dataSourceName")]
        public string DataSourceName { get; set; }

        [JsonProperty(PropertyName = "endpointId")]
        public string EndpointId { get; set; }

        [JsonProperty(PropertyName = "endpointUrl")]
        public string EndpointUrl { get; set; }

        [JsonProperty(PropertyName = "headers")]
        public IEnumerable<AuthorizationHeader> Headers { get; set; }

        [JsonProperty(PropertyName = "initialContextTemplate")]
        public string InitialContextTemplate { get; set; }

        [JsonProperty(PropertyName = "parameters")]
        public object Parameters { get; set; }

        [JsonProperty(PropertyName = "resultTemplate")]
        public string ResultTemplate { get; set; }

        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; }
    }

    public class DataSourceBindingBase
    {
        [JsonProperty(PropertyName = "callbackContextTemplate")]
        public string CallbackContextTemplate { get; set; }

        [JsonProperty(PropertyName = "callbackContextTemplate")]
        public string CallbackRequiredTemplate { get; set; }

        [JsonProperty(PropertyName = "dataSourceName")]
        public string DataSourceName { get; set; }

        [JsonProperty(PropertyName = "endpointId")]
        public string EndpointId { get; set; }

        [JsonProperty(PropertyName = "endpointUrl")]
        public string EndpointUrl { get; set; }

        [JsonProperty(PropertyName = "headers")]
        public IEnumerable<AuthorizationHeader> Headers { get; set; }

        [JsonProperty(PropertyName = "initialContextTemplate")]
        public string InitialContextTemplate { get; set; }

        [JsonProperty(PropertyName = "parameters")]
        public object Parameters { get; set; }

        [JsonProperty(PropertyName = "resultSelector")]
        public string ResultSelector { get; set; }

        [JsonProperty(PropertyName = "resultTemplate")]
        public string ResultTemplate { get; set; }

        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; }
    }

    public class Demand
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }

    public class IdentityReference
    {
        [JsonProperty(PropertyName = "_links")]
        public IdentityReferenceLink Links { get; set; }

        [JsonProperty(PropertyName = "descriptor")]
        public string Descriptor { get; set; }

        [JsonProperty(PropertyName = "directoryAlias")]
        public string DirectoryAlias { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "inactive")]
        public bool Inactive { get; set; }

        [JsonProperty(PropertyName = "isAadIdentity")]
        public bool IsAadIdentity { get; set; }

        [JsonProperty(PropertyName = "isContainer")]
        public bool IsContainer { get; set; }

        [JsonProperty(PropertyName = "isDeletedInOrigin")]
        public bool IsDeletedInOrigin { get; set; }

        [JsonProperty(PropertyName = "profileUrl")]
        public string ProfileUrl { get; set; }

        [JsonProperty(PropertyName = "uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class IdentityReferenceWithVote : IdentityReference
    {
        // https://docs.microsoft.com/en-us/javascript/api/azure-devops-extension-api/identityrefwithvote
        // Vote on a pull request:
        // 10 - approved
        // 5 - approved with suggestions
        // 0 - no vote
        // -5 - waiting for author
        // -10 - rejected
        [JsonProperty(PropertyName = "vote")]
        public int Vote { get; set; }

        [JsonProperty(PropertyName = "votedFor")]
        public IEnumerable<IdentityReferenceWithVote> VotedFor { get; set; }
    }

    public class ProcessParameters
    {
        [JsonProperty(PropertyName = "dataSourceBindings")]
        public IEnumerable<DataSourceBindingBase> DataSourceBindings { get; set; }

        [JsonProperty(PropertyName = "inputs")]
        public IEnumerable<TaskInputDefinitionBase> Inputs { get; set; }

        [JsonProperty(PropertyName = "sourceDefinitions")]
        public IEnumerable<TaskSourceDefinitionBase> SourceDefinitions { get; set; }
    }

    public class TaskInputDefinition
    {
        [JsonProperty(PropertyName = "aliases")]
        public IEnumerable<string> Aliases { get; set; }

        [JsonProperty(PropertyName = "defaultValue")]
        public string DefaultValue { get; set; }

        [JsonProperty(PropertyName = "groupName")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "helpMarkDown")]
        public string HelpMarkDown { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "options")]
        public object Options { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public object Properties { get; set; }

        [JsonProperty(PropertyName = "required")]
        public bool Required { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "validation")]
        public TaskInputValidation Validation { get; set; }

        [JsonProperty(PropertyName = "visibleRule")]
        public string VisibleRule { get; set; }
    }

    public class TaskInputDefinitionBase
    {
        [JsonProperty(PropertyName = "aliases")]
        public IEnumerable<string> Aliases { get; set; }

        [JsonProperty(PropertyName = "defaultValue")]
        public string DefaultValue { get; set; }

        [JsonProperty(PropertyName = "groupName")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "helpMarkDown")]
        public string HelpMarkDown { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public object Properties { get; set; }

        [JsonProperty(PropertyName = "required")]
        public bool Required { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "validation")]
        public TaskInputValidation Validation { get; set; }

        [JsonProperty(PropertyName = "visibleRule")]
        public string VisibleRule { get; set; }
    }

    public class TaskInputValidation
    {
        [JsonProperty(PropertyName = "expression")]
        public string Expression { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }

    public class TaskSourceDefinitionBase
    {
        [JsonProperty(PropertyName = "authKey")]
        public string AuthKey { get; set; }

        [JsonProperty(PropertyName = "endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty(PropertyName = "keySelector")]
        public string KeySelector { get; set; }

        [JsonProperty(PropertyName = "selector")]
        public string Selector { get; set; }

        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; }
    }

    public class TeamProjectCollectionReference
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class TeamProjectReference
    {
        [JsonProperty(PropertyName = "abbreviation")]
        public string Abbreviation { get; set; }

        [JsonProperty(PropertyName = "defaultTeamImageUrl")]
        public string DefaultTeamImageUrl { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "lastUpdateTime")]
        public string LastUpdateTime { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "revision")]
        public int Revision { get; set; }

        [JsonProperty(PropertyName = "state")]
        public ProjectState State { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "visibility")]
        public ProjectVisibility Visibility { get; set; }
    }

    public class WebApiTagDefinition
    {
        [JsonProperty(PropertyName = "active")]
        public bool Active { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class WebApiTeamReference
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
