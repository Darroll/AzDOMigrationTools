using System.Collections.Generic;
using Newtonsoft.Json;

namespace ADO.RestAPI.Viewmodel50
{
    public class GraphResponse
    {
        // This is just a container class for all REST API responses related to  ACLs, ACEs, permissions and security namespaces.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/graph/?view=azure-devops-rest-5.0

        public class GraphDescriptorResult
        {
            [JsonProperty(PropertyName = "_links")]
            public GraphDescriptorReferenceLinks Links { get; set; }

            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }
        }

        public class GraphGroups
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<GraphGroup> Value { get; set; }
        }

        public class GraphGroup
        {
            [JsonProperty(PropertyName = "_links")]
            public GraphGroupReferenceLinks Links { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "descriptor")]
            public string Descriptor { get; set; }

            [JsonProperty(PropertyName = "displayName")]
            public string DisplayName { get; set; }

            [JsonProperty(PropertyName = "domain")]
            public string Domain { get; set; }

            [JsonIgnore()]
            public bool LegacyDescriptor { get; set; }

            [JsonProperty(PropertyName = "isCrossProject")]
            public bool IsCrossProject { get; set; }

            [JsonProperty(PropertyName = "mailAddress")]
            public object MailAddress { get; set; }

            [JsonProperty(PropertyName = "origin")]
            public string Origin { get; set; }

            [JsonProperty(PropertyName = "originId")]
            public string OriginId { get; set; }

            [JsonProperty(PropertyName = "principalName")]
            public string PrincipalName { get; set; }

            [JsonProperty(PropertyName = "subjectKind")]
            public string SubjectKind { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class GroupMembers
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<GroupMember> Value { get; set; }
        }

        public class GroupMember
        {
            [JsonProperty(PropertyName = "_links")]
            public GraphMembershipReferenceLinks Links { get; set; }

            [JsonProperty(PropertyName = "containerDescriptor")]
            public string ContainerDescriptor { get; set; }

            [JsonProperty(PropertyName = "memberDescriptor")]
            public string MemberDescriptor { get; set; }
        }
    }
}
