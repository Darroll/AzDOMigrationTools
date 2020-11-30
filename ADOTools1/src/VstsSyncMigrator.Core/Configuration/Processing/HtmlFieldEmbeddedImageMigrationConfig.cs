using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class HtmlFieldEmbeddedImageMigrationConfig : ITfsProcessingConfig
    {
        /// <summary>
        /// Username used for VSTS basic authentication using alternate credentials. Leave empty for default credentials 
        /// </summary>
        [JsonProperty(PropertyName = "alternateCredentialsUsername")]
        public string AlternateCredentialsUsername { get; set; }

        /// <summary>
        /// Password used for VSTS basic authentication using alternate credentials. Leave empty for default credentials 
        /// </summary>
        [JsonProperty(PropertyName = "alternateCredentialsPassword")]
        public string AlternateCredentialsPassword { get; set; }

        /// <summary>
        /// Delete temporary files that were downloaded
        /// </summary>
        [JsonProperty(PropertyName = "deleteTemporaryImageFiles")]
        public bool DeleteTemporaryImageFiles { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "fromAnyCollection")]
        public bool FromAnyCollection { get; set; }

        /// <summary>
        /// Ignore 404 errors and continue on images that don't exist anymore
        /// </summary>
        [JsonProperty(PropertyName = "ignore404Errors")]
        public bool Ignore404Errors { get; set; }

        [JsonIgnore]
        public Type Processor
        {
            get { return typeof(HtmlFieldEmbeddedImageMigrationContext); }
        }

        [JsonProperty(PropertyName = "queryBit")]
        public string QueryBit { get; set; }

        /// <summary>
        /// TFS Aliases to use to match source images (e.g. https://myserver.company.com)
        /// </summary>
        [JsonProperty(PropertyName = "sourceServerAliases")]
        public string[] SourceServerAliases { get; set; }

        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}
