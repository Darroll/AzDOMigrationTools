using System;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
   public class RegexFieldMapConfig : IFieldMapConfig
    {
        [JsonProperty(PropertyName = "workItemTypeName")]
        public string WorkItemTypeName { get; set; }
        
        [JsonProperty(PropertyName = "sourceField")]
        public string SourceField { get; set; }

        [JsonProperty(PropertyName = "targetField")]
        public string TargetField { get; set; }

        [JsonProperty(PropertyName = "pattern")]
        public string Pattern { get; set; }

        [JsonProperty(PropertyName = "replacement")]
        public string Replacement { get; set; }

        [JsonIgnore]
        public Type FieldMap
        {
            get { return typeof(RegexFieldMap); }
        }
    }
}
