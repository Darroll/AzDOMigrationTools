using System.Collections.Generic;
using Newtonsoft.Json;

namespace ADO.RestAPI.Viewmodel50
{
    public class GitMinimalResponse
    {
        public class GitRepositories
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<GitRepository> Value { get; set; }
        }

        public class GitRepository
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        public class IdentityReference
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
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
            [JsonProperty(PropertyName = "comments")]
            public IEnumerable<PRThreadComment> Comments { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "properties")]
            public IDictionary<string, object> Properties { get; set; }

            [JsonProperty(PropertyName = "pullRequestThreadContext")]
            public GitResponse.PRCommentThreadContext PullRequestThreadContext { get; set; }

            [JsonProperty(PropertyName = "status")]
            public GitResponse.PRThreadStatus Status { get; set; }

            [JsonProperty(PropertyName = "threadContext")]
            public GitResponse.CommentThreadContext ThreadContext { get; set; }
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
            [JsonProperty(PropertyName = "commentType")]
            public GitResponse.PRThreadCommentType CommentType { get; set; }

            [JsonProperty(PropertyName = "content")]
            public string Content { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "parentCommentId")]
            public int ParentCommentId { get; set; }
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
            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "pullRequestId")]
            public int PullRequestId { get; set; }

            [JsonProperty(PropertyName = "reviewers")]
            public IEnumerable<IdentityReference> Reviewers { get; set; }

            [JsonProperty(PropertyName = "sourceRefName")]
            public string SourceRefName { get; set; }

            [JsonProperty(PropertyName = "targetRefName")]
            public string TargetRefName { get; set; }

            [JsonProperty(PropertyName = "title")]
            public string Title { get; set; }
        }
    }
}
