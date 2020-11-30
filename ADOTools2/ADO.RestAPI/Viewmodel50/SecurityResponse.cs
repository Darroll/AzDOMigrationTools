using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ADO.RestAPI.Viewmodel50
{
    public class SecurityResponse
    {
        // This is just a container class for all REST API responses related to  ACLs, ACEs, permissions and security namespaces.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/security/?view=azure-devops-rest-5.0

        public class AccessControlEntries
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<AccessControlEntry> Value { get; set; }
        }

        public class AccessControlEntry
        {
            [JsonProperty(PropertyName = "allow")]
            public int Allow { get; set; }

            [JsonProperty(PropertyName = "deny")]
            public int Deny { get; set; }

            [JsonProperty(PropertyName = "descriptor")]
            public string Descriptor { get; set; }

            [JsonProperty(PropertyName = "extendedInfo")]
            public ExtendedInformation ExtendedInfo { get; set; }

            [JsonIgnore]
            public string Identifier
            {
                get
                {
                    return Descriptor.Split(new char[] { ';' }).Last();
                }
            }

            [JsonIgnore]
            public string IdentityType
            {
                get
                {
                    return Descriptor.Split(new char[] { ';' }).First();
                }
            }
        }

        public class AccessControlLists
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<AccessControlList> Value { get; set; }
        }

        public class AccessControlList
        {
            [JsonProperty(PropertyName = "inheritPermissions")]
            public bool InheritPermissions { get; set; }

            [JsonProperty(PropertyName = "token")]
            public string Token { get; set; }

            [JsonProperty(PropertyName = "acesDictionary")]
            public Dictionary<string, AccessControlEntry> AcesDictionary { get; set; }
        }

        public class ActionDefinition
        {
            [JsonProperty(PropertyName = "bit")]
            public int Bit { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "displayName")]
            public string DisplayName { get; set; }

            [JsonProperty(PropertyName = "namespaceId")]
            public string NamespaceId { get; set; }
        }

        public class ExtendedInformation
        {
            [JsonProperty(PropertyName = "effectiveAllow")]
            public int EffectiveAllow { get; set; }

            [JsonProperty(PropertyName = "effectiveDeny")]
            public int EffectiveDeny { get; set; }

            [JsonProperty(PropertyName = "inheritedAllow")]
            public int InheritedAllow { get; set; }

            [JsonProperty(PropertyName = "inheritedDeny")]
            public int InheritedDeny { get; set; }
        }

        public class SecurityNamespaces
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<SecurityNamespaceDescription> Value { get; set; }
        }

        public class SecurityNamespaceDescription
        {
            [JsonProperty(PropertyName = "actions")]
            public IEnumerable<ActionDefinition> Actions { get; set; }

            [JsonProperty(PropertyName = "dataspaceCategory")]
            public string DataspaceCategory { get; set; }

            [JsonProperty(PropertyName = "displayName")]
            public string DisplayName { get; set; }

            [JsonProperty(PropertyName = "elementLength")]
            public int ElementLength { get; set; }

            [JsonProperty(PropertyName = "extensionType")]
            public string ExtensionType { get; set; }

            [JsonProperty(PropertyName = "isRemotable")]
            public bool IsRemotable { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "namespaceId")]
            public string NamespaceId { get; set; }

            [JsonProperty(PropertyName = "readPermission")]
            public int ReadPermission { get; set; }

            [JsonProperty(PropertyName = "separatorValue")]
            public string SeparatorValue { get; set; }

            [JsonProperty(PropertyName = "structureValue")]
            public int StructureValue { get; set; }

            [JsonProperty(PropertyName = "systemBitMask")]
            public int SystemBitMask { get; set; }

            [JsonProperty(PropertyName = "useTokenTranslator")]
            public bool UseTokenTranslator { get; set; }

            [JsonProperty(PropertyName = "writePermission")]
            public int WritePermission { get; set; }
        }
    }
}
