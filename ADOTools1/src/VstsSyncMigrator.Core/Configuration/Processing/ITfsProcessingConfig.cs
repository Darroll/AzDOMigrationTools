using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public interface ITfsProcessingConfig
    {
        #region - Properties

        [JsonProperty(PropertyName = "enabled")]
        bool Enabled { get; set; }

        [JsonIgnore]
        Type Processor { get; }

        #endregion

        #region - Methods

        /// <summary>
        /// Indicates, if this processor can be added to the list of current processors or not.
        /// Some processors are not compatible with each other.
        /// </summary>
        /// <param name="otherProcessors">List of already configured processors.</param>
        bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors);

        #endregion
    }
}
