using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ADO.Engine.BusinessEntities
{
    public class BusinessNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BusinessNodeType BusinessNodeType { get; set; }
        public bool Disabled { get; set; } //only used for TeamProject nodes
        public bool IsClone { get; set; } //only used for TeamProject nodes
        public bool IsOnPremiseProject { get; set; } //only used for TeamProject nodes
        public string AzureDevOpsServerFQDN { get; set; } //only used for TeamProject nodes
        public string OrganizationName { get; set; } //only used for TeamProject nodes
        public string TeamPrefix { get; set; } //only used for TeamProject nodes
        public string Process { get; set; } //only used for TeamProject nodes
        public List<string> TeamInclusions { get; set; } //only used for TeamProject nodes
        public List<string> AreaBasePaths { get; set; } //only used for TeamProject nodes
        public Dictionary<string, string> AreaPathMap { get; set; } //only used for TeamProject nodes
        public List<string> IterationBasePaths { get; set; }//only used for TeamProject nodes
        public Dictionary<string, string> IterationPathMap { get; set; }//only used for TeamProject nodes
        public BusinessTeam Team { get; set; }
        public BusinessNode[] Children { get; set; }

        public static BusinessNode FromBusinessHierarchy(
            BusinessHierarchy businessHierarchy,
            List<Cadence> defaultPortfolioCadences,
            BusinessNodeType programOrProductBusinessNodeType,
            List<Cadence> defaultProgramOrProductTeamCadences,
            List<Cadence> defaultTeamProjectTeamCadences,
            string defaultProjectTeamSuffix)
        {
            int idCounter = 0;

            BusinessNode newRootBusinessNode = new BusinessNode()
            {
                Id = ++idCounter,
                Name = businessHierarchy.Name,
                BusinessNodeType = businessHierarchy.BusinessNodeType,
                Disabled = businessHierarchy.Disabled,
                IsClone = businessHierarchy.IsClone,
                IsOnPremiseProject = businessHierarchy.IsOnPremiseProject,
                AzureDevOpsServerFQDN = businessHierarchy.AzureDevOpsServerFQDN,
                OrganizationName = businessHierarchy.OrganizationOrCollectionName,
                TeamPrefix = businessHierarchy.TeamPrefix,
                Team = businessHierarchy.BusinessHierarchyTeam?.ToTeam(),
                Process = businessHierarchy.Process,
                TeamInclusions = businessHierarchy.TeamInclusions,
                AreaBasePaths = businessHierarchy.AreaBasePaths,
                AreaPathMap = businessHierarchy.AreaPathMap,
                IterationBasePaths = businessHierarchy.IterationBasePaths,
                IterationPathMap = businessHierarchy.IterationPathMap,
                Children = new BusinessNode[businessHierarchy.Children.Count]
            };

            int portfolioCounter = 0;
            foreach (var portfolio in businessHierarchy.Children)
            {
                BusinessNode newPortfolioBusinessNode = new BusinessNode()
                {
                    Id = ++idCounter,
                    Name = portfolio.Name,
                    BusinessNodeType = portfolio.BusinessNodeType,
                    Disabled = portfolio.Disabled,
                    IsClone = portfolio.IsClone,
                    IsOnPremiseProject = portfolio.IsOnPremiseProject,
                    AzureDevOpsServerFQDN = portfolio.AzureDevOpsServerFQDN,
                    OrganizationName = portfolio.OrganizationOrCollectionName,
                    TeamPrefix = portfolio.TeamPrefix,
                    Process = portfolio.Process,
                    TeamInclusions = portfolio.TeamInclusions,
                    AreaBasePaths = portfolio.AreaBasePaths,
                    AreaPathMap = portfolio.AreaPathMap,
                    IterationBasePaths = portfolio.IterationBasePaths,
                    IterationPathMap = portfolio.IterationPathMap,
                    Team = new BusinessTeam()
                    {
                        Name = $"{portfolio.BusinessHierarchyTeam.Name}",
                        Description = $"{portfolio.BusinessHierarchyTeam.Name} is responsible for {TeamLevel.Epic.ToString()}-level work items",
                        TeamLevel = TeamLevel.Epic,
                        Cadences = defaultPortfolioCadences
                    },
                    Children = new BusinessNode[portfolio.Children.Count]
                };
                newRootBusinessNode.Children[portfolioCounter++] = newPortfolioBusinessNode;

                int progCounter = 0;
                foreach (var prog in portfolio.Children)
                {
                    BusinessNode newProgramOrProductBusinessNode = new BusinessNode()
                    {
                        Id = ++idCounter,
                        Name = prog.Name,
                        BusinessNodeType = prog.BusinessNodeType,// programOrProductBusinessNodeType,
                        Disabled = prog.Disabled,
                        IsClone = prog.IsClone,
                        IsOnPremiseProject = prog.IsOnPremiseProject,
                        AzureDevOpsServerFQDN = prog.AzureDevOpsServerFQDN,
                        OrganizationName = prog.OrganizationOrCollectionName,
                        TeamPrefix = prog.TeamPrefix,
                        Process = prog.Process,
                        TeamInclusions = prog.TeamInclusions,
                        AreaBasePaths = prog.AreaBasePaths,
                        AreaPathMap = prog.AreaPathMap,
                        IterationBasePaths = prog.IterationBasePaths,
                        IterationPathMap = prog.IterationPathMap,
                        Team = new BusinessTeam()
                        {
                            Name = $"{prog.BusinessHierarchyTeam.Name}",
                            Description = $"{prog.BusinessHierarchyTeam.Name} is responsible for {TeamLevel.Feature.ToString()}-level work items",
                            TeamLevel = TeamLevel.Feature,
                            Cadences = defaultProgramOrProductTeamCadences
                        },
                        Children = new BusinessNode[prog.Children.Count]
                    };
                    newPortfolioBusinessNode.Children[progCounter++] = newProgramOrProductBusinessNode;

                    int projectCounter = 0;
                    foreach (var project in prog.Children)
                    {
                        BusinessNode newProjectBusinessNode = new BusinessNode()
                        {
                            Id = ++idCounter,
                            Name = project.Name,
                            BusinessNodeType = project.BusinessNodeType,// BusinessNodeType.TeamProject,
                            Disabled = project.Disabled,
                            IsClone = project.IsClone,
                            IsOnPremiseProject = project.IsOnPremiseProject,
                            AzureDevOpsServerFQDN = project.AzureDevOpsServerFQDN,
                            OrganizationName = project.OrganizationOrCollectionName,
                            TeamPrefix = project.TeamPrefix,
                            Process = project.Process,
                            TeamInclusions = project.TeamInclusions,
                            AreaBasePaths = project.AreaBasePaths,
                            AreaPathMap = project.AreaPathMap,
                            IterationBasePaths = project.IterationBasePaths,
                            IterationPathMap = project.IterationPathMap,
                            Team = new BusinessTeam()
                            {
                                Name = $"{project.Name} {defaultProjectTeamSuffix}",
                                Description = $"{project.Name} Team is responsible for {TeamLevel.Requirement.ToString()}-level work items",
                                TeamLevel = TeamLevel.Requirement,
                                Cadences = defaultTeamProjectTeamCadences
                            },
                            Children = new BusinessNode[0],
                        };
                        newProgramOrProductBusinessNode.Children[projectCounter++] = newProjectBusinessNode;
                    }
                }
            }

            return newRootBusinessNode;
        }

        public static BusinessNode LoadFromCsv(
            string pathToCsv,
            string businessHierarchyName,
            bool sortByNameRecursively,
            BusinessNodeType programOrProductBusinessNodeType,
            List<Cadence> defaultPortfolioCadences,
            List<Cadence> defaultProgramOrProductTeamCadences,
            List<Cadence> defaultTeamProjectTeamCadences,
            string portfolioTeamSuffix,
            string programOrProductTeamSuffix,
            string defaultProjectTeamSuffix,
            out List<BusinessHierarchyCsv> businessHierarchyCsvList)
        {
            var bizHierarchy
                = BusinessHierarchy.LoadFromCsv(
                    pathToCsv,
                    businessHierarchyName,
                    portfolioTeamSuffix,
                    programOrProductTeamSuffix,
                    defaultProjectTeamSuffix,
                    programOrProductBusinessNodeType,
                    sortByNameRecursively,
                    out businessHierarchyCsvList
                    );
            var bizNode
                = BusinessNode.FromBusinessHierarchy(
                    bizHierarchy,
                    defaultPortfolioCadences,
                    programOrProductBusinessNodeType,
                    defaultProgramOrProductTeamCadences,
                    defaultTeamProjectTeamCadences,
                    defaultProjectTeamSuffix
                    );
            return bizNode;
        }
    }

    public class BusinessTeam
    {
        public string Name { get; set; }
        public TeamLevel TeamLevel { get; set; }
        public string Description { get; set; }
        public List<Cadence> Cadences { get; set; }
    }

    public class Cadence
    {
        public DateTime CadenceStart { get; set; }
        public DateTime CadenceEnd { get; set; }
        public CadenceType CadenceType { get; set; }
        public byte NumberOfWeeksPerSprint { get; set; }
        public byte NumberOfSprintsPerProgramIncrement { get; set; }
        public byte FiscalMonthStart { get; set; } //1 is January
        public DayOfWeek SprintDayOfWeekStart { get; set; } //0 is Sunday -- DayOfWeek.Sunday
        public int IterationCounterStart { get; set; }

        public Cadence()
        {

        }

        public Cadence(
            DateTime cadenceStart,
            DateTime cadenceEnd,
            CadenceType cadenceType,
            byte numberOfWeeksPerSprint = 0,
            byte numberOfSprintsPerProgramIncrement = 0,
            DayOfWeek sprintDayOfWeekStart = DayOfWeek.Sunday,
            byte fiscalMonthStart = 1,
            int iterationCounterStart = 1
            )
        {
            CadenceStart = cadenceStart;
            CadenceEnd = cadenceEnd;
            CadenceType = cadenceType;
            NumberOfWeeksPerSprint = numberOfWeeksPerSprint;
            NumberOfSprintsPerProgramIncrement = numberOfSprintsPerProgramIncrement;
            FiscalMonthStart = fiscalMonthStart;
            SprintDayOfWeekStart = sprintDayOfWeekStart;
            IterationCounterStart = iterationCounterStart;
            this.Validate();
        }

        public void Validate()
        {
            if (CadenceEnd < CadenceStart)
            {
                throw new ArgumentException($"Cadence Start Date {CadenceStart.ToLongDateString()} must precede Cadence End Date {CadenceEnd.ToLongDateString()}");
            }
            if (FiscalMonthStart < 1 || FiscalMonthStart > 12)
            {
                throw new ArgumentException($"Please ensure fiscalMonthStart {FiscalMonthStart} is between 1 and 12");
            }
        }

        public List<Iteration> GetIterations(string pathPrefix, TeamLevel teamLevel)
        {
            List<Iteration> iterations = new List<Iteration>();
            switch (CadenceType)
            {
                case CadenceType.FiscalYear:
                    {
                        int startYear = this.CadenceStart.Year;
                        Iteration possibleIteration = new Iteration()
                        {
                            StartDate = new DateTime(startYear, FiscalMonthStart, 1)
                        };
                        possibleIteration.FinishDate = possibleIteration.StartDate.AddYears(1).AddDays(-1);
                        possibleIteration.Name = $"{CadenceType.GetDescription()} {possibleIteration.StartDate.GetFinancialYear(FiscalMonthStart)}";
                        possibleIteration.Path = $"{(!string.IsNullOrEmpty(pathPrefix) ? pathPrefix : "")}\\{possibleIteration.Name}";
                        possibleIteration.TeamLevel = teamLevel;
                        while (possibleIteration.StartDate < this.CadenceEnd)
                        {
                            if (possibleIteration.StartDate >= this.CadenceStart)
                            {
                                iterations.Add(new Iteration()
                                {
                                    Name = possibleIteration.Name,
                                    StartDate = possibleIteration.StartDate,
                                    FinishDate = possibleIteration.FinishDate,
                                    Path = possibleIteration.Path,
                                    TeamLevel = possibleIteration.TeamLevel
                                });
                            }
                            //now increment
                            possibleIteration.StartDate = possibleIteration.FinishDate.AddDays(1);
                            possibleIteration.FinishDate = possibleIteration.StartDate.AddYears(1).AddDays(-1);
                            possibleIteration.Name = $"{CadenceType.GetDescription()} {possibleIteration.StartDate.GetFinancialYear(FiscalMonthStart)}";
                            possibleIteration.Path = $"{(!string.IsNullOrEmpty(pathPrefix) ? pathPrefix : "")}\\{possibleIteration.Name}";
                            possibleIteration.TeamLevel = teamLevel;
                        }
                        break;
                    }
                case CadenceType.FiscalSemester:
                    {
                        int startYear = this.CadenceStart.Year;
                        Iteration possibleIteration = new Iteration()
                        {
                            StartDate = new DateTime(startYear, GetMinMonth(CadenceType, FiscalMonthStart), 1)
                        };
                        possibleIteration.FinishDate = possibleIteration.StartDate.AddMonths(6).AddDays(-1);
                        possibleIteration.Name = $"{CadenceType.FiscalYear.GetDescription()} {possibleIteration.StartDate.GetFinancialYear(FiscalMonthStart)} - {Constants.FiscalSemesterAbbreviation}{possibleIteration.StartDate.GetFinancialSemester(FiscalMonthStart)}";
                        possibleIteration.Path = $"{(!string.IsNullOrEmpty(pathPrefix) ? pathPrefix : "")}\\{possibleIteration.Name}";
                        possibleIteration.TeamLevel = teamLevel;
                        while (possibleIteration.StartDate < this.CadenceEnd)
                        {
                            if (possibleIteration.StartDate >= this.CadenceStart)
                            {
                                iterations.Add(new Iteration()
                                {
                                    Name = possibleIteration.Name,
                                    StartDate = possibleIteration.StartDate,
                                    FinishDate = possibleIteration.FinishDate,
                                    Path = possibleIteration.Path,
                                    TeamLevel = possibleIteration.TeamLevel
                                });
                            }
                            //now increment
                            possibleIteration.StartDate = possibleIteration.FinishDate.AddDays(1);
                            possibleIteration.FinishDate = possibleIteration.StartDate.AddMonths(6).AddDays(-1);
                            possibleIteration.Name = $"{CadenceType.FiscalYear.GetDescription()} {possibleIteration.StartDate.GetFinancialYear(FiscalMonthStart)} - {Constants.FiscalSemesterAbbreviation}{possibleIteration.StartDate.GetFinancialSemester(FiscalMonthStart)}";
                            possibleIteration.Path = $"{(!string.IsNullOrEmpty(pathPrefix) ? pathPrefix : "")}\\{possibleIteration.Name}";
                            possibleIteration.TeamLevel = teamLevel;
                        }
                        break;
                    }
                case CadenceType.FiscalQuarter:
                    {
                        int startYear = this.CadenceStart.Year;
                        Iteration possibleIteration = new Iteration()
                        {
                            StartDate = new DateTime(startYear, GetMinMonth(CadenceType, FiscalMonthStart), 1)
                        };
                        possibleIteration.FinishDate = possibleIteration.StartDate.AddMonths(3).AddDays(-1);
                        possibleIteration.Name = $"{CadenceType.FiscalYear.GetDescription()} {possibleIteration.StartDate.GetFinancialYear(FiscalMonthStart)} - {Constants.FiscalQuarterAbbreviation}{possibleIteration.StartDate.GetFinancialQuarter(FiscalMonthStart)}";
                        possibleIteration.Path = $"{(!string.IsNullOrEmpty(pathPrefix) ? pathPrefix : "")}\\{possibleIteration.Name}";
                        possibleIteration.TeamLevel = teamLevel;
                        while (possibleIteration.StartDate < this.CadenceEnd)
                        {
                            if (possibleIteration.StartDate >= this.CadenceStart)
                            {
                                iterations.Add(new Iteration()
                                {
                                    Name = possibleIteration.Name,
                                    StartDate = possibleIteration.StartDate,
                                    FinishDate = possibleIteration.FinishDate,
                                    Path = possibleIteration.Path,
                                    TeamLevel = possibleIteration.TeamLevel
                                });
                            }
                            //now increment
                            possibleIteration.StartDate = possibleIteration.FinishDate.AddDays(1);
                            possibleIteration.FinishDate = possibleIteration.StartDate.AddMonths(3).AddDays(-1);
                            possibleIteration.Name = $"{CadenceType.FiscalYear.GetDescription()} {possibleIteration.StartDate.GetFinancialYear(FiscalMonthStart)} - {Constants.FiscalQuarterAbbreviation}{possibleIteration.StartDate.GetFinancialQuarter(FiscalMonthStart)}";
                            possibleIteration.Path = $"{(!string.IsNullOrEmpty(pathPrefix) ? pathPrefix : "")}\\{possibleIteration.Name}";
                            possibleIteration.TeamLevel = teamLevel;
                        }
                        break;
                    }
                case CadenceType.ProgramIncrement:
                    {
                        Iteration possibleIteration = new Iteration()
                        {
                            StartDate = GetNextDayOfWeek(this.CadenceStart, this.SprintDayOfWeekStart),
                        };
                        int iterationCount = IterationCounterStart;
                        int numberOfDaysPerProgramIncrement
                            = NumberOfWeeksPerSprint * 7 * NumberOfSprintsPerProgramIncrement;
                        if (numberOfDaysPerProgramIncrement <= 0)
                        {
                            throw new ArgumentException($"Either num of sprints or num of weeks per sprint must be positive");
                        }
                        possibleIteration.FinishDate = possibleIteration.StartDate.AddDays(numberOfDaysPerProgramIncrement).AddDays(-1);
                        possibleIteration.Name = $"{CadenceType.GetDescription()} {iterationCount++}";
                        possibleIteration.Path = $"{(!string.IsNullOrEmpty(pathPrefix) ? pathPrefix : "")}\\{possibleIteration.Name}";
                        possibleIteration.TeamLevel = teamLevel;
                        while (possibleIteration.StartDate < this.CadenceEnd)
                        {
                            if (possibleIteration.StartDate >= this.CadenceStart)
                            {
                                iterations.Add(new Iteration()
                                {
                                    Name = possibleIteration.Name,
                                    StartDate = possibleIteration.StartDate,
                                    FinishDate = possibleIteration.FinishDate,
                                    Path = possibleIteration.Path,
                                    TeamLevel = possibleIteration.TeamLevel
                                });
                            }
                            //now increment
                            possibleIteration.StartDate = possibleIteration.FinishDate.AddDays(1);
                            possibleIteration.FinishDate = possibleIteration.StartDate.AddDays(numberOfDaysPerProgramIncrement - 1);
                            possibleIteration.Name = $"{CadenceType.GetDescription()} {iterationCount++}";
                            possibleIteration.Path = $"{(!string.IsNullOrEmpty(pathPrefix) ? pathPrefix : "")}\\{possibleIteration.Name}";
                            possibleIteration.TeamLevel = teamLevel;
                        }
                        break;
                    }
                case CadenceType.Sprint:
                case CadenceType.Iteration:
                    {
                        Iteration possibleIteration = new Iteration()
                        {
                            StartDate = GetNextDayOfWeek(this.CadenceStart, this.SprintDayOfWeekStart),
                        };
                        int iterationCount = IterationCounterStart;
                        int numberOfDaysPerSprint
                            = NumberOfWeeksPerSprint * 7;
                        if (numberOfDaysPerSprint <= 0)
                        {
                            throw new ArgumentException($"num of weeks per sprint must be positive");
                        }
                        possibleIteration.FinishDate = possibleIteration.StartDate.AddDays(numberOfDaysPerSprint).AddDays(-1);
                        possibleIteration.Name = $"{CadenceType.GetDescription()} {iterationCount++}";
                        possibleIteration.Path = $"{(!string.IsNullOrEmpty(pathPrefix) ? pathPrefix : "")}\\{possibleIteration.Name}";
                        possibleIteration.TeamLevel = teamLevel;
                        while (possibleIteration.StartDate < this.CadenceEnd)
                        {
                            if (possibleIteration.StartDate >= this.CadenceStart)
                            {
                                iterations.Add(new Iteration()
                                {
                                    Name = possibleIteration.Name,
                                    StartDate = possibleIteration.StartDate,
                                    FinishDate = possibleIteration.FinishDate,
                                    Path = possibleIteration.Path,
                                    TeamLevel = possibleIteration.TeamLevel
                                });
                            }
                            //now increment
                            possibleIteration.StartDate = possibleIteration.FinishDate.AddDays(1);
                            possibleIteration.FinishDate = possibleIteration.StartDate.AddDays(numberOfDaysPerSprint - 1);
                            possibleIteration.Name = $"{CadenceType.GetDescription()} {iterationCount++}";
                            possibleIteration.Path = $"{(!string.IsNullOrEmpty(pathPrefix) ? pathPrefix : "")}\\{possibleIteration.Name}";
                            possibleIteration.TeamLevel = teamLevel;
                        }
                        break;
                    }

                default:
                    throw new NotImplementedException($"haven't implemented iterations for cadence type {CadenceType.ToString()}");
            }
            return iterations;
        }

        #region Private Helpers
        private DateTime GetNextDayOfWeek(DateTime cadenceStart, DayOfWeek sprintDayOfWeekStart)
        {
            if (cadenceStart.DayOfWeek == sprintDayOfWeekStart)
                return cadenceStart;
            else if (cadenceStart.DayOfWeek < sprintDayOfWeekStart)
            {
                return cadenceStart.AddDays(sprintDayOfWeekStart - cadenceStart.DayOfWeek);
            }
            else
                return cadenceStart.AddDays(7 - (cadenceStart.DayOfWeek - sprintDayOfWeekStart));
        }

        private int GetMinMonth(CadenceType cadenceType, byte fiscalMonthStart)
        {
            switch (cadenceType)
            {
                case CadenceType.FiscalSemester:
                    {
                        int monthStart = fiscalMonthStart;
                        while (monthStart > 6)
                        {
                            monthStart = monthStart - 6;
                        }
                        return monthStart;
                    }
                case CadenceType.FiscalQuarter:
                    {
                        int monthStart = fiscalMonthStart;
                        while (monthStart > 3)
                        {
                            monthStart = monthStart - 3;
                        }
                        return monthStart;
                    }

                default:
                    throw new NotImplementedException($"haven't implemented min month for cadence type {CadenceType.ToString()}");
            }
        }
        #endregion Private Helpers
    }

    public class Iteration
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public string Path { get; set; }
        public TeamLevel TeamLevel { get; set; }

        public override string ToString()
        {
            return $"{this.Name}: from {this.StartDate.DayOfWeek.ToString()}, {this.StartDate.ToLongDateString()} to {this.FinishDate.DayOfWeek.ToString()}, {this.FinishDate.ToLongDateString()}";
        }

        public bool ContainsIteration(Iteration childIteration, ContainsType containsType) // = ContainsType.MustStartWithinButMayFinishAfter is preferred
        {
            //parent iteration should be longer!
            int parentDuration = Duration();
            int childDuration = childIteration.Duration();
            if (parentDuration <= childDuration)
                return false;

            if (childIteration.StartDate > this.FinishDate)
                return false;

            if (childIteration.FinishDate < this.StartDate)
                return false;

            if (containsType != ContainsType.AllowDupesForSplittingPurposes)
            {
                //otherwise
                switch (containsType)
                {
                    case ContainsType.StrictContains:
                        {
                            if (childIteration.StartDate < this.StartDate || childIteration.FinishDate > this.FinishDate)
                            {
                                return false;
                            }
                            break;
                        }
                    case ContainsType.MustStartWithinButMayFinishAfter:
                        {
                            if (childIteration.StartDate < this.StartDate)
                            {
                                return false;
                            }
                            break;
                        }
                    case ContainsType.MustFinishWithinButMayStartEarlier:
                        {
                            if (childIteration.FinishDate > this.FinishDate)
                            {
                                return false;
                            }
                            break;
                        }
                    default:
                        throw new NotImplementedException($"not impl contains type {containsType.ToString()}");
                }
            }

            return true;
        }

        public int Duration()
        {
            return (int)this.FinishDate.Date.Subtract(this.StartDate.Date).TotalDays;
        }

        public bool ContainsDate(DateTime dateTime)
        {
            return dateTime.Date >= this.StartDate && dateTime.Date <= this.FinishDate;
        }
    }

    public enum ContainsType
    {
        AllowDupesForSplittingPurposes, //for splitting of PIs
        StrictContains, //may miss out on some
        MustStartWithinButMayFinishAfter, //preferred and default
        MustFinishWithinButMayStartEarlier,
    }

    public enum CadenceType
    {
        [Description("Sprint")]
        Sprint = 1,

        [Description("Iteration")]
        Iteration = 2,

        [Description("Program Increment")]
        ProgramIncrement = 3,

        [Description("Fiscal Quarter")]
        FiscalQuarter = 4,

        [Description("Fiscal Semester")]
        FiscalSemester = 5,

        [Description("Fiscal Year")]
        FiscalYear = 6,
    }

    public enum TeamLevel
    {
        None = 0,
        Epic = 1,
        Feature = 2,
        Requirement = 3
    }

    public enum BusinessNodeType
    {
        None = 0,
        Portfolio = 1,
        Program = 2,
        Product = 3,
        TeamProject = 4
    }
}