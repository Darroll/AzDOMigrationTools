using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class LinkMigrationConfig : ITfsProcessingConfig
    {
        [JsonProperty(PropertyName = "degreeOfParallelism")]
        public int DegreeOfParallelism { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonIgnore]
        public Type Processor
        {
            get { return typeof(LinkMigrationContext); }
        }

        [JsonProperty(PropertyName = "queryBit")]
        public string QueryBit { get; set; }

        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}