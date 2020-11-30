using Newtonsoft.Json;
using System.Collections.Generic;

namespace ADO.Engine.BusinessEntities
{
    public class Teams
    {
        public List<Team> TeamList { get; set; } = new List<Team>();
    }

    public class Team
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "projectName")]
        public string ProjectName { get; set; }
    }

}
