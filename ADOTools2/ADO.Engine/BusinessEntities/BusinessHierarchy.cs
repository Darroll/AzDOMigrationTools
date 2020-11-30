using ADO.Engine.Utilities;
using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ADO.Engine.BusinessEntities
{
    public class BusinessHierarchy
    {
        public string Name { get; set; }
        public BusinessNodeType BusinessNodeType { get; set; }
        public bool Disabled { get; set; }
        public bool IsClone { get; set; }
        public bool IsOnPremiseProject { get; set; }
        public string AzureDevOpsServerFQDN { get; set; }
        public string OrganizationOrCollectionName { get; set; }
        //public string ProjectName { get; set; }
        public string TeamPrefix { get; set; }
        public string Process { get; set; }
        public List<string> TeamInclusions { get; set; }
        public string AreaPrefixPath { get; set; }
        public List<string> AreaBasePaths { get; set; }
        public Dictionary<string, string> AreaPathMap { get; set; }
        public string IterationPrefixPath { get; set; }
        public List<string> IterationBasePaths { get; set; }
        public Dictionary<string, string> IterationPathMap { get; set; }
        public BusinessHierarchyTeam BusinessHierarchyTeam { get; set; }
        public List<BusinessHierarchy> Children { get; set; }

        public List<string> GetAreas()
        {
            List<string> allAreasInPreOrderTraversal = new List<string>();

            allAreasInPreOrderTraversal.Add($"\\{this.Name}\\{Constants.AreaStructureType}");
            foreach (var portfolio in this.Children)
            {
                allAreasInPreOrderTraversal = allAreasInPreOrderTraversal.Concat(GetAreasInternal(portfolio)).ToList();
            }

            return allAreasInPreOrderTraversal;
        }

        public List<BusinessHierarchyCsv> ToBusinessHierarchyCsvList(
            string organizationOrCollectionName
            )
        {
            List<BusinessHierarchyCsv> allAreasInPreOrderTraversal
                = new List<BusinessHierarchyCsv>();

            foreach (var portfolio in this.Children)
            {
                foreach (var programOrProduct in portfolio.Children)
                {
                    foreach (var teamProject in programOrProduct.Children)
                    {
                        allAreasInPreOrderTraversal.Add(new BusinessHierarchyCsv()
                        {
                            OrganizationOrCollection = organizationOrCollectionName,
                            Portfolio = portfolio.Name,
                            ProgramOrProduct = programOrProduct.Name,
                            Project = teamProject.Name,
                            Prefix = "",
                            Process = teamProject.Process,
                            TeamInclusionList = teamProject.TeamInclusions.SetList(Constants.DefaultDelineatorForListsInCsv),
                            AreaPrefixPath = teamProject.AreaPrefixPath,
                            AreaBasePathList = teamProject.AreaBasePaths.SetList(Constants.DefaultDelineatorForListsInCsv),
                            AreaPathMapDictionary = teamProject.AreaPathMap.SetDictionary(Constants.DefaultDelineatorForListsInCsv, Constants.DefaultKeyValueSeparator),
                            IterationPrefixPath = teamProject.IterationPrefixPath,
                            IterationBasePathList = teamProject.IterationBasePaths.SetList(Constants.DefaultDelineatorForListsInCsv),
                            IterationPathMapDictionary = teamProject.IterationPathMap.SetDictionary(Constants.DefaultDelineatorForListsInCsv, Constants.DefaultKeyValueSeparator),
                            IsClone = teamProject.IsClone,
                            IsOnPremiseProject = teamProject.IsOnPremiseProject,
                            AzureDevOpsServerFQDN = teamProject.AzureDevOpsServerFQDN
                        });
                    }
                }
            }

            return allAreasInPreOrderTraversal;
        }

        public static BusinessHierarchy LoadFromCsv(
            string pathToCsv,
            string businessHierarchyName,
            string portfolioTeamSuffix,
            string programOrProductTeamSuffix,
            string defaultProjectTeamSuffix,
            BusinessNodeType businessNodeTypePreferenceForProgramOrProduct,
            bool sortByNameRecursively,
            out List<BusinessHierarchyCsv> bizList
            )
        {
            CsvFileDescription inputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',', // tab delimited
                FirstLineHasColumnNames = true, // no column names in first record
                //FileCultureName = "nl-NL" // use formats used in The Netherlands
            };

            CsvContext cc = new CsvContext();

            bizList =
                cc.Read<BusinessHierarchyCsv>(pathToCsv, inputFileDescription).ToList();

            BusinessHierarchy businessHierarchy = new BusinessHierarchy()
            {
                Name = businessHierarchyName,
                Children = new List<BusinessHierarchy>(),
            };
            foreach (BusinessHierarchyCsv teamProject in bizList)
            {
                if (!string.IsNullOrEmpty(teamProject.Portfolio) && !string.IsNullOrEmpty(teamProject.ProgramOrProduct))
                {

                    businessHierarchy.AddTeamProject(
                        teamProject.OrganizationOrCollection,
                        teamProject.Project,
                        teamProject.ProgramOrProduct,
                        programOrProductTeamSuffix,
                        businessNodeTypePreferenceForProgramOrProduct,
                        teamProject.Portfolio,
                        portfolioTeamSuffix,
                        teamProject.Prefix,
                        teamProject.Process,
                        teamProject.TeamInclusionList,
                        teamProject.AreaPrefixPath,
                        teamProject.AreaBasePathList,
                        teamProject.AreaPathMapDictionary,
                        teamProject.IterationPrefixPath,
                        teamProject.IterationBasePathList,
                        teamProject.IterationPathMapDictionary,
                        teamProject.IsClone,
                        teamProject.IsOnPremiseProject,
                        teamProject.AzureDevOpsServerFQDN,
                        defaultProjectTeamSuffix
                        );
                }
                else if (string.IsNullOrEmpty(teamProject.Portfolio) && string.IsNullOrEmpty(teamProject.ProgramOrProduct))
                {
                    //we are expecting a clone!
                    if (teamProject.IsClone && bizList.Count == 1)
                    {
                        //just fix the root -- root should be a team project
                        businessHierarchy.Name = teamProject.Project;
                        businessHierarchy.BusinessNodeType = BusinessNodeType.TeamProject;
                        businessHierarchy.Disabled = false;// string.IsNullOrEmpty(teamProject.AreaPrefixPath);
                        businessHierarchy.IsClone = teamProject.IsClone;
                        businessHierarchy.IsOnPremiseProject = teamProject.IsOnPremiseProject;
                        businessHierarchy.AzureDevOpsServerFQDN = teamProject.AzureDevOpsServerFQDN;
                        businessHierarchy.OrganizationOrCollectionName = teamProject.OrganizationOrCollection;
                        businessHierarchy.TeamPrefix = teamProject.Prefix;
                        businessHierarchy.Process = teamProject.Process;
                        businessHierarchy.TeamInclusions = teamProject.TeamInclusionList.GetList(Constants.DefaultDelineatorForListsInCsv);
                        businessHierarchy.AreaPrefixPath = teamProject.AreaPrefixPath;
                        businessHierarchy.AreaBasePaths = teamProject.AreaBasePathList.GetList(Constants.DefaultDelineatorForListsInCsv);
                        businessHierarchy.AreaPathMap = teamProject.AreaPathMapDictionary.GetDictionary(Constants.DefaultDelineatorForListsInCsv, Constants.DefaultKeyValueSeparator);
                        businessHierarchy.IterationPrefixPath = teamProject.IterationPrefixPath;
                        businessHierarchy.IterationBasePaths = teamProject.IterationBasePathList.GetList(Constants.DefaultDelineatorForListsInCsv);
                        businessHierarchy.IterationPathMap = teamProject.IterationPathMapDictionary.GetDictionary(Constants.DefaultDelineatorForListsInCsv, Constants.DefaultKeyValueSeparator);
                        //don't forget team
                        businessHierarchy.BusinessHierarchyTeam = new BusinessHierarchyTeam()
                        {
                            Name = $"{teamProject.Project} {defaultProjectTeamSuffix}"
                        };
                    }
                    else
                    {
                        throw new NotImplementedException("have only implemented a clone");
                    }
                }
                else
                {
                    throw new NotImplementedException("have only implemented extreme cases not in the middle cases");
                }
            }

            if (sortByNameRecursively)
            {
                businessHierarchy.Children.Sort(new BusinessHierarchyNameComparer());
                foreach (var portfolio in businessHierarchy.Children)
                {
                    portfolio.Children.Sort(new BusinessHierarchyNameComparer());
                    foreach (var prog in portfolio.Children)
                    {
                        prog.Children.Sort(new BusinessHierarchyNameComparer());
                    }
                }
            }

            return businessHierarchy;
        }

        public BusinessHierarchy AddTeamProject(
            string organizationOrCollectionName,
            string projectName,
            string programOrProductName,
            string programOrProductTeamSuffix,
            BusinessNodeType businessNodeTypePreferenceForProgramOrProduct,
            string portfolioName,
            string portfolioTeamSuffix,
            string teamPrefix,
            string process,
            string teamInclusionList,
            string areaPrefixPath,
            string areaBasePathList,
            string areaPathMapDictionary,
            string iterationPrefixPath,
            string iterationBasePathList,
            string iterationPathMapDictionary,
            bool isClone,
            bool isOnPremiseProject,
            string azureDevOpsServerFQDN,
            string defaultProjectTeamSuffix)
        {

            if (!string.IsNullOrEmpty(portfolioName) && !string.IsNullOrEmpty(programOrProductName))
            {
                BusinessHierarchy portfolio = this.GetPortfolio(portfolioName);
                if (portfolio == null)
                {
                    this.AddPortfolio(portfolioName, portfolioTeamSuffix);
                }
                BusinessHierarchy programOrProduct = this.GetProgramOrProduct(programOrProductName, portfolioName);
                if (programOrProduct == null)
                {
                    programOrProduct = this.AddProgramOrProduct(programOrProductName, portfolioName, programOrProductTeamSuffix,
                        businessNodeTypePreferenceForProgramOrProduct);
                }
                BusinessHierarchy teamProject = this.GetTeamProject(projectName, programOrProductName, portfolioName);
                if (teamProject == null)
                {
                    teamProject = new BusinessHierarchy()
                    {
                        BusinessHierarchyTeam = new BusinessHierarchyTeam()
                        {
                            Name = $"{projectName} {defaultProjectTeamSuffix}"
                        },
                        BusinessNodeType = BusinessNodeType.TeamProject,
                        Children = new List<BusinessHierarchy>(),
                        Disabled = string.IsNullOrEmpty(areaPrefixPath),
                        IsClone = isClone,
                        IsOnPremiseProject = isOnPremiseProject,
                        AzureDevOpsServerFQDN = azureDevOpsServerFQDN,
                        OrganizationOrCollectionName = organizationOrCollectionName,
                        Name = projectName,
                        TeamPrefix = teamPrefix,
                        Process = process,
                        TeamInclusions = teamInclusionList.GetList(Constants.DefaultDelineatorForListsInCsv),
                        AreaPrefixPath = areaPrefixPath,
                        AreaBasePaths = areaBasePathList.GetList(Constants.DefaultDelineatorForListsInCsv),
                        AreaPathMap = areaPathMapDictionary.GetDictionary(Constants.DefaultDelineatorForListsInCsv, Constants.DefaultKeyValueSeparator),
                        IterationPrefixPath = iterationPrefixPath,
                        IterationBasePaths = iterationBasePathList.GetList(Constants.DefaultDelineatorForListsInCsv),
                        IterationPathMap = iterationPathMapDictionary.GetDictionary(Constants.DefaultDelineatorForListsInCsv, Constants.DefaultKeyValueSeparator)
                    };
                    programOrProduct.Children.Add(teamProject);

                }
                return teamProject;
            }
            else
            {
                throw new NotImplementedException("have only implemented case when a portfolio and productOrProgram exists");
            }
        }

        public BusinessHierarchy GetTeamProject(string projectName, string programOrProductName, string portfolioName)
        {
            var programOrProduct = this.GetProgramOrProduct(programOrProductName, portfolioName);
            if (programOrProduct == null)
                return null;
            return programOrProduct.Children.SingleOrDefault(pp => pp.Name == projectName);
        }

        public BusinessHierarchy AddProgramOrProduct(string programOrProductName, string portfolioName, string programOrProductTeamSuffix,
            BusinessNodeType businessNodeTypePreferenceForProgramOrProduct)
        {
            BusinessHierarchy portfolio = this.GetPortfolio(portfolioName);
            BusinessHierarchy newProgramOrProduct = new BusinessHierarchy()
            {
                Name = programOrProductName,
                BusinessNodeType = businessNodeTypePreferenceForProgramOrProduct,
                BusinessHierarchyTeam = new BusinessHierarchyTeam()
                {
                    Name = $"{programOrProductName} {programOrProductTeamSuffix}"
                },
                Children = new List<BusinessHierarchy>()
            };
            portfolio.Children.Add(newProgramOrProduct);
            return newProgramOrProduct;
        }

        public BusinessHierarchy GetProgramOrProduct(string programOrProductName, string portfolioName)
        {
            var portfolio = this.GetPortfolio(portfolioName);
            if (portfolio == null)
                return null;
            return portfolio.Children.SingleOrDefault(pp => pp.Name == programOrProductName);
        }

        public BusinessHierarchy AddPortfolio(string portfolioName, string portfolioTeamSuffix)
        {
            BusinessHierarchy newPortfolio = new BusinessHierarchy()
            {
                Name = portfolioName,
                BusinessNodeType = BusinessNodeType.Portfolio,
                BusinessHierarchyTeam = new BusinessHierarchyTeam()
                {
                    Name = $"{portfolioName} {portfolioTeamSuffix}"
                },
                Children = new List<BusinessHierarchy>()
            };
            this.Children.Add(newPortfolio);
            return newPortfolio;
        }

        public BusinessHierarchy GetPortfolio(string portfolioName)
        {
            return this.Children.SingleOrDefault(pf => pf.Name == portfolioName);
        }

        internal List<string> GetAreasInternal(BusinessHierarchy portfolio)
        {
            List<string> allAreasInPreOrderTraversal = new List<string>();

            allAreasInPreOrderTraversal.Add($"\\{this.Name}\\{Constants.AreaStructureType}\\{portfolio.Name}");
            foreach (var programOrProduct in portfolio.Children)
            {
                allAreasInPreOrderTraversal = allAreasInPreOrderTraversal.Concat(GetAreasInternal(portfolio, programOrProduct)).ToList();
            }

            return allAreasInPreOrderTraversal;
        }

        internal List<string> GetAreasInternal(BusinessHierarchy portfolio, BusinessHierarchy programOrProduct)
        {
            List<string> allAreasInPreOrderTraversal = new List<string>();

            allAreasInPreOrderTraversal.Add($"\\{this.Name}\\{Constants.AreaStructureType}\\{portfolio.Name}\\{programOrProduct.Name}");
            foreach (var teamProject in programOrProduct.Children)
            {
                allAreasInPreOrderTraversal.Add($"\\{this.Name}\\{Constants.AreaStructureType}\\{portfolio.Name}\\{programOrProduct.Name}\\{teamProject.Name}");
            }

            return allAreasInPreOrderTraversal;
        }
    }

    //public class Portfolio
    //{
    //    public string Name { get; set; }
    //    public BusinessHierarchyTeam PortfolioTeam { get; set; }
    //    public List<ProgramOrProduct> ProgramOrProducts { get; set; }
    //}
    //public class PortfolioTeam
    //{
    //    public string Name { get; set; }
    //}

    //public class ProgramOrProduct
    //{
    //    public string Name { get; set; }
    //    public BusinessHierarchyTeam ProgramOrProductTeam { get; set; }
    //    public List<TeamProject> TeamProjects { get; set; }
    //}

    //public class TeamProject
    //{
    //    public bool Disabled { get; set; }
    //    public bool IsClone { get; set; }
    //    public bool IsOnPremiseProject { get; set; }
    //    public string AzureDevOpsServerFQDN { get; set; }
    //    public string OrganizationOrCollectionName { get; set; }
    //    public string ProjectName { get; set; }
    //    public string TeamPrefix { get; set; }
    //    public string Process { get; set; }
    //    public List<string> TeamInclusions { get; set; }
    //    public string AreaPrefixPath { get; set; }
    //    public List<string> AreaBasePaths { get; set; }
    //    public Dictionary<string, string> AreaPathMap { get; set; }
    //    public string IterationPrefixPath { get; set; }
    //    public List<string> IterationBasePaths { get; set; }
    //    public Dictionary<string, string> IterationPathMap { get; set; }
    //}

    public class BusinessHierarchyTeam
    {
        public string Name { get; set; }

        public BusinessTeam ToTeam()
        {
            if (this == null)
                return null;
            else
            {
                return new BusinessTeam()
                {
                    Name = this.Name,
                    Cadences = new List<Cadence>(),
                    Description = null,
                    TeamLevel = TeamLevel.None
                };
            }
        }
    }

    //public class ProgramOrProductTeam
    //{
    //    public string Name { get; set; }
    //}
}
