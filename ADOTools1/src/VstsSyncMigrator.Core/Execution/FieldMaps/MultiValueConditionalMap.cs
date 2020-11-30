using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class MultiValueConditionalMap : FieldMapBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.MultiValueConditionalMap"));

        #endregion

        #region - Private Members

        private readonly MultiValueConditionalMapConfig _config;

        private bool FieldsExist(Dictionary<string, string> fieldsAndValues, WorkItem w)
        {
            // Initialize.
            bool exists = true;

            foreach (string field in fieldsAndValues.Keys)
                if (!w.Fields.Contains(field))
                    exists = false;

            return exists;
        }

        private void FieldsUpdate(Dictionary<string, string> fieldAndValues, WorkItem w)
        {
            foreach (string field in fieldAndValues.Keys)
                w.Fields[field].Value = fieldAndValues[field];
        }

        private bool FieldsValueMatch(Dictionary<string, string> fieldAndValues, WorkItem w)
        {
            bool matches = true;
            foreach (string field in fieldAndValues.Keys)
                if ((string)w.Fields[field].Value != fieldAndValues[field])
                    matches = false;

            return matches;
        }

        #endregion

        #region - Protected Members

        protected override void InternalExecute(WorkItem sourceWI, WorkItem targetWI)
        {
            if (FieldsExist(_config.SourceFieldsAndValues, sourceWI) && FieldsExist(_config.TargetFieldsAndValues, targetWI))
            {
                if (FieldsValueMatch(_config.SourceFieldsAndValues, sourceWI))
                    FieldsUpdate(_config.TargetFieldsAndValues, targetWI);

                // Send some traces.
                _mySource.Value.TraceInformation("[UPDATE] field mapped {0}:{1} to {2}:{3}", sourceWI.Id, _config.SourceFieldsAndValues.Keys.ToString(), targetWI.Id, _config.TargetFieldsAndValues.Keys.ToString());
                _mySource.Value.Flush();
            }
            else
            {
                // Send some traces.
                _mySource.Value.TraceInformation("[SKIPPED] Not all source and target fields exist", sourceWI.Id, _config.SourceFieldsAndValues.Keys.ToString(), targetWI.Id, _config.TargetFieldsAndValues.Keys.ToString());
                _mySource.Value.Flush();
            }
        }

        #endregion

        #region - Public Members

        public override string MappingDisplayName => string.Empty;

        public MultiValueConditionalMap(MultiValueConditionalMapConfig config)
        {
            _config = config;
        }

        #endregion
    }
}
