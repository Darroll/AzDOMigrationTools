using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using VstsSyncMigrator.Core.BusinessEntities;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemRevisionReplayMigrationConfig : ITfsProcessingConfig
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.Configuration.WorkItemRevisionReplayMigrationConfig"));

        #endregion

        #region - Public Members

        #region - Properties

        [JsonProperty(PropertyName = "areaPath")]
        public string AreaPath { get; set; }

        [JsonProperty(PropertyName = "degreeOfParallelism")]
        public int DegreeOfParallelism { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "force")]
        public bool Force { get; set; }

        [JsonProperty(PropertyName = "iterationPath")]
        public string IterationPath { get; set; }

        [JsonProperty(PropertyName = "keepAreaPath")]
        public bool KeepAreaPath { get; set; }

        [JsonProperty(PropertyName = "keepIterationPath")]
        public bool KeepIterationPath { get; set; }

        [JsonProperty(PropertyName = "keepState")]
        public bool KeepState { get; set; }

        [JsonProperty(PropertyName = "prefixPath")]
        public string PrefixPath { get; set; }

        [JsonProperty(PropertyName = "prefixProjectToNodes")]
        public bool PrefixProjectToNodes { get; set; }

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

        [JsonIgnore]
        public Type Processor
        {
            get { return typeof(WorkItemRevisionReplayMigrationContext); }
        }

        [JsonProperty(PropertyName = "queryBit")]
        public string QueryBit { get; set; }

        [JsonProperty(PropertyName = "updateSoureReflectedId")]
        public bool UpdateSourceReflectedId { get; set; }

        [JsonProperty(PropertyName = "isClone")]
        public bool IsClone { get; set; } = true;



        #endregion

        #region - Methods

        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            bool isCompatible = !otherProcessors.Any(x => x is WorkItemMigrationConfig);

            if (!isCompatible)
            {
                // Send some traces.
                _mySource.Value.TraceInformation($"Note: {GetType().Name} is not compatible with {typeof(WorkItemMigrationConfig).Name}");
                _mySource.Value.Flush();
            }

            // Return if it is compatible.
            return isCompatible;
        }

        #endregion

        #endregion
    }
}