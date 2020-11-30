using Newtonsoft.Json;
using System.Collections.Generic;

namespace ADO.Engine.BusinessEntities
{
    public class TeamAreasMinimal
    {
        [JsonProperty(PropertyName = "field")]
        public Field Field { get; set; }

        [JsonProperty(PropertyName = "defaultValue")]
        public string DefaultValue { get; set; }

        [JsonProperty(PropertyName = "values")]
        public List<AreaValue> Values { get; set; }
    }

    public class Field
    {
        [JsonProperty(PropertyName = "referenceName")]
        public string ReferenceName { get; set; }
    }

    public class AreaValue
    {
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "includeChildren")]
        public bool IncludeChildren { get; set; }
    }

}
