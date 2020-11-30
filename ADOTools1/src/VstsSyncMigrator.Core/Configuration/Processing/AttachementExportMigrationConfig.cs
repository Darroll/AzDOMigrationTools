using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class AttachementExportMigrationConfig : ITfsProcessingConfig
    {
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonIgnore]
        public Type Processor
        {
            get { return typeof(AttachementExportMigrationContext); }
        }

        [JsonProperty(PropertyName = "queryBit")]
        public string QueryBit { get; set; }


        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}