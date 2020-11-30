using Newtonsoft.Json;

namespace ADO.RestAPI.Viewmodel50
{
    #region - Reference Classes.

    public class AreaPathClassificationNodesReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class AvatarReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class BadgeReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class CollectionReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class ContainerReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class EditorReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class ParentReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class MemberReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class MembershipReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class MembershipStateReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class ProjectReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class SelfReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class StorageKeyReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class SubjectReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class TeamReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class TeamSettingsReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class WebReference
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    #endregion

    #region - Link Classes.

    public class BuildDefinitionReferenceLink
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }

        [JsonProperty(PropertyName = "web")]
        public WebReference Web { get; set; }

        [JsonProperty(PropertyName = "editor")]
        public EditorReference Editor { get; set; }

        [JsonProperty(PropertyName = "badge")]
        public BadgeReference Badge { get; set; }
    }

    public class GraphDescriptorReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }

        [JsonProperty(PropertyName = "storageKey")]
        public StorageKeyReference StorageKey { get; set; }

        [JsonProperty(PropertyName = "subject")]
        public SubjectReference Subject { get; set; }
    }

    public class GraphGroupReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }

        [JsonProperty(PropertyName = "memberships")]
        public MembershipReference Memberships { get; set; }

        [JsonProperty(PropertyName = "membershipState")]
        public MembershipStateReference MembershipState { get; set; }

        [JsonProperty(PropertyName = "storageKey")]
        public StorageKeyReference StorageKey { get; set; }
    }

    public class GraphMembershipReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }

        [JsonProperty(PropertyName = "member")]
        public MemberReference Member { get; set; }

        [JsonProperty(PropertyName = "container")]
        public ContainerReference Container { get; set; }
    }

    public class GitCommitReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }
    }

    public class GitForkReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }
    }

    public class GitPushReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }
    }

    public class GitReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }
    }

    public class IdentityReferenceLink
    {
        [JsonProperty(PropertyName = "avatar")]
        public AvatarReference Avatar { get; set; }
    }

    public class ProjectReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }

        [JsonProperty(PropertyName = "collection")]
        public CollectionReference Collection { get; set; }

        [JsonProperty(PropertyName = "web")]
        public WebReference Web { get; set; }
    }

    public class PRThreadReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }
    }

    public class PRThreadCommentReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }
    }

    public class PullRequestReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }
    }    

    public class QueryHierarchyItemReferenceLink
    {
        [JsonProperty(PropertyName = "avatar")]
        public AvatarReference Avatar { get; set; }
    }

    public class ReleaseDefinitionReferenceLink
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }

        [JsonProperty(PropertyName = "web")]
        public WebReference Web { get; set; }
    }

    public class SelfReferenceLink
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }
    }

    public class TeamFieldValuesReferenceLink
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }

        [JsonProperty(PropertyName = "project")]
        public ProjectReference Project { get; set; }

        [JsonProperty(PropertyName = "team")]
        public TeamReference Team { get; set; }

        [JsonProperty(PropertyName = "teamSettings")]
        public TeamSettingsReference TeamSettings { get; set; }

        [JsonProperty(PropertyName = "areaPathClassificationNodes")]
        public AreaPathClassificationNodesReference AreaPathClassificationNodes { get; set; }
    }

    public class TeamSettingReferenceLink
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }
    }

    public class TeamSettingsIterationReferenceLink
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }
    }
        
    public class WorkItemClassificationNodeReferenceLinks
    {
        [JsonProperty(PropertyName = "self")]
        public SelfReference Self { get; set; }

        [JsonProperty(PropertyName = "parent")]
        public ParentReference Parent { get; set; }
    }

    #endregion
}
