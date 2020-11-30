using Newtonsoft.Json;

namespace ADO.Engine.BusinessEntities
{
    public class TeamSettingsMinimal
    {
        [JsonProperty(PropertyName = "backlogIteration")]
        public BacklogIteration BacklogIteration { get; set; }
        
        [JsonProperty(PropertyName = "bugsBehavior")]
        public string BugsBehavior { get; set; }
        
        [JsonProperty(PropertyName = "workingDays")]
        public string[] WorkingDays { get; set; }
        
        [JsonProperty(PropertyName = "backlogVisibilities")]
        public BacklogVisibilities BacklogVisibilities { get; set; }
        
        [JsonProperty(PropertyName = "defaultIteration")]
        public DefaultIteration DefaultIteration { get; set; }
        
        [JsonProperty(PropertyName = "defaultIterationMacro")]
        public string DefaultIterationMacro { get; set; }
    }

    public class BacklogIteration
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }
    }

    public class BacklogVisibilities
    {
        [JsonProperty(PropertyName = "Microsoft.EpicCategory")]
        public bool MicrosoftEpicCategory { get; set; }

        [JsonProperty(PropertyName = "Microsoft.FeatureCategory")]
        public bool MicrosoftFeatureCategory { get; set; }

        [JsonProperty(PropertyName = "Microsoft.RequirementCategory")]
        public bool MicrosoftRequirementCategory { get; set; }
    }

    public class DefaultIteration
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }
    }

}
