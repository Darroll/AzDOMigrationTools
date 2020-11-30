using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Diagnostics;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemUpdate : ProcessingContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.WorkItemUpdate"));

        #endregion

        #region - Private Members

        private readonly WorkItemUpdateConfig _config;

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            // Initialize.
            int currentWI;
            int countWI = 0;
            long timeElapsed = 0;   // in milliseconds.

            // Create a stop watch to measure the query execution time.
            Stopwatch queryTimer = new Stopwatch();

            // Create a stop watch to measure the change against a work item.
            Stopwatch witModificationTimer = new Stopwatch();

            // Start timer.
            queryTimer.Start();

            // Get destination/target work item store.
            WorkItemStoreContext targetStore = new WorkItemStoreContext(Engine.Target, WorkItemStoreFlags.BypassRules);

            // Create a new query context.
            TfsQueryContext tfsqc = new TfsQueryContext(targetStore);

            // Add parameters to context.
            tfsqc.AddParameter("TeamProject", Engine.Target.Name);

            // Set the query.
            tfsqc.Query = string.Format(@"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.Id]", _config.QueryBit);
            //tfsqc.Query = string.Format(@"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc", _config.QueryBit);

            // Execute.
            WorkItemCollection workitems = tfsqc.Execute();

            // Send some traces.
            _mySource.Value.TraceInformation("Update {0} work items?", workitems.Count);
            _mySource.Value.Flush();

            // How many work items to process.
            currentWI = workitems.Count;

            foreach (WorkItem workitem in workitems)
            {
                // Reset and start.
                witModificationTimer.Reset();
                witModificationTimer.Start();

                // Send some traces.
                _mySource.Value.TraceInformation("Processing work item {0} - Type:{1} - ChangedDate:{2} - CreatedDate:{3}", workitem.Id, workitem.Type.Name, workitem.ChangedDate.ToShortDateString(), workitem.CreatedDate.ToShortDateString());
                _mySource.Value.Flush();

                // Open, change and save.
                workitem.Open();
                Engine.ApplyFieldMappings(workitem);

                #region - Procedd only if mapping incurs a change.

                if (workitem.IsDirty)
                {
                    if (!_config.WhatIf)
                    {
                        try
                        {
                            // Try to save.
                            workitem.Save();
                        }
                        catch (Exception)
                        {
                            // Retry in 5 seconds.
                            System.Threading.Thread.Sleep(5000);
                            workitem.Save();
                        }

                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("No save done: (What IF: enabled)");
                        _mySource.Value.Flush();
                    }

                    #endregion
                }
                else
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("No save done: (IsDirty: false)");
                    _mySource.Value.Flush();
                }

                // Stop timer.
                witModificationTimer.Stop();

                // Decrement number of work items to process.
                currentWI--;

                // Increment counter.
                countWI++;

                // Calculate stats.
                timeElapsed += witModificationTimer.ElapsedMilliseconds;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(timeElapsed / countWI));
                string averageAsString = string.Format(@"{0:s\:fff} seconds", average);
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * currentWI));

                // Calculate the time to completion.
                string estimatedTimeToCompletion = string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining);

                // Send some traces.
                _mySource.Value.TraceInformation("Average time of {0} per work item and {1} estimated to completion", averageAsString, estimatedTimeToCompletion);
                _mySource.Value.Flush();
            }

            // Stop timer.
            queryTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", queryTimer.Elapsed);
            _mySource.Value.Flush();
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "WorkItemUpdate"; }
        }

        public WorkItemUpdate(MigrationEngine me, WorkItemUpdateConfig config) : base(me)
        {
            _config = config;
        }

        #endregion
    }
}