using System;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public class FieldValuetoTagMapConfig : IFieldMapConfig
    {
        [JsonProperty(PropertyName = "workItemTypeName")]
        public string WorkItemTypeName { get; set; }

        [JsonProperty(PropertyName = "sourceField")]
        public string SourceField { get; set; }

        [JsonProperty(PropertyName = "pattern")]
        public string Pattern { get; set; }

        [JsonProperty(PropertyName = "formatExpression")]
        public string FormatExpression { get; set; }

        [JsonIgnore]
        public Type FieldMap
        {
            get { return typeof(FieldValuetoTagMap); }
        }
    }
}
