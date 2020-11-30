using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class FieldToFieldMap : FieldMapBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.FieldToFieldMap"));

        #endregion

        #region - Private Members

        private readonly FieldtoFieldMapConfig _config;

        #endregion

        #region - Protected Members

        protected override void InternalExecute(WorkItem sourceWI, WorkItem targetWI)
        {
            if (sourceWI.Fields.Contains(_config.SourceField) && targetWI.Fields.Contains(_config.TargetField))
            {
                // Convert formatting characters into html tags or encoded characters.
                if (sourceWI.Fields[_config.SourceField].FieldDefinition.FieldType == FieldType.PlainText && targetWI.Fields[_config.TargetField].FieldDefinition.FieldType == FieldType.Html)
                    targetWI.Fields[_config.TargetField].Value = ConvertPlaintextToHtmlFormat(sourceWI.Fields[_config.SourceField].Value.ToString());
                // Remove plaintext remnants from the html field.
                else if (sourceWI.Fields[_config.SourceField].FieldDefinition.FieldType == FieldType.Html && targetWI.Fields[_config.TargetField].FieldDefinition.FieldType == FieldType.Html)
                    targetWI.Fields[_config.TargetField].Value = RemovePlaintextRemnants(sourceWI.Fields[_config.SourceField].Value.ToString());
                // Take the content as is.
                else
                    targetWI.Fields[_config.TargetField].Value = sourceWI.Fields[_config.SourceField].Value.ToString();

                // Send some traces.
                _mySource.Value.TraceInformation("[UPDATE] field mapped {0}:{1} to {2}:{3}", sourceWI.Id, _config.SourceField, targetWI.Id, _config.TargetField);
                _mySource.Value.Flush();
            }
        }

        #endregion

        #region - Public Members

        public override string MappingDisplayName
        {
            get { return $"{_config.SourceField} {_config.TargetField}"; }
        }

        public FieldToFieldMap(FieldtoFieldMapConfig config)
        {
            _config = config;
        }

        #endregion
    }
}
