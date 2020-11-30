using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public class FieldValueMapConfig : IFieldMapConfig
    {
        [JsonProperty(PropertyName = "workItemTypeName")]
        public string WorkItemTypeName { get; set; }

        [JsonProperty(PropertyName = "sourceField")]
        public string SourceField { get; set; }

        [JsonProperty(PropertyName = "targetField")]
        public string TargetField { get; set; }

        [JsonProperty(PropertyName = "defaultValue")]
        public string DefaultValue { get; set; }

        [JsonProperty(PropertyName = "valueMapping")]
        public Dictionary<string, string> ValueMapping { get; set; }

        [JsonIgnore]
        public Type FieldMap
        {
            get { return typeof(FieldValueMap); }
        }

    }
}
