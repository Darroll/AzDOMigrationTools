using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ADO.Engine.BusinessEntities
{
    public class TeamIterationsMinimal
    {
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "value")]
        public List<TeamIteration> Value { get; set; }
    }

    public class TeamIteration
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public AttributesWithTimeFrame Attributes { get; set; }
    }

    public class AttributesWithTimeFrame
    {
        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "finishDate")]
        public DateTime FinishDate { get; set; }

        [JsonProperty(PropertyName = "timeFrame")]
        public string TimeFrame { get; set; }
    }

}
