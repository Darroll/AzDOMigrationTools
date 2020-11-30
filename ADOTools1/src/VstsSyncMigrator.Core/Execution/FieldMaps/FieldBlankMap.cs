using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class FieldBlankMap : FieldMapBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.FieldBlankMap"));

        #endregion

        #region - Private Members

        private readonly FieldBlankMapConfig _config;

        #endregion

        #region - Protected Members

        protected override void InternalExecute(WorkItem sourceWI, WorkItem targetWI)
        {
            if (targetWI.Fields.Contains(_config.TargetField))
            {
                targetWI.Fields[_config.TargetField].Value = String.Empty;

                // Send some traces.
                _mySource.Value.TraceInformation("[UPDATE] field mapped {0}:{1} to {2} blanked", sourceWI.Id, targetWI.Id, _config.TargetField);
                _mySource.Value.Flush();
            }
        }

        #endregion

        #region - Public Members

        public override string MappingDisplayName => $"{_config.TargetField}";

        public FieldBlankMap(FieldBlankMapConfig config)
        {
            _config = config;
        }

        #endregion
    }
}
