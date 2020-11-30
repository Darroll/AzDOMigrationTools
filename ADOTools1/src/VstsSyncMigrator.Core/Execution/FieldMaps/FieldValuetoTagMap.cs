using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.ComponentContext;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine
{
    public class FieldValuetoTagMap : IFieldMap
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.FieldValuetoTagMap"));

        #endregion

        #region - Private Members

        private readonly FieldValuetoTagMapConfig _config;

        #endregion

        #region - Public Members

        public string MappingDisplayName => $"{_config.SourceField}";

        public string Name
        {
            get { return "FieldValuetoTagMap"; }
        }

        public FieldValuetoTagMap(FieldValuetoTagMapConfig config)
        {
            _config = config;
        }

        public void Execute(WorkItem sourceWI, WorkItem targetWI)
        {
            if (sourceWI.Fields.Contains(_config.SourceField))
            {
                // Parse existing tags entry
                List<string> listOfExistingTags = targetWI.Tags.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                // Only proceed if value is available
                bool match = false;
                object value = sourceWI.Fields[_config.SourceField].Value;

                if (value != null)
                {
                    // regular expression matching is being used
                    if (!string.IsNullOrEmpty(_config.Pattern))
                        match = Regex.IsMatch(value.ToString(), _config.Pattern);
                    // always apply tag if value exists
                    else
                        match = true;
                }

                // add new tag if available
                if (match)
                {
                    // Format or simple to string.
                    string newTag = string.IsNullOrEmpty(_config.FormatExpression) ? value.ToString() : string.Format(_config.FormatExpression, value);
                    if (!string.IsNullOrWhiteSpace(newTag))
                        listOfExistingTags.Add(newTag);

                    // Rewrite tag values if changed.
                    string newTags = string.Join(";", listOfExistingTags.Distinct());
                    if (newTags != targetWI.Tags)
                    {
                        targetWI.Tags = newTags;

                        // Send some traces.
                        _mySource.Value.TraceInformation("[UPDATE] field tagged {0}:{1} to {2}:Tag with format of {3}", sourceWI.Id, _config.SourceField, targetWI.Id, _config.FormatExpression);
                        _mySource.Value.Flush();
                    }
                }
            }
        }

        #endregion
    }
}