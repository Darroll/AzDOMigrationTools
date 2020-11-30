using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ADO.RestAPI.ProcessMapping;

namespace ADO.RestAPI.Viewmodel50
{
    public class WorkResponse
    {
        // This is just a container class for all REST API responses related to teams.
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/core/teams?view=azure-devops-rest-5.0

        #region - Nested Classes and Enumerations.

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BoardColumnType
        {
            [EnumMember(Value = "inProgress")]
            InProgress,
            [EnumMember(Value = "incoming")]
            Incoming,
            [EnumMember(Value = "outgoing")]
            Outgoing
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BugsBehavior
        {
            [EnumMember(Value = "asRequirements")]
            AsRequirements,
            [EnumMember(Value = "asTasks")]
            AsTasks,
            [EnumMember(Value = "off")]
            Off
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum TimeFrame
        {
            [EnumMember(Value = "current")]
            Current,
            [EnumMember(Value = "future")]
            Future,
            [EnumMember(Value = "past")]
            Past
        }

        public class BacklogVisibilities
        {
            [JsonProperty(PropertyName = "Microsoft.EpicCategory")]
            public bool EpicCategory { get; set; }
            [JsonProperty(PropertyName = "Microsoft.FeatureCategory")]
            public bool FeatureCategory { get; set; }
            [JsonProperty(PropertyName = "Microsoft.RequirementCategory")]
            public bool RequirementCategory { get; set; }
        }

        #region Universal Board
        public class BoardColumns
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<BoardColumn> Value { get; set; }

            // todo: this field is added artificially, a solution would consist of using another viewmodel to store this.l
            [JsonProperty(PropertyName = "boardName")]
            public string BoardName { get; set; }
        }

        public class BoardColumn
        {
            [JsonProperty(PropertyName = "columnType")]
            public BoardColumnType ColumnType { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "isSplit")]
            public bool IsSplit { get; set; }

            [JsonProperty(PropertyName = "itemLimit")]
            public int ItemLimit { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "stateMappings")]
            public StateMappings StateMappings { get; set; }
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class StateMappings
        {
            [JsonProperty(PropertyName = "User Story")]
            public string UserStory { get; set; }

            [JsonProperty(PropertyName = "Product Backlog Item")]
            public string PBI { get; set; }

            [JsonProperty(PropertyName = "Requirement")]
            public string Requirement { get; set; }

            [JsonProperty(PropertyName = "Bug")]
            public string Bug { get; set; }

            [JsonProperty(PropertyName = "Feature")]
            public string Feature { get; set; }

            [JsonProperty(PropertyName = "Epic")]
            public string Epic { get; set; }

            internal string GetValue(string sourceBoardName)
            {
                switch (sourceBoardName)
                {
                    case "Epics":
                        {
                            return this.Epic;
                        }
                    case "Features":
                        {
                            return this.Feature;
                        }
                    case "Stories":
                        {
                            return this.UserStory;
                        }
                    case "Backlog Items":
                        {
                            return this.PBI;
                        }
                    case "Requirements":
                        {
                            return this.Requirement;
                        }
                    case "Bugs": //not really a board name
                        {
                            return this.Bug;
                        }
                    default:
                        {
                            throw new NotImplementedException($"unhandled board {sourceBoardName}");
                        }
                }
            }
            internal void SetValue(string sourceBoardName, string newValue)
            {
                switch (sourceBoardName)
                {
                    case "Epics":
                        {
                            this.Epic = newValue;
                            break;
                        }
                    case "Features":
                        {
                            this.Feature = newValue;
                            break;
                        }
                    case "Stories":
                        {
                            this.UserStory = newValue;
                            break;
                        }
                    case "Backlog Items":
                        {
                            this.PBI = newValue;
                            break;
                        }
                    case "Requirements":
                        {
                            this.Requirement = newValue;
                            break;
                        }
                    case "Bugs": //not really a board name
                        {
                            this.Bug = newValue;
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException($"unhandled board {sourceBoardName}");
                        }
                }
            }
        }

        #endregion Universal Board

        #region Universal Card Fields

        public class UniversalCards
        {
            [JsonProperty(PropertyName = "cards")]
            public UniversalCard Cards { get; set; }

            [JsonProperty(PropertyName = "boardName")]
            public string BoardName { get; set; }

            public List<CardField> GetCardList(string sourceBoardName)
            {
                switch (sourceBoardName)
                {
                    case "Epics":
                        {
                            return this.Cards.Epic;
                        }
                    case "Features":
                        {
                            return this.Cards.Feature;
                        }
                    case "Stories":
                        {
                            return this.Cards.UserStory;
                        }
                    case "Requirements":
                        {
                            return this.Cards.Requirement;
                        }
                    case "Backlog Items":
                        {
                            return this.Cards.PBI;
                        }

                    default:
                        throw new NotImplementedException($"can't fetch cards for {sourceBoardName}");
                }
            }

            public UniversalCards AddCardList(string targetBoardName, List<CardField> targetCardList)
            {
                switch (targetBoardName)
                {
                    case "Epics":
                        {
                            this.Cards.Epic = targetCardList.Select(tc => tc).ToList();
                            return (this);
                        }
                    case "Features":
                        {
                            this.Cards.Feature = targetCardList.Select(tc => tc).ToList();
                            return this;
                        }
                    case "Stories":
                        {
                            this.Cards.UserStory = targetCardList.Select(tc => tc).ToList();
                            return this;
                        }
                    case "Requirements":
                        {
                            this.Cards.Requirement = targetCardList.Select(tc => tc).ToList();
                            return this;
                        }
                    case "Backlog Items":
                        {
                            this.Cards.PBI = targetCardList.Select(tc => tc).ToList();
                            return this;
                        }

                    default:
                        throw new NotImplementedException($"can't fetch cards for {targetBoardName}");
                }
            }
        }
        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class UniversalCard
        {
            [JsonProperty(PropertyName = "Epic")]
            public List<CardField> Epic { get; set; }

            [JsonProperty(PropertyName = "Feature")]
            public List<CardField> Feature { get; set; }

            [JsonProperty(PropertyName = "User Story")]
            public List<CardField> UserStory { get; set; }

            [JsonProperty(PropertyName = "Product Backlog Item")]
            public List<CardField> PBI { get; set; }

            [JsonProperty(PropertyName = "Requirement")]
            public List<CardField> Requirement { get; set; }

            [JsonProperty(PropertyName = "Bug")]
            public List<CardField> Bug { get; set; }
        }
        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class CardField
        {
            [JsonProperty(PropertyName = "fieldIdentifier")]
            public string FieldIdentifier { get; set; }

            [JsonProperty(PropertyName = "displayFormat")]
            public string DisplayFormat { get; set; }
        }

        #endregion Universal Card Fields

        public class BoardRows
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            // todo: this field is added artificially, a solution would consist of using another viewmodel to store this.l
            [JsonProperty(PropertyName = "boardName")]
            public string BoardName { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<BoardRow> Value { get; set; }
        }

        public class BoardRow
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }



        public class BoardSuggestedValues
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<BoardSuggestedValue> Value { get; set; }
        }

        public class BoardSuggestedValue
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        #region Boards

        #region Agile
        public class AgileBoardColumns
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<AgileBoardColumn> Value { get; set; }

            // todo: this field is added artificially, a solution would consist of using another viewmodel to store this.l
            [JsonProperty(PropertyName = "boardName")]
            public string BoardName { get; set; }

            //public CmmiBoardColumns ToCmmiBoardColumns(Maps maps)
            //{
            //    CmmiBoardColumns newCmmiBoardColumns = new CmmiBoardColumns()
            //    {
            //        BoardName = ProcessMappingUtility2.MapBoardName(this.BoardName, Constants.AgileTemplateType, Constants.CmmiTemplateType, maps),
            //        Count = this.Count,
            //        Value = this.Value.Select(ab => ab.ToCmmiBoardColumn()).ToList()
            //    };
            //    return newCmmiBoardColumns;
            //}
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class AgileBoardColumn
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "itemLimit")]
            public int ItemLimit { get; set; }

            [JsonProperty(PropertyName = "stateMappings")]
            public AgileStateMappings StateMappings { get; set; }

            [JsonProperty(PropertyName = "isSplit")]
            public bool? IsSplit { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "columnType")]
            public BoardColumnType ColumnType { get; set; }

            //public CmmiBoardColumn ToCmmiBoardColumn()
            //{
            //    CmmiBoardColumn newCmmiBoardColumn = new CmmiBoardColumn()
            //    {
            //        Id = this.Id,
            //        Name = ProcessMappingUtility2.MapState(this.Name, Constants.AgileTemplateType, Constants.CmmiTemplateType),
            //        ColumnType = this.ColumnType,
            //        Description = this.Description,
            //        IsSplit = this.IsSplit,
            //        ItemLimit = this.ItemLimit,
            //        StateMappings = this.StateMappings.ToCmmiStateMappings()
            //    };
            //    return newCmmiBoardColumn;
            //}
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class AgileStateMappings
        {
            [JsonProperty(PropertyName = "User Story")]
            public string UserStory { get; set; }

            [JsonProperty(PropertyName = "Bug")]
            public string Bug { get; set; }

            [JsonProperty(PropertyName = "Feature")]
            public string Feature { get; set; }

            [JsonProperty(PropertyName = "Epic")]
            public string Epic { get; set; }

            //public CmmiStateMappings ToCmmiStateMappings()
            //{
            //    CmmiStateMappings newCmmiStateMappings = new CmmiStateMappings()
            //    {
            //        Bug = ProcessMappingUtility2.MapState(this.Bug, Constants.AgileTemplateType, Constants.CmmiTemplateType),
            //        Epic = ProcessMappingUtility2.MapState(this.Epic, Constants.AgileTemplateType, Constants.CmmiTemplateType),
            //        Feature = ProcessMappingUtility2.MapState(this.Feature, Constants.AgileTemplateType, Constants.CmmiTemplateType),
            //        Requirement = ProcessMappingUtility2.MapState(this.UserStory, Constants.AgileTemplateType, Constants.CmmiTemplateType)
            //    };
            //    return newCmmiStateMappings;
            //}
        }
        #endregion Agile

        #region Scrum
        public class ScrumBoardColumns
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<ScrumBoardColumn> Value { get; set; }

            // todo: this field is added artificially, a solution would consist of using another viewmodel to store this.l
            [JsonProperty(PropertyName = "boardName")]
            public string BoardName { get; set; }

            //public CmmiBoardColumns ToCmmiBoardColumns(Maps maps)
            //{
            //    CmmiBoardColumns newCmmiBoardColumns = new CmmiBoardColumns()
            //    {
            //        BoardName = ProcessMappingUtility2.MapBoardName(this.BoardName, Constants.ScrumTemplateType, Constants.CmmiTemplateType, maps),
            //        Count = this.Count,
            //        Value = this.Value.Select(ab => ab.ToCmmiBoardColumn()).ToList()
            //    };
            //    return newCmmiBoardColumns;
            //}
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class ScrumBoardColumn
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "itemLimit")]
            public int ItemLimit { get; set; }

            [JsonProperty(PropertyName = "stateMappings")]
            public ScrumStateMappings StateMappings { get; set; }

            [JsonProperty(PropertyName = "isSplit")]
            public bool? IsSplit { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "columnType")]
            public BoardColumnType ColumnType { get; set; }

            //public CmmiBoardColumn ToCmmiBoardColumn()
            //{
            //    CmmiBoardColumn newCmmiBoardColumn = new CmmiBoardColumn()
            //    {
            //        Id = this.Id,
            //        Name = ProcessMappingUtility2.MapState(this.Name, Constants.ScrumTemplateType, Constants.CmmiTemplateType),
            //        ColumnType = this.ColumnType,
            //        Description = this.Description,
            //        IsSplit = this.IsSplit,
            //        ItemLimit = this.ItemLimit,
            //        StateMappings = this.StateMappings.ToCmmiStateMappings()
            //    };
            //    return newCmmiBoardColumn;
            //}
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class ScrumStateMappings
        {
            [JsonProperty(PropertyName = "Product Backlog Item")]
            public string PBI { get; set; }

            [JsonProperty(PropertyName = "Bug")]
            public string Bug { get; set; }

            [JsonProperty(PropertyName = "Feature")]
            public string Feature { get; set; }

            [JsonProperty(PropertyName = "Epic")]
            public string Epic { get; set; }

            //public CmmiStateMappings ToCmmiStateMappings()
            //{
            //    CmmiStateMappings newCmmiStateMappings = new CmmiStateMappings()
            //    {
            //        Bug = ProcessMappingUtility2.MapState(this.Bug, Constants.ScrumTemplateType, Constants.CmmiTemplateType),
            //        Epic = ProcessMappingUtility2.MapState(this.Epic, Constants.ScrumTemplateType, Constants.CmmiTemplateType),
            //        Feature = ProcessMappingUtility2.MapState(this.Feature, Constants.ScrumTemplateType, Constants.CmmiTemplateType),
            //        Requirement = ProcessMappingUtility2.MapState(this.PBI, Constants.ScrumTemplateType, Constants.CmmiTemplateType)
            //    };
            //    return newCmmiStateMappings;
            //}
        }
        #endregion Scrum

        #region CMMI
        public class CmmiBoardColumns
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<CmmiBoardColumn> Value { get; set; }

            // todo: this field is added artificially, a solution would consist of using another viewmodel to store this.l
            [JsonProperty(PropertyName = "boardName")]
            public string BoardName { get; set; }

            public CmmiBoardColumns InsertMissingCmmiBoardColumns()
            {
                if (this.Value.Count < 4)
                {
                    var firstTwoColumns = this.Value.Take(2).ToList();
                    var lastColumn = new List<CmmiBoardColumn>() { this.Value.Last() };
                    CmmiBoardColumn missingResolvedColumn = new CmmiBoardColumn()
                    {
                        Name = "Resolved",
                        ItemLimit = 5,
                        IsSplit = false,
                        StateMappings = new CmmiStateMappings()
                        {
                            Epic = ProcessMappingUtility2.InsertMissingCmmiBoardColumnsFromScrum(this.BoardName, "Resolved", "Epics"),
                            Feature = ProcessMappingUtility2.InsertMissingCmmiBoardColumnsFromScrum(this.BoardName, "Resolved", "Features"),
                            Bug = ProcessMappingUtility2.InsertMissingCmmiBoardColumnsFromScrum(this.BoardName, "Resolved", "Bugs"),
                            Requirement = ProcessMappingUtility2.InsertMissingCmmiBoardColumnsFromScrum(this.BoardName, "Resolved", "Requirements")
                        },
                        Description = "",
                        ColumnType = BoardColumnType.InProgress
                    };
                    var newValue = firstTwoColumns.Concat(new List<CmmiBoardColumn> { missingResolvedColumn }).Concat(lastColumn).ToList();
                    return new CmmiBoardColumns()
                    {
                        BoardName = this.BoardName,
                        Count = newValue.Count,
                        Value = newValue
                    };
                }
                else
                {
                    //nothing to do
                    return this;
                }
            }
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class CmmiBoardColumn
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "itemLimit")]
            public int ItemLimit { get; set; }

            [JsonProperty(PropertyName = "stateMappings")]
            public CmmiStateMappings StateMappings { get; set; }

            [JsonProperty(PropertyName = "isSplit")]
            public bool? IsSplit { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "columnType")]
            public BoardColumnType ColumnType { get; set; }
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class CmmiStateMappings
        {
            [JsonProperty(PropertyName = "Requirement")]
            public string Requirement { get; set; }

            [JsonProperty(PropertyName = "Bug")]
            public string Bug { get; set; }

            [JsonProperty(PropertyName = "Feature")]
            public string Feature { get; set; }

            [JsonProperty(PropertyName = "Epic")]
            public string Epic { get; set; }
        }
        #endregion CMMI        

        #endregion Boards

        #region Team Fields and Settings

        public class TeamFieldValues
        {
            [JsonProperty(PropertyName = "_links")]
            public TeamFieldValuesReferenceLink Links { get; set; }

            [JsonProperty(PropertyName = "defaultValue")]
            public string DefaultValue { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "values")]
            public IList<TeamFieldValue> Values { get; set; }
        }

        public class TeamFieldValue
        {
            [JsonProperty(PropertyName = "includeChildren")]
            public bool IncludeChildren { get; set; }

            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }
        }

        public class TeamSettings
        {
            [JsonProperty(PropertyName = "_links")]
            public TeamSettingReferenceLink Links { get; set; }

            [JsonProperty(PropertyName = "backlogIteration")]
            public TeamSettingsIteration BacklogIteration { get; set; }

            [JsonProperty(PropertyName = "backlogVisibilities")]
            public BacklogVisibilities BacklogVisibilities { get; set; }

            [JsonProperty(PropertyName = "bugsBehavior")]
            public BugsBehavior BugsBehavior { get; set; }

            [JsonProperty(PropertyName = "defaultIteration")]
            public TeamSettingsIteration DefaultIteration { get; set; }

            [JsonProperty(PropertyName = "defaultIterationMacro")]
            public string DefaultIterationMacro { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            //[JsonProperty(PropertyName = "workingDays")]
            //public string WorkingDays { get; set; }
        }

        public class TeamSettingsIterations
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<TeamSettingsIteration> Value { get; set; }
        }

        public class TeamSettingsIteration
        {
            [JsonProperty(PropertyName = "_links")]
            public TeamSettingsIterationReferenceLink Links { get; set; }

            [JsonProperty(PropertyName = "attributes")]
            public TeamIterationAttributes Attributes { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "path")]
            public string Path { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }

        public class TeamIterationAttributes
        {
            [JsonProperty(PropertyName = "finishDate")]
            public string FinishDate { get; set; }

            [JsonProperty(PropertyName = "startDate")]
            public string StartDate { get; set; }

            [JsonProperty(PropertyName = "timeFrame")]
            public TimeFrame TimeFrame { get; set; }
        }

        #endregion Team Fields and Settings

        #region Card Fields

        #region Agile
        public class AgileCards
        {
            [JsonProperty(PropertyName = "cards")]
            public AgileCard Cards { get; set; }

            [JsonProperty(PropertyName = "boardName")]
            public string BoardName { get; set; }

            //public CmmiCards ToCmmiCards(Maps maps)
            //{
            //    CmmiCards newCmmiCards = new CmmiCards()
            //    {
            //        BoardName = ProcessMappingUtility2.MapBoardName(this.BoardName, Constants.AgileTemplateType, Constants.CmmiTemplateType, maps),
            //        Cards = this.Cards.ToCmmiCard()
            //    };
            //    return newCmmiCards;
            //}
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class AgileCard
        {
            [JsonProperty(PropertyName = "Epic")]
            public List<AgileCardField> Epic { get; set; }

            [JsonProperty(PropertyName = "Feature")]
            public List<AgileCardField> Feature { get; set; }

            [JsonProperty(PropertyName = "User Story")]
            public List<AgileCardField> UserStory { get; set; }

            [JsonProperty(PropertyName = "Bug")]
            public List<AgileCardField> Bug { get; set; }

            //public CmmiCard ToCmmiCard()
            //{
            //    CmmiCard newCmmiCard = new CmmiCard()
            //    {
            //        Bug = this.Bug?.Select(b => b.ToCmmiCardField()).ToList(),
            //        Epic = this.Epic?.Select(b => b.ToCmmiCardField()).ToList(),
            //        Feature = this.Feature?.Select(b => b.ToCmmiCardField()).ToList(),
            //        Requirement = this.UserStory?.Select(b => b.ToCmmiCardField()).ToList()
            //    };
            //    return newCmmiCard;
            //}
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class AgileCardField
        {
            [JsonProperty(PropertyName = "fieldIdentifier")]
            public string FieldIdentifier { get; set; }

            [JsonProperty(PropertyName = "displayFormat")]
            public string DisplayFormat { get; set; }

            //public CmmiCardField ToCmmiCardField()
            //{
            //    CmmiCardField newCmmiCardField = new CmmiCardField()
            //    {
            //        FieldIdentifier = ProcessMappingUtility2.MapField(this.FieldIdentifier, Constants.AgileTemplateType, Constants.CmmiTemplateType),
            //        DisplayFormat = this.DisplayFormat
            //    };
            //    return newCmmiCardField;
            //}
        }
        #endregion Agile

        #region Scrum
        public class ScrumCards
        {
            [JsonProperty(PropertyName = "cards")]
            public ScrumCard Cards { get; set; }

            [JsonProperty(PropertyName = "boardName")]
            public string BoardName { get; set; }

            //public CmmiCards ToCmmiCards(Maps maps)
            //{
            //    CmmiCards newCmmiCards = new CmmiCards()
            //    {
            //        BoardName = ProcessMappingUtility2.MapBoardName(this.BoardName, Constants.ScrumTemplateType, Constants.CmmiTemplateType, maps),
            //        Cards = this.Cards.ToCmmiCard()
            //    };
            //    return newCmmiCards;
            //}
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class ScrumCard
        {
            [JsonProperty(PropertyName = "Epic")]
            public List<ScrumCardField> Epic { get; set; }

            [JsonProperty(PropertyName = "Feature")]
            public List<ScrumCardField> Feature { get; set; }

            [JsonProperty(PropertyName = "Product Backlog Item")]
            public List<ScrumCardField> PBI { get; set; }

            [JsonProperty(PropertyName = "Bug")]
            public List<ScrumCardField> Bug { get; set; }

            //public CmmiCard ToCmmiCard()
            //{
            //    CmmiCard newCmmiCard = new CmmiCard()
            //    {
            //        Bug = this.Bug?.Select(b => b.ToCmmiCardField()).ToList(),
            //        Epic = this.Epic?.Select(b => b.ToCmmiCardField()).ToList(),
            //        Feature = this.Feature?.Select(b => b.ToCmmiCardField()).ToList(),
            //        Requirement = this.PBI?.Select(b => b.ToCmmiCardField()).ToList()
            //    };
            //    return newCmmiCard;
            //}
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class ScrumCardField
        {
            [JsonProperty(PropertyName = "fieldIdentifier")]
            public string FieldIdentifier { get; set; }

            [JsonProperty(PropertyName = "displayFormat")]
            public string DisplayFormat { get; set; }
            //public CmmiCardField ToCmmiCardField()
            //{
            //    CmmiCardField newCmmiCardField = new CmmiCardField()
            //    {
            //        FieldIdentifier = ProcessMappingUtility2.MapField(this.FieldIdentifier, Constants.ScrumTemplateType, Constants.CmmiTemplateType),
            //        DisplayFormat = this.DisplayFormat
            //    };
            //    return newCmmiCardField;
            //}
        }
        #endregion Scrum

        #region Cmmi
        public class CmmiCards
        {
            [JsonProperty(PropertyName = "cards")]
            public CmmiCard Cards { get; set; }

            [JsonProperty(PropertyName = "boardName")]
            public string BoardName { get; set; }
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class CmmiCard
        {
            [JsonProperty(PropertyName = "Epic")]
            public List<CmmiCardField> Epic { get; set; }

            [JsonProperty(PropertyName = "Feature")]
            public List<CmmiCardField> Feature { get; set; }

            [JsonProperty(PropertyName = "Requirement")]
            public List<CmmiCardField> Requirement { get; set; }

            [JsonProperty(PropertyName = "Bug")]
            public List<CmmiCardField> Bug { get; set; }
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class CmmiCardField
        {
            [JsonProperty(PropertyName = "fieldIdentifier")]
            public string FieldIdentifier { get; set; }

            [JsonProperty(PropertyName = "displayFormat")]
            public string DisplayFormat { get; set; }
        }
        #endregion Cmmi

        #endregion Card Fields

        #endregion
    }

}