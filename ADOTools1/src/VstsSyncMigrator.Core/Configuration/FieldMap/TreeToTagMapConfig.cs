using System;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
   public class TreeToTagMapConfig : IFieldMapConfig
    {
        [JsonProperty(PropertyName = "workItemTypeName")]
        public string WorkItemTypeName { get; set; }

        [JsonProperty(PropertyName = "toSkip")]
        public int ToSkip { get; set; }

        [JsonProperty(PropertyName = "timeTravel")]
        public int TimeTravel { get; set; }

        [JsonIgnore]
        public Type FieldMap
        {
            get { return typeof(TreeToTagFieldMap); }
        }
    }
}
