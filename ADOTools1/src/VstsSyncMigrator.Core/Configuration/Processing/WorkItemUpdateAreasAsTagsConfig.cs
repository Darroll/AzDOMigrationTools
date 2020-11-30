using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemUpdateAreasAsTagsConfig : ITfsProcessingConfig
    {
        [JsonProperty(PropertyName = "areaIterationPath")]
        public string AreaIterationPath { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonIgnore]
        public Type Processor
        {
            get { return typeof(WorkItemUpdateAreasAsTagsContext); }
        }

        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}