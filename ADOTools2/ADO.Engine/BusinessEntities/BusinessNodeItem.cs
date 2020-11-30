using System;
using System.Collections.Generic;

namespace ADO.Engine.BusinessEntities
{
    public class BusinessNodeItem
    {
        public BusinessNodeItem(
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
            Dictionary<string, string> iterationPathMap
            )
        {
            Id = id;
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
                Disabled = disabled;
            }
            else
            {
                throw new ArgumentNullException($"name should NOT be null or empty for node id {id} of business node type {businessNodeType.ToString()}");
                Name = $"{Constants.NullString}_{businessNodeType}_{id}";
                Disabled = true;
            }
            IsClone = isClone;
            IsOnPremiseProject = isOnPremiseProject;
            AzureDevOpsServerFQDN = azureDevOpsServerFQDN;
            BusinessNodeType = businessNodeType;
            Team = team;
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
        public BusinessTeam Team { get; set; }
        public bool Disabled { get; set; } //only used for TeamProject nodes
        public bool IsClone { get; set; } //only used for TeamProject nodes
        public bool IsOnPremiseProject { get; set; } //only used for TeamProject nodes
        public string AzureDevOpsServerFQDN { get; set; } //only used for TeamProject nodes
        //only for TeamProject nodes
        public string OrganizationName { get; set; }
        public string TeamPrefix { get; set; } //only used for TeamProject nodes
        public string Process { get; set; } //only used for TeamProject nodes
        public List<string> TeamInclusions { get; set; } //only used for TeamProject nodes
        public List<string> AreaBasePaths { get; set; } //only used for TeamProject nodes
        public Dictionary<string, string> AreaPathMap { get; set; } //only used for TeamProject nodes
        public List<string> IterationBasePaths { get; set; } //only used for TeamProject nodes
        public Dictionary<string, string> IterationPathMap { get; set; } //only used for TeamProject nodes

    }

    public class DualStateBusinessNodeItem : BusinessNodeItem
    {
        public DualStateBusinessNodeItem(
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
            Dictionary<string, string> iterationPathMap)
            : base(
                  id,
            name,
            businessNodeType,
            team,
            disabled,
            isClone,
            isOnPremiseProject,
            azureDevOpsServerFQDN,
            organizationName,
            teamPrefix,
            process,
            teamInclusions,
            areaBasePaths,
            areaPathMap,
            iterationBasePaths,
            iterationPathMap)
        {
            IsEnabled = true;
        }

        public DualStateBusinessNodeItem(BusinessNodeItem sourceItem)
            : base(
                  sourceItem.Id,
            sourceItem.Name,
            sourceItem.BusinessNodeType,
            sourceItem.Team,
            sourceItem.Disabled,
            sourceItem.IsClone,
            sourceItem.IsOnPremiseProject,
            sourceItem.AzureDevOpsServerFQDN,
            sourceItem.OrganizationName,
            sourceItem.TeamPrefix,
            sourceItem.Process,
            sourceItem.TeamInclusions,
            sourceItem.AreaBasePaths,
            sourceItem.AreaPathMap,
            sourceItem.IterationBasePaths,
            sourceItem.IterationPathMap)
        {
            IsEnabled = true;
        }

        public bool IsEnabled { get; set; }
    }
}
