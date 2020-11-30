using System;
using Newtonsoft.Json;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public class FieldBlankMapConfig : IFieldMapConfig
    {
        [JsonProperty(PropertyName = "workItemTypeName")]
        public string WorkItemTypeName { get; set; }

        [JsonProperty(PropertyName = "targetField")]
        public string TargetField { get; set; }

        [JsonIgnore]
        public Type FieldMap
        {
            get { return typeof(FieldBlankMap); }
        }
    }
}
