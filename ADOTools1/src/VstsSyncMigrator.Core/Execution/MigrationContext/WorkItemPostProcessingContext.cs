using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemPostProcessingContext : MigrationContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.WorkItemPostProcessingContext"));

        #endregion

        #region - Private Members

        private readonly WorkItemPostProcessingConfig _config;

        private string BuildQueryBitConstraints()
        {
            // Initialize.
            string constraints = string.Empty;

            if (_config.WorkItemIDs != null && _config.WorkItemIDs.Count > 0)
            {
                if (_config.WorkItemIDs.Count == 1)
                    constraints += string.Format(" AND [System.Id] = {0} ", _config.WorkItemIDs[0]);
                else
                    constraints += string.Format(" AND [System.Id] IN ({0}) ", string.Join(",", _config.WorkItemIDs));
            }

            if (Engine.WorkItemTypeDefinitions != null && Engine.WorkItemTypeDefinitions.Count > 0)
            {
                if (Engine.WorkItemTypeDefinitions.Count == 1)
                    constraints += string.Format(" AND [System.WorkItemType] = '{0}' ", Engine.WorkItemTypeDefinitions.Keys.First());
                else
                    constraints += string.Format(" AND [System.WorkItemType] IN ('{0}') ", string.Join("','", Engine.WorkItemTypeDefinitions.Keys));
            }

            if (!String.IsNullOrEmpty(_config.QueryBit))
                constraints += _config.QueryBit;
            return constraints;
        }

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            // Create a stop watch to measure the query execution time.
            Stopwatch queryTimer = new Stopwatch();

            // Create a stop watch to measure the change against a work item.
            Stopwatch modificationTimer = new Stopwatch();

            // Start timer.
            queryTimer.Start();

            WorkItemStoreContext sourceStore = new WorkItemStoreContext(Engine.Source, WorkItemStoreFlags.None);

            // Create a new query context.
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);

            // Add parameters to context.
            tfsqc.AddParameter("TeamProject", Engine.Source.Name);

            // Builds the constraint part of the query
            string constraints = BuildQueryBitConstraints();

            // Set the query.
            tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.Id]", constraints);
            //tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc", constraints);

            // Execute.
            WorkItemCollection sourceWIS = tfsqc.Execute();

            // Send some traces.
            _mySource.Value.TraceInformation("Migrate {0} work items?", sourceWIS.Count);
            _mySource.Value.Flush();

            WorkItemStoreContext targetStore = new WorkItemStoreContext(Engine.Target, WorkItemStoreFlags.BypassRules);
            Project destProject = targetStore.GetProject();

            // Send some traces.
            _mySource.Value.TraceInformation("Found target project as {0}", destProject.Name);
            _mySource.Value.Flush();

            int currentWI = sourceWIS.Count;
            int countWI = 0;
            long timeElapsed = 0;
            foreach (WorkItem sourceWI in sourceWIS)
            {
                // Reset and start.
                modificationTimer.Reset();
                modificationTimer.Start();

                WorkItem targetFound;
                targetFound = targetStore.FindReflectedWorkItem(sourceWI, Engine.ReflectedWorkItemIdFieldName, false);

                // Send some traces.
                _mySource.Value.TraceInformation("{0} - Updating: {1}-{2}", currentWI, sourceWI.Id, sourceWI.Type.Name);
                _mySource.Value.Flush();

                if (targetFound == null)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("{0} - WARNING: does not exist {1}-{2}", currentWI, sourceWI.Id, sourceWI.Type.Name);
                    _mySource.Value.Flush();
                }
                else
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("...Exists");
                    _mySource.Value.Flush();

                    targetFound.Open();
                    Engine.ApplyFieldMappings(sourceWI, targetFound);
                    if (targetFound.IsDirty)
                    {
                        try
                        {
                            targetFound.Save();

                            // Send some traces.
                            _mySource.Value.TraceInformation("Updated");
                            _mySource.Value.Flush();
                        }
                        catch (ValidationException ve)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("[FAILED] {0}", ve.Message);
                            _mySource.Value.Flush();
                        }

                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("No changes");
                        _mySource.Value.Flush();
                    }
                    sourceWI.Close();
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
            get { return "WorkItemPostProcessingContext"; }
        }

        public WorkItemPostProcessingContext(MigrationEngine me, WorkItemPostProcessingConfig config) : base(me)
        {
            _config = config;
        }

        #endregion
    }
}