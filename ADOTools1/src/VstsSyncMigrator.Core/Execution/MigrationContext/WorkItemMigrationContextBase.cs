using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VstsSyncMigrator.Engine
{
    public abstract class WorkItemMigrationContextBase : ITfsProcessingContext
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.WorkItemMigrationContextBase"));

        // Create locking object.
        private static readonly object _lock = new object();

        #endregion

        #region - Private Members

        private readonly MigrationEngine _me;
        private string _authorizedIdentityName;
        private List<String> _ignoreFields;
        private List<String> _ignoreFieldsWhenReprocessing;
        private ClassificationNodeDetection _classificationNodeValidation;
        private readonly Stopwatch _internalExecutionTimer = new Stopwatch();
        private readonly Stopwatch _populateOperationTimer = new Stopwatch();
        private readonly Stopwatch _queryTimer = new Stopwatch();
        private readonly Stopwatch _replayWITimer = new Stopwatch();

        private void PopulateIgnoreFieldLists()
        {
            // List of all possible fields.
            // https://docs.microsoft.com/en-us/azure/devops/boards/work-items/guidance/work-item-field?view=azure-devops

            // Set default base ignore fields.
            List<string> l1 = new List<string>
                {
                    "System.AreaId",
                    "System.CreatedBy",         // This is processed independently.
                    "System.CreatedDate",       // This is processed independently.
                    "System.Id",
                    "System.History",           // This is processed independently.
                    "System.IterationId",
                    "System.NodeName",
                    "System.TeamProject",
                    "System.WorkItemType",
                    "System.Watermark",
                    "Microsoft.VSTS.Common.ActivatedBy",
                    "Microsoft.VSTS.Common.ActivatedDate",
                    "Microsoft.VSTS.Common.ClosedBy",
                    "Microsoft.VSTS.Common.ClosedDate",
                    "Microsoft.VSTS.Common.ResolvedBy",
                    "Microsoft.VSTS.Common.ResolvedDate",
                    "Microsoft.VSTS.Common.StateChangeDate"
                };
            _ignoreFields = new List<string>();
            _ignoreFields.AddRange(l1);

            // You cannot set the ChangedDate :
            //  - Less than the changed date of previous revision
            //  - Greater than current date.
            List<string> l2 = new List<string> { "System.ChangedDate" };
            _ignoreFieldsWhenReprocessing = new List<string>();
            _ignoreFieldsWhenReprocessing.AddRange(_ignoreFields);
            _ignoreFieldsWhenReprocessing.AddRange(l2);
        }

        #endregion

        #region - Protected Members

        protected void AddIgnoreFieldWhenReprocessing(List<string> ignoreFields = null)
        {
            _ignoreFieldsWhenReprocessing.AddRange(ignoreFields);
        }

        protected string AuthorizedIdentityName
        {
            get { return _authorizedIdentityName; }
            set { _authorizedIdentityName = value; }
        }

        protected List<String> IgnoreFields
        {
            get { return _ignoreFields; }
        }

        protected List<String> IgnoreFieldsWhenReprocessing
        {
            get { return _ignoreFieldsWhenReprocessing; }
        }

        protected MigrationEngine Engine
        {
            get { return _me; }
        }

        protected Stopwatch InternalExecutionTimer
        {
            get { return _internalExecutionTimer; }
        }

        protected Stopwatch PopulateOperationTimer
        {
            get { return _populateOperationTimer; }
        }

        protected Stopwatch QueryTimer
        {
            get { return _queryTimer; }
        }

        protected Stopwatch ReplayWITimer
        {
            get { return _replayWITimer; }
        }

        protected WorkItemMigrationContextBase(MigrationEngine me)
        {
            // Store migration engine.
            _me = me;

            // Populate the list of fields to ignore.
            PopulateIgnoreFieldLists();

            // Instantiante timers.
            _internalExecutionTimer = new Stopwatch();
            _populateOperationTimer = new Stopwatch();
            _queryTimer = new Stopwatch();
            _replayWITimer = new Stopwatch();
        }

        protected abstract void PopulateWorkItem(WorkItem sourceWI, WorkItem targetWI, bool reprocessing = false);

        protected string SetTargetNodePath(string configuredAreaPath, string configuredIterationPath, bool configuredPrefixProjectToNodes,
                                            string configuredPrefixPath, string sourceNodePath, string sourceProjectName, string targetProjectName,
                                            WorkItemStore targetWIStore, string nodeType, bool isClone)
        {
            // Initialize.
            string canonicalNodePath;
            string nodePath;

            // Instantiate if needed.
            if (_classificationNodeValidation == null)
            {
                // Operation against the store will call internal methods
                // that are not thread-safe.
                // Creating a NodeDetecomatic involves to access the store.
                lock (_lock)
                    _classificationNodeValidation = new ClassificationNodeDetection(targetWIStore);
            }

            // Assign a determined iteration path.
            if (!isClone)
            {
                if (!string.IsNullOrEmpty(configuredIterationPath) && nodeType == "Iteration")
                {
                    string curatedValue = configuredIterationPath.TrimEnd('\\');
                    canonicalNodePath = $@"\{targetProjectName}\{nodeType}\{curatedValue}";
                    nodePath = $@"{targetProjectName}\{curatedValue}";
                }
                // Assign a determined area path.
                else if (!string.IsNullOrEmpty(configuredAreaPath) && nodeType == "Area")
                {
                    string curatedValue = configuredAreaPath.TrimEnd('\\');
                    canonicalNodePath = $@"\{targetProjectName}\{nodeType}\{curatedValue}";
                    nodePath = $@"{targetProjectName}\{curatedValue}";
                }
                else
                {
                    // Add the source project as prefix to current source node path.
                    if (configuredPrefixProjectToNodes)
                    {
                        canonicalNodePath = $@"\{targetProjectName}\{nodeType}\{sourceProjectName}\{sourceNodePath}";
                        nodePath = $@"{targetProjectName}\{sourceProjectName}\{sourceNodePath}";
                    }
                    // Add a custom prefix to current source node path.                
                    else if (!string.IsNullOrEmpty(configuredPrefixPath))
                    {
                        string curatedValue = configuredPrefixPath.TrimEnd('\\');
                        canonicalNodePath = $@"\{targetProjectName}\{nodeType}\{curatedValue}\{sourceNodePath}";
                        nodePath = $@"{targetProjectName}\{curatedValue}\{sourceNodePath}";
                    }
                    // Use the current source node path.
                    else
                    {
                        canonicalNodePath = $@"\{targetProjectName}\{nodeType}\{sourceNodePath}";
                        nodePath = $@"{targetProjectName}\{sourceNodePath}";
                    }
                }
            }
            else
            {
                //is clone
                //need to change logic when cloning
                nodePath = sourceNodePath;
                if (sourceNodePath.Contains("\\"))
                {
                    canonicalNodePath = $@"\{targetProjectName}\{nodeType}\{sourceNodePath.Substring(targetProjectName.Length + 1)}";
                }
                else
                {
                    canonicalNodePath = $@"\{targetProjectName}\{nodeType}";
                }
            }

            // Validate the node exists, if it does not, assign the root which means the project name.
            if (!_classificationNodeValidation.NodeExists(canonicalNodePath))
            {
                // Send some traces.
                _mySource.Value.TraceInformation($"The node '{canonicalNodePath}' does not exist, leaving as '{targetProjectName}'. This may be because it has been renamed or moved and no longer exists, or that you have not migrated the node structure yet.");
                _mySource.Value.Flush();

                // Set node path.
                nodePath = targetProjectName;
            }

            // Return node name.
            return nodePath;
        }

        #endregion

        #region - Internal Members


        internal abstract void InternalExecute();

        #endregion

        #region - Public Members

        public abstract string Name { get; }

        public ProcessingStatus Status { get; private set; } = ProcessingStatus.None;

        public void Execute()
        {
            // Send telemetry data.
            Telemetry.Current.TrackPageView(this.Name);

            // Send some traces.
            _mySource.Value.TraceInformation($"{Name} Start");
            _mySource.Value.Flush();

            // Create a stop watch to measure the execution time.
            Stopwatch executionTimer = new Stopwatch();

            // Keep execution attempt time.
            DateTime start = DateTime.Now;

            // Start timer.
            executionTimer.Start();

            try
            {
                // Change status to running.
                this.Status = ProcessingStatus.Running;

                // Execute processor.
                this.InternalExecute();

                // Change status to complete.
                this.Status = ProcessingStatus.Complete;

                // Stop timer.
                executionTimer.Stop();

                // Send some traces.
                _mySource.Value.TraceInformation($"{Name} Complete");
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Change status to failed.
                this.Status = ProcessingStatus.Failed;

                // Stop timer.
                executionTimer.Stop();

                // Send telemetry data.
                Telemetry.Current.TrackException(
                        ex,
                        new Dictionary<string, string>
                        {
                            {"Name", Name},
                            {"Target Project", _me.Target.Name},
                            {"Target Collection", _me.Target.Collection.Name},
                            {"Source Project", _me.Source.Name},
                            {"Source Collection", _me.Source.Collection.Name},
                            {"Status", Status.ToString()}
                        },
                        new Dictionary<string, double>
                        {
                            {"MigrationContextTime", executionTimer.ElapsedMilliseconds}
                        }
                    );

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"[EXCEPTION] {ex}");
                _mySource.Value.Flush();
            }
            finally
            {
                // Send telemetry data.
                Telemetry.Current.TrackRequest(this.Name, start, executionTimer.Elapsed, this.Status.ToString(), (this.Status == ProcessingStatus.Complete));
            }
        }

        #endregion
    }
}