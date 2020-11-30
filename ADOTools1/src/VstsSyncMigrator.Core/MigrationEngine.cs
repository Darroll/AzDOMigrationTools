using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.ComponentContext;
using VstsSyncMigrator.Engine.Configuration;
using VstsSyncMigrator.Engine.Configuration.FieldMap;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class MigrationEngine
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine"));

        #endregion

        #region - Private Members

        private readonly string _allFields = "*";                                                   // All fields wildcard.
        private string _sourceReflectedWorkItemIdFieldName = "ProcessName.ReflectedWorkItemId";     // Default value for source reflected work item id is...
        private string _reflectedWorkItemIdFieldName = "TfsMigrationTool.ReflectedWorkItemId";      // Default value for reflected work item id is...
        private ITeamProjectContext _sourceContext;
        private ITeamProjectContext _targetContext;
        private readonly List<ITfsProcessingContext> _processors;
        private readonly Dictionary<string, List<IFieldMap>> _fieldMaps;
        private readonly Dictionary<string, IWitdMapper> _workItemTypeDefinitions;

        private void AddFieldMap(string witName, IFieldMap fieldMappingConfig)
        {
            // If this work item type is not defined in dictionary, create it.
            if (!_fieldMaps.ContainsKey(witName))
                _fieldMaps.Add(witName, new List<IFieldMap>());

            // Add mapping configuration.
            _fieldMaps[witName].Add(fieldMappingConfig);
        }

        private void AddProcessor<TProcessor>()
        {
            // Generate the processor instruction dynamically.
            ITfsProcessingContext processor = (ITfsProcessingContext)Activator.CreateInstance(typeof(TProcessor), new object[] { this });

            // Add a new process.
            AddProcessor(processor);
        }

        private void AddProcessor(ITfsProcessingContext processor)
        {
            // Add a new processor for migration processing.
            _processors.Add(processor);
        }

        private void AddWorkItemTypeDefinition(string witdName, IWitdMapper witdMap = null)
        {
            // Add to work item type definitions dictionary if it is not already entered.
            if (!WorkItemTypeDefinitions.ContainsKey(witdName))
                WorkItemTypeDefinitions.Add(witdName, witdMap);
        }

        private void ProcessConfiguration(EngineConfiguration config)
        {
            // Enable telemetry data based on configuration.
            Telemetry.EnableTrace = config.TelemetryEnableTrace;

            // Connect to source collection/project.
            if (config.Source != null)
            {
                // Send some traces.
                _mySource.Value.TraceInformation($"Connecting to Source Project: {config.Source.Name} ({config.Source.Collection})");

                // Set source project.
                TeamProjectContext context = new TeamProjectContext(config.Source.Collection, config.Source.Name, config.Source.Token);                

                // Set the source.
                this.SetSource(context);
            }

            // Connect to target collection/project.
            if (config.Target != null)
            {
                // Send some traces.
                _mySource.Value.TraceInformation($"Connecting to Target Project: {config.Target.Name} ({config.Target.Collection})");

                // Set destination/target project.
                TeamProjectContext context = new TeamProjectContext(config.Target.Collection, config.Target.Name, config.Target.Token);

                // Set the target.
                this.SetTarget(context);
            }

            // Define the field name that will retain the source work item identifier.
            this.SetReflectedWorkItemIdFieldName(config.ReflectedWorkItemIDFieldName);

            // Define the field name that will be used to populate the ReflectedWorkItemIDFieldName property.
            this.SetSourceReflectedWorkItemIdFieldName(config.SourceReflectedWorkItemIDFieldName);

            // If field map configurations have been defined...
            if (config.FieldMaps != null)
            {
                foreach (IFieldMapConfig fieldMapConfig in config.FieldMaps)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Adding FieldMap {fieldMapConfig.FieldMap.Name}");

                    // Add field map configuration.
                    this.AddFieldMap(fieldMapConfig.WorkItemTypeName, (IFieldMap)Activator.CreateInstance(fieldMapConfig.FieldMap, fieldMapConfig));
                }

                // Flush traces.
                _mySource.Value.Flush();
            }

            // Add the list of work item type definitions to handle during the migration.
            foreach (string key in config.WorkItemTypeDefinition.Keys)
            {
                // Send some traces.
                _mySource.Value.TraceInformation($"Adding Work Item Type {key}");

                // Add work item type definition supported in this migration.
                this.AddWorkItemTypeDefinition(key, new DiscreteWitdMapper(config.WorkItemTypeDefinition[key]));
            }

            // Flush traces.
            _mySource.Value.Flush();

            // Get only enabled processors.
            List<ITfsProcessingConfig> enabledProcessors = config.Processors.Where(x => x.Enabled).ToList();

            // Execute each enabled processors.
            foreach (ITfsProcessingConfig pc in enabledProcessors)
            {
                if (pc.IsProcessorCompatible(enabledProcessors))
                {
                    ITfsProcessingContext processor;

                    // Send some traces.
                    _mySource.Value.TraceInformation($"Adding Processor {pc.Processor.Name}");
                    _mySource.Value.Flush();

                    // Regenerate processor object.
                    if (pc.Processor.Name == "TestVariablesMigrationContext")
                        processor = (ITfsProcessingContext)Activator.CreateInstance(pc.Processor, this);
                    else if (pc.Processor.Name == "WorkItemDelete")
                        processor = (ITfsProcessingContext)Activator.CreateInstance(pc.Processor, this);
                    else
                        processor = (ITfsProcessingContext)Activator.CreateInstance(pc.Processor, this, pc);

                    // Add processor.
                    this.AddProcessor(processor);
                }
                else
                {
                    // Raise an exception.
                    string errorMsg = $"[ERROR] Cannot add Processor {pc.Processor.Name}. Processor is not compatible with other enabled processors in configuration.";
                    throw (new InvalidOperationException(errorMsg));
                }
            }
        }

        private void ProcessFieldMapList(WorkItem sourceWI, WorkItem targetWI, List<IFieldMap> listOfFieldMappingConfigs)
        {
            foreach (IFieldMap fieldMappingConfig in listOfFieldMappingConfigs)
            {
                // Send some traces.
                _mySource.Value.TraceInformation($"Running Field Map: {fieldMappingConfig.Name} {fieldMappingConfig.MappingDisplayName}");
                _mySource.Value.Flush();

                // Perform the mapping operation.
                fieldMappingConfig.Execute(sourceWI, targetWI);
            }
        }

        private void SetReflectedWorkItemIdFieldName(string fieldName)
        {
            if (!string.IsNullOrEmpty(fieldName))
                _reflectedWorkItemIdFieldName = fieldName;
        }

        private void SetSource(ITeamProjectContext context)
        {
            _sourceContext = context;
        }

        private void SetSourceReflectedWorkItemIdFieldName(string fieldName)
        {
            if (!string.IsNullOrEmpty(fieldName))
                _sourceReflectedWorkItemIdFieldName = fieldName;
        }

        private void SetTarget(ITeamProjectContext context)
        {
            _targetContext = context;
        }

        #endregion

        #region - Internal Members

        internal ITeamProjectContext Source
        {
            get { return _sourceContext; }
        }

        internal ITeamProjectContext Target
        {
            get { return _targetContext; }
        }

        internal string ReflectedWorkItemIdFieldName
        {
            get { return _reflectedWorkItemIdFieldName; }
        }

        internal string SourceReflectedWorkItemIdFieldName
        {
            get { return _sourceReflectedWorkItemIdFieldName; }
        }

        internal Dictionary<string, IWitdMapper> WorkItemTypeDefinitions
        {
            get { return _workItemTypeDefinitions; }
        }

        internal void ApplyFieldMappings(WorkItem target)
        {
            // Apply the mappings from target for the all fields wildcard.
            if (_fieldMaps.ContainsKey(_allFields))
                ProcessFieldMapList(target, target, _fieldMaps[_allFields]);

            // Apply the mappings from target.
            if (_fieldMaps.ContainsKey(target.Type.Name))
                ProcessFieldMapList(target, target, _fieldMaps[target.Type.Name]);
        }

        internal void ApplyFieldMappings(WorkItem source, WorkItem target)
        {
            // Apply the mappings from source on target for all fields wildcard.
            if (_fieldMaps.ContainsKey(_allFields))
                ProcessFieldMapList(source, target, _fieldMaps[_allFields]);

            // Apply the mappings from source on target.
            if (_fieldMaps.ContainsKey(source.Type.Name))
                ProcessFieldMapList(source, target, _fieldMaps[source.Type.Name]);
        }

        internal bool TestIfSupportedWorkItemTypeDefinition(string witdName)
        {
            return _workItemTypeDefinitions.ContainsKey(witdName);
        }

        #endregion

        #region - Public Members

        public MigrationEngine(EngineConfiguration config)
        {
            // Instantiate.
            _processors = new List<ITfsProcessingContext>();
            _fieldMaps = new Dictionary<string, List<IFieldMap>>();
            _workItemTypeDefinitions = new Dictionary<string, IWitdMapper>();

            // Define configuration for processor.
            ProcessConfiguration(config);
        }

        public ProcessingStatus Run()
        {
            // Send telemetry data.
            Telemetry.Current.TrackEvent(
                    "EngineStart",
                    new Dictionary<string, string> { { "Engine", "Migration" } },
                    new Dictionary<string, double> {
                        { "Processors", _processors.Count },
                        { "Mappings", _fieldMaps.Count }
                    }
                );

            // Initialize stop watch.
            Stopwatch processorEngineTimer = new Stopwatch();
            processorEngineTimer.Start();

            // Initialize processor engine status.
            ProcessingStatus processorEngineState;

            // Send some traces.
            _mySource.Value.TraceInformation("Beginning run of {0} processors", _processors.Count.ToString());
            _mySource.Value.Flush();

            // Set status.
            processorEngineState = ProcessingStatus.Running;

            foreach (ITfsProcessingContext process in _processors)
            {
                // Initialize stop watch.
                Stopwatch processorTimer = new Stopwatch();

                // Start timer.
                processorTimer.Start();

                // Execute the processor process.
                process.Execute();

                // Stop timer.
                processorTimer.Stop();

                // Send telemetry data.
                Telemetry.Current.TrackEvent("ProcessorComplete", new Dictionary<string, string> { { "Processor", process.Name }, { "Status", process.Status.ToString() } }, new Dictionary<string, double> { { "ProcessingTime", processorTimer.ElapsedMilliseconds } });

                if (process.Status == ProcessingStatus.Failed)
                {
                    // It has failed.
                    processorEngineState = ProcessingStatus.Failed;

                    // Send some traces.
                    _mySource.Value.TraceInformation("The Processor {0} entered the failed state...stopping run", process.Name);
                    _mySource.Value.Flush();

                    // Leave the loop.
                    break;
                }
            }

            // Stop timer.
            processorEngineTimer.Stop();

            // Send some traces.
            Telemetry.Current.TrackEvent(
                    "EngineComplete",
                    new Dictionary<string, string> { { "Engine", "Migration" } },
                    new Dictionary<string, double> {
                        { "EngineTime", processorEngineTimer.ElapsedMilliseconds }
                    }
                );

            // Return the processor engine state.
            return processorEngineState;
        }

        #endregion
    }
}