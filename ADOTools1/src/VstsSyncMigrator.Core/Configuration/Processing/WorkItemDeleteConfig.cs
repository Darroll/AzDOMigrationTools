using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemDeleteConfig : ITfsProcessingConfig
    {
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonIgnore]
        public Type Processor
        {
            get { return typeof(WorkItemDelete); }
        }

        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}