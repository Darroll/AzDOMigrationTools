using System.Collections.Generic;
using TreeCollections;

namespace ADO.Engine.BusinessEntities
{
    public class BusinessNodeDataNode
    : SerialTreeNode<BusinessNodeDataNode>
    {
        // empty constructor is a type constraint imposed by the base class
        public BusinessNodeDataNode() { }

        public BusinessNodeDataNode(
            int id,
            string name,
            BusinessNodeType businessNodeType,
            BusinessTeam team,
            bool disabled,
            bool isClone,
            bool isOnPremiseProject,
            string azureDevOpsServerFQDN,
            string organizationName,
            string teamPrefix,
            string process,
            List<string> teamInclusions,
            List<string> areaBasePaths,
            Dictionary<string, string> areaPathMap,
            List<string> iterationBasePaths,
            Dictionary<string, string> iterationPathMap,
            params BusinessNodeDataNode[] children
            )
            : base(children)
        {
            Id = id;
            Name = name;
            BusinessNodeType = businessNodeType;
            Team = team;
            Disabled = disabled;
            IsClone = isClone;            
            IsOnPremiseProject = isOnPremiseProject;
            AzureDevOpsServerFQDN = azureDevOpsServerFQDN;
            OrganizationName = organizationName;
            TeamPrefix = teamPrefix;
            Process = process;
            TeamInclusions = teamInclusions;
            AreaBasePaths = areaBasePaths;
            AreaPathMap = areaPathMap;
            IterationBasePaths = iterationBasePaths;
            IterationPathMap = iterationPathMap;            
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public BusinessNodeType BusinessNodeType { get; set; }
        public bool Disabled { get; set; } //only used for TeamProject nodes
        public bool IsClone { get; set; } //only used for TeamProject nodes
        public bool IsOnPremiseProject { get; set; } //only used for TeamProject nodes
        public string AzureDevOpsServerFQDN { get; set; } //only used for TeamProject nodes
        public string OrganizationName { get; set; } //only for TeamProject nodes
        public string TeamPrefix { get; set; } //only used for TeamProject nodes
        public string Process { get; set; } //only used for TeamProject nodes
        public List<string> TeamInclusions { get; set; } //only used for TeamProject nodes
        public List<string> AreaBasePaths { get; set; } //only used for TeamProject nodes
        public Dictionary<string, string> AreaPathMap { get; set; } //only used for TeamProject nodes
        public List<string> IterationBasePaths { get; set; } //only used for TeamProject nodes
        public Dictionary<string, string> IterationPathMap { get; set; } //only used for TeamProject nodes

        public BusinessTeam Team { get; set; }
    }
}
