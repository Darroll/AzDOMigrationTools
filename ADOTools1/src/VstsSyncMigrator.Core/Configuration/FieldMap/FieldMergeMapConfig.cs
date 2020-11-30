using System;
using Newtonsoft.Json;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public class FieldMergeMapConfig : IFieldMapConfig
    {
        [JsonProperty(PropertyName = "workItemTypeName")]
        public string WorkItemTypeName { get; set; }

        [JsonProperty(PropertyName = "sourceField1")]
        public string SourceField1 { get; set; }

        [JsonProperty(PropertyName = "sourceField2")]
        public string SourceField2 { get; set; }

        [JsonProperty(PropertyName = "targetField")]
        public string TargetField { get; set; }

        [JsonProperty(PropertyName = "formatExpression")]
        public string FormatExpression { get; set; }

        [JsonProperty(PropertyName = "force")]
        public bool Force { get; set; }

        [JsonProperty(PropertyName = "doneMatch")]
        public string DoneMatch { get; set; } = "##DONE##";

        [JsonIgnore]
        public Type FieldMap
        {
            get { return typeof(FieldMergeMap); }
        }
    }
}
