using System.Runtime.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADO.RestAPI.Viewmodel50
{
    public class ExtentionMgmtMinimalResponse
    {
        // This is just a container class for all REST API responses related to Extension Management.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/extensionmanagement/installed%20extensions?view=azure-devops-rest-5.0

        #region - Nested Classes and Enumerations.

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ExtensionStateFlags
        {
            [EnumMember(Value = "autoUpgradeError")]
            AutoUpgradeError,
            [EnumMember(Value = "builtIn")]
            BuiltIn,
            [EnumMember(Value = "disabled")]
            Disabled,
            [EnumMember(Value = "error")]
            Error,
            [EnumMember(Value = "multiVersion")]
            MultiVersion,
            [EnumMember(Value = "needsReauthorization")]
            NeedsReauthorization,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "trusted")]
            Trusted,
            [EnumMember(Value = "unInstalled")]
            UnInstalled,
            [EnumMember(Value = "versionCheckError")]
            VersionCheckError,
            [EnumMember(Value = "warning")]
            Warning
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum InstalledExtenstionStateIssueType
        {
            [EnumMember(Value = "error")]
            Error,
            [EnumMember(Value = "warning")]
            Warning
        }

        public class InstalledExtensions
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<InstalledExtension> Value { get; set; }
        }

        public class InstalledExtension
        {
            [JsonProperty(PropertyName = "baseUri")]
            public string BaseUri { get; set; }

            [JsonProperty(PropertyName = "extensionId")]
            public string ExtensionId { get; set; }

            [JsonProperty(PropertyName = "extensionName")]
            public string ExtensionName { get; set; }

            [JsonProperty(PropertyName = "flags")]
            public string Flags { get; set; }

            [JsonProperty(PropertyName = "installState")]
            public InstalledExtensionState InstallState { get; set; }

            [JsonProperty(PropertyName = "language")]
            public string Language { get; set; }

            [JsonProperty(PropertyName = "lastPublished")]
            public string LastPublished { get; set; }

            [JsonProperty(PropertyName = "manifestVersion")]
            public uint ManifestVersion { get; set; }

            [JsonProperty(PropertyName = "publisherId")]
            public string PublisherId { get; set; }

            [JsonProperty(PropertyName = "publisherName")]
            public string PublisherName { get; set; }

            [JsonProperty(PropertyName = "registrationId")]
            public string RegistrationId { get; set; }

            [JsonProperty(PropertyName = "scopes")]
            public IEnumerable<string> Scopes { get; set; }

            [JsonProperty(PropertyName = "version")]
            public string Version { get; set; }
        }

        public class InstalledExtensionState
        {
            [JsonProperty(PropertyName = "flags")]
            public ExtensionStateFlags Flags { get; set; }

            [JsonProperty(PropertyName = "installationIssues")]
            public IEnumerable<InstalledExtensionStateIssue> InstallationIssues { get; set; }

            [JsonProperty(PropertyName = "lastUpdated")]
            public string LastUpdated { get; set; }
        }

        public class InstalledExtensionStateIssue
        {
            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }

            [JsonProperty(PropertyName = "source")]
            public string Source { get; set; }

            [JsonProperty(PropertyName = "type")]
            public InstalledExtenstionStateIssueType Type { get; set; }
        }

        #endregion
    }
}
