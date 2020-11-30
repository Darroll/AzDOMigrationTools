using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VstsSyncMigrator.Core.BusinessEntities;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemQueryMigrationConfig : ITfsProcessingConfig
    {
        #region - Private Members

        /// <summary>
        /// The name of the shared folder, setting the default name
        /// </summary>

        #endregion

        #region - Public Members

        /// <summary>
        /// Is this processor enabled
        /// </summary>
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Do we add the source project name into the folder path
        /// </summary>
        [JsonProperty(PropertyName = "prefixProjectToNodes")]
        public bool PrefixProjectToNodes { get; set; }

        [JsonProperty(PropertyName = "prefixPath")]
        public string PrefixPath { get; set; }

        [JsonProperty(PropertyName = "useAreaPathMap")]
        public bool UseAreaPathMap { get; set; }

        [JsonProperty(PropertyName = "areaPathMap")]
        public Dictionary<string, string> AreaPathMap { get; set; }

        [JsonProperty(PropertyName = "fullAreaPathMap")]
        public SerializableClassificationNodeMapWithCache FullAreaPathMap { get; set; }
        
        [JsonProperty(PropertyName = "useIterationPathMap")]
        public bool UseIterationPathMap { get; set; }

        [JsonProperty(PropertyName = "iterationPathMap")]
        public Dictionary<string,string> IterationPathMap { get; set; }

        [JsonProperty(PropertyName = "fullIterationPathMap")]
        public SerializableClassificationNodeMapWithCache FullIterationPathMap { get; set; }

        [JsonIgnore]
        public Type Processor
        {
            get { return typeof(WorkItemQueryMigrationContext); }
        }

        /// <summary>
        /// The name of the shared folder, made a parameter incase it every needs to be edited
        /// </summary>
        [JsonProperty(PropertyName = "sharedFolderName")]
        public string SharedFolderName { get; set; } = "Shared Queries";

        [JsonProperty(PropertyName = "isClone")]
        public bool IsClone { get; set; } = true;

        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }

        #endregion
    }
}