using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.ComponentContext;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine
{
    public class FieldToTagFieldMap : IFieldMap
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.FieldToTagFieldMap"));

        #endregion

        #region - Private Members

        private readonly FieldtoTagMapConfig _config;

        #endregion

        #region - Public Members

        public string MappingDisplayName => _config.SourceField;

        public string Name
        {
            get { return "FieldToTagFieldMap"; }
        }

        public FieldToTagFieldMap(FieldtoTagMapConfig config)
        {
            _config = config;
        }

        public void Execute(WorkItem sourceWI, WorkItem targetWI)
        {
            if (sourceWI.Fields.Contains(_config.SourceField))
            {
                // Initialize.
                List<string> listOfNewTags = targetWI.Tags.Split(char.Parse(@";")).ToList();

                // to tag
                if (sourceWI.Fields[_config.SourceField].Value != null)
                {
                    string value = sourceWI.Fields[_config.SourceField].Value.ToString();
                    if (string.IsNullOrEmpty(_config.FormatExpression))
                        listOfNewTags.Add(value);
                    else
                        listOfNewTags.Add(string.Format(_config.FormatExpression, value));

                    targetWI.Tags = string.Join(";", listOfNewTags.ToArray());

                    // Send some traces.
                    _mySource.Value.TraceInformation("[UPDATE] field tagged {0}:{1} to {2}:Tag with foramt of {3}", sourceWI.Id, _config.SourceField, targetWI.Id, _config.FormatExpression);
                    _mySource.Value.Flush();
                }
            }
        }

        #endregion
    }
}