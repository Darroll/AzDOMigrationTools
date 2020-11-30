using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemUpdateConfig : ITfsProcessingConfig
    {
        [JsonProperty(PropertyName = "whatIf")]
        public bool WhatIf { get; set; }

        [JsonProperty(PropertyName = "queryBit")]
        public string QueryBit { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonIgnore]
        public Type Processor
        {
            get { return typeof(WorkItemUpdate); }
        }

        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}