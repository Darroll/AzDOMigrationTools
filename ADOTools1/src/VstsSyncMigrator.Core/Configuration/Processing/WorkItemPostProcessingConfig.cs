using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemPostProcessingConfig : ITfsProcessingConfig
    {
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonIgnore]
        public Type Processor
        {
            get { return typeof(WorkItemPostProcessingContext); }
        }

        [JsonProperty(PropertyName = "queryBit")]
        public string QueryBit { get; set; }

        [JsonProperty(PropertyName = "workItemIDs")]
        public IList<int> WorkItemIDs { get; set; }

        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}