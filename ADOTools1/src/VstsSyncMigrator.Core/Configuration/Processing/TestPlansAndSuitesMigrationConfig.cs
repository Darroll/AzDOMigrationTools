using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VstsSyncMigrator.Core.BusinessEntities;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class TestPlansAndSuitesMigrationConfig : ITfsProcessingConfig
    {
        [JsonProperty(PropertyName = "areaPath")]
        public string AreaPath { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "iterationPath")]
        public string IterationPath { get; set; }

        [JsonProperty(PropertyName = "onlyElementsWithTag")]
        public string OnlyElementsWithTag { get; set; }

        [JsonProperty(PropertyName = "prefixProjectToNodes")]
        public bool PrefixProjectToNodes { get; set; }

        [JsonProperty(PropertyName = "prefixPath")]
        public string PrefixPath { get; set; }

        [JsonProperty(PropertyName = "useAreaPathMap")]
        public bool UseAreaPathMap { get; set; }

        [JsonProperty(PropertyName = "areaPathMap")]
        public Dictionary<string, string> AreaPathMap { get; set; }

        [JsonProperty(PropertyName = "fullAreaPathMap")]
        public SerializableClassificationNodeMapWithCache FullAreaPathMap { get; set; }

        [JsonProperty(PropertyName = "useIterationPathMap")]
        public bool UseIterationPathMap { get; set; }

        [JsonProperty(PropertyName = "iterationPathMap")]
        public Dictionary<string, string> IterationPathMap { get; set; }

        [JsonProperty(PropertyName = "fullIterationPathMap")]
        public SerializableClassificationNodeMapWithCache FullIterationPathMap { get; set; }

        /// <summary>
        /// Remove Invalid Links, see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
        /// </summary>
        [JsonProperty(PropertyName = "removeInvalidTestSuiteLinks")]
        public bool RemoveInvalidTestSuiteLinks { get; set; }

        [JsonIgnore]
        public Type Processor
        {
            get { return typeof(TestPlandsAndSuitesMigrationContext); }
        }

        [JsonProperty(PropertyName = "isClone")]
        public bool IsClone { get; set; } = true;

        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}