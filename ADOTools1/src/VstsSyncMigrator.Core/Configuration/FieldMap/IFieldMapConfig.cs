using System;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public interface IFieldMapConfig
    {
        [JsonProperty(PropertyName = "workItemTypeName")]
        string WorkItemTypeName { get; set; }

        [JsonIgnore]
        Type FieldMap { get; }
    }
}
