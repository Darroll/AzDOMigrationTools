using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class FieldMergeMapMulti : FieldMapBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.FieldMergeMapMulti"));

        #endregion

        #region - Private Members

        private readonly FieldMergeMapMultiConfig _config;

        #endregion

        #region - Protected Members        

        protected override void InternalExecute(WorkItem sourceWI, WorkItem targetWI)
        {
            // Initialize.
            bool process;
            bool storeDoneMatch;
            string verb;
            string fieldValue;
            List<string> listOfMapValues = new List<string>();
            List<FieldType> supportedFieldTypes = new List<FieldType>
                {
                    FieldType.Html,                    
                    FieldType.PlainText,
                    FieldType.String
                };

            // Validate the target field exists.
            if (!targetWI.Fields.Contains(_config.TargetField))
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Warning, 1, $"Target field {_config.TargetField} does not exist, skipping mapping");
                _mySource.Value.Flush();

                return;
            }

            // Validate if the target field has a supported type for merging.
            if (!supportedFieldTypes.Contains(targetWI.Fields[_config.TargetField].FieldDefinition.FieldType))
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Warning, 1, $"Unsupported target field {_config.TargetField}, skipping mapping");
                _mySource.Value.Flush();

                return;
            }

            foreach (string sourceField in _config.SourceFields)
            {
                if (!sourceWI.Fields.Contains(sourceField))
                {
                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Warning, 1, $"Unable to find source field {sourceField}, use a null value");
                    _mySource.Value.Flush();

                    // Add an empty mapping.
                    fieldValue = string.Empty;
                }
                else if (sourceWI.Fields[sourceField].Value == null)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Value is null for source field {sourceField}, use a null value");
                    _mySource.Value.Flush();

                    // Add an empty mapping.
                    fieldValue = string.Empty;
                }
                else if (sourceWI.Fields[sourceField].Value.ToString() == string.Empty)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Value is empty for source field {sourceField}, use a null value");
                    _mySource.Value.Flush();

                    // Add an empty mapping.
                    fieldValue = string.Empty;
                }
                else
                {
                    // Convert formatting characters into html tags or encoded characters.
                    if (sourceWI.Fields[sourceField].FieldDefinition.FieldType == FieldType.PlainText && targetWI.Fields[_config.TargetField].FieldDefinition.FieldType == FieldType.Html)
                        fieldValue = ConvertPlaintextToHtmlFormat(sourceWI.Fields[sourceField].Value.ToString());
                    // Remove plaintext remnants from the html field.
                    else if (sourceWI.Fields[sourceField].FieldDefinition.FieldType == FieldType.Html && targetWI.Fields[_config.TargetField].FieldDefinition.FieldType == FieldType.Html)
                        fieldValue = RemovePlaintextRemnants(sourceWI.Fields[sourceField].Value.ToString());
                    // Take the content as is.
                    else
                        fieldValue = sourceWI.Fields[sourceField].Value.ToString();
                }

                // Add to list of values.
                listOfMapValues.Add(fieldValue);
            }

            // Evaluate if the field merge map multi fields will execute.
            if (_config.Force)
            {
                process = true;
                storeDoneMatch = true;
                verb = "UPDATE/REPROCESS";
            }
            else if (targetWI.Fields[_config.TargetField].Value.ToString().Contains(_config.DoneMatch))
            {
                process = true;
                storeDoneMatch = true;
                verb = "UPDATE";
            }
            else
            {
                process = false;
                storeDoneMatch = false;
                verb = "SKIP";
            }            

            if (process)
            {
                // Merge fields.
                string mergedFieldValue = string.Format(_config.FormatExpression, listOfMapValues.ToArray());

                // Create a new expression with the formatted fields described and add the done match expression.
                if (storeDoneMatch)
                    targetWI.Fields[_config.TargetField].Value = mergedFieldValue + _config.DoneMatch;
                // Create a new expression with the formatted fields described.
                else
                    targetWI.Fields[_config.TargetField].Value = mergedFieldValue;

                // Send some traces.
                _mySource.Value.TraceInformation("[{0}] field merged {1}:{2} to {3}:{4}", verb, sourceWI.Id, string.Join("+", _config.SourceFields), targetWI.Id, _config.TargetField);
            }
            else
            {
                // Send some traces.
                _mySource.Value.TraceInformation("[{0}] field already merged {1}:{2} to {3}:{4}", verb, sourceWI.Id, string.Join("+", _config.SourceFields), targetWI.Id, _config.TargetField);
            }

            // Flush traces.
            _mySource.Value.Flush();
        }

        #endregion

        #region - Public Members

        public override string MappingDisplayName
        {
            get{ return string.Join("/", _config.SourceFields) + $"-> {_config.TargetField}"; }
        }

        public FieldMergeMapMulti(FieldMergeMapMultiConfig config)
        {
            _config = config;

            if(string.IsNullOrWhiteSpace(_config.DoneMatch))
                throw new ArgumentNullException($"The {nameof(config.DoneMatch)} configuration parameter cannot be null or empty.");
        }

        #endregion
    }
}
