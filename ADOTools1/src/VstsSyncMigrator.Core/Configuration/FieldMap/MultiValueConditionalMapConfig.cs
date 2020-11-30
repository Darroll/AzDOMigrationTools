using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public class MultiValueConditionalMapConfig : IFieldMapConfig
    {
        [JsonProperty(PropertyName = "workItemTypeName")]
        public string WorkItemTypeName { get; set; }

        [JsonProperty(PropertyName = "sourceFieldsAndValues")]
        public Dictionary<string,string> SourceFieldsAndValues { get; set; }

        [JsonProperty(PropertyName = "targetFieldsAndValues")]
        public Dictionary<string, string> TargetFieldsAndValues { get; set; }

        [JsonIgnore]
        public Type FieldMap
        {
            get { return typeof(MultiValueConditionalMap); }
        }
    }
}
