using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public class FieldMergeMapMultiConfig : IFieldMapConfig
    {
        [JsonProperty(PropertyName = "workItemTypeName")]
        public string WorkItemTypeName { get; set; }

        [JsonProperty(PropertyName = "sourceFields")]
        public List<string> SourceFields { get; set; }

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
            get { return typeof(FieldMergeMapMulti); }
        }
    }
}
