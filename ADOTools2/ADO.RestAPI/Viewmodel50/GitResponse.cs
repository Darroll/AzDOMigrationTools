using System.Runtime.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADO.RestAPI.Viewmodel50
{
    public class GitResponse
    {
        // This is just a container class for all REST API responses related to git repositories.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/git/repositories/list?view=azure-devops-rest-5.0
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/git/repositories/get%20repository?view=azure-devops-rest-5.0
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/git/repositories/get%20repository%20with%20parent?view=azure-devops-rest-5.0

        #region - Nested Classes and Enumerations.

        [JsonConverter(typeof(StringEnumConverter))]
        public enum GitStatusState
        {
            [EnumMember(Value = "error")]
            Error,
            [EnumMember(Value = "failed")]
            Failed,
            [EnumMember(Value = "notApplicable")]
            NotApplicable,
            [EnumMember(Value = "notSet")]
            NotSet,
            [EnumMember(Value = "pending")]
            Pending,
            [EnumMember(Value = "succeeded")]
            Succeeded
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum PRAsyncStatus
        {
            [EnumMember(Value = "conflicts")]
            Conflicts,
            [EnumMember(Value = "failure")]
            Failure,
            [EnumMember(Value = "notSet")]
            NotSet,
            [EnumMember(Value = "queued")]
            Queued,
            [EnumMember(Value = "rejectedByPolicy")]
            RejectedByPolicy,
            [EnumMember(Value = "succeeded")]
            Succeeded
        }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PRMergeFailureType
        {
            [EnumMember(Value = "caseSensitive")]
            CaseSensitive,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "objectTooLarge")]
            ObjectTooLarge,
            [EnumMember(Value = "unknown")]
            Unknown
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum PRMergeOptions
        {
            [EnumMember(Value = "detectRenameFalsePositives")]
            DetectRenameFalsePositives,
            [EnumMember(Value = "disableRenames")]
            DisableRenames
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum PRStatus
        {
            [EnumMember(Value = "abandoned")]
            Abandoned,
            [EnumMember(Value = "active")]
            Active,
            [EnumMember(Value = "all")]
            All,
            [EnumMember(Value = "completed")]
            Completed,
            [EnumMember(Value = "notSet")]
            NotSet
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum PRThreadCommentType
        {
            [EnumMember(Value = "codeChange")]
            CodeChange,
            [EnumMember(Value = "system")]
            System,
            [EnumMember(Value = "text")]
            Text,
            [EnumMember(Value = "unknown")]
            Unknown
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum PRThreadStatus
        {
            [EnumMember(Value = "active")]
            Active,
            [EnumMember(Value = "byDesign")]
            ByDesign,
            [EnumMember(Value = "closed")]
            Closed,
            [EnumMember(Value = "fixed")]
            Fixed,
            [EnumMember(Value = "pending")]
            Pending,
            [EnumMember(Value = "unknown")]
            Unknown,
            [EnumMember(Value = "wontFix")]
            WontFix
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum VersionControlChangeType
        {
            [EnumMember(Value = "add")]
            Add,
            [EnumMember(Value = "all")]
            All,
            [EnumMember(Value = "branch")]
            Branch,
            [EnumMember(Value = "delete")]
            Delete,
            [EnumMember(Value = "edit")]
            Edit,
            [EnumMember(Value = "encoding")]
            Encoding,
            [EnumMember(Value = "lock")]
            Lock,
            [EnumMember(Value = "merge")]
            Merge,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "property")]
            Property,
            [EnumMember(Value = "rename")]
            Rename,
            [EnumMember(Value = "rollback")]
            Rollback,
            [EnumMember(Value = "sourceRename")]
            SourceRename,
            [EnumMember(Value = "targetRename")]
            TargetRename,
            [EnumMember(Value = "undelete")]
            Undelete
        }

        public class ChangeCountDictionary
        {

        }

        public class CommentIterationContext
        {
            [JsonProperty(PropertyName = "firstComparingIteration")]
            public int FirstComparingIteration { get; set; }

            [JsonProperty(PropertyName = "secondComparingIteration")]
            public int SecondComparingIteration { get; set; }
        }

        public class CommentPosition
        {
            [JsonProperty(PropertyName = "line")]
            public int Line { get; set; }

            [JsonProperty(PropertyName = "offset")]
            public int Offset { get; set; }
        }

        public class CommentThreadContext
        {
            [JsonProperty(PropertyName = "FilePath")]
            public string FilePath { get; set; }

            [JsonProperty(PropertyName = "leftFileEnd")]
            public CommentPosition LeftFileEnd { get; set; }

            [JsonProperty(PropertyName = "leftFileStart")]
            public CommentPosition LeftFileStart { get; set; }

            [JsonProperty(PropertyName = "rightFileEnd")]
            public CommentPosition RightFileEnd { get; set; }

            [JsonProperty(PropertyName = "rightFileStart")]
            public CommentPosition RightFileStart { get; set; }
        }

        public class CommentTrackingCriteria
        {
            [JsonProperty(PropertyName = "firstComparingIteration")]
            public int FirstComparingIteration { get; set; }

            [JsonProperty(PropertyName = "origFilePath")]
            public string OrigFilePath { get; set; }

            [JsonProperty(PropertyName = "origLeftFileEnd")]
            public CommentPosition OrigLeftFileEnd { get; set; }

            [JsonProperty(PropertyName = "origLeftFileStart")]
            public CommentPosition OrigLeftFileStart { get; set; }

            [JsonProperty(PropertyName = "origRightFileEnd")]
            public CommentPosition OrigRightFileEnd { get; set; }

            [JsonProperty(PropertyName = "origRightFileStart")]
            public CommentPosition OrigRightFileStart { get; set; }

            [JsonProperty(PropertyName = "secondComparingIteration")]
            public int SecondComparingIteration { get; set; }
        }
        
        public class GitChange
        {
            [JsonProperty(PropertyName = "changeId")]
            public int ChangeId { get; set; }

            [JsonProperty(PropertyName = "changeId")]
            public VersionControlChangeType ChangeType { get; set; }

            [JsonProperty(PropertyName = "changeId")]
            public string Item { get; set; }

            [JsonProperty(PropertyName = "changeId")]
            public ItemContent NewContent { get; set; }

            [JsonProperty(PropertyName = "changeId")]
            public GitTemplate NewContentTemplate { get; set; }

            [JsonProperty(PropertyName = "changeId")]
            public string OriginalPath { get; set; }

            [JsonProperty(PropertyName = "changeId")]
            public string SourceServerItem { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class GitCommitReference
        {
            [JsonProperty(PropertyName = "_links")]
            public GitCommitReferenceLinks Links { get; set; }

            [JsonProperty(PropertyName = "author")]
            public GitUserDate Author { get; set; }

            // todo: no indication was given on this.
            [JsonProperty(PropertyName = "changeCounts")]
            public ChangeCountDictionary ChangeCounts { get; set; }

            [JsonProperty(PropertyName = "changes")]
            public IEnumerable<GitChange> Changes { get; set; }

            [JsonProperty(PropertyName = "comment")]
            public string Comment { get; set; }

            [JsonProperty(PropertyName = "commentTruncated")]
            public bool CommentTruncated { get; set; }

            [JsonProperty(PropertyName = "commitId")]
            public string CommitId { get; set; }

            [JsonProperty(PropertyName = "commiter")]
            public GitUserDate Commiter { get; set; }

            [JsonProperty(PropertyName = "parents")]
            public IEnumerable<string> Parents { get; set; }

            [JsonProperty(PropertyName = "push")]
            public GitPushReference Push { get; set; }

            [JsonProperty(PropertyName = "remoteUrl")]
            public string RemoteUrl { get; set; }

            [JsonProperty(PropertyName = "statuses")]
            public IEnumerable<GitStatus> Statuses { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "workItems")]
            public IEnumerable<ResourceReference> WorkItems { get; set; }
        }

        public class GitForkReference
        {
            [JsonProperty(PropertyName = "_links")]
            public string GitForkReferenceLinks { get; set; }

            [JsonProperty(PropertyName = "creator")]
            public IdentityReference Creator { get; set; }

            [JsonProperty(PropertyName = "isLocked")]
            public bool IsLocked { get; set; }

            [JsonProperty(PropertyName = "isLockedBy")]
            public IdentityReference IsLockedBy { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "objectId")]
            public string ObjectId { get; set; }

            [JsonProperty(PropertyName = "peeledObjectId")]
            public string PeeledObjectId { get; set; }

            [JsonProperty(PropertyName = "repository")]
            public GitRepository Repository { get; set; }

            [JsonProperty(PropertyName = "statuses")]
            public IEnumerable<GitStatus> Statuses { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class GitPullRequestCompletionOptions
        {
            [JsonProperty(PropertyName = "bypassPolicy")]
            public bool BypassPolicy { get; set; }

            [JsonProperty(PropertyName = "bypassReason")]
            public string BypassReason { get; set; }

            [JsonProperty(PropertyName = "deleteSourceBranch")]
            public bool DeleteSourceBranch { get; set; }

            [JsonProperty(PropertyName = "mergeCommitMessage")]
            public string MergeCommitMessage { get; set; }

            [JsonProperty(PropertyName = "squashMerge")]
            public bool SquashMerge { get; set; }

            [JsonProperty(PropertyName = "transitionWorkItems")]
            public bool TransitionWorkItems { get; set; }

            [JsonProperty(PropertyName = "triggeredByAutoComplete")]
            public bool TriggeredByAutoComplete { get; set; }
        }

        public class GitPushReference
        {
            [JsonProperty(PropertyName = "_links")]
            public GitPushReferenceLinks Links { get; set; }

            [JsonProperty(PropertyName = "date")]
            public string Date { get; set; }

            [JsonProperty(PropertyName = "pushId")]
            public string PushId { get; set; }

            [JsonProperty(PropertyName = "pushedBy")]
            public IdentityReference PushedBy { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class GitRepositories
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<GitRepository> Value { get; set; }
        }

        public class GitRepository
        {
            [JsonProperty(PropertyName = "_links")]
            public GitReferenceLinks Links { get; set; }

            [JsonProperty(PropertyName = "defaultBranch")]
            public string DefaultBranch { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "isFork")]
            public bool IsFork { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "parentRepository")]
            public GitRepositoryReference ParentRepository { get; set; }

            // todo: The Visibility property returned from Get Pull Requests By Project is wrong.
            // Ignore it for now.
            [JsonIgnore()]
            public TeamProjectReference Project { get; set; }

            [JsonProperty(PropertyName = "remoteUrl")]
            public string RemoteUrl { get; set; }

            [JsonProperty(PropertyName = "size")]
            public int Size { get; set; }

            [JsonProperty(PropertyName = "sshUrl")]
            public string SshUrl { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "validRemoteUrls")]
            public IEnumerable<string> ValidRemoteUrls { get; set; }
        }

        public class GitRepositoryReference
        {
            [JsonProperty(PropertyName = "collection")]
            public TeamProjectCollectionReference Collection { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "isFork")]
            public bool IsFork { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "project")]
            public TeamProjectReference Project { get; set; }

            [JsonProperty(PropertyName = "remoteUrl")]
            public string RemoteUrl { get; set; }

            [JsonProperty(PropertyName = "sshUrl")]
            public string SshUrl { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class GitStatus
        {
            [JsonProperty(PropertyName = "_links")]
            public string Links { get; set; }

            [JsonProperty(PropertyName = "context")]
            public GitStatusContext Context { get; set; }

            [JsonProperty(PropertyName = "createdBy")]
            public IdentityReference CreatedBy { get; set; }

            [JsonProperty(PropertyName = "creationDate")]
            public string CreationDate { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "state")]
            public GitStatusState State { get; set; }

            [JsonProperty(PropertyName = "targetUrl")]
            public string TargetUrl { get; set; }

            [JsonProperty(PropertyName = "updatedDate")]
            public string UpdatedDate { get; set; }
        }

        public class GitStatusContext
        {
            [JsonProperty(PropertyName = "genre")]
            public string Genre { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        public class GitTemplate
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
        }

        public class GitUserDate
        {
            [JsonProperty(PropertyName = "date")]
            public string Date { get; set; }

            [JsonProperty(PropertyName = "email")]
            public string Email { get; set; }

            [JsonProperty(PropertyName = "imageUrl")]
            public string ImageUrl { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        public class ItemContent
        {
            [JsonProperty(PropertyName = "content")]
            public string Content { get; set; }

            [JsonProperty(PropertyName = "contentType")]
            public ItemContentType ContentType { get; set; }
        }

        public class PRCommentThreads
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<PRCommentThread> Value { get; set; }
        }

        public class PRCommentThread
        {
            [JsonProperty(PropertyName = "_links")]
            public PRThreadReferenceLinks Links { get; set; }

            [JsonProperty(PropertyName = "comments")]
            public IEnumerable<PRThreadComment> Comments { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "identities")]
            public Dictionary<string, IdentityReference> Identities { get; set; }

            [JsonProperty(PropertyName = "isDeleted")]
            public bool IsDeleted { get; set; }

            [JsonProperty(PropertyName = "createdDate")]
            public string LastUpdatedDate { get; set; }

            [JsonProperty(PropertyName = "properties")]
            public IDictionary<string, object> Properties { get; set; }

            [JsonProperty(PropertyName = "publishedDate")]
            public string PublishedDate { get; set; }

            [JsonProperty(PropertyName = "pullRequestThreadContext")]
            public PRCommentThreadContext PullRequestThreadContext { get; set; }

            [JsonProperty(PropertyName = "status")]
            public PRThreadStatus Status { get; set; }

            [JsonProperty(PropertyName = "threadContext")]
            public CommentThreadContext ThreadContext { get; set; }
        }

        public class PRCommentThreadContext
        {
            [JsonProperty(PropertyName = "changeTrackingId")]
            public int? ChangeTrackingId { get; set; }

            [JsonProperty(PropertyName = "iterationContext")]
            public CommentIterationContext IterationContext { get; set; }

            [JsonProperty(PropertyName = "trackingCriteria")]
            public CommentTrackingCriteria TrackingCriteria { get; set; }
        }

        public class PullRequests
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<PullRequest> Value { get; set; }
        }

        public class PullRequest
        {
            [JsonProperty(PropertyName = "artifactId")]
            public string ArtifactId { get; set; }

            [JsonProperty(PropertyName = "autoCompleteSetBy")]
            public IdentityReference AutoCompleteSetBy { get; set; }

            [JsonProperty(PropertyName = "closedBy")]
            public IdentityReference ClosedBy { get; set; }

            [JsonProperty(PropertyName = "closedDate")]
            public string ClosedDate { get; set; }

            [JsonProperty(PropertyName = "codeReviewId")]
            public int CodeReviewId { get; set; }

            [JsonProperty(PropertyName = "commits")]
            public IEnumerable<GitCommitReference> Commits { get; set; }

            [JsonProperty(PropertyName = "completionOptions")]
            public GitPullRequestCompletionOptions CompletionOptions { get; set; }

            [JsonProperty(PropertyName = "completionQueueTime")]
            public string CompletionQueueTime { get; set; }

            [JsonProperty(PropertyName = "createdBy")]
            public IdentityReference CreatedBy { get; set; }

            [JsonProperty(PropertyName = "creationDate")]
            public string CreationDate { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "forkSource")]
            public string ForkSource { get; set; }

            [JsonProperty(PropertyName = "isDraft")]
            public bool IsDraft { get; set; }

            [JsonProperty(PropertyName = "labels")]
            public IEnumerable<WebApiTagDefinition> Labels { get; set; }

            [JsonProperty(PropertyName = "lastMergeCommit")]
            public GitCommitReference LastMergeCommit { get; set; }

            [JsonProperty(PropertyName = "lastMergeSourceCommit")]
            public GitCommitReference LastMergeSourceCommit { get; set; }

            [JsonProperty(PropertyName = "lastMergeTargetCommit")]
            public GitCommitReference LastMergeTargetCommit { get; set; }

            [JsonProperty(PropertyName = "mergeFailureMessage")]
            public string MergeFailureMessage { get; set; }

            [JsonProperty(PropertyName = "mergeFailureType")]
            public PRMergeFailureType MergeFailureType { get; set; }

            [JsonProperty(PropertyName = "mergeId")]
            public string MergeId { get; set; }

            [JsonProperty(PropertyName = "mergeOptions")]
            public PRMergeOptions MergeOptions { get; set; }

            [JsonProperty(PropertyName = "mergeStatus")]
            public PRAsyncStatus MergeStatus { get; set; }

            [JsonProperty(PropertyName = "pullRequestId")]
            public int PullRequestId { get; set; }

            [JsonProperty(PropertyName = "remoteUrl")]
            public string RemoteUrl { get; set; }

            [JsonProperty(PropertyName = "repository")]
            public GitRepository Repository { get; set; }

            [JsonProperty(PropertyName = "reviewers")]
            public IEnumerable<IdentityReferenceWithVote> Reviewers { get; set; }

            [JsonProperty(PropertyName = "sourceRefName")]
            public string SourceRefName { get; set; }

            [JsonProperty(PropertyName = "status")]
            public PRStatus Status { get; set; }

            [JsonProperty(PropertyName = "supportsIterations")]
            public bool SupportsIterations { get; set; }

            [JsonProperty(PropertyName = "targetRefName")]
            public string TargetRefName { get; set; }

            [JsonProperty(PropertyName = "title")]
            public string Title { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "workItemRefs")]
            public IEnumerable<ResourceReference> WorkItemRefs { get; set; }
        }

        public class PRThreadComments
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<PRThreadComment> Value { get; set; }
        }

        public class PRThreadComment
        {
            [JsonProperty(PropertyName = "_links")]
            public PRThreadCommentReferenceLinks Links { get; set; }

            [JsonProperty(PropertyName = "author")]
            public IdentityReference Author { get; set; }

            [JsonProperty(PropertyName = "commentType")]
            public PRThreadCommentType CommentType { get; set; }

            [JsonProperty(PropertyName = "content")]
            public string Content { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "isDeleted")]
            public bool IsDeleted { get; set; }

            [JsonProperty(PropertyName = "createdDate")]
            public string LastContentUpdatedDate { get; set; }

            [JsonProperty(PropertyName = "parentCommentId")]
            public int ParentCommentId { get; set; }

            [JsonProperty(PropertyName = "publishedDate")]
            public string PublishedDate { get; set; }

            [JsonProperty(PropertyName = "usersLiked")]
            public IEnumerable<IdentityReference> UsersLiked { get; set; }
        }

        public class ResourceReference
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        #endregion
    }
}
