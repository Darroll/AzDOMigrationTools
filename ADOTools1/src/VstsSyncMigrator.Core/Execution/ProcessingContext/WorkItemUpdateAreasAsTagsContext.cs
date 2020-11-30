using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemUpdateAreasAsTagsContext : ProcessingContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.WorkItemUpdateAreasAsTagsContext"));

        #endregion

        #region - Private Members

        private readonly WorkItemUpdateAreasAsTagsConfig _config;

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
            Stopwatch modificationTimer = new Stopwatch();

            // Start timer.
            queryTimer.Start();

            // Get destination/target work item store.
            WorkItemStoreContext targetStore = new WorkItemStoreContext(Engine.Target, WorkItemStoreFlags.BypassRules);

            // Create a new query context.
            TfsQueryContext tfsqc = new TfsQueryContext(targetStore);

            // Add parameters to context.
            tfsqc.AddParameter("TeamProject", Engine.Target.Name);
            tfsqc.AddParameter("AreaPath", _config.AreaIterationPath);

            // Set the query.
            tfsqc.Query = @"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject and [System.AreaPath] under @AreaPath";

            // Execute.
            WorkItemCollection workitems = tfsqc.Execute();

            // Send some traces.
            _mySource.Value.TraceInformation("Update {0} work items?", workitems.Count);
            _mySource.Value.Flush();

            // How many work items to process.
            currentWI = workitems.Count;

            // Browse each work item.
            foreach (WorkItem workitem in workitems)
            {
                // Reset and start.
                modificationTimer.Reset();
                modificationTimer.Start();

                // Send some traces.
                _mySource.Value.TraceInformation("{0} - Updating: {1}-{2}", currentWI, workitem.Id, workitem.Type.Name);
                _mySource.Value.Flush();

                string areaPath = workitem.AreaPath;
                // Tokenize but skip first 4 items in the list.
                List<string> bits = new List<string>(areaPath.Split(char.Parse(@"\"))).Skip(4).ToList();
                // Extract the tags.
                List<string> tags = workitem.Tags.Split(char.Parse(@";")).ToList();
                // Generate a new set of tags.
                List<string> newTags = tags.Union(bits).ToList();
                // Set a string list.
                string newTagListAsString = string.Join(";", newTags.ToArray());
                // Proceed only if different.
                if (newTagListAsString != workitem.Tags)
                {
                    // Open, change and save.
                    workitem.Open();
                    workitem.Tags = newTagListAsString;
                    workitem.Save();

                }

                // Stop timer.
                modificationTimer.Stop();

                // Decrement number of work items to process.
                currentWI--;

                // Increment counter.
                countWI++;

                // Calculate stats.
                timeElapsed += modificationTimer.ElapsedMilliseconds;
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
            get { return "WorkItemUpdateAreasAsTagsContext"; }
        }

        public WorkItemUpdateAreasAsTagsContext(MigrationEngine me, WorkItemUpdateAreasAsTagsConfig config) : base(me)
        {
            _config = config;
        }

        #endregion
    }
}