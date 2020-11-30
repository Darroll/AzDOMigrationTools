using System.Collections.Generic;
using Newtonsoft.Json;

namespace ADO.RestAPI.Viewmodel50
{
    public class DistributedTaskMinimalResponse
    {
        // This is just a container class for all REST API responses related to tasks.

        #region - Nested Classes and Enumerations.

        public class Tasks
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public IList<Task> Value { get; set; }
        }

        public class Task
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "version")]
            public TaskVersion Version { get; set; }
        }

        public class TaskVersion
        {
            [JsonProperty(PropertyName = "isTest")]
            public bool IsTest { get; set; }

            [JsonProperty(PropertyName = "major")]
            public int Major { get; set; }

            [JsonProperty(PropertyName = "minor")]
            public int Minor { get; set; }

            [JsonProperty(PropertyName = "patch")]
            public int Patch { get; set; }
        }

        #endregion
    }
}