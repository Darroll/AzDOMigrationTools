using System.Runtime.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADO.RestAPI.Viewmodel50
{
    public class WorkItemTrackingResponse
    {
        // This is just a container class for all REST API responses related to work item tracking.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/?view=azure-devops-rest-5.0

        #region - Nested Classes and Enumerations.

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CustomizationType
        {
            [EnumMember(Value = "custom")]
            Custom,
            [EnumMember(Value = "inherited")]
            Inherited,
            [EnumMember(Value = "system")]
            System
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LinkQueryMode
        {
            [EnumMember(Value = "linksOneHopDoesNotContain")]
            LinksOneHopDoesNotContain,
            [EnumMember(Value = "linksOneHopMayContain")]
            LinksOneHopMayContain,
            [EnumMember(Value = "linksOneHopMustContain")]
            LinksOneHopMustContain,
            [EnumMember(Value = "linksRecursiveDoesNotContain")]
            LinksRecursiveDoesNotContain,
            [EnumMember(Value = "linksRecursiveMayContain")]
            LinksRecursiveMayContain,
            [EnumMember(Value = "linksRecursiveMustContain")]
            LinksRecursiveMustContain,
            [EnumMember(Value = "workItems")]
            WorkItems
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LogicalOperation
        {
            [EnumMember(Value = "and")]
            And,
            [EnumMember(Value = "none")]
            None,
            [EnumMember(Value = "or")]
            Or
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ProcessType
        {
            [EnumMember(Value = "custom")]
            Custom,
            [EnumMember(Value = "inherited")]
            Inherited,
            [EnumMember(Value = "system")]
            System
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum QueryRecursionOption
        {
            [EnumMember(Value = "childFirst")]
            ChildFirst,
            [EnumMember(Value = "parentFirst")]
            ParentFirst
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum QueryType
        {
            [EnumMember(Value = "flat")]
            Flat,
            [EnumMember(Value = "oneHop")]
            OneHop,
            [EnumMember(Value = "tree")]
            Tree
        }

        public class Processes
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<Process> Value { get; set; }
        }

        public class Process
        {
            [JsonProperty(PropertyName = "_links")]
            public object Links { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "isDefault")]
            public bool IsDefault { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "type")]
            public ProcessType Type { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class ProcessInfos
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<ProcessInfo> Value { get; set; }
        }

        public class ProcessInfo
        {
            [JsonProperty(PropertyName = "customizationType")]
            public CustomizationType CustomizationType { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "isDefault")]
            public bool IsDefault { get; set; }

            [JsonProperty(PropertyName = "isEnabled")]
            public bool IsEnabled { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "parentProcessTypeId")]
            public string ParentProcessTypeId { get; set; }

            [JsonProperty(PropertyName = "projects")]
            public IEnumerable<ProjectReference> Projects { get; set; }

            [JsonProperty(PropertyName = "referenceName")]
            public string ReferenceName { get; set; }

            [JsonProperty(PropertyName = "typeId")]
            public string TypeId { get; set; }
        }

        public class ProjectReference
        {
            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class QueryHierarchyItems
        {
            // This data class makes use of definitions from these links:
            // https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/queries/get?view=azure-devops-rest-5.0
            // https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/queries/list?view=azure-devops-rest-5.0

            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<QueryHierarchyItem> Value { get; set; }
        }

        public class QueryHierarchyItem
        {
            [JsonProperty(PropertyName = "_links")]
            public QueryHierarchyItemReferenceLink Links { get; set; }

            [JsonProperty(PropertyName = "children")]
            public IEnumerable<QueryHierarchyItem> Children { get; set; }

            [JsonProperty(PropertyName = "clauses")]
            public IEnumerable<WorkItemQueryClause> Clauses { get; set; }

            [JsonProperty(PropertyName = "columns")]
            public IEnumerable<WorkItemFieldReference> Columns { get; set; }

            [JsonProperty(PropertyName = "createdBy")]
            public IdentityReference CreatedBy { get; set; }

            [JsonProperty(PropertyName = "createdDate")]
            public string CreatedDate { get; set; }

            [JsonProperty(PropertyName = "filterOptions")]
            public LinkQueryMode FilterOptions { get; set; }

            [JsonProperty(PropertyName = "hasChildren")]
            public bool HasChildren { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "isDeleted")]
            public bool IsDeleted { get; set; }

            [JsonProperty(PropertyName = "isFolder")]
            public bool IsFolder { get; set; }

            [JsonProperty(PropertyName = "isInvalidSyntax")]
            public bool IsInvalidSyntax { get; set; }

            [JsonProperty(PropertyName = "isPublic")]
            public bool IsPublic { get; set; }

            [JsonProperty(PropertyName = "lastExecutedBy")]
            public IdentityReference LastExecutedBy { get; set; }

            [JsonProperty(PropertyName = "lastExecutedDate")]
            public string LastExecutedDate { get; set; }

            [JsonProperty(PropertyName = "lastModifiedBy")]
            public IdentityReference LastModifiedBy { get; set; }

            [JsonProperty(PropertyName = "lastModifiedDate")]
            public string LastModifiedDate { get; set; }

            [JsonProperty(PropertyName = "linkClauses")]
            public WorkItemQueryClause LinkClauses { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "path")]
            public string Path { get; set; }

            [JsonProperty(PropertyName = "queryRecursionOption")]
            public QueryRecursionOption QueryRecursionOption { get; set; }

            [JsonProperty(PropertyName = "queryType")]
            public QueryType QueryType { get; set; }

            [JsonProperty(PropertyName = "sortColumns")]
            public IEnumerable<WorkItemQuerySortColumn> SortColumns { get; set; }

            [JsonProperty(PropertyName = "sourceClauses")]
            public WorkItemQueryClause SourceClauses { get; set; }

            [JsonProperty(PropertyName = "targetClauses")]
            public WorkItemQueryClause TargetClauses { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "wiql")]
            public string Wiql { get; set; }
        }

        public class WorkItemClassificationNodes
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<WorkItemClassificationNode> Value { get; set; }
        }

        public class WorkItemClassificationNode
        {
            [JsonProperty(PropertyName = "_links")]
            public WorkItemClassificationNodeReferenceLinks Links { get; set; }

            [JsonProperty(PropertyName = "attributes")]
            public object Attributes { get; set; }

            [JsonProperty(PropertyName = "children")]
            public IList<WorkItemClassificationNode> Children { get; set; }

            [JsonProperty(PropertyName = "hasChildren")]
            public bool HasChildren { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "identifier")]
            public string Identifier { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "path")]
            public string Path { get; set; }

            [JsonProperty(PropertyName = "structureType")]
            public string StructureType { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class WorkItemFieldReference
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "referenceName")]
            public string ReferenceName { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class WorkItemFieldOperation
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "referenceName")]
            public string ReferenceName { get; set; }
        }

        public class WorkItemQuerySortColumn
        {
            [JsonProperty(PropertyName = "descending")]
            public bool Descending { get; set; }

            [JsonProperty(PropertyName = "field")]
            public WorkItemFieldReference Field { get; set; }
        }

        public class WorkItemQueryClause
        {
            [JsonProperty(PropertyName = "clauses")]
            public WorkItemQueryClause Clauses { get; set; }

            [JsonProperty(PropertyName = "field")]
            public WorkItemFieldReference Field { get; set; }

            [JsonProperty(PropertyName = "fieldValue")]
            public WorkItemFieldReference FieldValue { get; set; }

            [JsonProperty(PropertyName = "isFieldValue")]
            public bool IsFieldValue { get; set; }

            [JsonProperty(PropertyName = "logicalOperator")]
            public LogicalOperation LogicalOperator { get; set; }

            [JsonProperty(PropertyName = "operator")]
            public WorkItemFieldOperation Operator { get; set; }

            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }
        }
        
        #endregion
    }
}
