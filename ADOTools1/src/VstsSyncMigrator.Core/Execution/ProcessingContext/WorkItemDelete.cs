using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemDelete : ProcessingContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.WorkItemDelete"));

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            WorkItemStoreContext targetStore = new WorkItemStoreContext(Engine.Target, WorkItemStoreFlags.BypassRules);
            TfsQueryContext tfsqc = new TfsQueryContext(targetStore);
            tfsqc.AddParameter("TeamProject", Engine.Target.Name);
            tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject  AND [System.AreaPath] UNDER '{0}\_DeleteMe'", Engine.Target.Name);
            WorkItemCollection workitems = tfsqc.Execute();

            // Send some traces.
            _mySource.Value.TraceInformation("Update {0} work items?", workitems.Count);
            _mySource.Value.Flush();

            int current = workitems.Count;
            //int count = 0;
            //long elapsedms = 0;
            var tobegone = (from WorkItem wi in workitems where wi.AreaPath.Contains("_DeleteMe") select wi.Id).ToList();

            foreach (int begone in tobegone)
            {
                targetStore.Store.DestroyWorkItems(new List<int>() { begone });

                // Send some traces.
                _mySource.Value.TraceInformation("Deleted {0}", begone);
                _mySource.Value.Flush();
            }

            stopwatch.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
            _mySource.Value.Flush();
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "WorkItemDelete"; }
        }

        public WorkItemDelete(MigrationEngine me) : base(me)
        {

        }

        #endregion
    }
}