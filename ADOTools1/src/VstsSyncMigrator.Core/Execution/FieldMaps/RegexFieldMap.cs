using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.ComponentContext;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine
{
    public class RegexFieldMap : IFieldMap
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.RegexFieldMap"));

        #endregion

        #region - Private Members

        private readonly RegexFieldMapConfig _config;

        #endregion

        #region - Public Members

        public string MappingDisplayName => $"{_config.SourceField} {_config.TargetField}";

        public string Name
        {
            get { return "RegexFieldMap"; }
        }

        public RegexFieldMap(RegexFieldMapConfig config)
        {
            _config = config;
        }

        public void Execute(WorkItem sourceWI, WorkItem targetWI)
        {
            if (sourceWI.Fields.Contains(_config.SourceField) && sourceWI.Fields[_config.SourceField].Value != null && targetWI.Fields.Contains(_config.TargetField))
            {
                if (Regex.IsMatch(sourceWI.Fields[_config.SourceField].Value.ToString(), _config.Pattern))
                {
                    targetWI.Fields[_config.TargetField].Value = Regex.Replace(sourceWI.Fields[_config.SourceField].Value.ToString(), _config.Pattern, _config.Replacement);

                    // Send some traces.
                    _mySource.Value.TraceInformation("[UPDATE] field tagged {0}:{1} to {2}:{3} with regex pattern of {4} resulting in {5}", sourceWI.Id, _config.SourceField, targetWI.Id, _config.TargetField, _config.Pattern, targetWI.Fields[_config.TargetField].Value);
                    _mySource.Value.Flush();
                }
            }
        }

        #endregion
    }
}