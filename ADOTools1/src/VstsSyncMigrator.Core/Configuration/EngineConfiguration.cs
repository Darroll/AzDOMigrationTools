using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VstsSyncMigrator.Core;
using VstsSyncMigrator.Engine.Configuration.FieldMap;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine.Configuration
{
    public class EngineConfiguration
    {
        #region - Static Members

        public static EngineConfiguration GetDefault()
        {
            EngineConfiguration config = new EngineConfiguration
            {
                TelemetryEnableTrace = true,
                LoadSecretsFromEnvironmentVariables = false,
                Source = new TeamProjectConfig() { Name = "DemoProjs", Collection = new Uri("https://sdd2016.visualstudio.com/") },
                Target = new TeamProjectConfig() { Name = "DemoProjt", Collection = new Uri("https://sdd2016.visualstudio.com/") },
                ReflectedWorkItemIDFieldName = "TfsMigrationTool.ReflectedWorkItemId",
                SourceReflectedWorkItemIDFieldName = "ProcessName.ReflectedWorkItemId",
                FieldMaps = new List<IFieldMapConfig>
                {
                    new MultiValueConditionalMapConfig()
                    {
                        WorkItemTypeName = "*",
                        SourceFieldsAndValues = new Dictionary<string, string>
                            {
                                { "Field1", "Value1" },
                                { "Field2", "Value2" }
                            },
                        TargetFieldsAndValues = new Dictionary<string, string>
                            {
                                { "Field1", "Value1" },
                                { "Field2", "Value2" }
                            }
                    },
                    new FieldBlankMapConfig()
                    {
                        WorkItemTypeName = "*",
                        TargetField = "TfsMigrationTool.ReflectedWorkItemId"
                    },
                    new FieldValueMapConfig()
                    {
                        WorkItemTypeName = "*",
                        SourceField = "System.State",
                        TargetField = "System.State",
                        DefaultValue = "New",
                        ValueMapping = new Dictionary<string, string> {
                        { "Approved", "New" },
                        { "New", "New" },
                        { "Committed", "Active" },
                        { "In Progress", "Active" },
                        { "To Do", "New" },
                        { "Done", "Closed" }
                    }
                    },
                    new FieldtoFieldMapConfig()
                    {
                        WorkItemTypeName = "*",
                        SourceField = "Microsoft.VSTS.Common.BacklogPriority",
                        TargetField = "Microsoft.VSTS.Common.StackRank"
                    },
                    new FieldtoTagMapConfig()
                    {
                        WorkItemTypeName = "*",
                        SourceField = "System.State",
                        FormatExpression = "ScrumState:{0}"
                    },
                    new FieldMergeMapConfig()
                    {
                        WorkItemTypeName = "*",
                        SourceField1 = "System.Description",
                        SourceField2 = "Microsoft.VSTS.Common.AcceptanceCriteria",
                        TargetField = "System.Description",
                        FormatExpression = @"{0} <br/><br/><h3>Acceptance Criteria</h3>{1}"
                    },
                    new RegexFieldMapConfig()
                    {
                        WorkItemTypeName = "*",
                        SourceField = "COMPANY.PRODUCT.Release",
                        TargetField = "COMPANY.DEVISION.MinorReleaseVersion",
                        Pattern = @"PRODUCT \d{4}.(\d{1})",
                        Replacement = "$1"
                    },
                    new FieldValuetoTagMapConfig()
                    {
                        WorkItemTypeName = "*",
                        SourceField = "Microsoft.VSTS.CMMI.Blocked",
                        Pattern = @"Yes",
                        FormatExpression = "{0}"
                    },
                    new TreeToTagMapConfig()
                    {
                        WorkItemTypeName = "*",
                        TimeTravel = 1,
                        ToSkip = 3
                    }
                },
                WorkItemTypeDefinition = new Dictionary<string, string> {
                    { "Bug", "Bug" },
                    { "Product Backlog Item", "Product Backlog Item" }
                },

                // Create an empty list of processors.
                Processors = new List<ITfsProcessingConfig>()
            };

            // Define processors.
            config.Processors.Add(new WorkItemMigrationConfig() { Enabled = false, PrefixProjectToNodes = true, UpdateSoureReflectedId = true, QueryBit = @"AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')" });
            config.Processors.Add(new WorkItemRevisionReplayMigrationConfig() { DegreeOfParallelism = 8, Enabled = false, PrefixProjectToNodes = true, UpdateSourceReflectedId = true, QueryBit = @"AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')" });
            config.Processors.Add(new WorkItemUpdateConfig() { Enabled = false, QueryBit = @"AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')" });
            config.Processors.Add(new LinkMigrationConfig() { DegreeOfParallelism = 8, Enabled = false, QueryBit = @"AND ([System.ExternalLinkCount] > 0 OR [System.RelatedLinkCount] > 0)" });
            config.Processors.Add(new WorkItemPostProcessingConfig() { Enabled = false, QueryBit = "AND [TfsMigrationTool.ReflectedWorkItemId] = '' ", WorkItemIDs = new List<int> { 1, 2, 3 } });
            config.Processors.Add(new WorkItemDeleteConfig() { Enabled = false });
            config.Processors.Add(new AttachementExportMigrationConfig() { Enabled = false, QueryBit = @"AND [System.AttachedFileCount] > 0" });
            config.Processors.Add(new AttachementImportMigrationConfig() { Enabled = false });
            config.Processors.Add(new WorkItemQueryMigrationConfig() { Enabled = false, PrefixProjectToNodes = false });
            config.Processors.Add(new TestVariablesMigrationConfig() { Enabled = false });
            config.Processors.Add(new TestConfigurationsMigrationConfig() { Enabled = false });
            config.Processors.Add(new TestPlansAndSuitesMigrationConfig() { Enabled = false, PrefixProjectToNodes = true });
            config.Processors.Add(new FixGitCommitLinksConfig() { Enabled = false, TargetRepository = config.Target.Name });

            // Return configuration.
            return config;
        }

        public void Validate()
        {
            // Nothing to do for now.

            // load secrets from environment variables
            if (LoadSecretsFromEnvironmentVariables)
            {
                Source.Token = Utility.LoadFromEnvironmentVariables(Source.Token);
                Target.Token = Utility.LoadFromEnvironmentVariables(Target.Token);
            }
        }

        #endregion

        #region - Public Members

        [JsonProperty(PropertyName = "telemetryEnableTrace")]
        public bool TelemetryEnableTrace { get; set; }

        [JsonProperty(PropertyName = "workaroundForQuerySOAPBugEnabled")]
        public bool WorkaroundForQuerySOAPBugEnabled { get; set; }

        [JsonProperty(PropertyName = "source")]
        public TeamProjectConfig Source { get; set; }

        [JsonProperty(PropertyName = "target")]
        public TeamProjectConfig Target { get; set; }

        [JsonProperty(PropertyName = "reflectedWorkItemIDFieldName")]
        public string ReflectedWorkItemIDFieldName { get; set; }

        [JsonProperty(PropertyName = "sourceReflectedWorkItemIDFieldName")]
        public string SourceReflectedWorkItemIDFieldName { get; set; }

        [JsonProperty(PropertyName = "fieldMaps")]
        public List<IFieldMapConfig> FieldMaps { get; set; }

        [JsonProperty(PropertyName = "workItemTypeDefinition")]
        public Dictionary<string, string> WorkItemTypeDefinition { get; set; }

        [JsonProperty(PropertyName = "processors")]
        public List<ITfsProcessingConfig> Processors { get; set; }
        
        [JsonProperty(PropertyName = "loadSecretsFromEnvironmentVariables")]
        public bool LoadSecretsFromEnvironmentVariables { get; set; }

        #endregion
    }
}