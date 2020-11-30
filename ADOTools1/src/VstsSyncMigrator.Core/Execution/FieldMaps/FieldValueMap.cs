using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class FieldValueMap : FieldMapBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.FieldValueMap"));

        #endregion

        #region - Private Members

        private readonly FieldValueMapConfig _config;

        #endregion

        #region - Protected Members

        protected override void InternalExecute(WorkItem sourceWI, WorkItem targetWI)
        {
            if (sourceWI.Fields.Contains(_config.SourceField))
            {
                // Initialize.
                string sourceValue = sourceWI.Fields[_config.SourceField].Value?.ToString();

                if (sourceValue != null && _config.ValueMapping.ContainsKey(sourceValue))
                {
                    targetWI.Fields[_config.TargetField].Value = _config.ValueMapping[sourceValue];

                    // Send some traces.
                    _mySource.Value.TraceInformation($"[UPDATE] field value mapped {sourceWI.Id}:{_config.SourceField} to {targetWI.Id}:{_config.TargetField}");
                    _mySource.Value.Flush();
                }
                else if (sourceValue != null && !string.IsNullOrEmpty(_config.DefaultValue))
                {
                    targetWI.Fields[_config.TargetField].Value = _config.DefaultValue;

                    // Send some traces.
                    _mySource.Value.TraceInformation($"[UPDATE] field set to default value {sourceWI.Id}:{_config.SourceField} to {targetWI.Id}:{_config.TargetField}");
                    _mySource.Value.Flush();
                }
            }

        }

        #endregion

        #region - Public Members

        public override string MappingDisplayName => $"{_config.SourceField} {_config.TargetField}";

        public FieldValueMap(FieldValueMapConfig config)
        {
            _config = config;
        }

        #endregion
    }
}
