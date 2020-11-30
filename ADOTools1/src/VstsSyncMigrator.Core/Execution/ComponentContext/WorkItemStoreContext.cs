using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemStoreContext
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.WorkItemStoreContext"));

        // Create locking object.
        private static readonly object _lock = new object();

        #endregion

        #region - Private Members

        private readonly WorkItemStoreFlags _bypassRules;                       // todo: Why is it required? It is used nowhere.
        private readonly ITeamProjectContext _targetTfs;
        private readonly WorkItemStore _wiStore;                              
        private readonly ConcurrentDictionary<int, WorkItem> _foundWIs;         // Thead-safe cache for found work items.

        #endregion

        #region - Public Members

        public WorkItemStore Store
        {
            get { return _wiStore; }
        }

        public WorkItemStoreContext(ITeamProjectContext targetTfs, WorkItemStoreFlags bypassRules)
        {
            // Keep access attempt time.
            DateTime startTime = DateTime.UtcNow;

            // Create a stop watch to measure the access time.
            Stopwatch accessTimer = System.Diagnostics.Stopwatch.StartNew();

            // Start timer.
            accessTimer.Start();

            // Assign destination/target team project context.
            _targetTfs = targetTfs;

            // Assign bypass rules.
            _bypassRules = bypassRules;

            try
            {
                // Access work item store.
                _wiStore = new WorkItemStore(targetTfs.Collection, bypassRules);

                // Stop timer.
                accessTimer.Stop();

                // Send telemetry data.
                Telemetry.Current.TrackDependency("Azure DevOps", "TeamService", "GetWorkItemStore", startTime, accessTimer.Elapsed, true);
            }
            catch (Exception ex)
            {
                // Stop timer.
                accessTimer.Stop();

                // Send telemetry data.
                Telemetry.Current.TrackDependency("Azure DevOps", "TeamService", "GetWorkItemStore", startTime, accessTimer.Elapsed, false);
                Telemetry.Current.TrackException(
                            ex,
                            new Dictionary<string, string> {
                                { "CollectionUrl", targetTfs.Collection.Uri.ToString() }
                            },
                            new Dictionary<string, double> {
                                { "Time",accessTimer.ElapsedMilliseconds }
                            }
                       );

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"[EXCEPTION] {ex}");
                _mySource.Value.Flush();

                throw;
            }
           
            // Instantiate the cache for found work items.
            _foundWIs = new ConcurrentDictionary<int, WorkItem>();
        }

        public Project GetProject()
        {
            return (from Project x in Store.Projects where x.Name.ToUpper() == _targetTfs.Name.ToUpper() select x).SingleOrDefault();
        }

        public string GenerateReflectedWorkItemId(WorkItem w)
        {
            // Initialize.
            string rwwid = null;

            // Get the ADO project name where this work item exists.
            // Reading this property calls internal methods that browse
            // a collection and this operation is not thread-safe.
            // Synchronize the access for this property.
            lock(_lock)
            {
                rwwid = string.Format("{0}/{1}/_workitems/edit/{2}", w.Store.TeamProjectCollection.Uri.ToString().TrimEnd('/'), w.Project.Name, w.Id);
            }

            // Return the reflected work item id generated.
            return rwwid;
        }

        public int GetReflectedWorkItemId(WorkItem w, string reflectedWorkItemIdField)
        {
            string rwiid = w.Fields[reflectedWorkItemIdField].Value.ToString();
            if (Regex.IsMatch(rwiid, @"(http(s)?://)?([\w-]+\.)+[\w-]+(/[\w- ;,./?%&=]*)?"))
                return int.Parse(rwiid.Substring(rwiid.LastIndexOf(@"/") + 1));
            return 0;
        }

        public WorkItem FindReflectedWorkItem(WorkItem wiToFind, string reflectedWorkItemIdField, bool cache)
        {
            // Initialize.
            string reflectedWorkItemIdToFind = GenerateReflectedWorkItemId(wiToFind);

            // Try to read from the cache first.
            // Return work item found from cache.
            if (_foundWIs.TryGetValue(wiToFind.Id, out WorkItem foundWI))
                return foundWI;

            // Does this field exist on the work item at the source? Is it defined with a value?
            if (wiToFind.Fields.Contains(reflectedWorkItemIdField) && !string.IsNullOrEmpty(wiToFind.Fields[reflectedWorkItemIdField]?.Value?.ToString()))
            {
                // Initialize.
                string rwiid = wiToFind.Fields[reflectedWorkItemIdField].Value.ToString();
                int idToFind = GetReflectedWorkItemId(wiToFind, reflectedWorkItemIdField);
                // Is it found?
                if (idToFind == 0)
                    foundWI = null;
                else
                {
                    // Try to find the work item at the destination?
                    foundWI = Store.GetWorkItem(idToFind);
                    // Confirm the reflected work item id field has the reference to work item at the source.
                    if (!(foundWI.Fields[reflectedWorkItemIdField].Value.ToString() == rwiid))
                        foundWI = null;
                }                
            }
            
            // If there is no reference on the work item at the source, try to find the reflected work item id directly at the destination.
            if (foundWI == null)
                foundWI = FindReflectedWorkItemByReflectedWorkItemId(reflectedWorkItemIdToFind, reflectedWorkItemIdField);

            // Cache the work item found if needed.
            // Add to cache or update work item stored with this id.
            if (foundWI != null && cache)
                _foundWIs.AddOrUpdate(wiToFind.Id, foundWI, (k, v) => v = foundWI);

            // Return the work item found at the destination.
            return foundWI;
        }

        public WorkItem FindReflectedWorkItemByReflectedWorkItemId(WorkItem w, string reflectedWotkItemIdField)
        {
            return FindReflectedWorkItemByReflectedWorkItemId(GenerateReflectedWorkItemId(w), reflectedWotkItemIdField);
        }

        public WorkItem FindReflectedWorkItemByReflectedWorkItemId(int refId, string reflectedWotkItemIdField, bool cache)
        {
            // Initialize.
            int sourceIdKey = ~refId;

            // Try to read from the cache.
            if (_foundWIs.TryGetValue(sourceIdKey, out var workItem))
                return workItem;

            IEnumerable<WorkItem> QueryWorkItems()
            {
                TfsQueryContext query = new TfsQueryContext(this);
                query.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject]=@TeamProject AND [{0}] Contains '@idToFind'", reflectedWotkItemIdField);
                query.AddParameter("idToFind", refId.ToString());
                query.AddParameter("TeamProject", _targetTfs.Name);
                foreach(WorkItem wi in query.Execute())
                    yield return wi;
            }

            var foundWorkItem = QueryWorkItems().FirstOrDefault(wi => wi.Fields[reflectedWotkItemIdField].Value.ToString().EndsWith("/" + refId));
            if (cache && foundWorkItem != null) _foundWIs[sourceIdKey] = foundWorkItem;
            return foundWorkItem;
        }

        public WorkItem FindReflectedWorkItemByReflectedWorkItemId(string refId, string reflectedWotkItemIdField)
        {
            TfsQueryContext query = new TfsQueryContext(this);
            query.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject]=@TeamProject AND [{0}] = @idToFind", reflectedWotkItemIdField);
            query.AddParameter("idToFind", refId.ToString());
            query.AddParameter("TeamProject", _targetTfs.Name);
            return FindWorkItemByQuery(query);
        }

        public WorkItem FindWorkItemByQuery(TfsQueryContext query)
        {
            // Initialize.
            WorkItemCollection wic;

            // Execute the query.
            wic = query.Execute();

            if (wic.Count == 0)
                return null;
            return wic[0];
        }

        public WorkItem GetRevision(WorkItem w, int revision)
        {
            // Get work item specified revision.
            return Store.GetWorkItem(w.Id, revision);
        }

        #endregion
    }
}
